using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpMultipartParser.Data {
    /// <summary>
    /// Represents a binary file in a multipart request
    /// </summary>
    public class BinaryData : MultipartData {
        /// <summary>
        /// The binary data included in the file
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// </summary>
        public BinaryData() {
            IsBinary = true;
        }
    }
}
