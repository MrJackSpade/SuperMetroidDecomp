using System.Text;

namespace SuperMetroid.Core;

/// <summary>
/// One field of a type as a debugger state names it (#2602): the type that declares it and its name. A state writes each
/// field's name before its value, and a restore checks every name against this build's, so a state whose layout this build
/// does not have is refused rather than misread.
/// </summary>
public sealed partial class StateGraphField
{
    /// <summary>Names a field <paramref name="name"/> of the type <paramref name="declaringType"/> names.</summary>
    /// <param name="declaringType">The name of the type that declares the field.</param>
    /// <param name="name">The field's name: a field's, a property's for the value it keeps, or a constructor parameter's.</param>
    public StateGraphField(string declaringType, string name)
    {
        DeclaringType = Ensure.NotNull(declaringType);
        Name = Ensure.NotNull(name);
        byte[] encoded = [.. Encode(declaringType), .. Encode(name)];
        Encoded = encoded;
    }

    /// <summary>The name of the type that declares the field.</summary>
    public string DeclaringType { get; }

    /// <summary>The field's name.</summary>
    public string Name { get; }

    /// <summary>
    /// The two names as a state stores them, one after the other: each as its UTF-8 length in seven-bit groups, low group
    /// first, then its UTF-8 bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Encoded { get; }

    /// <summary>How many bits of a stored name's length each length byte carries.</summary>
    private const int LengthGroupBits = 7;

    /// <summary>The bit a length byte sets when another length byte follows it.</summary>
    private const uint MoreLengthFollows = 1u << LengthGroupBits;

    /// <summary>The bytes of one stored name.</summary>
    private static byte[] Encode(string text)
    {
        byte[] utf8 = Encoding.UTF8.GetBytes(text);
        var bytes = new List<byte>();
        for (uint length = (uint)utf8.Length; ; length >>= LengthGroupBits)
        {
            if (length < MoreLengthFollows)
            {
                bytes.Add((byte)length);
                break;
            }
            bytes.Add((byte)(length | MoreLengthFollows));
        }
        bytes.AddRange(utf8);
        return [.. bytes];
    }

    /// <inheritdoc/>
    public override string ToString() => $"{DeclaringType}::{Name}";
}
