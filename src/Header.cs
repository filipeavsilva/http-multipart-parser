using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser
{
    internal class Header
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
