namespace SuperMetroid.Core.Assets;

/// <summary>
/// The sprite artwork gameplay has shown, in the order it showed it (#1392, #1406, #1422, #1463): a room's setup shows the
/// standard sprites, its load each graphics-set member's look, and its encounters their looks; a look shown later replaces
/// what an earlier one showed wherever the installed artwork says they meet. Enemy frames select their pre-coloured pictures
/// from it, and the Ceres Ridley arena's backgrounds read it. The record names looks only; where each lands is the import
/// pipeline's knowledge.
/// </summary>
public sealed class ShownSpriteArtwork
{
    /// <summary>The looks in the order they were last shown; showing a look again moves it last.</summary>
    internal readonly List<SpriteArtworkSource> shown = [];

    /// <summary>Each look's place among every showing ever made, alongside <see cref="shown"/>.</summary>
    private readonly List<long> _stamps = [];

    /// <summary>The looks still visible, newest first, as <see cref="visibleUnder"/> judged them; none since the last showing.</summary>
    [NonSerialized] private SpriteArtworkSource[]? visible;

    /// <summary>The overlap relation <see cref="visible"/> was judged under.</summary>
    [NonSerialized] private ISpriteArtworkOverlap? visibleUnder;

    /// <summary>The place of the latest showing among all ever made; zero before the first.</summary>
    public long Recorded { get; private set; }

    /// <summary>Shows <paramref name="look"/> over whatever showed before it; a look shown again supersedes its earlier showing.</summary>
    public void Show(SpriteArtworkSource look)
    {
        Ensure.NotNull(look);
        int earlier = shown.IndexOf(look);
        if (earlier >= 0)
        {
            shown.RemoveAt(earlier);
            _stamps.RemoveAt(earlier);
        }
        Recorded++;
        shown.Add(look);
        _stamps.Add(Recorded);
        visible = null;
    }

    /// <summary>
    /// The looks still visible, newest first: each look no later look hides, as <paramref name="overlap"/> judges them.
    /// </summary>
    public IReadOnlyList<SpriteArtworkSource> VisibleNewestFirst(ISpriteArtworkOverlap overlap)
    {
        Ensure.NotNull(overlap);
        if (visible is not null && ReferenceEquals(visibleUnder, overlap))
            return visible;
        // A look hidden by a later look is hidden by the latest look that hides it, which nothing later hides, so judging
        // each look against the visible later ones alone finds every hidden one.
        var kept = new List<SpriteArtworkSource>();
        for (int index = shown.Count - 1; index >= 0; index--)
        {
            SpriteArtworkSource look = shown[index];
            if (!kept.Exists(later => overlap.Hides(later, look)))
                kept.Add(look);
        }
        visible = [.. kept];
        visibleUnder = overlap;
        return visible;
    }

    /// <summary>The looks shown after the showing whose place is <paramref name="recorded"/>, newest first.</summary>
    /// <param name="recorded">A value <see cref="Recorded"/> held.</param>
    public IEnumerable<SpriteArtworkSource> ShownAfter(long recorded)
    {
        for (int index = shown.Count - 1; index >= 0 && _stamps[index] > recorded; index--)
            yield return shown[index];
    }
}
