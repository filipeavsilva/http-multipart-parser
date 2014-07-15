using HttpMultipartParser.Data;
using System.IO;

namespace HttpMultipartParser.Decoding {
	internal interface IContentDecoder {
		BinaryData DecodePart (byte[] content, string contentType);

		TextData DecodePart (string content, string contentType);

		StreamedFileData DecodeAndStreamPart (BufferedStream content, string contentType);
	}
}
