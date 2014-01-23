using System;
using HttpMultipartParser.Data;

namespace HttpMultipartParser.Decoding {
	public interface IContentDecoder {
		MultipartData DecodePart (Stream s);
	}
}
