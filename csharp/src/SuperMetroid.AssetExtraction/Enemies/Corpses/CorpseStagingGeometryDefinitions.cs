using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>
/// Bounded, pinned bank-$A9 corpse staging geometry (#1406): where the cartridge copies a dead Sidehopper's characters into
/// its work memory, the columns its pixel movers walk, the character rows each species' corpse lays out, and where the dead
/// Torizo's sand falls. Only the extraction reads it, to rot the staged characters and picture each rot step.
/// </summary>
/// <remarks>No tile pixels or cartridge byte decoder are stored here.</remarks>
internal static class CorpseStagingGeometryDefinitions
{
    /// <summary>
    /// Five tile rows copied by CorpseRottingInit_Sidehopper_Param1_0/2 at $A9:DEC1/DF08.
    /// The native atlas is sixteen tiles wide; each staged silhouette is five tiles wide.
    /// The second silhouette starts after five staged rows, nine atlas tiles to the right.
    /// </summary>
    internal static CorpseGraphicsCopy SidehopperInitialCopy(int variant, int row)
    {
        if ((uint)row >= 5) throw new ArgumentOutOfRangeException(nameof(row));
        int silhouette = variant == 0 ? 0 : 1;
        int leftClip = row == 0 && silhouette == 0 ? 2 : 0;
        int width = row == 0 ? (silhouette == 0 ? 3 : 2) : 5;
        return new((silhouette * 9 + row * 16 + leftClip) * 32,
            (silhouette * 25 + row * 5 + leftClip) * 32, width * 32);
    }

    /// <summary>Planes-0/1 tile-column words used by the $A9:E468/E564 Sidehopper pixel movers.</summary>
    internal static int SidehopperColumnWordOffset(int variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return ((variant == 0 ? 0 : 25) + column) * 16;
    }

    /// <summary>
    /// Missing first-row tiles in the two sidehopper silhouettes: first two columns in
    /// variant zero, last three in the alternate variant. Native movers $A9:E468/E564
    /// and copiers $A9:E4F5/E5F6 skip these columns until pixel row eight.
    /// </summary>
    internal static int SidehopperColumnMinimumY(int variant, int column)
    {
        if ((uint)column >= 5) throw new ArgumentOutOfRangeException(nameof(column));
        return (variant == 0 ? column < 2 : column >= 2) ? 8 : 0;
    }

    /// <summary>
    /// The bytes a pixel row's two-row fall from a character's last two rows carries back into the next character row,
    /// on top of the row of characters it skips: the planes-0/1 words of the character's six rows above them.
    /// </summary>
    internal const int WrapCarryBack = 12;

    /// <summary>The rows of characters <paramref name="species"/>'s staged corpse lays out and the characters in each.</summary>
    internal static CorpseRotationExtent RotationExtent(RottingCorpseSpecies species) => species switch
        {
            RottingCorpseSpecies.Torizo => new CorpseRotationExtent(13, 10),
            RottingCorpseSpecies.Sidehopper => new CorpseRotationExtent(6, 5),
            RottingCorpseSpecies.Zoomer or RottingCorpseSpecies.Ripper => new CorpseRotationExtent(3, 3),
            RottingCorpseSpecies.Skree => new CorpseRotationExtent(5, 2),
            _ => throw new InvalidOperationException($"Undefined RottingCorpseSpecies {species}."),
        };

    /// <summary>$A9:D67C: eight words per sand strip, second strip displaced by$120 staging bytes.</summary>
    internal static ushort SandDestination(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x120 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));

    /// <summary>$A9:D69C: matching eight-word source strips are separated by$200 bytes.</summary>
    internal static ushort SandSource(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x200 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
}
