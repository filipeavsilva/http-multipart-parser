using System.IO;
using System.Text;
using NUnit.Framework;
using FluentAssertions;

namespace TestHttpMultipartParser {
    [TestFixture]
	public class TestBufferedStream {

		[TestCase]
		public void lines_with_multiple_line_endings_are_read_correctly(){
			//Test reading lines from a stream, with multiple line endings
			var ms_r = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\rSecondLine"));
			var ms_n = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\nSecondLine"));
			var ms_rn = new MemoryStream(Encoding.UTF8.GetBytes("FirstLine\r\nSecondLine"));

			var bs_r = new HttpMultipartParser.BufferedStream (ms_r, Encoding.UTF8);
			var bs_n = new HttpMultipartParser.BufferedStream (ms_n, Encoding.UTF8);
			var bs_rn = new HttpMultipartParser.BufferedStream (ms_rn, Encoding.UTF8);

			string terminator;
            bs_r.ReadLine(out terminator).Should().Be("FirstLine");
			terminator.Should().Be("\r");
			bs_r.ReadLine (out terminator).Should().Be("SecondLine");
			terminator.Should().Be(null);

			bs_n.ReadLine (out terminator).Should().Be("FirstLine");
			terminator.Should().Be("\n");
			bs_n.ReadLine ().Should().Be("SecondLine");

			bs_rn.ReadLine (out terminator).Should().Be("FirstLine");
			terminator.Should().Be("\r\n");
			bs_rn.ReadLine ().Should().Be("SecondLine");
		}

        [TestCase]
		public void reading_line_from_empty_stream_returns_empty_string(){
			//Test reading lines from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

			bs.ReadLine().Should().Be(string.Empty);
		}

		[TestCase]
		public void bytes_are_read_correctly(){
			//Test reading bytes from a stream
			var ms = new MemoryStream(new byte[]{10, 25, 32});

			var bs = new HttpMultipartParser.BufferedStream (ms);

			bs.ReadByte().Should().Be((byte)10);
			bs.ReadByte().Should().Be((byte)25);
			bs.ReadByte().Should().Be((byte)32);
            bs.ReadByte().Should().Be(null);

		}

		[TestCase]
		public void reading_bytes_from_empty_stream_returns_null(){
			//Test reading bytes from an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

            bs.ReadByte().Should().Be(null);
		}

		[TestCase]
		public void read_if_next_reads_only_if_match_is_found(){
			//Test reading matching byte sequences at the start of a stream
			var ms = new MemoryStream(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});

			var bs = new HttpMultipartParser.BufferedStream (ms);

            bs.ReadIfNext(new byte[][]{new byte[]{6, 6, 6}, new byte[]{3, 2, 1}}).Should().BeNull(); //Doesn't match
            bs.ReadIfNext(new byte[][]{new byte[]{7, 8, 9}, new byte[]{1, 2, 3}, new byte[]{1, 2}}).Should().BeEquivalentTo(new byte[]{1, 2, 3}); //Matches the first available match, even if longer
			bs.ReadByte().Should().Be((byte)4); //It did actually read the sequence
            bs.ReadIfNext(new byte[][]{ new byte[]{ 5, 6, 7 }, new byte[]{ 5, 6, 7, 8 } }).Should().BeEquivalentTo(new byte[] { 5, 6, 7 }); //Matches the first available match, even if shorter
		}

		[TestCase]
		public void read_if_next_from_empty_string_returns_null(){
			//Test reading matching byte sequences at the start of an empty stream
			var ms = new MemoryStream();

			var bs = new HttpMultipartParser.BufferedStream (ms);

            bs.ReadIfNext(new byte[][]{ new byte[]{ 6, 6, 6 }, new byte[]{ 3, 2, 1 } }).Should().BeNull();
		}

	}
}
