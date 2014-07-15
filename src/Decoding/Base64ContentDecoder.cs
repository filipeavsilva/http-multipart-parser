using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.Decoding
{
    internal class Base64ContentDecoder : IContentDecoder
    {
        private Func<BufferedStream, WriteFilePart> _genWriteMultipartFileDelegateFunc;
        private Func<BufferedStream, GetFileData> _genGetFileDataFunc;
        private Func<BufferedStream, DiscardPart> _genDiscardFileDelegateFunc;
        private Encoding _encoding;

        public Base64ContentDecoder(Func<BufferedStream, WriteFilePart> genWriteMultipartFileDelegateFunc,
                                    Func<BufferedStream, GetFileData> genGetFileDataDelegateFunc,
                                    Func<BufferedStream, DiscardPart> genDiscardFileDelegateFunc,
                                    Encoding encoding)
        {
            _genWriteMultipartFileDelegateFunc = genWriteMultipartFileDelegateFunc;
            _genGetFileDataFunc = genGetFileDataDelegateFunc;
            _genDiscardFileDelegateFunc = genDiscardFileDelegateFunc;
            _encoding = encoding;
        }

        public Data.BinaryData DecodePart(byte[] content, string contentType)
        {
            string base64String = _encoding.GetString(content);

            return new Data.BinaryData
            {
                Data = Convert.FromBase64String(base64String),
                ContentType = contentType
            };
        }

        public Data.BufferedData DecodePart(string content, string contentType)
        {
            byte[] bytes = Convert.FromBase64String(content);

            return new Data.BufferedData
            {
                Text = _encoding.GetString(bytes),
                ContentType = contentType
            };
        }

        public Data.StreamedData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            //Streamed base64?
            throw new NotImplementedException();
        }
    }
}
