using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Installed RGB5 animation frames independent of cartridge storage and enemy mechanics.</summary>
public sealed class EnemyAuxiliaryColorCatalog
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => EnemyAuxiliaryColors.ContentIdentity(this);

    /// <summary>
    /// Each family's colour frames: the mirrored eight-frame face-block glow sequence, the Sidehopper drain stages with its
    /// separate corpse colors, and Golden Torizo's eight body and eight rear-belly health bands.
    /// </summary>
    internal readonly IReadOnlyDictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryColourFrames> families;

    /// <summary>Recognizes each supplied family's stock paint relationship while retaining unmatched edits.</summary>
    internal EnemyAuxiliaryColorCatalog(Dictionary<EnemyAuxiliaryPalette, RgbColor[][]> frames) =>
        families = new Dictionary<EnemyAuxiliaryPalette, EnemyAuxiliaryColourFrames>
        {
            [EnemyAuxiliaryPalette.FaceBlock] = new(frames[EnemyAuxiliaryPalette.FaceBlock], AuxiliaryPaint.FaceGlow),
            [EnemyAuxiliaryPalette.DeadSidehopper] = new(frames[EnemyAuxiliaryPalette.DeadSidehopper], AuxiliaryPaint.SidehopperDrain),
            [EnemyAuxiliaryPalette.GoldenTorizoBody] = new(frames[EnemyAuxiliaryPalette.GoldenTorizoBody], AuxiliaryPaint.TorizoFront),
            [EnemyAuxiliaryPalette.GoldenTorizoBelly] = new(frames[EnemyAuxiliaryPalette.GoldenTorizoBelly], AuxiliaryPaint.TorizoRear),
        };

    /// <summary>Resolves one selected RGB5 word from the face-block glow, sidehopper drain/corpse, or Golden Torizo health rows without advancing timers, choosing health bands, or writing CGRAM.</summary>
    /// <param name="palette">One of the four installed auxiliary-palette identities.</param>
    /// <param name="frame">Zero-based frame: 0..7 for the face-block glow or Torizo health bands, or 0..6 for sidehopper stages; frames are not necessarily elapsed animation frames.</param>
    /// <param name="color">Zero-based supplied color: 0..3 for face-block, 0..14 for sidehopper, or 0..15 for Torizo; sidehopper entries omit the native transparent color zero.</param>
    /// <returns>The packed RGB5 word preserving independently supplied edits.</returns>
    public RgbColor Resolve(EnemyAuxiliaryPalette palette, int frame, int color) => EnemyAuxiliaryColors.Resolve(this, palette, frame, color);
}
