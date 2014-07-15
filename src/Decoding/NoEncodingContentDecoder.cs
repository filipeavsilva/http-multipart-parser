using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser.Decoding
{
    /// <summary>
    /// Content Decoder for 7BIT, 8BIT and BINARY encodings since,
    /// according to the RFC, 'all of them really mean "none"'
    /// </summary>
    internal class NoEncodingContentDecoder : IContentDecoder
    {
        private Func<BufferedStream, WriteFilePart> _genWriteMultipartFileDelegateFunc;
        private Func<BufferedStream, GetFileData> _genGetFileDataFunc;
        private Func<BufferedStream, DiscardPart> _genDiscardFileDelegateFunc;
        private Encoding _encoding;

        public NoEncodingContentDecoder(Func<BufferedStream, WriteFilePart> genWriteMultipartFileDelegateFunc,
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
            return new Data.BinaryData
            {
                ContentType = contentType,
                Data = content
            };
        }

        public Data.BufferedData DecodePart(string content, string contentType)
        {
            return new Data.BufferedData
            {
                ContentType = contentType,
                Text = content
            };
        }

        public Data.StreamedData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            return new Data.StreamedData
            {
                ContentType = contentType,
                IsBinary = ContentTypes.IsBinary(contentType),
                ToFile = _genWriteMultipartFileDelegateFunc(content),
                GetData = _genGetFileDataFunc(content),
                Skip = _genDiscardFileDelegateFunc(content)
            };
        }
    }
}
