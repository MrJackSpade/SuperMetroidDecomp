using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>
/// Bounded, pinned bank-$A9 corpse staging geometry (#1406): where the cartridge copies a dead Sidehopper's characters into
/// its work memory, and where the dead Torizo's sand falls. Only the staging oracle reads it, which checks the installed
/// artwork stages as the cartridge's does.
/// </summary>
/// <remarks>No tile pixels or cartridge byte decoder are stored here.</remarks>
[EncodingOwner("bank-$A9 corpse work-memory and staging layout")]
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

    /// <summary>$A9:D67C: eight words per sand strip, second strip displaced by$120 staging bytes.</summary>
    internal static ushort SandDestination(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x120 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));

    /// <summary>$A9:D69C: matching eight-word source strips are separated by$200 bytes.</summary>
    internal static ushort SandSource(int line) => (uint)line < 16
        ? (ushort)((line / 8) * 0x200 + (line % 8) * 2) : throw new ArgumentOutOfRangeException(nameof(line));
}
