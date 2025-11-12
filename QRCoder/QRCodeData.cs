using System.Collections;

namespace QRCoder;

/// <summary>
/// Represents the data structure of a QR code.
/// </summary>
public class QRCodeData
{
    /// <summary>
    /// Gets or sets the module matrix of the QR code.
    /// </summary>
    public List<BitArray> ModuleMatrix { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QRCodeData"/> class with the specified version and padding option.
    /// </summary>
    /// <param name="version">The version of the QR code.</param>
    /// <param name="addPadding">Indicates whether padding should be added to the QR code.</param>
    public QRCodeData()
    {
        Version = 2;
        var size = 25;
        ModuleMatrix = new List<BitArray>(size);
        for (var i = 0; i < size; i++)
        {
            ModuleMatrix.Add(new BitArray(size));
        }
    }

    /// <summary>
    /// Gets the raw data of the QR code.
    /// </summary>
    /// <returns>Returns the raw data of the QR code as a byte array.</returns>
    public byte[] GetRawData()
    {
        var totalModules = ModuleMatrix.Count * ModuleMatrix.Count;
        var byteCount = (totalModules + 7) / 8; // Round up to nearest byte
        var result = new byte[byteCount];

        var bitIndex = 0;
        byte currentByte = 0;
        var byteIndex = 0;

        foreach (BitArray row in ModuleMatrix)
        {
            for (var i = 0; i < row.Length; i++)
            {
                if (row[i])
                {
                    currentByte |= (byte)(1 << (7 - (bitIndex % 8)));
                }

                bitIndex++;
                if (bitIndex % 8 == 0)
                {
                    result[byteIndex++] = currentByte;
                    currentByte = 0;
                }
            }
        }

        // Write final partial byte if needed
        if (bitIndex % 8 != 0)
        {
            result[byteIndex] = currentByte;
        }

        return result;
    }

    /// <summary>
    /// Gets the version of the QR code.
    /// </summary>
    public int Version { get; private set; }
}
