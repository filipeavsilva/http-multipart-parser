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
        public Stream TestFileUpload(Stream body) {
            HTTPMultipartParser parser = new HTTPMultipartParser(body, EFileHandlingType.ALL_STREAMED);

            string a = "";
            foreach (StreamedFileData fileData in parser.Parse()) {
                if (fileData.IsBinary) {
                    byte[] x = (byte[])fileData.GetData();
                } else {
                    a += fileData.Name + ": '" + (string)fileData.GetData() + "'";
                }
            }

            return null;
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
