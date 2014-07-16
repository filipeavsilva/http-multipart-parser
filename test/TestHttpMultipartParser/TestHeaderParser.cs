using HttpMultipartParser.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace TestHttpMultipartParser
{
    [TestFixture]
    public class TestHeaderParser
    {
        [TestCase]
        public void valid_headers_from_stream_and_encoding_parsed_correctly_and_dictionary_keys_are_case_insensitive()
        {
            var headers = new StringBuilder().Append("User-Agent: user_agent").AppendLine()
                                             .Append("Content-Length: 12345").AppendLine()
                                             .Append("X-Custom-Header: abcde").AppendLine()
                                             .Append("Content-Type: multipart/form-data; boundary=------abcde").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream), Encoding.UTF8);

            Assert.AreEqual(4, parsed.Count);
            Assert.AreEqual("user_agent", parsed["user-agent"].Value);
            Assert.AreEqual("12345", parsed["Content-Length"].Value);
            Assert.AreEqual("abcde", parsed["X-CUSTOM-HEADER"].Value);
            Assert.AreEqual("multipart/form-data", parsed["content-type"].Value);
            Assert.AreEqual(1, parsed["content-type"].Parameters.Count);
            Assert.AreEqual("------abcde", parsed["content-type"].Parameters["BounDarY"]);
        }

        [TestCase]
        public void invalid_lines_from_stream_are_ignored()
        {
            var headers = new StringBuilder().Append("User-Agent=user_agent").AppendLine()
                                             .Append("kjafhlksajdhfaksjdhfaslkjfhasldk").AppendLine()
                                             .ToString();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream), Encoding.UTF8);

            Assert.AreEqual(0, parsed.Count);
        }

        [TestCase]
        public void valid_headers_from_stream_with_default_encoding_parsed_correctly_and_dictionary_keys_are_case_insensitive()
        {
            var headers = new StringBuilder().Append("User-Agent: user_agent").AppendLine()
                                             .Append("Content-Length: 12345").AppendLine()
                                             .Append("X-Custom-Header: abcde").AppendLine()
                                             .Append("Content-Type: multipart/form-data; boundary=------abcde").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            Assert.AreEqual(4, parsed.Count);
            Assert.AreEqual("user_agent", parsed["user-agent"].Value);
            Assert.AreEqual("12345", parsed["Content-length"].Value);
            Assert.AreEqual("abcde", parsed["X-Custom-Header"].Value);
            Assert.AreEqual("multipart/form-data", parsed["CONTENT-type"].Value);
            Assert.AreEqual(1, parsed["content-type"].Parameters.Count);
            Assert.AreEqual("------abcde", parsed["content-type"].Parameters["BounDarY"]);
        }

        [TestCase]
        public void repeated_headers_use_the_value_of_the_last_one()
        {
            var headers = new StringBuilder().Append("User-Agent: user_agent").AppendLine()
                                             .Append("User-Agent: user_agent_2").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            Assert.AreEqual(1, parsed.Count);
            Assert.AreEqual("user_agent_2", parsed["user-agent"].Value);
        }

        [TestCase]
        public void headers_with_multiple_parameters_are_read_correctly_and_dictionary_keys_are_case_sensitive()
        {
            var headers = new StringBuilder().Append("X-Custom-Header: abcde; custom-prop1=12345; custom_prop2=xpto;      custom--prop3   =   666;").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            var parameters = parsed["X-Custom-Header"].Parameters;
            Assert.AreEqual(3, parameters.Count);
            Assert.AreEqual("12345", parameters["custom-prop1"]);
            Assert.AreEqual("xpto", parameters["CUSTOM_PROP2"]);
            Assert.AreEqual("666", parameters["custom--Prop3"]);
        }

        [TestCase]
        public void repeated_parameters_use_the_value_of_the_last_one()
        {
            var headers = new StringBuilder().Append("X-Custom-Header: abcde; custom-prop1=12345; custom-prop1=xpto").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            var parameters = parsed["X-Custom-Header"].Parameters;
            Assert.AreEqual(1, parameters.Count);
            Assert.AreEqual("xpto", parameters["custom-prop1"]);
        }
    }
}
