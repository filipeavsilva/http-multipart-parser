using HttpMultipartParser.Data;
using System.IO;

namespace HttpMultipartParser.Decoding {
	internal interface IContentDecoder {
		BinaryData DecodePart (byte[] content, string contentType);

		BufferedData DecodePart (string content, string contentType);

		StreamedData DecodeAndStreamPart (BufferedStream content, string contentType);
	}
}
