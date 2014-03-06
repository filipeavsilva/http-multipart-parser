using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpTools.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HttpMultipartParser.Test
{
    [TestClass]
    public class ContentTransferEncodingTest
    {

        [TestMethod]
        public void TestParse_Base64_String()
        {
            var content = "--7a1d30f95d847a130f0e77ca\n" +
                          "Content-Disposition: form-data; name=\"this_is_name\"; filename=\"this_is_filename\"\n" +
                          "Content-Transfer-Encoding: Base64\n" +
                          "Content-Type: text/plain\n\n" +
                          "SGVsbG8gVGhpcyBpcyBhIGNvbnRlbnQ=\n" +  // reads 'Hello This is a content'
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
                Assert.IsFalse(part.IsBinary);
                Assert.AreEqual("Hello This is a content", (string)part.GetData());
                Assert.AreEqual("Base64", part.ContentTransferEncoding);
                Assert.AreEqual("text/plain", part.ContentType);
            }
        }


        [TestMethod]
        public void TestParse_Base64_Binary()
        {
            var content = "--7a1d30f95d847a130f0e77ca\n" +
                          "Content-Disposition: form-data; name=\"this_is_name\"; filename=\"this_is_filename\"\n" +
                          "Content-Transfer-Encoding: Base64\n" +
                          "Content-Type: application/octet-stream\n\n" +
                          "SGVsbG8gVGhpcyBpcyBhIGNvbnRlbnQ=\n" +  // reads 'Hello This is a content'
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
                Assert.IsTrue(part.IsBinary);
                Assert.AreEqual("Base64", part.ContentTransferEncoding);
                Assert.AreEqual("application/octet-stream", part.ContentType);

                var bytes = (byte[])part.GetData();
                var stringContent = Encoding.UTF8.GetString(bytes);
                Assert.AreEqual("Hello This is a content", stringContent);
            }
        }

    }
}
