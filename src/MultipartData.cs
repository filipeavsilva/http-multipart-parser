using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpTools.Util {
    /// <summary>
    /// One part of data in a multipart request's body.
    /// </summary>
    public class MultipartData {
        /// <summary>
        /// The part's name ("name" attribute of the form field) 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The original file name
        /// (or null if it is not a file)
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The part's content type (e.g. "text/plain" or "application/octet-stream")
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The part's content transfer encoding (e.g. "Base64" or "8bit")
        /// </summary>
        public string ContentTransferEncoding { get; set; }

        /// <summary>
        /// True if the part has a binary type, false otherwise
        /// </summary>
        public bool IsBinary { get; set; }

        /// <summary>
        /// True if the part is a file, false otherwise
        /// </summary>
        public bool IsFile {
            get {
                return FileName != null;
            }
        }
    }
}
