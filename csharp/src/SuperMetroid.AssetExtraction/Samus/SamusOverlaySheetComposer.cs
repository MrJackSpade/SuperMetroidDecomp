using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Drawing;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of single sprites drawn over Samus's body (#1392): each arm-cannon cover over the character its
/// direction and frame upload, and the chest cover over the standard OBJ sheet, coloured as Samus's body sheets are.
/// </summary>
internal static class SamusOverlaySheetComposer
{
    /// <summary>What the overlay sheet composes with the body's arm cannon.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs) => Sources(inputs, Ensure.NotNull(inputs).SamusBody.ArmCannon);

    /// <summary>What the overlay sheet composes with <paramref name="cannon"/>.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SamusArmCannonArtworkCatalog cannon)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(cannon);
        var frames = new List<SheetFrameSource>();
        for (int direction = 0; direction < SamusDrawingDefinitions.ArmCannon.DirectionCount; direction++)
        {
            // `$90:C7DF` uploads the frame's cover to the character the direction's attributes name.
            SpritePart cover = cannon.CoverAppearance(direction);
            for (int frame = SamusOverlaySheetDefinitions.FirstCoverFrame; frame <= SamusOverlaySheetDefinitions.LastCoverFrame; frame++)
            {
                frames.Add(new(SamusOverlaySheetDefinitions.ArmCannonFrame(direction, frame),
                    SpritePartFrames.SmallObject(cover, cover.Character), cannon.CoverTile(direction, frame)));
            }
        }
        frames.Add(new(SamusOverlaySheetDefinitions.ChestCoverFrame,
            SpritePartFrames.SmallObject(SamusOverlaySheetDefinitions.ChestCoverAppearance, 0), inputs.StandardObjects));
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.Sizes(GameplayRenderDefinitions.ObjectSelection));
    }
}
