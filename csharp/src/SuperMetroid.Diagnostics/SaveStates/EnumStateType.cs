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
    /// <inheritdoc/>
    internal override bool ReachesObjects => false;

    /// <summary>
    /// Each member's stored name, worked out once, made when a state first writes a value of the enum: the registry holds
    /// every enum of the game, and most are never written, so registering one makes nothing.
    /// </summary>
    private ConcurrentDictionary<TEnum, string>? memberNames;

    /// <summary>Each name decoded to a defined member, made when a state first reads a value of the enum.</summary>
    private ConcurrentDictionary<string, TEnum>? decoded;

    /// <inheritdoc/>
    protected override string Named() => Types.Format(name);

    /// <summary>The stored name of <paramref name="value"/>.</summary>
    internal string MemberName(TEnum value)
    {
        ConcurrentDictionary<TEnum, string> names = memberNames ?? Made(ref memberNames, new ConcurrentDictionary<TEnum, string>());
        return names.TryGetValue(value, out string? known) ? known : names.GetOrAdd(value, static member => member.ToString());
    }

    /// <summary>The cache <paramref name="field"/> holds, which is <paramref name="made"/> unless another thread made it first.</summary>
    private static TCache Made<TCache>(ref TCache? field, TCache made) where TCache : class =>
        Interlocked.CompareExchange(ref field, made, null) ?? made;

    /// <summary>The bits every defined member sets, which a flags value's bits must lie within.</summary>
    private static ulong DefinedBits()
    {
        ulong bits = 0;
        foreach (TEnum member in Enum.GetValues<TEnum>())
            bits |= Bits(member);
        return bits;
    }

    /// <inheritdoc/>
    internal override void Write(GraphWriter writer, object value)
    {
        writer.Kind(PayloadKind.EnumName);
        writer.WriteName(MemberName((TEnum)value));
    }

    /// <inheritdoc/>
    internal override object Read(GraphReader reader, int referenceId, PayloadKind kind) =>
        kind == PayloadKind.EnumName ? Decode(reader.ReadName(), reader.InStructElements) : throw UnexpectedKind(kind);

    /// <summary>The member a state names <paramref name="text"/>, validated in this build's domain.</summary>
    /// <exception cref="InvalidDataException">The name names no member of this build, or an undefined value.</exception>
    internal TEnum Decode(string text, bool zeroStorageSlot)
    {
        ConcurrentDictionary<string, TEnum> names = decoded ?? Made(ref decoded, new ConcurrentDictionary<string, TEnum>(StringComparer.Ordinal));
        if (names.TryGetValue(text, out TEnum known))
            return known;
        if (!Enum.TryParse(text, ignoreCase: false, out TEnum value))
            throw new InvalidDataException($"Debugger state {Name} value '{text}' names no member of this build.");
        ulong bits = Bits(value);
        if (zeroStorageSlot && bits == 0)
            return value;
        // A decoded name is kept, so each name's members are looked up once.
        bool defined = flags ? (bits & ~DefinedBits()) == 0 : Enum.IsDefined(value);
        if (!defined)
            throw new InvalidDataException($"Debugger state {Name} value '{text}' is not a {(flags ? "combination of defined flags" : "defined member")}.");
        names.TryAdd(text, value);
        return value;
    }

    /// <inheritdoc/>
    public IPackedEnumTable ReadTable(GraphReader reader)
    {
        var table = new PackedEnumTable<TEnum>(this);
        int count = reader.ReadLength("packed enum member");
        for (int index = 0; index < count; index++)
            table.Add(Decode(reader.ReadName(), zeroStorageSlot: true));
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
