namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>The members of <typeparamref name="TEnum"/> one packed run holds, in the order it first holds each.</summary>
/// <typeparam name="TEnum">The enum.</typeparam>
/// <param name="type">The enum's handling.</param>
internal sealed class PackedEnumTable<TEnum>(EnumStateType<TEnum> type) : IPackedEnumTable where TEnum : struct, Enum
{
    private readonly List<TEnum> members = [];
    private readonly Dictionary<TEnum, int> places = [];

    /// <inheritdoc/>
    public Type Enum => typeof(TEnum);

    /// <summary>The place of <paramref name="member"/>, adding it when first held.</summary>
    internal int Index(TEnum member)
    {
        if (places.TryGetValue(member, out int place))
            return place;
        place = members.Count;
        members.Add(member);
        places.Add(member, place);
        return place;
    }

    /// <summary>The member at <paramref name="index"/>.</summary>
    /// <exception cref="InvalidDataException">The table holds no such member.</exception>
    internal TEnum Member(int index) => (uint)index < (uint)members.Count
        ? members[index]
        : throw new InvalidDataException($"A packed run names member {index} of {type.Name}, whose table holds {members.Count}.");

    /// <summary>Adds a member read from a stored table.</summary>
    internal void Add(TEnum member) => members.Add(member);

    /// <inheritdoc/>
    public void Write(BinaryWriter writer)
    {
        writer.Write(type.EncodedName);
        writer.Write(members.Count);
        foreach (TEnum member in members)
            writer.Write(type.EncodedMember(member));
    }
}
