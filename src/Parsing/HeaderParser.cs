using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpMultipartParser.Parsing
{
    internal interface IHeaderParser
    {
        IDictionary<string, Header> Parse(BufferedStream request);
        IDictionary<string, Header> Parse(BufferedStream request, Encoding encoding);
    }

    internal class HeaderParser : IHeaderParser
    {
        public IDictionary<string, Header> Parse(BufferedStream request)
        {
            return Parse(request, Defaults.ENCODING);
        }

        public IDictionary<string, Header> Parse(BufferedStream request, Encoding encoding)
        {
            var line = request.ReadLine();
            Dictionary<string, Header> result = new Dictionary<string, Header>(StringComparer.InvariantCultureIgnoreCase);

            while (!string.IsNullOrEmpty(line))
            {
                var header = ParseSingleHeader(line);
                if (header != null)
                    if (result.ContainsKey(header.Name))
                        result[header.Name] = header; //If there are multiple headers, always use the last one
                    else
                        result.Add(header.Name, header);

                line = request.ReadLine();
            }

            return result;
        }

        private Header ParseSingleHeader(string line)
        {
            Header result = null;

            var headerRegex = new Regex(@"^([\w-]+):\s*([^;]+)\s*((;\s*[\w-]+\s*=\s*[^;]+\s*)*)");
            var paramRegex = new Regex(@";\s*([\w-]+)\s*=\s*([^;]+)\s*");

            var match = headerRegex.Match(line);

            if (!string.IsNullOrEmpty(match.Groups[0].Value))
            {
                result = new Header
                {
                    Name = match.Groups[1].Value,
                    Value = match.Groups[2].Value,
                    Parameters = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                };

                //Check header parameters
                var paramString = match.Groups[3].Value;
                if (!string.IsNullOrEmpty(paramString))
                {
                    foreach (Match param in paramRegex.Matches(paramString))
                    {
                        var name = param.Groups[1].Value;
                        var value = param.Groups[2].Value;
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                        {
                            if (!result.Parameters.ContainsKey(name))
                                result.Parameters.Add(name, value);
                            else
                                result.Parameters[name] = value;
                        }
                    }
                }
            }

            return result;
        }
    }
}
