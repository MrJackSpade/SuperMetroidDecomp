namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Development-tool cartridge banks of the enemy composition families, for oracles that read compositions from the
/// cartridge; never linked by player hosts.
/// </summary>
internal static class EnemyCompositionBanksTooling
{
    /// <summary>The ordinary composition families' banks.</summary>
    extension(EnemySpritemapDefinitions)
    {
        /// <summary>Bank $A2 containing Boyon's native spritemap pointers.</summary>
        internal static byte BoyonBank => 0xa2;

        /// <summary>Bank $A2 used to resolve ordinary Rio visual frames.</summary>
        internal static byte RioBank => 0xa2;

        /// <summary>Bank $A2 used to resolve Lower Norfair Rio frames.</summary>
        internal static byte LowerNorfairRioBank => 0xa2;

        /// <summary>Bank $A2 used to resolve Norfair Rio frames.</summary>
        internal static byte NorfairRioBank => 0xa2;

        /// <summary>Bank $A2 containing Puyo composition pointers.</summary>
        internal static byte PuyoBank => 0xa2;

        /// <summary>Bank $A8 containing Bull composition pointers.</summary>
        internal static byte BullBank => 0xa8;

        /// <summary>Bank $A8 containing Alcoon composition pointers.</summary>
        internal static byte AlcoonBank => 0xa8;

        /// <summary>Bank $A8 containing Beetom composition pointers.</summary>
        internal static byte BeetomBank => 0xa8;

        /// <summary>Bank $A3 containing Hopper composition pointers.</summary>
        internal static byte HopperBank => 0xa3;

        /// <summary>Bank $A2 containing Choot composition pointers.</summary>
        internal static byte ChootBank => 0xa2;

        /// <summary>Bank $A3 containing horizontal Zoomer composition pointers.</summary>
        internal static byte HZoomerBank => 0xa3;

        /// <summary>Bank $A3 containing Sbug composition pointers.</summary>
        internal static byte SbugBank => 0xa3;

        /// <summary>Bank $A8 containing Fune and Namihe composition pointers.</summary>
        internal static byte FuneNamiheBank => 0xa8;

        /// <summary>Bank $A2 containing Kamer platform composition pointers.</summary>
        internal static byte KamerPlatformBank => 0xa2;

        /// <summary>Bank $A3 containing elevator composition pointers.</summary>
        internal static byte ElevatorBank => 0xa3;

        /// <summary>Bank $B4 containing room sprite-object composition pointers.</summary>
        internal static byte RoomSpriteObjectBank => 0xb4;

        /// <summary>Bank $A3 containing Skultera composition pointers.</summary>
        internal static byte SkulteraBank => 0xa3;

        /// <summary>Bank $A3 containing Waver composition pointers.</summary>
        internal static byte WaverBank => 0xa3;

        /// <summary>Bank $A3 containing Zoa composition pointers.</summary>
        internal static byte ZoaBank => 0xa3;

        /// <summary>Bank $A3 containing Skree and Metaree composition pointers.</summary>
        internal static byte SkreeMetareeBank => 0xa3;

        /// <summary>Bank $B3 containing pipe-bug composition pointers.</summary>
        internal static byte PipeBugBank => 0xb3;

        /// <summary>Bank $A6 containing Fake Kraid composition pointers.</summary>
        internal static byte FakeKraidBank => 0xa6;

        /// <summary>Bank $A7 containing Kraid nail composition pointers.</summary>
        internal static byte KraidNailBank => 0xa7;

        /// <summary>Bank $A2 containing Owtch and Stoke composition pointers.</summary>
        internal static byte OwtchStokeBank => 0xa2;

        /// <summary>Bank $A2 containing Ripper composition pointers.</summary>
        internal static byte RipperBank => 0xa2;

        /// <summary>Bank $A3 containing Fireflea composition pointers.</summary>
        internal static byte FirefleaBank => 0xa3;

        /// <summary>Bank $A8 containing Magdollite composition pointers.</summary>
        internal static byte MagdolliteBank => 0xa8;

        /// <summary>Bank $A6 containing boulder composition pointers.</summary>
        internal static byte BoulderBank => 0xa6;

        /// <summary>Bank $A8 containing Atomic composition pointers.</summary>
        internal static byte AtomicBank => 0xa8;

        /// <summary>Two-byte part count followed by five bytes per native OAM part.</summary>
        internal static int NativeFrameBytes(int parts) => 2 + 5 * parts;
    }

    /// <summary>The extended composition families' banks.</summary>
    extension(EnemyExtendedFrameDefinitions)
    {
        /// <summary>Native instruction bank for the initial Pirate frame family.</summary>
        internal static byte Bank => 0xb2;
    }
}
