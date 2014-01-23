using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using HttpMultipartParser;
using System.ServiceModel.Web;
using HttpMultipartParser.Data;

namespace TestService {
	// NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in Web.config and in the associated .svc file.
	public class Service1 : IService1 {
		private string dir_path = "test_files";
		private bool step_through = false; //Change this to switch between 

		public Service1 () {
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			if (Directory.Exists(dir_path)) {
				Directory.Delete(dir_path, true);
			}
			Directory.CreateDirectory(dir_path);
			dir_path = Path.GetFullPath(dir_path);

		}


		private Stream TestFileUpload (Stream body, HTTPMultipartParser parser) {

			if (step_through) {
				//This code was made for walking^Wstepping through...
				string a = "";
				foreach (StreamedFileData fileData in parser.Parse()) {
					if (fileData.IsBinary) {
						byte[] x = (byte[])fileData.GetData();
					} else {
						a += fileData.Name + ": '" + (string)fileData.GetData() + "'";
					}
				}
			} else { //Will save files to disk
				foreach (StreamedFileData fileData in parser.Parse()) {
					fileData.ToFile(Path.Combine(dir_path, fileData.FileName));
				}
			}

			StreamWriter fieldFile = null;
			if (!step_through) {
				//Create a file to save non-file fields
				fieldFile = File.CreateText(Path.Combine(dir_path, "__multipart_fields.txt"));
			}

			//Save/step through any other fields/buffered files
			foreach (var kvp in parser.Fields) {
				string name = kvp.Key;
				MultipartData field = kvp.Value;

				if (!step_through) {
					if (field.IsBinary) {
						File.WriteAllBytes(Path.Combine(dir_path, field.FileName ?? field.Name), ((BinaryData)field).Data);
					} else {
						if (field.IsFile) {
							File.WriteAllText(Path.Combine(dir_path, field.FileName ?? field.Name), ((TextData)field).Data);
						} else {
							fieldFile.WriteLine("[ ===== " + field.Name + " ====]");
							fieldFile.WriteLine(((TextData)field).Data);
							fieldFile.WriteLine();
						}
					}
				}
			}

			if (!step_through) {
				fieldFile.Close();
			}

			return null;
		}

		public Stream TestFileUpload_AllBuffered (Stream body) {
			HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_BUFFERED);

			return TestFileUpload(body, parser);
		}

		public Stream TestFileUpload_AllStreamed (Stream body) {
			HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_STREAMED);

			return TestFileUpload(body, parser);
		}

		public Stream TestFileUpload_StreamedBinary (Stream body) {
			HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.STREAMED_BINARY);

			return TestFileUpload(body, parser);
		}

		public Stream TestFileUpload_StreamedText (Stream body) {
			HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.STREAMED_TEXT);

			return TestFileUpload(body, parser);
		}



		private Stream String2Stream (string s) {
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		private object GetStreamData (Stream stuff, string contentType) {
			switch (contentType) {
				case "application/octet-stream":
					return StreamToByteArray(stuff);
				case "application/json":
				case "text/xml":
				default: //Anything that is just text
					StreamReader sr = new StreamReader(stuff);
					return sr.ReadToEnd();
			}
		}

		private byte[] StreamToByteArray (Stream stream) {
			int length = stream.Length > int.MaxValue ? int.MaxValue : Convert.ToInt32(stream.Length);
			byte[] buffer = new byte[length];
			stream.Read(buffer, 0, length);
			return buffer;
		}
	}
}
