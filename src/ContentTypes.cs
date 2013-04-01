using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpTools.Util {
    /// <summary>
    /// Class to determine if data is binary or not from its media type
    /// </summary>
    public static class ContentTypes {

        #region Change this


        /// <summary>
        /// Types to be considered binary go here
        /// </summary>
        private static string[] binary = {
            "application/octet-stream",
            "application/ogg",
            "application/pdf",
            "application/font-woff",
            "application/xop+xml",
            "application/zip",
            "application/gzip",
            "audio/basic",
            "audio/L24",
            "audio/mp4",
            "audio/mpeg",
            "audio/ogg",
            "audio/vorbis",
            "audio/vnd.rn-realaudio",
            "audio/vnd.wave",
            "audio/webm",
            "image/gif",
            "image/jpeg",
            "image/pjpeg",
            "image/png",
            "image/svg+xml",
            "image/tiff",
            "image/vnd.microsoft.icon",
            "model/example",
            "model/iges",
            "model/mesh",
            "model/x3d+binary",
            "video/mpeg",
            "video/mp4",
            "video/ogg",
            "video/quicktime",
            "video/webm",
            "video/x-matroska",
            "video/x-ms-wmv",
            "video/x-flv",
            "application/vnd.oasis.opendocument.spreadsheet",
            "application/vnd.oasis.opendocument.presentation",
            "application/vnd.oasis.opendocument.graphics",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/x-deb",
            "application/x-dvi",
            "application/x-font-ttf",
            "application/x-rar-compressed",
            "application/x-shockwave-flash",
            "application/x-stuffit",
            "application/x-tar",
            "application/x-www-form-urlencoded Form Encoded Data; Documented in HTML 4.01 Specification, Section 17.13.4.1",
            "application/x-xpinstall",
            "audio/x-aac",
            "audio/x-caf",
            "image/x-xcf"
        };

        /// <summary>
        /// Types to be considered text go here
        /// </summary>
        private static string[] text = {
            "text/1d-interleaved-parityfec",
            "text/calendar",
            "text/css",
            "text/csv",
            "text/directory",
            "text/dns",
            "text/ecmascript",
            "text/encaprtp",
            "text/enriched",
            "text/example",
            "text/fwdred",
            "text/grammar-ref-list",
            "text/html",
            "text/javascript",
            "text/jcr-cnd",
            "text/mizar",
            "text/n3",
            "text/parityfec",
            "text/plain",
            "text/provenance-notation",
            "text/prs.fallenstein.rst",
            "text/prs.lines.tag",
            "text/raptorfec",
            "text/RED",
            "text/rfc822-headers",
            "text/richtext",
            "text/rtf",
            "text/rtp-enc-aescm128",
            "text/rtploopback",
            "text/rtx",
            "text/sgml",
            "text/t140",
            "text/tab-separated-values",
            "text/troff",
            "text/turtle",
            "text/ulpfec",
            "text/uri-list",
            "text/vcard",
            "text/vnd.abc",
            "text/vnd.curl",
            "text/vnd.debian.copyright",
            "text/vnd.DMClientScript",
            "text/vnd.dvb.subtitle",
            "text/vnd.esmertec.theme-descriptor",
            "text/vnd.fly",
            "text/vnd.fmi.flexstor",
            "text/vnd.graphviz",
            "text/vnd.in3d.3dml",
            "text/vnd.in3d.spot",
            "text/vnd.IPTC.NewsML",
            "text/vnd.IPTC.NITF",
            "text/vnd.latex-z",
            "text/vnd.motorola.reflex",
            "text/vnd.ms-mediapackage",
            "text/vnd.net2phone.commcenter.command",
            "text/vnd.radisys.msml-basic-layout",
            "text/vnd.si.uricatalogue",
            "text/vnd.sun.j2me.app-descriptor",
            "text/vnd.trolltech.linguist",
            "text/vnd.wap.si",
            "text/vnd.wap.sl",
            "text/vnd.wap.wml",
            "text/vnd.wap.wmlscript",
            "text/xml",
            "text/xml-external-parsed-entity",
            "application/atom+xml",
            "application/ecmascript",
            "application/EDI-X12",
            "application/EDIFACT",
            "application/json",
            "application/javascript",
            "application/postscript",
            "application/rdf+xml",
            "application/rss+xml",
            "application/soap+xml",
            "application/xhtml+xml",
            "application/xml",
            "application/xml-dtd",
            "message/http",
            "message/imdn+xml",
            "message/partial",
            "message/rfc822",
            "model/vrml",
            "model/x3d+vrml",
            "model/x3d+xml",
            "application/vnd.oasis.opendocument.text",
            "application/vnd.mozilla.xul+xml",
            "application/vnd.google-earth.kml+xml",
            "application/x-javascript",
            "application/x-latex",
            "application/x-www-form-urlencoded",
            "text/x-jquery-tmpl",
            "application/x-mpegURL",
            "text/x-gwt-rpc"
        };

        /// <summary>
        /// The default value. true for binary, false for text.
        /// </summary>
        private static bool defaultValue = true; //Default to binary


        #endregion Change this

//***********************************************************************************************************************************

        /// <summary>
        /// Keeps the correspondence between types and their binary status
        /// </summary>
        private static Dictionary<string, bool> correspondence;

        /// <summary>
        /// Prepare the dictionary from the two arrays
        /// </summary>
        static ContentTypes() {
            correspondence = new Dictionary<string, bool>();

            foreach (string type in binary) {
                if (!correspondence.ContainsKey(type.ToLower()))
                    correspondence.Add(type.ToLower(), true);
            }

            foreach (string type in text) {
                if (!correspondence.ContainsKey(type.ToLower()))
                    correspondence.Add(type.ToLower(), false);
            }
        }

        /// <summary>
        /// Determine if a given media type is or not binary
        /// </summary>
        /// <param name="type">The media type to check</param>
        /// <returns>true if the type is considered binary, false otherwise</returns>
        public static bool IsBinary(string type) {
            if (correspondence.ContainsKey(type.ToLower()))
                return correspondence[type.ToLower()];

            return defaultValue;
        }
    }
}
