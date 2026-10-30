using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using static SuperMetroid.AssetExtraction.Frontend.Pause.Artwork.PauseEquipmentLabelArtworkDefinitions;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The stock tiles of the equipment labels (<see cref="PauseEquipmentLabelArtworkDefinitions"/>), which the import pictures
/// each label from (#1467).
/// </summary>
internal static class PauseEquipmentLabelGlyphs
{
    /// <summary>Assembles named equipment text from its packed atlas runs, shared suffixes and padding.</summary>
    /// <remarks>The referenced glyph pixels remain independently authored interface artwork.</remarks>
    internal static BackgroundTile StockTile(PauseEquipmentLabelId label, int index)
    {
        if ((uint)index >= PauseEquipmentLabelTiles.StockWordCount(label)) throw new ArgumentOutOfRangeException(nameof(index));
        if (index == 0) return new BackgroundTile(Marker, LabelPalette, highPriority: false);
        int text = index - 1;
        int glyph = label switch
        {
            PauseEquipmentLabelId.Charge => text < 3 ? ChargePrefix + text : ChargeEnding,
            PauseEquipmentLabelId.Ice => Run(IceText, 2, text),
            PauseEquipmentLabelId.Wave => Run(WaveText, 3, text),
            PauseEquipmentLabelId.Spazer => SpazerText + text,
            PauseEquipmentLabelId.Plasma => PlasmaText + text,
            PauseEquipmentLabelId.Varia => text < 3 ? VariaText + text : Run(SuitText, 3, text - 3),
            PauseEquipmentLabelId.Gravity => text < 4 ? GravityText + text : Run(SuitText, 3, text - 4),
            PauseEquipmentLabelId.MorphBall => text < 4 ? MorphingPrefix + text
                : text < 6 ? MorphingBallMiddle + text - 4 : text == 6 ? BallMiddle : BallEnding,
            PauseEquipmentLabelId.Bombs => Run(BombsText, 3, text),
            PauseEquipmentLabelId.SpringBall => Run(SpringBallText, 7, text),
            PauseEquipmentLabelId.ScrewAttack => Run(ScrewAttackText, 7, text),
            PauseEquipmentLabelId.HiJump => Run(HiJumpText, 7, text),
            PauseEquipmentLabelId.SpaceJump => Run(SpaceJumpText, 6, text),
            PauseEquipmentLabelId.SpeedBooster => SpeedBoosterText + text,
            PauseEquipmentLabelId.Hyper => text < 3 ? HyperText + text : text == 3 ? HyperEndcap : Padding,
            _ => throw new ArgumentOutOfRangeException(nameof(label)),
        };
        return new BackgroundTile(glyph, LabelPalette, highPriority: false);
    }

    /// <summary>Advances through a contiguous glyph run, then pads the remaining strip with the native background tile.</summary>
    private static int Run(int first, int length, int index) => index < length ? first + index : Padding;
}
