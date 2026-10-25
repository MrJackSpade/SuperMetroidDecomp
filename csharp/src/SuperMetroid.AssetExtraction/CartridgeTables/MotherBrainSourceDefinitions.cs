using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
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

    /// <summary>Mother Brain's death and escape colour images.</summary>
    internal static class Death
    {
        /// <summary>$AD:E9E8, the body fade pointers followed by a null word.</summary>
        internal const int BodyFadeTable = 0xade9e8;

        /// <summary>$AD:EA0A, the first of sixteen consecutive body-and-leg fade images, each fourteen body then fourteen leg colours.</summary>
        internal const int FirstBodyFadeColors = 0xadea0a;

        /// <summary>$AD:F107, the corpse fade pointers followed by a null word.</summary>
        internal const int CorpseFadeTable = 0xadf107;

        /// <summary>$AD:F119, the first of eight consecutive fifteen-colour corpse-head images.</summary>
        internal const int FirstCorpseFadeColors = 0xadf119;

        /// <summary>$A9:9534, the fourteen colours installed for the exploded escape door.</summary>
        internal const int ExplodedDoorColors = 0xa99534;

        /// <summary>The source of body-and-leg fade image <paramref name="frame"/> (0..15).</summary>
        internal static int BodyFadeColors(int frame) =>
            FirstBodyFadeColors + frame * 2 * MotherBrainDeathDefinitions.BodyColorCount * ColorWordBytes;

        /// <summary>The source of corpse-head fade image <paramref name="frame"/> (0..7).</summary>
        internal static int CorpseFadeColors(int frame) =>
            FirstCorpseFadeColors + frame * MotherBrainDeathDefinitions.CorpseColorCount * ColorWordBytes;
    }

    /// <summary>The rainbow beam's colour cycle.</summary>
    internal static class Beam
    {
        /// <summary>$88:E833, the beam's colour cycle, ending in a signed terminator.</summary>
        internal const int ColorTable = 0x88e833;

        /// <summary>The bytes between consecutive cycle colours: the cartridge's cursor steps four bytes, skipping every other colour word.</summary>
        internal const int ColorEntryBytes = 2 * ColorWordBytes;
    }

    /// <summary>Mother Brain's corpse characters.</summary>
    internal static class Corpse
    {
        /// <summary>$B7:CE00, the tile-aligned source of the corpse sheet, containing the right-hand corpse frame.</summary>
        internal const int Characters = 0xb7ce00;
    }

    /// <summary>The sheets whose pages Mother Brain's encounter shows its sprites.</summary>
    internal static class SpecialSprites
    {
        /// <summary>
        /// Where each sheet's characters begin: $B7:9000 (legs, eight pages), $B1:8800 (Baby Metroid, four), $B7:A000
        /// (attack, four) and $AB:F400 (exploded door, two). The installed manifest names each sheet's hash by its source.
        /// </summary>
        internal static readonly IReadOnlyDictionary<MotherBrainSpritePageSheet, int> Sources =
            new Dictionary<MotherBrainSpritePageSheet, int>
            {
                [MotherBrainSpritePageSheet.Legs] = 0xb79000,
                [MotherBrainSpritePageSheet.BabyMetroid] = 0xb18800,
                [MotherBrainSpritePageSheet.Attack] = 0xb7a000,
                [MotherBrainSpritePageSheet.ExplodedDoor] = 0xabf400,
            };
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
