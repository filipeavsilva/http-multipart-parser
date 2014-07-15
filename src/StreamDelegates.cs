using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser
{
	//Delegate types to be included in returned streamed file objects

	/// <summary>
	/// Delegate for writing a file included in a multipart request
	/// to a file on disk
	/// </summary>
	/// <param name="filePath">The path where to write the file</param>
	public delegate void WritePartToFile (string filePath);

	/// <summary>
	/// Delegate for obtaining text data from a streamed part of a
	/// multipart request
	/// </summary>
	/// <returns>
	/// The text included in the part
	/// </returns>
	public delegate string GetTextData ();

    /// <summary>
    /// Delegate for obtaining binary data from a streamed part of a
    /// multipart request
    /// </summary>
    /// <returns>
    /// The bytes included in the part
    /// </returns>
    public delegate string GetBinaryData ();

	/// <summary>
	/// Delegate for discarding a part of a streamed multipart request.
	/// Reads the request stream until the end of the part.
	/// </summary>
	public delegate void DiscardPart ();
}
