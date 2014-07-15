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
	public delegate void WriteMultiPartFile (string filePath);

	/// <summary>
	/// Delegate for obtaining the data of a file included in a
	/// multipart request
	/// </summary>
	/// <returns>
	/// The file's data.
	/// <i>string</i> if it is a text file,
	/// <i>byte[]</i> otherwise.
	/// </returns>
	public delegate object GetFileData ();

	/// <summary>
	/// Delegate for discarding a file included in a multipart request.
	/// Reads the request stream until the end of the file.
	/// </summary>
	public delegate void DiscardFile ();
}
