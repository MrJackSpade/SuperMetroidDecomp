using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// The enemy sheet frames that are only pieces of a body (#1513), read from the placements catalog the draw path reads.
/// The enemy draw list publishes an extended frame as one draw per component placement, every one from the enemy's one
/// slot in the same update, each at the enemy's origin plus that component's fixed offset, and names each draw's sheet
/// frame <see cref="EnemySheetDefinitions.ComponentFrameName"/>. A frame placed in more than one piece is therefore never
/// drawn whole: its pieces move together and none is drawn alone, so each piece's sheet frame is a part image, and the
/// slot publishes more than one frame for one moving part in the update it shows the frame.
/// </summary>
internal static class ExtendedFrameParts
{
    /// <summary>The marker <see cref="FrameArtworkVariation.PictureName"/> starts each sequence step's suffix with.</summary>
    private const char StepSuffix = '|';

    /// <summary>The marker the rot picture name of a frame drawn over a rotting corpse's rows starts its suffix with.</summary>
    private const string RotSuffix = "@rot";

    /// <summary>
    /// Each piece's sheet frame name, of every enemy frame with an installed sheet key that <paramref name="placements"/>
    /// displays in more than one piece.
    /// </summary>
    internal static IReadOnlyDictionary<string, ExtendedFramePart> Of(EnemyExtendedFramePlacementCatalog placements)
    {
        ArgumentNullException.ThrowIfNull(placements);
        var parts = new Dictionary<string, ExtendedFramePart>(StringComparer.Ordinal);
        // Only a frame with an installed composition has a sheet key, so only such a frame can name a sheet frame.
        foreach (EnemyFrameId frame in EnemySpritemapCompositions.All.Concat(EnemyExtendedFrameCompositions.All).Select(composition => composition.Frame))
        {
            if (!placements.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedComponentPlacement> components) || components.Length < 2)
                continue;
            for (int component = 0; component < components.Length; component++)
                parts.Add(EnemySheetDefinitions.ComponentFrameName(frame, component), new ExtendedFramePart(frame, component, components.Length));
        }
        return parts;
    }

    /// <summary>
    /// The sheet frame <paramref name="picture"/> pictures: the name itself, or for a picture of a frame at an encounter
    /// artwork step or over a corpse's rot, the frame's name before that picture's suffix.
    /// </summary>
    internal static string PicturedFrame(string picture)
    {
        ArgumentException.ThrowIfNullOrEmpty(picture);
        int step = picture.IndexOf(StepSuffix, StringComparison.Ordinal);
        int rot = picture.IndexOf(RotSuffix, StringComparison.Ordinal);
        int end = step < 0 ? rot : rot < 0 ? step : Math.Min(step, rot);
        return end < 0 ? picture : picture[..end];
    }
}
