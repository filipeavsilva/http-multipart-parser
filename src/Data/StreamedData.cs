using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HttpMultipartParser.Data {
    /// <summary>
    /// Represents a part in a streamed multipart request.
    /// Contains methods to access the part's data,
    /// write it to a file or discard it.
    /// </summary>
    public class StreamedFileData : MultipartData {
        /// <summary>
        /// Delegate to write the part's data into a file in disk
        /// </summary>
        public WritePartToFile ToFile { get; set; }

        /// <summary>
        /// Delegate to discard the part
        /// </summary>
        public DiscardPart Discard { get; set; }

        /// <summary>
        /// Delegate to fetch the text data from the part into memory
        /// </summary>
        /// <returns>
        /// The text data contained in the part
        /// </returns>
        public GetTextData GetTextData { get; set; }

        /// <summary>
        /// Delegate to fetch the binary data from the part into memory
        /// </summary>
        /// <returns>
        /// The binary data contained in the part
        /// </returns>
        public GetBinaryData GetBinaryData { get; set; }
    }
}
