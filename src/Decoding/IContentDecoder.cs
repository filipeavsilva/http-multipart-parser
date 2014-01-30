using HttpMultipartParser.Data;
using System.IO;

namespace HttpMultipartParser.Decoding {
	public interface IContentDecoder {
		MultipartData DecodePart (Stream s);
	}
}
