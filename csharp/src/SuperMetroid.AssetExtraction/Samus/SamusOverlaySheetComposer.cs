using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Composes the sheet of single sprites drawn over Samus's body (#1392): each arm-cannon cover over the character its
/// direction and frame upload, coloured as Samus's body sheet is. The chest cover is composed into the forward-facing
/// body's own frames (#1513).
/// </summary>
internal static class SamusOverlaySheetComposer
{
    /// <summary>The cover frames an open or opening cannon shows; the closed frame 0 draws nothing.</summary>
    internal const int FirstCoverFrame = 1, LastCoverFrame = 3;

    /// <summary>Every frame the sheet holds.</summary>
    internal static IEnumerable<string> Frames =>
        SamusProjectileDirectionCodec.InAimOrder
            .SelectMany(direction => Enumerable.Range(FirstCoverFrame, LastCoverFrame - FirstCoverFrame + 1)
                .Select(frame => SamusOverlaySheetDefinitions.ArmCannonFrame(direction, frame)));

    /// <summary>What the overlay sheet composes with the body's arm cannon.</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs) => Sources(inputs, Ensure.NotNull(inputs).ArmCannonCover);

    /// <summary>What the overlay sheet composes with the cover characters <paramref name="cover"/> (#1636).</summary>
    internal static SpriteSheetSources Sources(SamusSheetInputs inputs, SamusArmCannonCoverCharacters cover)
    {
        Ensure.NotNull(inputs);
        Ensure.NotNull(cover);
        SamusArmCannonSourceArtwork cannon = cover.Cannon;
        var frames = new List<SheetFrameSource>();
        foreach (SamusProjectileDirection direction in SamusProjectileDirectionCodec.InAimOrder)
        {
            // `$90:C7DF` uploads the frame's cover to the character the direction's attributes name.
            int row = direction.ToAimIndex();
            SpritePart appearance = cannon.CoverAppearance(row);
            for (int frame = FirstCoverFrame; frame <= LastCoverFrame; frame++)
            {
                frames.Add(new(SamusOverlaySheetDefinitions.ArmCannonFrame(direction, frame),
                    SpritePartFrames.SmallObject(appearance, appearance.Character), cover.CoverTile(row, frame)));
            }
        }
        return new SpriteSheetSources(frames, inputs.SuitedColours(), ObjectSelectionCodec.GameplaySizes, Shades: inputs.Shades);
    }
}
