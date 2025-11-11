using System.Collections;

namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Data segment optimized for alphanumeric data encoding.
    /// </summary>
    private sealed class AlphanumericDataSegment(string alphanumericText) : DataSegment(alphanumericText)
    {
        /// <summary>
        /// Calculates the total bit length for this segment when encoded for a specific QR code version.
        /// </summary>
        /// <param name="version">The QR code version (1-40, or -1 to -4 for Micro QR).</param>
        /// <returns>The total number of bits required for this segment.</returns>
        public override int GetBitLength() => GetBitLength(Text.Length);

        /// <summary>
        /// Calculates the total bit length for encoding alphanumeric text of a given length for a specific QR code version.
        /// Includes mode indicator, count indicator, and data bits.
        /// </summary>
        /// <param name="textLength">The length of the alphanumeric text.</param>
        /// <param name="version">The QR code version (1-40, or -1 to -4 for Micro QR).</param>
        /// <returns>The total number of bits required.</returns>
        public static int GetBitLength(int textLength)
        {
            var modeIndicatorLength = 4;
            var countIndicatorLength = 9; //GetCountIndicatorLength(version, EncodingMode.Alphanumeric);
            var dataLength = AlphanumericEncoder.GetBitLength(textLength);
            var length = modeIndicatorLength + countIndicatorLength + dataLength;

            return length;
        }

        /// <summary>
        /// Writes this data segment to an existing BitArray at the specified index.
        /// </summary>
        /// <param name="bitArray">The target BitArray to write to.</param>
        /// <param name="startIndex">The starting index in the BitArray.</param>
        /// <param name="version">The QR code version (1-40, or -1 to -4 for Micro QR).</param>
        /// <returns>The next index in the BitArray after the last bit written.</returns>
        public override int WriteTo(BitArray bitArray, int startIndex) => WriteTo(Text, 0, Text.Length, bitArray, startIndex);

        /// <summary>
        /// Writes a portion of alphanumeric text to a BitArray at the specified index.
        /// Includes mode indicator, count indicator, and data bits.
        /// </summary>
        /// <param name="text">The full alphanumeric text.</param>
        /// <param name="offset">The starting index in the text to encode from.</param>
        /// <param name="length">The number of characters to encode.</param>
        /// <param name="bitArray">The target BitArray to write to.</param>
        /// <param name="bitIndex">The starting index in the BitArray.</param>
        /// <param name="version">The QR code version (1-40, or -1 to -4 for Micro QR).</param>
        /// <returns>The next index in the BitArray after the last bit written.</returns>
        public static int WriteTo(string text, int offset, int length, BitArray bitArray, int bitIndex)
        {
            // write mode indicator
            bitIndex = DecToBin(2, 4, bitArray, bitIndex);

            // write count indicator
            var countIndicatorLength = 9; //GetCountIndicatorLength(version, EncodingMode.Alphanumeric);
            bitIndex = DecToBin(length, countIndicatorLength, bitArray, bitIndex);

            // write data - encode alphanumeric text
            bitIndex = AlphanumericEncoder.WriteToBitArray(text, offset, length, bitArray, bitIndex);

            return bitIndex;
        }
    }
}
