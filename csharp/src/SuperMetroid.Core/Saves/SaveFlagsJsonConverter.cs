using System.Text.Json;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Writes a set of flags into a save as its members' installed keys in the save's camel case, and reads it back (#2520):
/// a value a member names is written as that member, and any other as each flag it holds in the written-out order, separated by a
/// comma and a space; reading takes the flags in any order, their case ignored, as saves have always been read.
/// </summary>
/// <typeparam name="TFlags">The flags the converter reads and writes.</typeparam>
/// <param name="keys">The flags' written-out keys.</param>
/// <param name="order">Each flag on its own, in the order a save lists a set of them (#2567).</param>
/// <param name="union">The set holding both sets' flags.</param>
internal sealed partial class SaveFlagsJsonConverter<TFlags>(InstalledKeys<TFlags> keys, IReadOnlyList<TFlags> order, Func<TFlags, TFlags, TFlags> union)
    : SaveKeyJsonConverter<TFlags>(keys)
    where TFlags : struct, Enum
{
    /// <summary>Each flag on its own in the order a save lists them; the empty set is no flag.</summary>
    private readonly TFlags[] flags = [.. Ensure.NotNull(order)];

    /// <summary>The set holding both sets' flags.</summary>
    private readonly Func<TFlags, TFlags, TFlags> union = Ensure.NotNull(union);

    /// <inheritdoc />
    /// <exception cref="JsonException">The set holds a flag no member names.</exception>
    private protected override string NameOf(TFlags value)
    {
        if (MemberName(value) is { } name)
            return name;
        TFlags[] held = [.. flags.Where(flag => value.HasFlag(flag))];
        if (!EqualityComparer<TFlags>.Default.Equals(held.Aggregate(default(TFlags), union), value))
            throw new JsonException($"{value} holds a {typeof(TFlags).Name} flag no member names.");
        return string.Join(", ", held.Select(flag => MemberName(flag)));
    }

    /// <inheritdoc />
    /// <exception cref="JsonException">A part of the text names no flag.</exception>
    private protected override TFlags Named(string text) =>
        text.Split(',').Select(part => TryMember(part.Trim(), out TFlags member) ? member : throw new JsonException($"'{text}' names no {typeof(TFlags).Name}."))
            .Aggregate(default(TFlags), union);
}
