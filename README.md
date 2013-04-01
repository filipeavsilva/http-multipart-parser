#HTTP Multipart parser

A parser for  HTTP Multipart requests.

This parser receives a Stream with a "raw" HTTP multipart request body (starting
with the multipart boundary), and parses it into the various form fields.

As for files, the parser offers two ways of handling them: They may be
buffered, which means their content will be stored along with the other
fields, or they can be streamed. When streaming, the Parse() method returns an
IEnumerable of streamed file objects.

##Basic usage

To parse a multipart http request, you have only to instantiate the parser
(optionally specifying an encoding and a file handling type), and tell it to
Parse().

```c#
//Instantiate the parser
HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_BUFFERED);
//Parse the data
parser.Parse();
```
###Accessing data

After parsing, the request data can be accessed through the `Fields` dictionary.
This dictionary is indexed by part name (the name of the html form field), and
contains objects descending from the `MultipartData` class. These objects' class
may be either `TextData` for textual data or `BinaryData` for binary information.
Here's an example of how to access the fields:

```c#
HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_BUFFERED);
parser.Parse();
var fields = parser.Fields;

TextData nameData = (TextData)fields["name"];

//Or, if you don't know the if it is binary or not
MultipartData shady = fields["shady_field"];

if(shady.IsBinary) {
	byte[] bytes = ((BinaryData)shady).Data;
	//...do things...
} else {
	string text = ((TextData)shady).Data;
	//...do other things...
}

//And, if it is a file...

TextData file = (TextData)fields["the_file"];

if(file.IsFile){
	System.IO.File.WriteAllText(Path.Combine(@"C:\Downloads", file.FileName),
			file.Data);
} else {
	//Egads, it's not a file. Now what?!...
}

```

###Streaming files

If you need to upload large files, however, buffering them can become a memory
problem. For those cases, the parser allows files to be streamed (if the
connection and server configuration permits it). For that, a parser must be
instantiated with a streamed file handling type. Afterwards, the `Parse` method
will return an `IEnumerable<StreamedFileData>` with the streamed files, which
must then be iterated.
(Note: Since the multipart request comes in a stream which cannot be seeked, and
 putting the files in memory would defeat the purpose of this, you must iterate
 through all the files in the IEnumerable, and read them (which advances the
 stream), before the parsing is complete. Therefore, unless you know the order
 in which the parts come, there is no guarantee that a given part is available
 before the iteration is over.)

```c#
HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_STREAMED);

//Parse the data, iterating through the streamed files.
foreach (StreamedFileData file in parser.Parse()) {
		if(file.Name == "file_to_write") { //We know we want to write this file
			file.ToFile(Path.Combine("C:\\Downloads", file.FileName));
		} else if(file.Name == "tiny_file") { //OK, this one is tiny, we can keep it
			if(file.IsBinary){
				byte[] bytes = (byte[])file.GetData();
			} else {
				string text = (string)file.GetData();
			}
		} else { //We don't care about the others
			file.Discard(); //This is still needed to advance the stream
		}
}

//Now we can go get the fields...
string field = ((TextData)parser.Fields["another_field"]);
//...
```

####Streaming options

The parser can handle files in one of four different ways (specified in the
		`EFileHandlingType` enum):
* `ALL_STREAMED`: All files are streamed
* `ALL_BUFFERED`: All files are buffered
* `STREAMED_BINARY`: Binary files are streamed, text files are buffered
* `STREAMED_TEXT`: Text files are streamed, binary files are buffered

##More information

For more info, checkout the documentation in the `docs` folder.

Comments, questions, suggestions? [Contact
me](mailto:%66%69%6c%69%70%65%2e%61%76%2e%73%69%6c%76%61%40%67%6d%61%69%6c%2e%63%6f%6d?subject=Http%20Multipart%20Parser
		"Contact me")
