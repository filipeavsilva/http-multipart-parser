using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpTools.Util;
using System.IO;
using System.Text;

namespace HttpMultipartParser.Test
{
    [TestClass]
    public class MultipartParserTest
    {
        [TestMethod]
        public void TestParse_ContentDisposition_FilenameFirst()
        {
            var content = "--7a1d30f95d847a130f0e77ca\n" +
                          "Content-Disposition: form-data; filename=\"this_is_filename\"; name=\"this_is_name\"\n" +
                          "Content-Type: text/plain\n\n" +
                          "This is a content\n" +
                          "--7a1d30f95d847a130f0e77ca";
            var raw = Encoding.UTF8.GetBytes(content);
            using (var stream = new MemoryStream())
            {
                stream.Write(raw, 0, raw.Length);
                stream.Position = 0;

                var parser = new HTTPMultipartParser(stream, EFileHandlingType.ALL_STREAMED);
                var part = parser.Parse().ElementAt(0);

                Assert.AreEqual("this_is_filename", part.FileName);
                Assert.AreEqual("this_is_name", part.Name);
                Assert.AreEqual("This is a content", (string)part.GetData());
                Assert.AreEqual("text/plain", part.ContentType);
            }
        }


        [TestMethod]
        public void TestParse_ContentDisposition_NameFirst()
        {
            var content = "--7a1d30f95d847a130f0e77ca\n" +
                          "Content-Disposition: form-data; name=\"this_is_name\"; filename=\"this_is_filename\"\n" +
                          "Content-Type: text/plain\n\n" +
                          "This is a content\n" +
                          "--7a1d30f95d847a130f0e77ca";
            var raw = Encoding.UTF8.GetBytes(content);
            using (var stream = new MemoryStream())
            {
                stream.Write(raw, 0, raw.Length);
                stream.Position = 0;

                var parser = new HTTPMultipartParser(stream, EFileHandlingType.ALL_STREAMED);
                var part = parser.Parse().ElementAt(0);

                Assert.AreEqual("this_is_filename", part.FileName);
                Assert.AreEqual("this_is_name", part.Name);
                Assert.AreEqual("This is a content", (string)part.GetData());
                Assert.AreEqual("text/plain", part.ContentType);
            }
        }
    }
}
