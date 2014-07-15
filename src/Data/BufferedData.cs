using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpMultipartParser.Data {
    /// <summary>
    /// A part of a buffered multipart request
    /// </summary>
    public class BufferedData : MultipartData {
        /// <summary>
        /// The part's text data
        /// (Only available if IsBinary == false, otherwise is null)
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The part's binary data
        /// (Only available if IsBinary == true, otherwise is null)
        /// </summary>
        public byte[] Bytes { get; set; }
    }
}
