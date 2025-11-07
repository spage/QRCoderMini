using System.Buffers;

namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a block of codewords in a QR code. QR codes are divided into several blocks for error correction purposes.
    /// Each block contains a series of data codewords followed by error correction codewords.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="CodewordBlock"/> struct with specified arrays of code words and error correction (ECC) words.
    /// </remarks>
    /// <param name="codeWordsOffset">The offset of the data codewords within the main BitArray. Data codewords carry the actual information.</param>
    /// <param name="codeWordsLength">The length in bits of the data codewords within the main BitArray.</param>
    /// <param name="eccWords">The array of error correction codewords for this block. These codewords help recover the data if the QR code is damaged.</param>
    private readonly struct CodewordBlock(int codeWordsOffset, int codeWordsLength, ArraySegment<byte> eccWords)
    {

        /// <summary>
        /// Gets the offset of the data codewords in the BitArray.
        /// </summary>
        public int CodeWordsOffset { get; } = codeWordsOffset;

        /// <summary>
        /// Gets the length of the data codewords in the BitArray.
        /// </summary>
        public int CodeWordsLength { get; } = codeWordsLength;

        /// <summary>
        /// Gets the error correction codewords associated with this block.
        /// </summary>
        public ArraySegment<byte> ECCWords { get; } = eccWords;

        private static List<CodewordBlock>? codewordBlocks;

        public static List<CodewordBlock> GetList(int capacity)
            => Interlocked.Exchange(ref codewordBlocks, null) ?? new List<CodewordBlock>(capacity);

        public static void ReturnList(List<CodewordBlock> list)
        {
            foreach (CodewordBlock item in list)
            {
                ArrayPool<byte>.Shared.Return(item.ECCWords.Array!);
            }

            list.Clear();
            _ = Interlocked.CompareExchange(ref codewordBlocks, list, null);
        }
    }
}
