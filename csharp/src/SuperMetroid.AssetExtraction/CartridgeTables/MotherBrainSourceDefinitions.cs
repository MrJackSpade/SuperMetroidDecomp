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
        internal static int FadeColors(int paletteIndex) =>
            FirstFadeColors + (paletteIndex - 1) * BabyMetroidCutsceneColorDefinitions.FadeColorCount * ColorWordBytes;
    }

    /// <summary>Mother Brain's bank-$AD colour images and the pointer tables that order them.</summary>
    internal static class Palettes
    {
        /// <summary>The bank every Mother Brain palette pointer table's bank-local pointers resolve in.</summary>
        internal const int PaletteBank = 0xad0000;

        /// <summary>The bytes of one bank-local pointer in a palette pointer table.</summary>
        internal const int PointerBytes = sizeof(ushort);

        /// <summary>$AD:E434, the zero-terminated rainbow image pointers the rainbow handler at $A9:BCFD consumes.</summary>
        internal const int RainbowPointerTable = 0xade434;

        /// <summary>$A9:9474, the normal body and brain colours (after the transparent colour) the rainbow's end restores.</summary>
        internal const int NormalBrainColors = 0xa99474;

        /// <summary>$A9:9494, the normal rear-leg colours the rainbow's end restores; phase two's setup also installs them.</summary>
        internal const int NormalRearLegColors = 0xa99494;

        /// <summary>$AD:EF87, the eight drained-to-grey image pointers followed by zero.</summary>
        internal const int DrainedToGreyPointerTable = 0xadef87;

        /// <summary>$AD:ED9C, the eight revival (grey-to-colour) image pointers followed by zero; the fake-death resurrection reuses them.</summary>
        internal const int RevivalFromGreyPointerTable = 0xaded9c;

        /// <summary>$AD:ED8A, the eight fake-death brain grey-fade pointers followed by zero.</summary>
        internal const int FakeDeathToGreyPointerTable = 0xaded8a;

        /// <summary>$AD:E6A2, the four health-band body and brain image pointers.</summary>
        internal const int HealthBodyPointerTable = 0xade6a2;

        /// <summary>$AD:E742, the four health-band rear-leg image pointers.</summary>
        internal const int HealthRearLegPointerTable = 0xade742;
    }

    /// <summary>Mother Brain's room, attack, room-entry and room-light colour images.</summary>
    internal static class RoomColors
    {
        /// <summary>The bank of the room-flash list at $A9:D046 and the bank-local palette pointers it names.</summary>
        internal const int FlashBank = 0xa90000;

        /// <summary>$A9:94B4, phase two's fifteen attack colours.</summary>
        internal const int PhaseTwoAttackColors = 0xa994b4;

        /// <summary>$A9:9494, phase two's fifteen rear-leg colours: the normal rear-leg image the rainbow also restores.</summary>
        internal const int PhaseTwoRearLegColors = Palettes.NormalRearLegColors;

        /// <summary>$A9:9514, the room-entry glass-shard colours.</summary>
        internal const int InitialGlassShardColors = 0xa99514;

        /// <summary>$A9:94F4, the room-entry tube-projectile colours.</summary>
        internal const int InitialTubeProjectileColors = 0xa994f4;

        /// <summary>$AD:F3D3, the first room-light image shown; the following images sit at descending addresses.</summary>
        internal const int RecoveryLightsFirstColors = 0xadf3d3;

        /// <summary>The bytes of one room-light image, two fourteen-colour slices: the stride between images, descending.</summary>
        internal const int RecoveryLightsImageBytes = 2 * MotherBrainRoomColorDefinitions.RecoveryLightsSliceColors * ColorWordBytes;

        /// <summary>The source of room-light image <paramref name="index"/> (0..6): $AD:F3D3 minus the index's images.</summary>
        internal static int RecoveryLightsColors(int index) => RecoveryLightsFirstColors - index * RecoveryLightsImageBytes;
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
