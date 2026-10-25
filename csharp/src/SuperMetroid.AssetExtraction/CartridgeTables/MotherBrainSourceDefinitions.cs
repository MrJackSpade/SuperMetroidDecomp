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
