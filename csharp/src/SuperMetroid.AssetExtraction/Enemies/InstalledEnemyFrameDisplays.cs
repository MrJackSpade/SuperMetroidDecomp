using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Every enemy frame's display binding (#1603, #1638): for each frame an enemy shows, the frame displayed for it, among the
/// ordinary frames (<see cref="EnemySpritemapDefinitions"/>) and the extended frames
/// (<see cref="EnemyExtendedFrameDefinitions"/>). What gameplay reads to draw every enemy frame and the names of the frames
/// each enemy's sheet holds follow from the bindings and the frame definitions alone: no composition is read, so the game
/// reads them from the installed display bindings (<see cref="EnemyFrameDisplayFiles"/>) while the import's compositions
/// stay import data.
/// </summary>
internal sealed class InstalledEnemyFrameDisplays
{
    /// <summary>The ordinary frames.</summary>
    private static readonly Lazy<HashSet<EnemyFrameId>> OrdinaryFrames = new(() => [.. EnemySpritemapDefinitions.Frames.Select(frame => frame.Frame)]);

    /// <summary>The extended frames, by identity.</summary>
    private static readonly Lazy<Dictionary<EnemyFrameId, EnemyFrameName>> ExtendedFrames = new(() =>
        EnemyExtendedFrameDefinitions.Frames.ToDictionary(frame => frame.Frame));

    /// <summary>The ordinary frames' bindings.</summary>
    private readonly Dictionary<EnemyFrameId, EnemyFrameId> ordinary;

    /// <summary>The extended frames' bindings.</summary>
    private readonly Dictionary<EnemyFrameId, EnemyFrameId> extended;

    /// <summary>Retains the validated <paramref name="ordinary"/> and <paramref name="extended"/> bindings.</summary>
    internal InstalledEnemyFrameDisplays(Dictionary<EnemyFrameId, EnemyFrameId> ordinary, Dictionary<EnemyFrameId, EnemyFrameId> extended)
    {
        this.ordinary = ordinary;
        this.extended = extended;
    }

    /// <summary>The bindings of the import's compositions <paramref name="spritemaps"/> and <paramref name="extendedFrames"/>.</summary>
    internal static InstalledEnemyFrameDisplays Of(EnemySpritemapCatalog spritemaps, EnemyExtendedFrameCatalog extendedFrames) =>
        new(spritemaps.selection.DisplayFrames, extendedFrames.selection.DisplayFrames);

    /// <summary>
    /// Every frame as gameplay draws it: every named frame is installed; an ordinary frame is pictured unless it draws
    /// nothing, and an extended frame unless it is drawn only on BG2, so a frame shown only on a boss body has no picture.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame is both ordinary and extended, or two bindings bind one frame.</exception>
    internal EnemyFrameDisplays Displays()
    {
        var frames = new HashSet<EnemyFrameId>();
        var pictured = new HashSet<EnemyFrameId>();
        var displayFrames = new Dictionary<EnemyFrameId, EnemyFrameId>();
        foreach (EnemyFrameId frame in OrdinaryFrames.Value)
        {
            Install(frames, frame);
            if (!CommonEnemyEmptyFrames.DrawsNothing(frame))
                pictured.Add(frame);
        }
        foreach ((EnemyFrameId frame, EnemyFrameName definition) in ExtendedFrames.Value)
        {
            Install(frames, frame);
            if (!EnemyExtendedFrameBounds.IsBg2Only(definition))
                pictured.Add(frame);
        }
        foreach ((EnemyFrameId shown, EnemyFrameId selected) in ordinary.Concat(extended))
            if (!displayFrames.TryAdd(shown, selected))
                throw new InvalidDataException($"The enemy display bindings bind enemy frame {shown} twice.");
        return new EnemyFrameDisplays(frames, pictured, displayFrames);
    }

    /// <summary>
    /// The names of the frames <paramref name="enemy"/>'s sheets hold, in composition order: each frame it shows that is
    /// displayed through an ordinary frame, or through an extended frame with a sprite piece.
    /// </summary>
    internal IEnumerable<string> FrameNames(EnemyDefinitionId enemy) =>
        EnemyDefinitionFrameDefinitions.Frames.TryGetValue(enemy, out IReadOnlyList<EnemyFrameId>? shown)
            ? shown.Where(Composed).Select(EnemySheetDefinitions.FrameName) : [];

    /// <summary>
    /// Whether the frame an enemy shows is composed on its sheet: its ordinary binding selects an ordinary frame, or the
    /// extended frame displayed for it, the one its binding selects or else itself, has a sprite piece. The empty frame
    /// draws nothing to compose.
    /// </summary>
    private bool Composed(EnemyFrameId frame)
    {
        if (ordinary.TryGetValue(frame, out EnemyFrameId selected) && OrdinaryFrames.Value.Contains(selected))
            return true;
        EnemyFrameId? displayed = extended.TryGetValue(frame, out selected) && ExtendedFrames.Value.ContainsKey(selected) ? selected
            : frame != EnemyFrameId.Empty && ExtendedFrames.Value.ContainsKey(frame) ? frame : null;
        return displayed is { } stored && !EnemyExtendedFrameBounds.IsBg2Only(ExtendedFrames.Value[stored]);
    }

    /// <summary>Adds <paramref name="frame"/> to the installed frames.</summary>
    /// <exception cref="InvalidDataException">The frame is already installed.</exception>
    private static void Install(HashSet<EnemyFrameId> frames, EnemyFrameId frame)
    {
        if (!frames.Add(frame))
            throw new InvalidDataException($"Enemy frame {frame} is both an ordinary and an extended frame.");
    }
}
