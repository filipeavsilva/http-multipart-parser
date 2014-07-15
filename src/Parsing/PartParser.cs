using HttpMultipartParser.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.Parsing
{
    internal interface IPartParser
    {
        MultipartData ParsePart(BufferedStream part, IDictionary<string, Header> headers);
    }

    internal class PartParser : IPartParser
    {
        private IHeaderParser _headerParser;

        public PartParser(IHeaderParser headerParser)
        {
            _headerParser = headerParser;
        }

        public MultipartData ParsePart(BufferedStream part, IDictionary<string, Header> headers)
        {
            throw new NotImplementedException();
        }
    }
}
