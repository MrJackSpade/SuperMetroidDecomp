namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>The members of <typeparamref name="TEnum"/> one packed run holds, in the order it first holds each.</summary>
/// <typeparam name="TEnum">The enum.</typeparam>
/// <param name="type">The enum's handling.</param>
internal sealed class PackedEnumTable<TEnum>(EnumStateType<TEnum> type) : IPackedEnumTable where TEnum : struct, Enum
{
    private readonly List<TEnum> members = [];

    /// <inheritdoc/>
    public Type Enum => typeof(TEnum);

    /// <summary>Adds a member read from a stored table.</summary>
    internal void Add(TEnum member) => members.Add(member);

    /// <inheritdoc/>
    public void Write(GraphWriter writer)
    {
        writer.WriteName(type.Name);
        writer.Output.Write(members.Count);
        foreach (TEnum member in members)
            writer.WriteName(type.MemberName(member));
    }
}
