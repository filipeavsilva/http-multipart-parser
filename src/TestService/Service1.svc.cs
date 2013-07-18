using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
using HttpTools.Util;
using System.ServiceModel.Web;

namespace TestService {
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in Web.config and in the associated .svc file.
    public class Service1 : IService1 {
        private Stream TestFileUpload(Stream body, HTTPMultipartParser parser) {

            //This code was made for walking^Wstepping through...
            /*string a = "";
            foreach (StreamedFileData fileData in parser.Parse()) {
                if (fileData.IsBinary) {
                    byte[] x = (byte[])fileData.GetData();
                } else {
                    a += fileData.Name + ": '" + (string)fileData.GetData() + "'";
                }
            }*/
            parser.ParseToEnd();

            return null;
        }

        public Stream TestFileUpload_AllBuffered(Stream body) {
            HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_BUFFERED);

            return TestFileUpload(body, parser);
        }

        public Stream TestFileUpload_AllStreamed(Stream body) {
            HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_STREAMED);

            return TestFileUpload(body, parser);
        }

        public Stream TestFileUpload_StreamedBinary(Stream body) {
            HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.STREAMED_BINARY);

            return TestFileUpload(body, parser);
        }

        public Stream TestFileUpload_StreamedText(Stream body) {
            HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.STREAMED_TEXT);

            return TestFileUpload(body, parser);
        }



        private Stream String2Stream(string s) {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private object GetStreamData(Stream stuff, string contentType) {
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

        private byte[] StreamToByteArray(Stream stream) {
            int length = stream.Length > int.MaxValue ? int.MaxValue : Convert.ToInt32(stream.Length);
            byte[] buffer = new byte[length];
            stream.Read(buffer, 0, length);
            return buffer;
        }
    }
}
