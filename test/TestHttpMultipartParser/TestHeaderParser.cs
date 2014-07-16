using HttpMultipartParser.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using FluentAssertions;

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

            parsed.Count.Should().Be(4);
            parsed["user-agent"].Value.Should().Be("user_agent");
            parsed["Content-Length"].Value.Should().Be("12345");
            parsed["X-CUSTOM-HEADER"].Value.Should().Be("abcde");
            parsed["content-type"].Value.Should().Be("multipart/form-data");
            parsed["content-type"].Parameters.Count.Should().Be(1);
            parsed["content-type"].Parameters["BounDarY"].Should().Be("------abcde");
        }

        [TestCase]
        public void invalid_lines_from_stream_are_ignored()
        {
            var headers = new StringBuilder().Append("User-Agent=user_agent").AppendLine()
                                             .Append("kjafhlksajdhfaksjdhfaslkjfhasldk").AppendLine()
                                             .ToString();

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream), Encoding.UTF8);

            parsed.Count.Should().Be(0);
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

            parsed.Count.Should().Be(4);
            parsed["user-agent"].Value.Should().Be("user_agent");
            parsed["Content-length"].Value.Should().Be("12345");
            parsed["X-Custom-Header"].Value.Should().Be("abcde");
            parsed["CONTENT-type"].Value.Should().Be("multipart/form-data");
            parsed["content-type"].Parameters.Count.Should().Be(1);
            parsed["content-type"].Parameters["BounDarY"].Should().Be("------abcde");
        }

        [TestCase]
        public void repeated_headers_use_the_value_of_the_last_one()
        {
            var headers = new StringBuilder().Append("User-Agent: user_agent").AppendLine()
                                             .Append("User-Agent: user_agent_2").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            parsed.Count.Should().Be(1);
            parsed["user-agent"].Value.Should().Be("user_agent_2");
        }

        [TestCase]
        public void headers_with_multiple_parameters_are_read_correctly_and_dictionary_keys_are_case_sensitive()
        {
            var headers = new StringBuilder().Append("X-Custom-Header: abcde; custom-prop1=12345; custom_prop2=xpto;      custom--prop3   =   666;").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            var parameters = parsed["X-Custom-Header"].Parameters;
            parameters.Count.Should().Be(3);
            parameters["custom-prop1"].Should().Be("12345");
            parameters["CUSTOM_PROP2"].Should().Be("xpto");
            parameters["custom--Prop3"].Should().Be("666");
        }

        [TestCase]
        public void repeated_parameters_use_the_value_of_the_last_one()
        {
            var headers = new StringBuilder().Append("X-Custom-Header: abcde; custom-prop1=12345; custom-prop1=xpto").AppendLine()
                                             .ToString();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(headers));

            var parsed = new HeaderParser().Parse(new HttpMultipartParser.BufferedStream(stream));

            var parameters = parsed["X-Custom-Header"].Parameters;
            parameters.Count.Should().Be(1);
            parameters["custom-prop1"].Should().Be("xpto");
        }
    }
}
