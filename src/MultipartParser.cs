using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HttpMultipartParser.Data;

//
// HTTPMultipartParser
// 
// Parses a multipart stream and returns the included fields.
// Has the option to buffer included files into memory and return them as fields
//   or to stream those files to reduce memory consumption.
//   
// 2013 Filipe Silva https://github.com/darchangel
//
using System.Net;


namespace HttpMultipartParser {

	//Delegate types to be included in returned streamed file objects
	/// <summary>
	/// Delegate for writing a file included in a multipart request
	/// to a file on disk
	/// </summary>
	/// <param name="filePath">The path where to write the file</param>
	public delegate void WriteMultiPartFile (string filePath);

	/// <summary>
	/// Delegate for obtaining the data of a file included in a
	/// multipart request
	/// </summary>
	/// <returns>
	/// The file's data.
	/// <i>string</i> if it is a text file,
	/// <i>byte[]</i> otherwise.
	/// </returns>
	public delegate object GetFileData ();

	/// <summary>
	/// Delegate for discarding a file included in a multipart request.
	/// Reads the request stream until the end of the file.
	/// </summary>
	public delegate void DiscardFile ();

	/// <summary>
	/// Parser for HTTP Multipart content.
	/// Parses the data into a dictionary of fields, indexed by name.
	/// If files are set to be streamed, provides an IEnumerable of
	/// the found files to be written to disk (or discarded).
	/// </summary>
	public class HTTPMultipartParser {

		/// <summary>
		/// Fields in the multipart data, indexed by field name
		/// </summary>
		public Dictionary<string, MultipartData> Fields {
			get;
			private set;
		}

		private readonly IDictionary<string, string> _headers;
		private readonly Encoding _encoding = Encoding.UTF8;
		private readonly EFileHandlingType _parseType = EFileHandlingType.ALL_BUFFERED; //Defaults to buffering everything
		private readonly BufferedStream _stream;
		private string _boundary = null;
		private byte[] _boundaryBytes = null;
		private readonly byte[] _finishBytes; //Bytes indicating the end of the stream.
		private StreamedFileData _fileWaiting = null; //The last unread streamed file returned. Null if it was already read.
		private bool _finished = false; //true if the stream has been parsed completely.
		private byte[] _terminatorBytes;


		public HTTPMultipartParser (Stream body, IDictionary<string, string> headers=null, EFileHandlingType fileHandling=EFileHandlingType.ALL_BUFFERED, Encoding encoding = null) {
			_headers = headers ?? new Dictionary<string, string> ();
			_parseType = fileHandling;
			Fields = new Dictionary<string, MultipartData>();

			if (encoding != null) {
				_encoding = encoding;
			} else if (_headers.ContainsKey(HttpRequestHeader.ContentEncoding.ToString())){
				_encoding = Encoding.GetEncoding(_headers[HttpRequestHeader.ContentEncoding.ToString ()]);
			}

			_stream = new BufferedStream(body);

			_finishBytes = _encoding.GetBytes("--");
		}

		/// <summary>
		/// Create a new Multipart parser, with a string body
		/// </summary>
		/// <param name="body">Request body</param>
		/// <param name="headers">Request headers</param>
		/// <param name="encoding">Specific content encoding</param>
		public HTTPMultipartParser (string body, IDictionary<string, string> headers=null, Encoding encoding = null) {
			_headers = headers ?? new Dictionary<string, string> ();
			_parseType = EFileHandlingType.ALL_BUFFERED; //No use streaming if you've got a string...
			Fields = new Dictionary<string, MultipartData>();

			if (encoding != null) {
				_encoding = encoding;
			} else if (_headers.ContainsKey(HttpRequestHeader.ContentEncoding.ToString())){
				_encoding = Encoding.GetEncoding(_headers[HttpRequestHeader.ContentEncoding.ToString ()]);
			}

			_stream = new BufferedStream(new MemoryStream (_encoding.GetBytes (body)), _encoding);

			_finishBytes = _encoding.GetBytes("--");
		}

		/// <summary>
		/// Parse the multipart request
		/// </summary>
		/// <returns>
		/// If any file type is set to be streamed, returns an IEnumerable of the streamed files.
		/// If no files are streamed, returns an empty IEnumerable.
		/// The enumerable must be read to the end before all fields (not only files) are available
		/// </returns>
		public IEnumerable<StreamedFileData> Parse () {
			if (this._finished) //It's done!
				yield break;

			if (_boundary == null) { //Nothing read yet
				// The first line should contain the delimiter
				string terminator;
				_boundary = _stream.ReadLine(out terminator);
				_terminatorBytes = _encoding.GetBytes (terminator); //Keep the line terminator bytes

				if (_boundary.EndsWith("--")) { //For some reason the request came empty
					this._finished = true; //Stop parsing
					_stream.Close();
					yield break;
				}

				_boundaryBytes = _encoding.GetBytes(/*terminator + */_boundary); //Include the line terminator in the boundary bytes,
				// to avoid including it in any binary data
			}

			if (!string.IsNullOrEmpty(_boundary)) { //If it is null here, then something is wrong

				string name = null;
				string filename = null;
				string contentType = null;
				bool isBinary = false;
				bool isFile = false;

				if (_fileWaiting != null) { //There is a streamed file waiting. Let's save it.
					if (_fileWaiting.IsBinary) {
						Fields.Add(_fileWaiting.Name, new BinaryData {
							ContentType = _fileWaiting.ContentType,
							Name = _fileWaiting.Name,
							FileName = _fileWaiting.FileName,
							Data = (byte[])_fileWaiting.GetData()
						});
					} else {
						Fields.Add(_fileWaiting.Name, new TextData {
							ContentType = _fileWaiting.ContentType,
							Name = _fileWaiting.Name,
							FileName = _fileWaiting.FileName,
							Data = (string)_fileWaiting.GetData()
						});
					}

					_fileWaiting = null; //There isn't one anymore...
				}

				string line = _stream.ReadLine();
				while (!this._finished && line != null) {
					if (line.StartsWith("Content-Disposition")) { //Field name and data name
						Regex nameRe = new Regex(@"name=""(.*?)""");
						Match nameMatch = nameRe.Match(line);
						if (nameMatch.Success) {
							name = nameMatch.Groups[1].Value;
						}

						Regex fileNameRe = new Regex(@"filename=""(.*?)""");
						Match fileNameMatch = fileNameRe.Match(line);
						if (fileNameMatch.Success) {
							filename = fileNameMatch.Groups[1].Value;
							isFile = true;
						}

					} else if (line.StartsWith("Content-Type")) { //File type
						contentType = line.Remove(0, 14).Trim(); //Removes 'Content-Type: '(14 chars) (and trims just in case)
						isBinary = ContentTypes.IsBinary(contentType); //Checks if it is binary data

					} else if (line == string.Empty) { //Data begins
						if (isBinary) { //Binary data, always file
							if (this._parseType == EFileHandlingType.ALL_BUFFERED ||
									this._parseType == EFileHandlingType.STREAMED_TEXT) { //Buffered file

								Fields.Add(name, new BinaryData {
									ContentType = contentType ?? "application/octet-stream",
									Name = name,
									FileName = filename,
									Data = ReadBinaryFile()
								});

							} else { //Stream it
								StreamedFileData file = new StreamedFileData {
									Name = name,
									ContentType = contentType ?? "application/octet-stream",
									IsBinary = true,
									FileName = filename,

									ToFile = WriteBinaryStreamToFile,
									Discard = DiscardBinaryFile,
									GetData = ReadBinaryFile
								};

								_fileWaiting = file;
								yield return file;
								isFile = false;
							}

						} else { //Text data
							if (isFile &&
									(this._parseType == EFileHandlingType.ALL_STREAMED ||
									this._parseType == EFileHandlingType.STREAMED_TEXT)) { //Stream it
								StreamedFileData file = new StreamedFileData {
									Name = name,
									ContentType = contentType ?? "text/plain",
									IsBinary = false,
									FileName = filename,

									ToFile = WriteTextStreamToFile,
									Discard = DiscardTextFile,
									GetData = ReadTextFile
								};

								_fileWaiting = file;
								yield return file;
								isFile = false;

							} else { //Non-file or buffered file

								if (filename == null) {
									Fields.Add(name, new TextData {
										ContentType = contentType ?? "text/plain",
										Name = name,
										Data = ReadTextFile(),
									});
								} else {
									Fields.Add(name, new TextData {
										ContentType = contentType ?? "text/plain",
										Name = name,
										FileName = filename,
										Data = ReadTextFile()
									});
								}

							}
						}
						//reset stuff
						name = null;
						filename = null;
						contentType = null;
						isBinary = false;
						isFile = false;
					}

					//Keep on readin'
					line = _stream.ReadLine();
				}

				yield break; //FINISHED!
			} else {
				//Should it be an ArgumentException?
				throw new ArgumentException("Stream is not a well-formed multipart string");
			}
		}

		/// <summary>
		/// Parse the multipart request to the end and returns the resulting fields.
		/// 
		/// If there are any streamed files, they will be saved to the Fields dictionary
		/// </summary>
		public void ParseToEnd () {
			foreach (var item in this.Parse()) {
				//Do nothing, just read it
			}
			//All done!
		}

		//Reads a text file part into a string
		private string ReadTextFile () {
			StringBuilder data = new StringBuilder();
			string line = _stream.ReadLine();

			while (!line.StartsWith(_boundary)) {
				data.Append("\r\n").Append(line); //Honor existing line breaks
				line = _stream.ReadLine();
			}

			if (line == this._boundary + "--") { //Data ends
				_stream.Close(); //Close the stream
				this._finished = true;
			}

			data.Remove(0, 2); //Remove the first \r\n
			return data.ToString();
		}

		//Reads a text file but ignores it
		private void DiscardTextFile () {
			string line = _stream.ReadLine();

			while (!line.StartsWith(_boundary)) {
				line = _stream.ReadLine();
			}

			if (line == this._boundary + "--") { //Data ends
				_stream.Close(); //Close the stream
				this._finished = true;
			}
		}

		//Writes a text file part to a physical file
		private void WriteTextStreamToFile (string filePath) {
			string dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			StreamWriter file = new StreamWriter(filePath);
			string line = _stream.ReadLine();

			while (line != null && !line.StartsWith(this._boundary)) {
				file.WriteLine(line);
				line = _stream.ReadLine();
			}

			if (line == this._boundary + "--") { //Data ends
				_stream.Close(); //Close the stream
				this._finished = true;
			}

			file.Close();
		}

		//Writes a binary file part to a physical file
		private void WriteBinaryStreamToFile (string filePath) {
			string dir = Path.GetDirectoryName(filePath);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			FileStream file = File.Open(filePath, FileMode.Create);

			long numBytes = BinaryDataToStream(file);

			file.SetLength(numBytes); //Cut the file to size
			file.Flush();
			file.Close();
		}

		//Reads a binary file part into a byte array
		private byte[] ReadBinaryFile () {
			MemoryStream ms = new MemoryStream();

			long numBytes = BinaryDataToStream(ms);

			//Finally, return the bytes
			byte[] result = new byte[numBytes];
			ms.Position = 0;
			ms.Read(result, 0, (int)numBytes);
			return result;
		}

		//Prepares the partial search table for the KMP algorithm
		private int[] KMP_PartialSearchTable (byte[] bytes) {
			int position = 2;
			int candidate = 0;
			int[] table = new int[bytes.Length];

			table[0] = -1;
			table[1] = 0;
			while (position < bytes.Length) {
				if (bytes[position - 1] == bytes[candidate]) {
					candidate++;
					table[position] = candidate;
					position++;
				} else if (candidate > 0) {
					candidate = table[candidate];
				} else {
					table[position] = 0;
					position++;
				}
			}

			return table;
		}

		//Reads a binary file but ignores its contents
		private void DiscardBinaryFile () {
			BinaryDataToStream(Stream.Null); //Just read everything
		}

		//Writes the next block of binary data to a stream (e.g. MemoryStream or FileStream).
		//Returns the number of valid bytes written (the stream may contain more)
		//Implements the Knuth–Morris–Pratt algorithm to detect the Multipart boundary
		private long BinaryDataToStream (Stream s) {

			long streamLength = 0; //How many bytes in the stream are to be considered

			//KMP Algorithm vars
			int i = 0;
			int[] partialSearch = KMP_PartialSearchTable(_boundaryBytes);
			List<byte> backtrack = new List<byte>(); //List for all the backtrackable bytes
			//The 'S[m]' in the KMP algorithm corresponds to the beginning of this list

			byte? _data = _stream.ReadByte();

			while (_data.HasValue) {
				byte data;

				if (i < backtrack.Count) {//m + i still falls inside the backtrack, no new data was fetched
					data = backtrack[i];
				} else {
					data = _data.Value;
					s.WriteByte(data);
					streamLength++;
					backtrack.Add(data); //Add to the backtrack list
				}

				if (_boundaryBytes[i] == data) {
					if (i == _boundaryBytes.Length - 1) {
						streamLength -= backtrack.Count; //return m
						//Check if next bytes are \r and/or \n,or if data is finished (extra '--')
						byte[] next = _stream.ReadIfNext(new byte[][] { _terminatorBytes, _finishBytes });

						if (next != null) { //Clear also line endings or data end from the file
							streamLength -= next.Length;
						}

						if (next == _finishBytes) { //data ended!
							_stream.Close();
							this._finished = true;
						}

						break;
					}

					i++;
				} else {
					backtrack.RemoveRange(0, i - partialSearch[i]); //m = m + i - T[i]
					if (partialSearch[i] > -1)
						i = partialSearch[i];
					else
						i = 0;
				}

				if (i >= backtrack.Count) { //m + i falls outside the backtrack list
					_data = _stream.ReadByte(); //Let's get a new one!
				}
			}

			return streamLength;
		}
	}
}
