using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

//
// HTTPMultipartParser
// 
// Parses a multipart form data stream and returns the included fields.
// Has the option to read included files into memory and return them as fields
//   or to stream those files to reduce memory consumption.
//   
// 2013 Filipe Silva https://github.com/Darchangel
//

///<summary></summary>
namespace HttpTools.Util { //Feel free to change this if needed

    //Delegate types to be included in returned streamed file objects
    /// <summary>
    /// Delegate for writing a file included in a multipart request
    /// to a file on disk
    /// </summary>
    /// <param name="filePath">The path where to write the file</param>
    public delegate void WriteMultiPartFile(string filePath);

    /// <summary>
    /// Delegate for obtaining the data of a file included in a
    /// multipart request
    /// </summary>
    /// <returns>
    /// The file's data.
    /// <i>string</i> if it is a text file,
    /// <i>byte[]</i> otherwise.
    /// </returns>
    public delegate object GetFileData();

    /// <summary>
    /// Delegate for discarding a file included in a multipart request.
    /// Reads the request stream until the end of the file.
    /// </summary>
    public delegate void DiscardFile();

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

        private Encoding encoding = Encoding.UTF8;
        private EFileHandlingType parseType = EFileHandlingType.ALL_BUFFERED; //Defaults to buffering everything
        private Stream stream;
        private string boundary = null;
        private byte[] boundaryBytes = null;
        private byte[] finishBytes = Encoding.UTF8.GetBytes("--"); //Bytes indicating the end of the stream.
        private StreamedFileData fileWaiting = null; //The last unread streamed file returned. Null if it was already read.
        private bool finished = false; //true if the stream has been parsed completely.

        /// <summary>
        /// Prepare a Multipart Parser
        /// </summary>
        /// <param name="stream">The HTTP multipart request body</param>
        public HTTPMultipartParser(Stream stream) {
            this.stream = stream;
            this.Fields = new Dictionary<string, MultipartData>();
        }

        /// <summary>
        /// Prepare a Multipart Parser with a specific encoding
        /// </summary>
        /// <param name="stream">The HTTP multipart request body</param>
        /// <param name="encoding">The encoding to use when parsing</param>
        public HTTPMultipartParser(Stream stream, Encoding encoding) {
            this.stream = stream;
            this.encoding = encoding;
            this.Fields = new Dictionary<string, MultipartData>();

            CRbytes = encoding.GetBytes("\r");
            LFbytes = encoding.GetBytes("\n");
            CRLFbytes = encoding.GetBytes("\r\n");
            finishBytes = encoding.GetBytes("--");
        }

        /// <summary>
        /// Prepare a Multipart Parser and specify the file handling type
        /// </summary>
        /// <param name="stream">The HTTP multipart request body</param>
        /// <param name="type">How the parser should handle files</param>
        public HTTPMultipartParser(Stream stream, EFileHandlingType type) {
            this.stream = stream;
            this.parseType = type;
            this.Fields = new Dictionary<string, MultipartData>();
        }

        /// <summary>
        /// Prepare a Multipart Parser with a specific encoding, specifying the file handling
        /// </summary>
        /// <param name="stream">The HTTP multipart request body</param>
        /// <param name="encoding">The encoding to use when parsing</param>
        /// <param name="type">How the parser should handle files</param>
        public HTTPMultipartParser(Stream stream, Encoding encoding, EFileHandlingType type) {
            this.stream = stream;
            this.encoding = encoding;
            this.parseType = type;
            this.Fields = new Dictionary<string, MultipartData>();

            CRbytes = encoding.GetBytes("\r");
            LFbytes = encoding.GetBytes("\n");
            CRLFbytes = encoding.GetBytes("\r\n");
            finishBytes = encoding.GetBytes("--");
        }

        //If in any STREAMED mode, yields an object when a file (of the correct type) is found, nothing when it is finished
        //If in buffered mode, collects every field into the Fields dictionary and 
        /// <summary>
        /// Parse the multipart request
        /// </summary>
        /// <returns>
        /// If any file type is set to be streamed, returns an IEnumerable of the streamed files.
        /// If no files are streamed, returns an empty IEnumerable.
        /// </returns>
        public IEnumerable<StreamedFileData> Parse() {
            if (this.finished) //It's done!
                yield break;

            if (boundary == null) { //Nothing read yet
                // The first line should contain the delimiter
                string terminator;
                boundary = this.ReadLine(out terminator);
                boundaryBytes = encoding.GetBytes(terminator + boundary); //Include the line terminator in the boundary bytes,
                // to avoid including it in any binary data
            }

            if (!string.IsNullOrEmpty(boundary)) { //If it is null here, then something is wrong

                string name = null;
                string filename = null;
                string contentType = null;
                bool isBinary = false;
                bool isFile = false;

                if (fileWaiting != null) { //There is a streamed file waiting. Let's save it.
                    if (fileWaiting.IsBinary) {
                        Fields.Add(fileWaiting.Name, new BinaryData {
                            ContentType = fileWaiting.ContentType,
                            Name = fileWaiting.Name,
                            FileName = fileWaiting.FileName,
                            Data = (byte[])fileWaiting.GetData()
                        });
                    } else {
                        Fields.Add(fileWaiting.Name, new TextData {
                            ContentType = fileWaiting.ContentType,
                            Name = fileWaiting.Name,
                            FileName = fileWaiting.FileName,
                            Data = (string)fileWaiting.GetData()
                        });
                    }

                    fileWaiting = null; //There isn't one anymore...
                }

                string line = this.ReadLine();
                while (!this.finished && line != null) {
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
                            if (this.parseType == EFileHandlingType.ALL_BUFFERED ||
                                this.parseType == EFileHandlingType.STREAMED_TEXT) { //Buffered file

                                Fields.Add(name, new BinaryData {
                                    ContentType = contentType ?? "application/octet-stream",
                                    Name = name,
                                    FileName = filename,
                                    Data = ReadBinaryFile()
                                });

                                //reset stuff
                                name = null;
                                filename = null;
                                contentType = null;
                                isBinary = false;
                                isFile = false;

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

                                fileWaiting = file;
                                yield return file;
                                isFile = false;
                            }

                        } else { //Text data
                            if (isFile &&
                                (this.parseType == EFileHandlingType.ALL_STREAMED ||
                                this.parseType == EFileHandlingType.STREAMED_TEXT)) { //Stream it
                                StreamedFileData file = new StreamedFileData {
                                    Name = name,
                                    ContentType = contentType ?? "text/plain",
                                    IsBinary = false,
                                    FileName = filename,

                                    ToFile = WriteTextStreamToFile,
                                    Discard = DiscardTextFile,
                                    GetData = ReadTextFile
                                };

                                fileWaiting = file;
                                yield return file;
                                isFile = false;

                            } else { //Non-file or buffered file

                                if (filename == null) {
                                    Fields.Add(name, new TextData {
                                        ContentType = contentType ?? "text/plain",
                                        Name = name,
                                        Data = ReadTextFile()
                                    });
                                } else {
                                    Fields.Add(name, new TextData {
                                        ContentType = contentType ?? "text/plain",
                                        Name = name,
                                        FileName = filename,
                                        Data = ReadTextFile()
                                    });
                                }

                                //reset stuff
                                name = null;
                                filename = null;
                                contentType = null;
                                isBinary = false;
                                isFile = false;
                            }
                        }
                    }

                    //Keep on readin'
                    line = this.ReadLine();
                }

                yield break; //FINISHED!
            } else {
                //Should it be an ArgumentException?
                throw new ArgumentException("Stream is not a well-formed multipart string");
            }
        }

        //Reads a text file part into a string
        private string ReadTextFile() {
            StringBuilder data = new StringBuilder();
            string line = this.ReadLine();

            while (!line.StartsWith(boundary)) {
                data.Append("\r\n").Append(line); //Honor existing line breaks
                line = this.ReadLine();
            }

            if (line == this.boundary + "--") { //Data ends
                stream.Close(); //Close the stream
                this.finished = true;
            }

            data.Remove(0, 2); //Remove the first \r\n
            return data.ToString();
        }

        //Reads a text file but ignores it
        private void DiscardTextFile() {
            string line = this.ReadLine();

            while (!line.StartsWith(boundary)) {
                line = this.ReadLine();
            }

            if (line == this.boundary + "--") { //Data ends
                stream.Close(); //Close the stream
                this.finished = true;
            }
        }

        //Writes a text file part to a physical file
        private void WriteTextStreamToFile(string filePath) {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            StreamWriter file = new StreamWriter(filePath);
            string line = this.ReadLine();

            while (line != null && !line.StartsWith(this.boundary)) {
                file.WriteLine(line);
                line = this.ReadLine();
            }

            if (line == this.boundary + "--") { //Data ends
                stream.Close(); //Close the stream
                this.finished = true;
            }

            file.Close();
        }

        //Writes a binary file part to a physical file
        private void WriteBinaryStreamToFile(string filePath) {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileStream file = File.Open(filePath, FileMode.Create);

            int numBytes = BinaryDataToStream(file);

            file.SetLength(numBytes); //Cut the file to size
            file.Flush();
            file.Close();
        }

        //Reads a binary file part into a byte array
        private byte[] ReadBinaryFile() {
            MemoryStream ms = new MemoryStream();

            int numBytes = BinaryDataToStream(ms);

            //Finally, return the bytes
            byte[] result = new byte[numBytes];
            ms.Position = 0;
            ms.Read(result, 0, numBytes);
            return result;
        }

        //Prepares the partial search table for the KMP algorithm
        private int[] KMP_PartialSearchTable(byte[] bytes) {
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
        private void DiscardBinaryFile() {
            BinaryDataToStream(Stream.Null); //Just read everything
        }

        //Writes the next block of binary data to a stream (e.g. MemoryStream or FileStream).
        //Returns the number of valid bytes written (the stream may contain more)
        //Implements the Knuth–Morris–Pratt algorithm to detect the Multipart boundary
        private int BinaryDataToStream(Stream s) {

            int streamLength = 0; //How many bytes in the stream are to be considered

            //KMP Algorithm vars
            int i = 0;
            int[] partialSearch = KMP_PartialSearchTable(boundaryBytes);
            List<byte> backtrack = new List<byte>(); //List for all the backtrackable bytes
            //The 'S[m]' in the KMP algorithm corresponds to the beginning of this list

            byte? _data = ReadByte();

            while (_data.HasValue) {
                byte data;

                if (i < backtrack.Count) {//m + i still falls inside the backtrack, no new data was fetched
                    data = backtrack[i];
                } else {
                    data = _data.Value;
                    s.WriteByte(data);
                    backtrack.Add(data); //Add to the backtrack list
                }

                streamLength++;

                if (boundaryBytes[i] == data) {
                    if (i == boundaryBytes.Length - 1) {
                        streamLength -= backtrack.Count; //return m
                        //Check if next bytes are \r and/or \n,or if data is finished (extra '--')
                        byte[] next = ReadIfNext(new byte[][] { LFbytes, CRLFbytes, CRbytes, finishBytes });
                        if (next == finishBytes) { //data ended!
                            stream.Close();
                            this.finished = true;
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
                    _data = ReadByte(); //Let's get a new one!
                }
            }

            s.Position = streamLength;
            return streamLength;
        }

        #region Stream buffer stuff

        /* This region accesses the multipart stream using a byte buffer.
	     * This is needed to have control over the buffer (the stream needs
         * to be accessed from multiple places (to stream files), and e.g. StreamReaders "steal" bytes
         * into their internal buffers, making them unavailable to other reads).
         * Thus, lo and behold, a custom stream reader is born! */

        private static int BUF_LEN = 4096;

        //Byte arrays to keep line endings
        private byte[] CRbytes = Encoding.UTF8.GetBytes("\r");
        private byte[] LFbytes = Encoding.UTF8.GetBytes("\n");
        private byte[] CRLFbytes = Encoding.UTF8.GetBytes("\r\n");

        private byte[] buffer = new byte[BUF_LEN];
        private int bufferStart = 0, bufferEnd = -1;

        //Reads a line of text (reads bytes until a line ending (\n, \r or \r\n)
        private string ReadLine() {
            string _;
            return ReadLine(out _); //Just ignore the out string
        }

        //Reads a line of text (reads bytes until a line ending (\n, \r or \r\n).
        // Returns in the out parameter the line ending found.
        private string ReadLine(out string lineTerminator) {
            MemoryStream ms = new MemoryStream();
            int lineSize = 0;
            lineTerminator = null; //Yes, could go wrong, but this _isn't supposed_ to happen.

            while (true) {
                if (!(bufferEnd > 0 && bufferStart < bufferEnd)) { //Buffer is empty
                    if (!FillBuffer())
                        break;
                }

                //While buffer has data and no line endings found
                while (bufferStart <= bufferEnd &&
                        buffer[bufferStart] != CRbytes[0] &&
                        buffer[bufferStart] != LFbytes[0]) {

                    ms.WriteByte(buffer[bufferStart]);
                    lineSize++;
                    bufferStart++;
                }

                if (bufferStart <= bufferEnd) { //Stopped due to finding CR or LF

                    if (StartsWith(buffer, bufferStart, LFbytes)) { //LF found, line ends with LF
                        bufferStart += LFbytes.Length; //Skip the LF char
                        lineTerminator = "\n";

                    } else if (StartsWith(buffer, bufferStart, CRbytes)) { //CR found
                        bufferStart += CRbytes.Length; //Skip the CR right away

                        if (bufferStart <= bufferEnd) { //Stuff still in the buffer
                            if (StartsWith(buffer, bufferStart, LFbytes)) { //Was it a CRLF, maybe?
                                bufferStart += LFbytes.Length; //Skip the LF as well
                                lineTerminator = "\r\n";
                            } else {
                                lineTerminator = "\r";
                            }

                        } else { //Buffer finished. Next bytes could be an LF
                            FillBuffer();

                            if (StartsWith(buffer, bufferStart, LFbytes)) { //Now, was it a CRLF?
                                bufferStart += LFbytes.Length; //Skip the LF
                                lineTerminator = "\r\n";
                            } else { //Just CR
                                lineTerminator = "\r";
                            }
                        }
                    }
                    break;
                }
                //Else, just the end of the buffer. Will be filled up in the next iteration
            }

            byte[] line = new byte[lineSize];
            ms.Position = 0;
            ms.Read(line, 0, lineSize);

            return encoding.GetString(line);
        }

        //Try to read a single byte from the buffer
        private byte? ReadByte() {
            if (!(bufferStart < bufferEnd)) { //Buffer is empty
                if (!FillBuffer())
                    return null;
            }

            byte result = buffer[bufferStart];
            bufferStart++;
            return result;
        }

        private bool FillBuffer() {
            if (this.finished)
                return false;

            int offset = bufferEnd - bufferStart; //Check if there is still something in the buffer

            if (offset >= 0) { //Pass the existing data to the beginning
                for (int i = 0; i <= offset; i++) {
                    buffer[i] = buffer[bufferStart + i];
                }
            }

            //Read into the rest of the buffer
            int read = stream.Read(buffer, offset + 1, buffer.Length - (offset + 1));
            if (read <= 0)
                return false;

            bufferStart = 0;
            bufferEnd = read + offset; //+ 1 (from offset) - 1 (from read);

            return true;
        }

        //Search the buffer for one of multiple byte sequences.
        //If one is found, read it from the buffer and return it.
        private byte[] ReadIfNext(byte[][] possibilities) {
            /*TODO: If a possibility is contained in another this method will
             *  miss the longer one if the shortest comes first (e.g. "\r" coming before "\r\n").
             *  
             * Will be fixed when time permits. For now, just make sure things like e.g. "\r\n" come before "\r".
             */
            byte[] found = null;

            foreach (byte[] maybe in possibilities) {
                if (bufferEnd - bufferStart < maybe.Length - 1)
                    FillBuffer();

                if (StartsWith(buffer, bufferStart, maybe)) {
                    bufferStart += maybe.Length; //Remove it from the buffer
                    found = maybe;
                    break;
                }
            }

            return found;
        }

        //Checks if a byte array (from a given offset) starts with another.
        private bool StartsWith(byte[] searchIn, int searchFrom, byte[] searchThis) {
            int index = 0;

            while (index < searchThis.Length) {
                if (searchFrom + index >= searchIn.Length ||
                    searchThis[index] != searchIn[searchFrom + index])
                    return false;
                index++;
            }

            return true;
        }

        #endregion
    }
}
