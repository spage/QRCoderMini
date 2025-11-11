namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// This class contains the alignment patterns used in QR codes.
    /// </summary>
    private static class AlignmentPatterns
    {
        /// <summary>
        /// A predefined alignment pattern for version 2 QR codes.
        /// </summary>
        public static readonly List<Point> alignmentPattern =
        [
            new Point(4, 4), new Point(4, 16), new Point(16, 4), new Point(16, 16)
        ];
    }
}
