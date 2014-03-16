using System;
using System.Text;
using System.IO;

namespace HttpMultipartParser {
	/* Access the multipart stream using a byte buffer.
     * This is needed to have control over the buffer (the stream needs
     * to be accessed from multiple places (to stream files), and e.g. StreamReaders "steal" bytes
     * into their internal buffers, making them unavailable to other reads).
     * Thus, lo and behold, a custom stream reader is born! */
	internal class BufferedStream { //TODO: Is this class really needed? 

		private readonly Stream _stream;
		private readonly static int BUF_LEN = 4096;
		private readonly byte[] buffer = new byte[BUF_LEN];
        private readonly Encoding _encoding = Defaults.ENCODING;
		//Byte arrays to keep line endings
		private byte[] CRbytes;
		private byte[] LFbytes;

		private int bufferStart = 0, bufferEnd = -1;

		internal BufferedStream (Stream stream){
			_stream = stream;
			InitBytes();
		}

		internal BufferedStream (Stream stream, Encoding encoding) {
			_stream = stream;
			_encoding = encoding;

			InitBytes();
		}

		private void InitBytes(){
			CRbytes = _encoding.GetBytes("\r");
			LFbytes = _encoding.GetBytes("\n");
		}

		//Reads a line of text (reads bytes until a line ending (\n, \r or \r\n)
		internal string ReadLine () {
			string _;
			return ReadLine(out _); //Just ignore the out string
		}

		//Reads a line of text (reads bytes until a line ending (\n, \r or \r\n).
		// Returns, in the out parameter, the line ending found.
		internal string ReadLine (out string lineTerminator) {
			var ms = new MemoryStream();
			int lineSize = 0;
			lineTerminator = null; //Null if the buffer ends without a new line

			while (true) {
				if (bufferStart > bufferEnd) { //Buffer is empty
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

			return _encoding.GetString(line);
		}

		//Try to read a single byte from the buffer
		internal byte? ReadByte () {
			if (bufferStart > bufferEnd) { //Buffer is empty
				if (!FillBuffer ())
					return null;
			}

			byte result = buffer[bufferStart];
			bufferStart++;
			return result;
		}

		private bool FillBuffer () {

			int offset = bufferEnd - bufferStart; //Check if there is still something in the buffer

			if (offset >= 0) { //Pass the existing data to the beginning
				for (int i = 0; i <= offset; i++) {
					buffer[i] = buffer[bufferStart + i];
				}
			}

			//Read into the rest of the buffer
			int read = _stream.Read(buffer, offset + 1, buffer.Length - (offset + 1));
			if (read <= 0)
				return false;

			bufferStart = 0;
			bufferEnd = read + offset; //+ 1 (from offset) - 1 (from read);

			return true;
		}

		//Check if the next byte sequence in the buffer is one of the supplied ones.
		//If it is, read it from the buffer and return it.
		internal byte[] ReadIfNext (byte[][] possibilities) {
			/*			TODO: If a possibility is contained in another this method will
			 *  miss the longer one if the shortest comes first (e.g. "\r" coming before "\r\n").
			 *  Should we add a 'greedy' parameter, to match as long as possible?
			 * Will be fixed when time permits. For now, just make sure things like e.g. "\r\n" come before "\r".
			 */
			byte[] found = null;

			foreach (byte[] maybe in possibilities) {
				if (bufferEnd - bufferStart < maybe.Length - 1)
					FillBuffer();

				if (StartsWith(buffer, bufferStart, maybe)) {
					bufferStart += maybe.Length; //Remove it from the buffer (i.e. "read" it)
					found = maybe;
					break;
				}
			}

			return found;
		}

		//Checks if a byte array (from a given offset) starts with another.
		private static bool StartsWith (byte[] searchIn, int searchFrom, byte[] searchThis) {
			int index = 0;

			while (index < searchThis.Length) {
				if (searchFrom + index >= searchIn.Length ||
					searchThis[index] != searchIn[searchFrom + index])
					return false;
				index++;
			}

			return true;
		}

		internal void Close(){
			_stream.Close ();
		}

	}
}

