using System;
using NUnit.Framework;
using System.IO;
using System.Text;

namespace TestHttpMultipartParser {
	[TestFixture]
	public class TestBufferedStream {

		[TestCase]
		public void TestReadLine(){
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

		[TestCase]
		public void TestReadLineEmpty(){
			//Test reading lines from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.AreEqual(string.Empty, bs.ReadLine ());
		}

		[TestCase]
		public void TestReadByte(){
			//Test reading bytes from a stream
			var ms = new MemoryStream(new byte[]{10, 25, 32});

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.AreEqual (10, bs.ReadByte());
			Assert.AreEqual (25, bs.ReadByte());
			Assert.AreEqual (32, bs.ReadByte());
			Assert.IsNull(bs.ReadByte());
		}

		[TestCase]
		public void TestReadByteEmpty(){
			//Test reading bytes from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadByte());
		}

		[TestCase]
		public void TestReadIfNext(){
			//Test reading matching byte sequences at the start of a stream
			var ms = new MemoryStream(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadIfNext(new byte[][]{new byte[]{6, 6, 6}, new byte[]{3, 2, 1}})); //Doesn't match
			Assert.AreEqual (new byte[]{1, 2, 3}, bs.ReadIfNext(new byte[][]{new byte[]{7, 8, 9}, new byte[]{1, 2, 3}, new byte[]{1, 2}})); //Matches the first available match, even if longer
			Assert.AreEqual (4, bs.ReadByte()); //Actually read the sequence
			Assert.AreEqual (new byte[]{5, 6, 7}, bs.ReadIfNext(new byte[][]{new byte[]{5, 6, 7}, new byte[]{5, 6, 7, 8}})); //Matches the first available match, even if shorter
		}

		[TestCase]
		public void TestReadIfNextEmpty(){
			//Test reading matching byte sequences at the start of an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			Assert.IsNull (bs.ReadIfNext(new byte[][]{new byte[]{6, 6, 6}, new byte[]{3, 2, 1}}));
		}

	}
}