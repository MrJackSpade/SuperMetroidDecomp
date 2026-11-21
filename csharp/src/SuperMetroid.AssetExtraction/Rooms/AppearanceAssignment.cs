using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One assignment of a value to each appearance input that reaches some characters (#1466, #2565): each input held with the
/// value it takes, so a value is found by its input, never by its place in a second list.
/// </summary>
internal sealed class AppearanceAssignment
{
    /// <summary>Each input's value.</summary>
    private readonly Dictionary<AppearanceInput, string> values;

    /// <summary>Assigns each of <paramref name="inputs"/> its value in <paramref name="values"/>.</summary>
    /// <param name="inputs">The inputs, in the order a picture's name spells them.</param>
    /// <param name="values">Each input's value.</param>
    /// <exception cref="ArgumentException">An input has no value, or a value is of no input.</exception>
    internal AppearanceAssignment(ImmutableArray<AppearanceInput> inputs, Dictionary<AppearanceInput, string> values)
    {
        Ensure.NotNull(values);
        if (values.Count != inputs.Length || inputs.Any(input => !values.ContainsKey(input)))
            throw new ArgumentException("An appearance assignment gives each of its inputs one value.", nameof(values));
        Inputs = inputs;
        this.values = values;
    }

    /// <summary>The assignment of no input: what a picture no input reaches shows.</summary>
    internal static AppearanceAssignment OfNoInput { get; } = new([], new Dictionary<AppearanceInput, string>());

    /// <summary>The inputs, in the order a picture's name spells them.</summary>
    internal ImmutableArray<AppearanceInput> Inputs { get; }

    /// <summary>Whether every input shows what the room loads.</summary>
    internal bool ShowsLoadThroughout => values.Values.All(value => value == RoomAppearanceValues.Loaded);

    /// <summary>The value <paramref name="input"/> takes, when the assignment gives it one.</summary>
    internal bool TryValueOf(AppearanceInput input, [NotNullWhen(true)] out string? value) => values.TryGetValue(input, out value);

    /// <summary>The value <paramref name="input"/> takes.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The assignment gives the input no value.</exception>
    internal string ValueOf(AppearanceInput input) => Ensure.Listed(values, input);

    /// <summary>The name of <paramref name="subject"/>'s picture under the assignment.</summary>
    internal string NameOf(string subject) => AppearancePictureNames.Of(subject, Inputs, InNameOrder());

    /// <summary>The name of block <paramref name="block"/>'s picture under the assignment.</summary>
    internal string NameOfBlock(int block) => RoomBlockPictureNames.Of(block, Inputs, InNameOrder());

    /// <summary>Each input's value, in the order of <see cref="Inputs"/>, as a picture's name spells them.</summary>
    private string[] InNameOrder() => [.. Inputs.Select(ValueOf)];
}
