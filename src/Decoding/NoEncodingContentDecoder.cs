using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpMultipartParser.Data;

namespace HttpMultipartParser.Decoding
{
    /// <summary>
    /// Content Decoder for 7BIT, 8BIT and BINARY encodings since,
    /// according to the RFC, 'all of them really mean "none"'
    /// </summary>
    internal class NoEncodingContentDecoder : IContentDecoder
    {
        private readonly Func<BufferedStream, WritePartToFile> _genWritePartToFileDelegateFunc;
        private readonly Func<BufferedStream, GetTextData> _genGetTextDataDelegateFunc;
        private readonly Func<BufferedStream, GetBinaryData> _genGetBinaryDataDelegateFunc;
        private readonly Func<BufferedStream, DiscardPart> _genDiscardPartDelegateFunc;
        private Encoding _encoding;

        public NoEncodingContentDecoder(Func<BufferedStream, WritePartToFile> genWriteMultipartFileDelegateFunc,
                                        Func<BufferedStream, GetTextData> genGetTextDataDelegateFunc,
                                        Func<BufferedStream, GetBinaryData> genGetBinaryDataDelegateFunc,
                                        Func<BufferedStream, DiscardPart> genDiscardPartDelegateFunc,
                                        Encoding encoding)
        {
            _genWritePartToFileDelegateFunc = genWriteMultipartFileDelegateFunc;
            _genGetTextDataDelegateFunc = genGetTextDataDelegateFunc;
            _genGetBinaryDataDelegateFunc = genGetBinaryDataDelegateFunc;
            _genDiscardPartDelegateFunc = genDiscardPartDelegateFunc;
            _encoding = encoding;
        }

        public BufferedData DecodePart(byte[] content, string contentType)
        {
            return new BufferedData
            {
                ContentType = contentType,
                Bytes = content,
                IsBinary = true
            };
        }

        public BufferedData DecodePart(string content, string contentType)
        {
            return new BufferedData
            {
                ContentType = contentType,
                Text = content,
                IsBinary = false
            };
        }

        public Data.StreamedData DecodeAndStreamPart(BufferedStream content, string contentType)
        {
            return new Data.StreamedData
            {
                ContentType = contentType,
                IsBinary = ContentTypes.IsBinary(contentType),
                ToFile = _genWritePartToFileDelegateFunc(content),
                GetTextData = _genGetTextDataDelegateFunc(content),
                GetBinaryData = _genGetBinaryDataDelegateFunc(content),
                Discard = _genDiscardPartDelegateFunc(content)
            };
        }
    }
}
