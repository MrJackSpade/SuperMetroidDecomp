using System.Text.Json;

namespace SuperMetroid.Core.Saves;

/// <summary>
/// Writes a set of flags into a save as its members' installed keys in the save's camel case, and reads it back (#2520):
/// a value a member names is written as that member, and any other as each flag it holds, lowest first, separated by a
/// comma and a space; reading takes the flags in any order, their case ignored, as saves have always been read.
/// </summary>
/// <typeparam name="TFlags">The flags the converter reads and writes.</typeparam>
/// <param name="keys">The flags' written-out keys.</param>
/// <param name="union">The set holding both sets' flags.</param>
internal sealed class SaveFlagsJsonConverter<TFlags>(InstalledKeys<TFlags> keys, Func<TFlags, TFlags, TFlags> union)
    : SaveKeyJsonConverter<TFlags>(keys)
    where TFlags : struct, Enum
{
    /// <summary>Each flag on its own, lowest first; the empty set is no flag.</summary>
    private readonly TFlags[] flags = [.. Enum.GetValues<TFlags>().Where(flag => !EqualityComparer<TFlags>.Default.Equals(flag, default))];

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
        text.Split(',').Select(part => Member(part.Trim()) ?? throw new JsonException($"'{text}' names no {typeof(TFlags).Name}."))
            .Aggregate(default(TFlags), union);
}
