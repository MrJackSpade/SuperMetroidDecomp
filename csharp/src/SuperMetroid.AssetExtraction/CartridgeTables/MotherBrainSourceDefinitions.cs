using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where the cartridge keeps the Tourian encounter's tables extraction reads (#1413): Mother Brain, the Baby Metroid and
/// the Shitroid. The import pipeline's private knowledge of the cartridge, decoded once into the installed colour
/// documents and definitions.
/// </summary>
[CpuBusBoundary("catalog: Mother Brain encounter cartridge sources")]
internal static class MotherBrainSourceDefinitions
{
    /// <summary>The bytes of one RGB5 colour word.</summary>
    internal const int ColorWordBytes = sizeof(ushort);

    /// <summary>The cutscene Baby Metroid's colour images.</summary>
    internal static class BabyMetroid
    {
        /// <summary>$A9:94D4, colours 1..15 of the Baby's initial image.</summary>
        internal const int InitialColors = 0xa994d4;

        /// <summary>$AD:E8E2, the seven fade-image pointers; index zero is never displayed.</summary>
        internal const int FadePointerTable = 0xade8e2;

        /// <summary>$AD:E90C, the first displayed fourteen-colour fade-to-black image; the six images follow in order.</summary>
        internal const int FirstFadeColors = 0xade90c;

        /// <summary>
        /// The source of displayed fade image <paramref name="paletteIndex"/> (one-based, 1..6), from $AD:E90C through
        /// $AD:E998 in fourteen-colour strides; the last image is entirely black.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The fade index is outside 1..6.</exception>
        internal static int FadeColors(int paletteIndex) =>
            paletteIndex is >= 1 and <= BabyMetroidCutsceneColorDefinitions.FadeFrameCount
                ? FirstFadeColors + (paletteIndex - 1) *
                    BabyMetroidCutsceneColorDefinitions.FadeColorCount * ColorWordBytes
                : throw new ArgumentOutOfRangeException(nameof(paletteIndex));
    }

    /// <summary>The Shitroid's bank-$A9 colour images.</summary>
    internal static class Shitroid
    {
        /// <summary>$A9:F6D1..$A9:F710, the eight consecutive four-colour innard-pulse rows.</summary>
        internal const int NormalCycle = 0xa9f6d1;

        /// <summary>$A9:F8C6, the sixteen-colour target image for the sidehopper victim.</summary>
        internal const int SidehopperTarget = 0xa9f8c6;

        /// <summary>$A9:F8E6, the sixteen-colour target image for the Shitroid.</summary>
        internal const int ShitroidTarget = 0xa9f8e6;

        /// <summary>$A9:F8A6, the sixteen-colour target image for the dead sidehopper; the first of the three targets.</summary>
        internal const int DeadSidehopperTarget = 0xa9f8a6;
    }
}
