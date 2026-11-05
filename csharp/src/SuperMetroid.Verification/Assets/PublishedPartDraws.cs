using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// The rule that an actor publishes at most one frame per independently moving part in one update (#1513), read from
/// the draws one update published. A moving part is what a draw's source names (an enemy slot, a projectile slot, a
/// rope segment), placed at the draw's origin. Two draws of one update break the rule when:
/// <list type="bullet">
/// <item>they come from one source: that part publishes more than one frame, so each is a piece of its picture; or</item>
/// <item>they come from different sources of one kind and land on one origin: they share the place they move by, so they
/// are pieces of one part split across sources, as a body published as a top and a bottom half is.</item>
/// </list>
/// The frames such draws show are part images: pieces of a body that is only ever drawn together with the others.
/// </summary>
internal static class PublishedPartDraws
{
    /// <summary>Each breach of the rule among <paramref name="draws"/>, the draws of one update.</summary>
    internal static IReadOnlyList<string> Violations(IEnumerable<SpriteDraw> draws)
    {
        ArgumentNullException.ThrowIfNull(draws);
        SpriteDraw[] published = [.. draws];
        var violations = new List<string>();
        foreach (IGrouping<SpriteSource, SpriteDraw> source in published.GroupBy(draw => draw.Source))
        {
            SpriteDraw[] frames = [.. source];
            if (frames.Length > 1)
                violations.Add($"{source.Key.Kind}#{source.Key.Instance} publishes {frames.Length} frames in one update: {string.Join("; ", frames.Select(draw => draw.ToString()))}");
        }
        foreach (IGrouping<(SpriteSourceKind Kind, Position16<ScreenPixels> Origin), SpriteDraw> placed in published.GroupBy(draw => (draw.Source.Kind, draw.Origin)))
        {
            SpriteSource[] sources = [.. placed.Select(draw => draw.Source).Distinct()];
            if (sources.Length > 1)
                violations.Add($"{sources.Length} {placed.Key.Kind} sources publish frames at one origin in one update, pieces of one part: {string.Join("; ", placed.Select(draw => draw.ToString()))}");
        }
        return violations;
    }
}
