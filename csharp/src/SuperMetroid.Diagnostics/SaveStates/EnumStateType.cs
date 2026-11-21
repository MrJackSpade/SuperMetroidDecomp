using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// An enum, held by its member's name, or names joined for flags, so a state does not depend on member numbering (#1507).
/// A restored value re-enters the game's closed domain only as a defined member, or for flags a combination of defined
/// members (#627); a zero inside an element of an array of value types is unused storage and is accepted.
/// </summary>
/// <typeparam name="TEnum">The enum.</typeparam>
internal sealed class EnumStateType<TEnum>(StateGraphTypes types, StateGraphName name, bool flags) : StateGraphType(typeof(TEnum), types), IPackedEnumType
    where TEnum : struct, Enum
{
    /// <summary>Each member's stored name, encoded once.</summary>
    private readonly ConcurrentDictionary<TEnum, byte[]> encoded = new();

    /// <summary>Each name decoded to a defined member.</summary>
    private readonly ConcurrentDictionary<string, TEnum> decoded = new(StringComparer.Ordinal);

    /// <summary>The bits every defined member sets, for a flags enum.</summary>
    private readonly ulong definedBits = Enum.GetValues<TEnum>().Aggregate(0UL, static (bits, member) => bits | Bits(member));

    private byte[]? prefix;

    /// <summary>What every framed member starts with: its marker, number zero, name and payload kind.</summary>
    internal byte[] Prefix => prefix ??= [(byte)ObjectMarker.New, 0, 0, 0, 0, .. EncodedName, (byte)PayloadKind.EnumName];

    /// <inheritdoc/>
    protected override string Named() => Types.Format(name);

    /// <summary>The stored name of <paramref name="value"/>.</summary>
    internal byte[] EncodedMember(TEnum value) =>
        encoded.TryGetValue(value, out byte[]? known) ? known : encoded.GetOrAdd(value, static member => StateGraphNames.Encode(member.ToString()));

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.EnumName);
        writer.Output.Write(EncodedMember((TEnum)value));
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.EnumName ? Decode(reader.Input.ReadString(), reader.InStructElements) : throw UnexpectedKind(kind);

    /// <summary>The member a state names <paramref name="text"/>, validated in this build's domain.</summary>
    /// <exception cref="InvalidDataException">The name names no member of this build, or an undefined value.</exception>
    internal TEnum Decode(string text, bool zeroStorageSlot)
    {
        if (decoded.TryGetValue(text, out TEnum known))
            return known;
        if (!Enum.TryParse(text, ignoreCase: false, out TEnum value))
            throw new InvalidDataException($"Debugger state {Name} value '{text}' names no member of this build.");
        ulong bits = Bits(value);
        if (zeroStorageSlot && bits == 0)
            return value;
        bool defined = flags ? (bits & ~definedBits) == 0 : Enum.IsDefined(value);
        if (!defined)
            throw new InvalidDataException($"Debugger state {Name} value '{text}' is not a {(flags ? "combination of defined flags" : "defined member")}.");
        decoded.TryAdd(text, value);
        return value;
    }

    /// <inheritdoc/>
    internal override void WritePacked(PackedWriter writer, object value) => writer.WriteEnum((TEnum)value);

    /// <inheritdoc/>
    internal override object ReadPacked(PackedReader reader) => reader.ReadEnum<TEnum>();

    /// <inheritdoc/>
    public IPackedEnumTable ReadTable(GraphReader reader)
    {
        var table = new PackedEnumTable<TEnum>(this);
        int count = reader.ReadLength("packed enum member");
        for (int index = 0; index < count; index++)
            table.Add(Decode(reader.Input.ReadString(), zeroStorageSlot: true));
        return table;
    }

    /// <summary>A member's bits, zero-extended from the enum's underlying integer.</summary>
    private static ulong Bits(TEnum value) => Unsafe.SizeOf<TEnum>() switch
    {
        1 => Unsafe.As<TEnum, byte>(ref value),
        2 => Unsafe.As<TEnum, ushort>(ref value),
        4 => Unsafe.As<TEnum, uint>(ref value),
        _ => Unsafe.As<TEnum, ulong>(ref value),
    };
}
