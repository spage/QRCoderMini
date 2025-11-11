namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Specifies the encoding modes for the characters in a QR code.
    /// </summary>
    internal enum EncodingMode
    {
        /// <summary>
        /// Alphanumeric encoding mode, which is used to encode alphanumeric characters (0-9, A-Z, space, and some punctuation).
        /// Two characters are encoded into 11 bits.
        /// </summary>
        Alphanumeric = 2,

        /// <summary>
        /// Byte encoding mode, primarily using the ISO-8859-1 character set. Each character is encoded into 8 bits.
        /// When combined with ECI, it can be adapted to use other character sets.
        /// </summary>
        Byte = 4,
    }
}
