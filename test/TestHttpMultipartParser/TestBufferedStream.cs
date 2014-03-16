using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace TestHttpMultipartParser {
	[TestClass]
	public class TestBufferedStream {

		[TestMethod]
		public void lines_with_multiple_line_endings_are_read_correctly(){
			//Test reading lines from a stream, with multiple line endings
			var ms_r = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\rSecondLine"));
			var ms_n = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\nSecondLine"));
			var ms_rn = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\r\nSecondLine"));

			var bs_r = new HttpMultipartParser.BufferedStream (ms_r, Encoding.UTF8);
			var bs_n = new HttpMultipartParser.BufferedStream (ms_n, Encoding.UTF8);
			var bs_rn = new HttpMultipartParser.BufferedStream (ms_rn, Encoding.UTF8);

			string terminator;
			Assert.AreEqual ("FirstLine", bs_r.ReadLine (out terminator));
			Assert.AreEqual ("\r", terminator);
			Assert.AreEqual ("SecondLine", bs_r.ReadLine (out terminator));
			Assert.AreEqual (null, terminator);

			Assert.AreEqual ("FirstLine", bs_n.ReadLine (out terminator));
			Assert.AreEqual ("\n", terminator);
			Assert.AreEqual ("SecondLine", bs_n.ReadLine ());

			Assert.AreEqual ("FirstLine", bs_rn.ReadLine (out terminator));
			Assert.AreEqual ("\r\n", terminator);
			Assert.AreEqual ("SecondLine", bs_rn.ReadLine ());
		}

		[TestMethod]
		public void reading_line_from_empty_stream_returns_empty_string(){
			//Test reading lines from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.AreEqual(string.Empty, bs.ReadLine ());
		}

		[TestMethod]
		public void bytes_are_read_correctly(){
			//Test reading bytes from a stream
			var ms = new MemoryStream(new byte[]{10, 25, 32});

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.AreEqual ((byte)10, bs.ReadByte());
			Assert.AreEqual ((byte)25, bs.ReadByte());
			Assert.AreEqual ((byte)32, bs.ReadByte());
			Assert.IsNull(bs.ReadByte());
		}

		[TestMethod]
		public void reading_bytes_from_empty_stream_returns_null(){
			//Test reading bytes from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadByte());
		}

		[TestMethod]
		public void read_if_next_reads_only_if_match_is_found(){
			//Test reading matching byte sequences at the start of a stream
			var ms = new MemoryStream(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadIfNext(new byte[][]{new byte[]{6, 6, 6}, new byte[]{3, 2, 1}})); //Doesn't match
			CollectionAssert.AreEqual (new byte[]{1, 2, 3}, bs.ReadIfNext(new byte[][]{new byte[]{7, 8, 9}, new byte[]{1, 2, 3}, new byte[]{1, 2}})); //Matches the first available match, even if longer
			Assert.AreEqual((byte)4, bs.ReadByte()); //It did actually read the sequence
			CollectionAssert.AreEqual (new byte[]{5, 6, 7}, bs.ReadIfNext(new byte[][]{new byte[]{5, 6, 7}, new byte[]{5, 6, 7, 8}})); //Matches the first available match, even if shorter
		}

		[TestMethod]
		public void read_if_next_from_empty_string_returns_null(){
			//Test reading matching byte sequences at the start of an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadIfNext(new byte[][]{new byte[]{6, 6, 6}, new byte[]{3, 2, 1}}));
		}

	}
}