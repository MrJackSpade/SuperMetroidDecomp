using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of Samus's suit explosion (#1392): each frame's spritemap, facing right and left, over the death
/// characters, coloured as Samus's other sheets are; the death palette recolours it through its palette map.
/// </summary>
internal static class SamusDeathSheetComposer
{
    /// <summary>What the death sheet composes.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs)
    {
        Ensure.NotNull(inputs);
        InstalledSamusBodyArtwork body = inputs.SamusBody.Body;
        var frames = new List<SheetFrameSource>();
        foreach (HorizontalDirection facing in SamusDeathSheetDefinitions.Facings)
        {
            for (int frame = 0; frame < SamusDeathSheetDefinitions.ExplosionFrameCount; frame++)
            {
                string name = SamusDeathSheetDefinitions.ExplosionFrame(facing, frame);
                // A zero entry in the installed pointer table names direct-page scratch the port does not model and
                // draws nothing (owner decision on #1318), so its frame is empty.
                frames.Add(new(name, body.Spritemaps.Source.TryGetFrame(ExplosionSpritemap(facing, frame), out SpriteFrame? spritemap)
                    ? spritemap : new SpriteFrame(name, []), inputs.DeathTiles.Explosion));
            }
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }

    /// <summary>The Samus spritemap the explosion's frame <paramref name="frame"/> shows facing <paramref name="facing"/>.</summary>
    internal static int ExplosionSpritemap(HorizontalDirection facing, int frame) =>
        (facing == HorizontalDirection.Left ? SamusDeathSequenceDefinitions.LeftExplosionSpritemap
            : SamusDeathSequenceDefinitions.RightExplosionSpritemap) + frame;
}
