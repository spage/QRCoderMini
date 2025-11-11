using System.Collections;

namespace QRCoder;

/// <summary>
/// Represents the data structure of a QR code.
/// </summary>
public class QRCodeData : IDisposable
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
    /// Gets the raw data of the QR code with the specified compression mode.
    /// </summary>
    /// <param name="compressMode">The compression mode used for the raw data.</param>
    /// <returns>Returns the raw data of the QR code as a byte array.</returns>
    public byte[] GetRawData()
    {
        using var output = new MemoryStream();
        Stream targetStream = output;

        try
        {
            // Build data queue
            var capacity = (ModuleMatrix.Count * ModuleMatrix.Count) + 7; // Total modules + max padding for byte alignment
            var dataQueue = new Queue<int>(capacity);
            foreach (BitArray row in ModuleMatrix)
            {
                for (var i = 0; i < row.Length; i++)
                {
                    dataQueue.Enqueue(row[i] ? 1 : 0);
                }
            }

            var mod = (int)((uint)ModuleMatrix.Count * (uint)ModuleMatrix.Count % 8);
            for (var i = 0; i < 8 - mod; i++)
            {
                dataQueue.Enqueue(0);
            }

            // Process queue
            while (dataQueue.Count > 0)
            {
                byte b = 0;
                for (var i = 7; i >= 0; i--)
                {
                    b += (byte)(dataQueue.Dequeue() << i);
                }

                targetStream.WriteByte(b);
            }
        }
        finally
        {
            if (targetStream != output)
            {
                targetStream.Dispose();
            }
        }

        return output.ToArray();
    }

    /// <summary>
    /// Gets the version of the QR code.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Releases all resources used by the <see cref="QRCodeData"/>.
    /// </summary>
    public virtual void Dispose()
    {
        ModuleMatrix = null!;
        Version = 0;
        GC.SuppressFinalize(this);
    }
}
