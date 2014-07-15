using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.Decoding
{
    internal class Base64ContentDecoder : IContentDecoder
    {
        private Func<BufferedStream, WriteMultiPartFile> _genWriteMultipartFileDelegateFunc;
        private Func<BufferedStream, GetFileData> _genGetFileDataFunc;
        private Func<BufferedStream, DiscardFile> _genDiscardFileDelegateFunc;
        private Encoding _encoding;

        public Base64ContentDecoder(Func<BufferedStream, WriteMultiPartFile> genWriteMultipartFileDelegateFunc,
                                    Func<BufferedStream, GetFileData> genGetFileDataDelegateFunc,
                                    Func<BufferedStream, DiscardFile> genDiscardFileDelegateFunc,
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

        public Data.TextData DecodePart(string content, string contentType)
        {
            byte[] bytes = Convert.FromBase64String(content);

            return new Data.TextData
            {
                Data = _encoding.GetString(bytes),
                ContentType = contentType
            };
        }

        public Data.StreamedFileData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            //Streamed base64?
            throw new NotImplementedException();
        }
    }
}
