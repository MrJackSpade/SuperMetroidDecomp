using System.Collections.Concurrent;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The encoded names of one enum's members by their stored bits, each formatted and encoded once (#1532): the name
/// <see cref="Enum.ToString()"/> gives the member, as <see cref="BinaryWriter.Write(string)"/> writes it. A debugger state
/// stores an enum by its member's name, so a state does not depend on member numbering.
/// </summary>
internal sealed class DebuggerEnumNames
{
    /// <summary>Each enum's names, or null for an enum whose underlying type is not an integer.</summary>
    private static readonly ConcurrentDictionary<Type, DebuggerEnumNames?> enums = new();

    /// <summary>The enum.</summary>
    private readonly Type type;

    /// <summary>How many bytes the underlying integer stores.</summary>
    private readonly StorageWidth width;

    /// <summary>Whether the underlying integer is signed.</summary>
    private readonly bool signed;

    /// <summary>Each member's encoded name, by its bits zero-extended from the underlying integer's width.</summary>
    private readonly ConcurrentDictionary<ulong, byte[]> names = new();

    /// <summary>The names of <paramref name="enumType"/>, whose underlying integer is <paramref name="width"/> wide.</summary>
    private DebuggerEnumNames(Type enumType, StorageWidth width, bool signed)
    {
        type = enumType;
        this.width = width;
        this.signed = signed;
    }

    /// <summary>The names of <paramref name="enumType"/>, or null when its underlying type is not an integer.</summary>
    internal static DebuggerEnumNames? For(Type enumType) => enums.GetOrAdd(enumType, static candidate =>
        Type.GetTypeCode(candidate) switch
        {
            TypeCode.SByte => new DebuggerEnumNames(candidate, StorageWidth.One, signed: true),
            TypeCode.Byte => new DebuggerEnumNames(candidate, StorageWidth.One, signed: false),
            TypeCode.Int16 => new DebuggerEnumNames(candidate, StorageWidth.Two, signed: true),
            TypeCode.UInt16 => new DebuggerEnumNames(candidate, StorageWidth.Two, signed: false),
            TypeCode.Int32 => new DebuggerEnumNames(candidate, StorageWidth.Four, signed: true),
            TypeCode.UInt32 => new DebuggerEnumNames(candidate, StorageWidth.Four, signed: false),
            TypeCode.Int64 => new DebuggerEnumNames(candidate, StorageWidth.Eight, signed: true),
            TypeCode.UInt64 => new DebuggerEnumNames(candidate, StorageWidth.Eight, signed: false),
            TypeCode.Empty or TypeCode.Object or TypeCode.DBNull or TypeCode.Boolean or TypeCode.Char or
                TypeCode.Single or TypeCode.Double or TypeCode.Decimal or TypeCode.DateTime or TypeCode.String => null,
            _ => throw new NotSupportedException($"Enum {candidate.FullName} has an unknown underlying type code."),
        });

    /// <summary>The unsigned integer type as wide as the enum's underlying type, which its bits are read as.</summary>
    internal Type BitsType => width switch
    {
        StorageWidth.One => typeof(byte),
        StorageWidth.Two => typeof(ushort),
        StorageWidth.Four => typeof(uint),
        StorageWidth.Eight => typeof(ulong),
        _ => throw new InvalidOperationException($"Enum {type.FullName} has no storage width."),
    };

    /// <summary>
    /// The bits of the boxed member <paramref name="member"/>, zero-extended from the underlying integer's width, as a
    /// compiled writer converts a member to them. A boxed enum unboxes as its underlying integer.
    /// </summary>
    internal ulong BitsOf(object member) => width switch
    {
        StorageWidth.One => signed ? unchecked((byte)(sbyte)member) : (byte)member,
        StorageWidth.Two => signed ? unchecked((ushort)(short)member) : (ushort)member,
        StorageWidth.Four => signed ? unchecked((uint)(int)member) : (uint)member,
        StorageWidth.Eight => signed ? unchecked((ulong)(long)member) : (ulong)member,
        _ => throw new InvalidOperationException($"Enum {type.FullName} has no storage width."),
    };

    /// <summary>The encoded name of the member whose bits are <paramref name="bits"/>.</summary>
    internal byte[] Of(ulong bits) => names.TryGetValue(bits, out byte[]? known) ? known : names.GetOrAdd(bits, Name);

    /// <summary>The encoded name <see cref="Enum.ToString()"/> gives the member whose bits are <paramref name="bits"/>.</summary>
    private byte[] Name(ulong bits)
    {
        object member = !signed ? Enum.ToObject(type, bits) : width switch
        {
            StorageWidth.One => Enum.ToObject(type, (long)unchecked((sbyte)(byte)bits)),
            StorageWidth.Two => Enum.ToObject(type, (long)unchecked((short)(ushort)bits)),
            StorageWidth.Four => Enum.ToObject(type, (long)unchecked((int)(uint)bits)),
            StorageWidth.Eight => Enum.ToObject(type, unchecked((long)bits)),
            _ => throw new InvalidOperationException($"Enum {type.FullName} has no storage width."),
        };
        string name = member.ToString() ?? throw new InvalidOperationException($"Enum {type.FullName} has no name.");
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
            writer.Write(name);
        return stream.ToArray();
    }

    /// <summary>How many bytes an enum's underlying integer stores.</summary>
    private enum StorageWidth
    {
        /// <summary>One byte: <see cref="byte"/> or <see cref="sbyte"/>.</summary>
        One,

        /// <summary>Two bytes: <see cref="ushort"/> or <see cref="short"/>.</summary>
        Two,

        /// <summary>Four bytes: <see cref="uint"/> or <see cref="int"/>.</summary>
        Four,

        /// <summary>Eight bytes: <see cref="ulong"/> or <see cref="long"/>.</summary>
        Eight,
    }
}
