using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpTools.Util {
    /// <summary>
    /// Controls how file data is handled by the parser
    /// </summary>
    public enum EFileHandlingType {
        /// <summary>
        /// All files are streamed
        /// </summary>
        ALL_STREAMED,
        /// <summary>
        /// All files are buffered
        /// </summary>
        ALL_BUFFERED,
        /// <summary>
        /// Binary files are streamed, text files are buffered
        /// </summary>
        STREAMED_BINARY,
        /// <summary>
        /// Text files are streamed, binary files are buffered
        /// </summary>
        STREAMED_TEXT
    }
}
