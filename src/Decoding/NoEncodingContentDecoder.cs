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
        private Func<BufferedStream, WriteMultiPartFile> _genWriteMultipartFileDelegateFunc;
        private Func<BufferedStream, GetFileData> _genGetFileDataFunc;
        private Func<BufferedStream, DiscardFile> _genDiscardFileDelegateFunc;
        private Encoding _encoding;

        public NoEncodingContentDecoder(Func<BufferedStream, WriteMultiPartFile> genWriteMultipartFileDelegateFunc,
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
            return new Data.BinaryData
            {
                ContentType = contentType,
                Data = content
            };
        }

        public Data.TextData DecodePart(string content, string contentType)
        {
            return new Data.TextData
            {
                ContentType = contentType,
                Data = content
            };
        }

        public Data.StreamedFileData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            return new Data.StreamedFileData
            {
                ContentType = contentType,
                IsBinary = ContentTypes.IsBinary(contentType),
                ToFile = _genWriteMultipartFileDelegateFunc(content),
                GetData = _genGetFileDataFunc(content),
                Discard = _genDiscardFileDelegateFunc(content)
            };
        }
    }
}
