using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HttpMultipartParser {
	/// <summary>
	/// Class to determine if data is binary or not from its media type
	/// </summary>
	public static class ContentTypes {

		/// <summary>
		/// The default value. true for binary, false for text.
		/// </summary>
		private static bool defaultValue = true; //Default to binary



		//***********************************************************************************************************************************
		/// <summary>
		/// Keeps the correspondence between types and their binary status
		/// </summary>
		private static readonly Dictionary<string, bool> correspondence = new Dictionary<string, bool>{

			//Binary types here
			{ "application/octet-stream", true },
			{ "application/ogg", true },
			{ "application/pdf", true },
			{ "application/font-woff", true },
			{ "application/xop+xml", true },
			{ "application/zip", true },
			{ "application/gzip", true },
			{ "audio/basic", true },
			{ "audio/L24", true },
			{ "audio/mp4", true },
			{ "audio/mpeg", true },
			{ "audio/ogg", true },
			{ "audio/vorbis", true },
			{ "audio/vnd.rn-realaudio", true },
			{ "audio/vnd.wave", true },
			{ "audio/webm", true },
			{ "image/gif", true },
			{ "image/jpeg", true },
			{ "image/pjpeg", true },
			{ "image/png", true },
			{ "image/svg+xml", true },
			{ "image/tiff", true },
			{ "image/vnd.microsoft.icon", true },
			{ "model/example", true },
			{ "model/iges", true },
			{ "model/mesh", true },
			{ "model/x3d+binary", true },
			{ "video/mpeg", true },
			{ "video/mp4", true },
			{ "video/ogg", true },
			{ "video/quicktime", true },
			{ "video/webm", true },
			{ "video/x-matroska", true },
			{ "video/x-ms-wmv", true },
			{ "video/x-flv", true },
			{ "application/vnd.oasis.opendocument.spreadsheet", true },
			{ "application/vnd.oasis.opendocument.presentation", true },
			{ "application/vnd.oasis.opendocument.graphics", true },
			{ "application/vnd.ms-excel", true },
			{ "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", true },
			{ "application/vnd.ms-powerpoint", true },
			{ "application/vnd.openxmlformats-officedocument.presentationml.presentation", true },
			{ "application/vnd.openxmlformats-officedocument.wordprocessingml.document", true },
			{ "application/x-deb", true },
			{ "application/x-dvi", true },
			{ "application/x-font-ttf", true },
			{ "application/x-rar-compressed", true },
			{ "application/x-shockwave-flash", true },
			{ "application/x-stuffit", true },
			{ "application/x-tar", true },
			{ "application/x-www-form-urlencoded", true },
			{ "application/x-xpinstall", true },
			{ "audio/x-aac", true },
			{ "audio/x-caf", true },
			{ "image/x-xcf", true },


			//Text types here
			{ "text/1d-interleaved-parityfec", false },
			{ "text/calendar", false },
			{ "text/css", false },
			{ "text/csv", false },
			{ "text/directory", false },
			{ "text/dns", false },
			{ "text/ecmascript", false },
			{ "text/encaprtp", false },
			{ "text/enriched", false },
			{ "text/example", false },
			{ "text/fwdred", false },
			{ "text/grammar-ref-list", false },
			{ "text/html", false },
			{ "text/javascript", false },
			{ "text/jcr-cnd", false },
			{ "text/mizar", false },
			{ "text/n3", false },
			{ "text/parityfec", false },
			{ "text/plain", false },
			{ "text/provenance-notation", false },
			{ "text/prs.fallenstein.rst", false },
			{ "text/prs.lines.tag", false },
			{ "text/raptorfec", false },
			{ "text/RED", false },
			{ "text/rfc822-headers", false },
			{ "text/richtext", false },
			{ "text/rtf", false },
			{ "text/rtp-enc-aescm128", false },
			{ "text/rtploopback", false },
			{ "text/rtx", false },
			{ "text/sgml", false },
			{ "text/t140", false },
			{ "text/tab-separated-values", false },
			{ "text/troff", false },
			{ "text/turtle", false },
			{ "text/ulpfec", false },
			{ "text/uri-list", false },
			{ "text/vcard", false },
			{ "text/vnd.abc", false },
			{ "text/vnd.curl", false },
			{ "text/vnd.debian.copyright", false },
			{ "text/vnd.DMClientScript", false },
			{ "text/vnd.dvb.subtitle", false },
			{ "text/vnd.esmertec.theme-descriptor", false },
			{ "text/vnd.fly", false },
			{ "text/vnd.fmi.flexstor", false },
			{ "text/vnd.graphviz", false },
			{ "text/vnd.in3d.3dml", false },
			{ "text/vnd.in3d.spot", false },
			{ "text/vnd.IPTC.NewsML", false },
			{ "text/vnd.IPTC.NITF", false },
			{ "text/vnd.latex-z", false },
			{ "text/vnd.motorola.reflex", false },
			{ "text/vnd.ms-mediapackage", false },
			{ "text/vnd.net2phone.commcenter.command", false },
			{ "text/vnd.radisys.msml-basic-layout", false },
			{ "text/vnd.si.uricatalogue", false },
			{ "text/vnd.sun.j2me.app-descriptor", false },
			{ "text/vnd.trolltech.linguist", false },
			{ "text/vnd.wap.si", false },
			{ "text/vnd.wap.sl", false },
			{ "text/vnd.wap.wml", false },
			{ "text/vnd.wap.wmlscript", false },
			{ "text/xml", false },
			{ "text/xml-external-parsed-entity", false },
			{ "application/atom+xml", false },
			{ "application/ecmascript", false },
			{ "application/EDI-X12", false },
			{ "application/EDIFACT", false },
			{ "application/json", false },
			{ "application/javascript", false },
			{ "application/postscript", false },
			{ "application/rdf+xml", false },
			{ "application/rss+xml", false },
			{ "application/soap+xml", false },
			{ "application/xhtml+xml", false },
			{ "application/xml", false },
			{ "application/xml-dtd", false },
			{ "message/http", false },
			{ "message/imdn+xml", false },
			{ "message/partial", false },
			{ "message/rfc822", false },
			{ "model/vrml", false },
			{ "model/x3d+vrml", false },
			{ "model/x3d+xml", false },
			{ "application/vnd.oasis.opendocument.text", false },
			{ "application/vnd.mozilla.xul+xml", false },
			{ "application/vnd.google-earth.kml+xml", false },
			{ "application/x-javascript", false },
			{ "application/x-latex", false },
			{ "application/x-www-form-urlencoded", false },
			{ "text/x-jquery-tmpl", false },
			{ "application/x-mpegURL", false },
			{ "text/x-gwt-rpc", false },

		};

		/// <summary>
		/// Determine if a given media type is or not binary
		/// </summary>
		/// <param name="type">The media type to check</param>
		/// <returns>true if the type is considered binary, false otherwise</returns>
		public static bool IsBinary (string type) {
			return correspondence.ContainsKey (type.ToLower ()) ? correspondence [type.ToLower ()] : defaultValue;

		}
	}
}
