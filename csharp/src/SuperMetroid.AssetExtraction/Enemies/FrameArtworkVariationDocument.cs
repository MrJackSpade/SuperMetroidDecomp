using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>How the installed enemy sheets' manifest spells a frame's <see cref="FrameArtworkVariation"/> (#1513).</summary>
internal sealed record FrameArtworkVariationDocument
{
    /// <summary>For each step, the step whose picture the frame shows then, or null for its loaded picture or a step it is not drawn at.</summary>
    public required int?[] Pictures { get; init; }

    /// <summary>Whether the frame is drawn at each step.</summary>
    public required bool[] Drawn { get; init; }

    /// <summary>Whether the frame is drawn before the sequence reaches its first step.</summary>
    public required bool DrawnBeforeFirstStep { get; init; }

    /// <summary>The manifest's spelling of <paramref name="variation"/>.</summary>
    internal static FrameArtworkVariationDocument Of(FrameArtworkVariation variation) => new()
    {
        Pictures = [.. Ensure.NotNull(variation).Pictures], Drawn = [.. variation.Drawn], DrawnBeforeFirstStep = variation.DrawnBeforeFirstStep,
    };

    /// <summary>The variation the manifest spells for <paramref name="sequence"/>.</summary>
    /// <exception cref="InvalidDataException">The document is incomplete, or pictures a step the frame is not drawn at.</exception>
    internal FrameArtworkVariation Variation(EncounterArtworkSequence sequence) =>
        new(sequence, [.. Pictures ?? throw new InvalidDataException($"A frame's {sequence} variation lists no pictures.")],
            [.. Drawn ?? throw new InvalidDataException($"A frame's {sequence} variation lists no drawn steps.")], DrawnBeforeFirstStep);
}
