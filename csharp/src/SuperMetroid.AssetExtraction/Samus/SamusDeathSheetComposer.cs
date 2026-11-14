using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of Samus's suit explosion (#1392): each frame's spritemap, facing right and left, over the death
/// characters, coloured as Samus's other sheets are; the death colours recolour it through its shades.
/// </summary>
internal static class SamusDeathSheetComposer
{
    /// <summary>Right-facing base index in Samus's death-explosion spritemap table.</summary>
    public const int RightExplosionSpritemap = 2076;

    /// <summary>Left-facing base index in Samus's death-explosion spritemap table.</summary>
    public const int LeftExplosionSpritemap = 2085;

    /// <summary>Every facing the explosion is drawn in.</summary>
    internal static IReadOnlyList<HorizontalDirection> Facings { get; } = [HorizontalDirection.Right, HorizontalDirection.Left];

    /// <summary>The explosion frames each facing shows.</summary>
    internal const int ExplosionFrameCount = SamusDeathExplosionTimingDefinitions.StepCount;

    /// <summary>Every frame the sheet holds.</summary>
    internal static IEnumerable<string> Frames =>
        Facings.SelectMany(facing => Enumerable.Range(0, ExplosionFrameCount).Select(frame => SamusDeathSheetDefinitions.ExplosionFrame(facing, frame)));

    /// <summary>What the death sheet composes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        SamusBodySources body = inputs.SamusBody.Body;
        var frames = new List<SheetFrameSource>();
        foreach (HorizontalDirection facing in Facings)
        {
            for (int frame = 0; frame < ExplosionFrameCount; frame++)
            {
                string name = SamusDeathSheetDefinitions.ExplosionFrame(facing, frame);
                // A zero entry in the installed pointer table names direct-page scratch the port does not model and
                // draws nothing (owner decision on #1318), so its frame is empty.
                frames.Add(new(name, body.Spritemaps.TryGetFrame(ExplosionSpritemap(facing, frame), out SpriteFrame? spritemap)
                    ? spritemap : new SpriteFrame(name, []), inputs.DeathTiles.Explosion));
            }
        }
        return new SpriteSheetSources(frames, ExplosionColours(inputs), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades);
    }

    /// <summary>
    /// The colours the explosion is drawn in (#1513): Samus's suited colours, with her body without the suit in the
    /// colours of the first death step. The explosion loads that step's suited and suitless colours before its first frame
    /// and a later step's before each later frame, so the suitless colours always hold a death step's while it draws; the
    /// other steps show through the sheet's shades.
    /// </summary>
    private static RgbColor[] ExplosionColours(SamusSheetInputs inputs)
    {
        RgbColor[] colours = inputs.SuitedColours();
        for (int colour = 0; colour < SamusSheetInks.GroupColours; colour++)
            colours[SamusSheetInks.Suitless + colour] = inputs.ShadeSources.Death.SuitlessColor(0, colour);
        return colours;
    }

    /// <summary>The Samus spritemap the explosion's frame <paramref name="frame"/> shows facing <paramref name="facing"/>.</summary>
    internal static int ExplosionSpritemap(HorizontalDirection facing, int frame) =>
        (facing == HorizontalDirection.Left ? LeftExplosionSpritemap
            : RightExplosionSpritemap) + frame;
}
