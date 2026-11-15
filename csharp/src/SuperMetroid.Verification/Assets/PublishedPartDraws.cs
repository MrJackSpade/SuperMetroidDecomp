using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The rule that an actor publishes at most one frame per independently moving part in one update (#1513), read from
/// the draws one update published. A moving part is placed at its draw's origin, so two draws of one kind that land on
/// one origin in one update share the place they move by: they are pieces of one part, as a body published as a top and
/// a bottom half is. The frames such draws show are part images: pieces of a body that is only ever drawn together with
/// the others.
/// </summary>
internal static class PublishedPartDraws
{
    /// <summary>Each breach of the rule among <paramref name="draws"/>, the draws of one update.</summary>
    internal static IReadOnlyList<string> Violations(IEnumerable<SpriteDraw> draws)
    {
        ArgumentNullException.ThrowIfNull(draws);
        SpriteDraw[] published = [.. draws];
        var violations = new List<string>();
        foreach (IGrouping<(SpriteSourceKind Kind, Offset16<ScreenPixels> Origin), SpriteDraw> placed in published.GroupBy(draw => (draw.Source.Kind, draw.Origin)))
        {
            SpriteDraw[] frames = [.. placed];
            if (frames.Length > 1)
                violations.Add($"{frames.Length} {placed.Key.Kind} frames are published at one origin in one update, pieces of one part: {string.Join("; ", frames.Select(draw => draw.ToString()))}");
        }
        return violations;
    }
}
