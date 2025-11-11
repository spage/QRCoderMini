namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Defines the levels of error correction available in QR codes.
    /// Each level specifies the proportion of data that can be recovered if the QR code is partially obscured or damaged.
    /// </summary>
    public enum ECCLevel
    {
        M = 1, // Medium: Recovers approximately 15% of data
    }
}
