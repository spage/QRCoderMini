
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace QRCoder.Extensions;
internal static class StringExtensions
{
    /// <summary>
    /// Indicates whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <returns>
    ///   <see langword="true"/> if the <paramref name="value"/> is null, empty, or white space; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? value)
        => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Converts a hex color string to a byte array.
    /// </summary>
    /// <param name="colorString">Color in HEX format like #ffffff.</param>
    /// <returns>Returns the color as a byte array.</returns>
    internal static byte[] HexColorToByteArray(this string colorString)
    {
        var offset = 0;
        if (colorString.StartsWith('#'))
        {
            offset = 1;
        }

        var byteColor = new byte[(colorString.Length - offset) / 2];
        for (var i = 0; i < byteColor.Length; i++)
        {
            byteColor[i] = byte.Parse(colorString.AsSpan((i * 2) + offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return byteColor;
    }

    /// <summary>
    /// Appends an integer value to the StringBuilder using invariant culture formatting.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="num">The integer value to append.</param>
    internal static void AppendInvariant(this StringBuilder sb, int num)
        => sb.Append(num.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Appends a float value to the StringBuilder using invariant culture formatting with G7 precision.
    /// </summary>
    /// <param name="sb">The StringBuilder to append to.</param>
    /// <param name="num">The float value to append.</param>
    internal static void AppendInvariant(this StringBuilder sb, float num)
        => sb.Append(num.ToString("G7", CultureInfo.InvariantCulture));
}
