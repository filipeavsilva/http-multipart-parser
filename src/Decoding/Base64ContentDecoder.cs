using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpMultipartParser.Data;

namespace HttpMultipartParser.Decoding
{
    internal class Base64ContentDecoder : IContentDecoder
    {
        private Func<BufferedStream, WritePartToFile> _genWriteMultipartFileDelegateFunc;
        private Func<BufferedStream, GetBinaryData> _genGetBinaryDataDelegateFunc;
        private Func<BufferedStream, GetTextData> _genGetTextDataDelegateFunc;
        private Func<BufferedStream, DiscardPart> _genDiscardPartDelegateFunc;
        private Encoding _encoding;

        public Base64ContentDecoder(Func<BufferedStream, WritePartToFile> genWriteMultipartFileDelegateFunc,
                                    Func<BufferedStream, GetBinaryData> genGetBinaryDataDelegateFunc,
                                    Func<BufferedStream, GetTextData> genGetTextDataDelegateFunc,
                                    Func<BufferedStream, DiscardPart> genDiscardFileDelegateFunc,
                                    Encoding encoding)
        {
            _genWriteMultipartFileDelegateFunc = genWriteMultipartFileDelegateFunc;
            _genGetBinaryDataDelegateFunc = genGetBinaryDataDelegateFunc;
            _genGetTextDataDelegateFunc = genGetTextDataDelegateFunc;
            _genDiscardPartDelegateFunc = genDiscardFileDelegateFunc;
            _encoding = encoding;
        }

        public BufferedData DecodePart(byte[] content, string contentType) {
            //Assume the bytes are just an encoded string...
            return DecodePart(_encoding.GetString(content), contentType);
        }

        public BufferedData DecodePart(string content, string contentType)
        {
            bool isBinary = ContentTypes.IsBinary(contentType);

            var data = new Data.BufferedData
            {
                ContentType = contentType,
                IsBinary = isBinary
            };

            var bytes = Convert.FromBase64String(content);
            if(isBinary)
                data.Bytes = bytes;
            else
                data.Text = _encoding.GetString(bytes);
        }

        public StreamedData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            //Streamed base64?
            throw new NotImplementedException();
        }
    }
}
