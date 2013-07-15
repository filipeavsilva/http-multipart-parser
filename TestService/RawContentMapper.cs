using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ServiceModel.Channels;

namespace TestService{
    public class RawContentMapper : WebContentTypeMapper {
        public override WebContentFormat GetMessageFormatForContentType(string contentType) {
            return WebContentFormat.Raw; // always
        }
    }
}
