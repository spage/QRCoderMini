namespace QRCoder;

public partial class QRCodeGenerator
{
    /// <summary>
    /// Represents a 2D point with integer coordinates.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Point"/> struct with specified X and Y coordinates.
    /// </remarks>
    /// <param name="x">The X-coordinate of the point.</param>
    /// <param name="y">The Y-coordinate of the point.</param>
    private readonly struct Point(int x, int y)
    {
        /// <summary>
        /// Gets the X-coordinate of the point.
        /// </summary>
        public int X { get; } = x;

        /// <summary>
        /// Gets the Y-coordinate of the point.
        /// </summary>
        public int Y { get; } = y;
    }
}
