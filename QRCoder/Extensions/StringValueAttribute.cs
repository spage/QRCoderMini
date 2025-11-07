using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace QRCoder.Extensions;

/// <summary>
/// Used to represent a string value for a value in an enum.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="StringValueAttribute"/> class.
/// Init a StringValue Attribute.
/// </remarks>
/// <param name="value"></param>
[Obsolete("This attribute will be removed in a future version of QRCoder.")]
[AttributeUsage(AttributeTargets.Field)]
public class StringValueAttribute(string value) : Attribute
{
    /// <summary>
    /// Gets or sets holds the alue in an enum.
    /// </summary>
    public string StringValue { get; protected set; } = value;
}

/// <summary>
/// Enumeration extension methods.
/// </summary>
[Obsolete("This class will be removed in a future version of QRCoder.")]
public static class CustomExtensions
{
    /// <summary>
    /// Will get the string value for a given enum's value.
    /// </summary>
    /// <returns></returns>
    [RequiresUnreferencedCode("This method uses reflection to examine the provided enum value.")]
    public static string? GetStringValue(this Enum value)
    {
        FieldInfo fieldInfo = value.GetType().GetField(value.ToString())!;
        var attr = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
        return attr!.Length > 0 ? attr[0].StringValue : null;
    }
}
