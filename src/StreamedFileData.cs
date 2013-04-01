using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpTools.Util {
    /// <summary>
    /// Represents a streamed file in a multipart request.
    /// Contains methods to access the streamed file's data,
    /// write it to a file or discard it.
    /// </summary>
    public class StreamedFileData : MultipartData {
        /// <summary>
        /// Delegate to write the file to disk
        /// </summary>
        public WriteMultiPartFile ToFile { get; set; }

        /// <summary>
        /// Delegate to discard the file
        /// (simply advance the stream over it)
        /// </summary>
        public DiscardFile Discard { get; set; }

        /// <summary>
        /// Delegate to obtain the file data in memory
        /// </summary>
        /// <returns>
        /// If the file is binary (Binary == true), a byte[] with
        /// the file contents. If not, returns a string.
        /// </returns>
        public GetFileData GetData { get; set; }
    }
}
