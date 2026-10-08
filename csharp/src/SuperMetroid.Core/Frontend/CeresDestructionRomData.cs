using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge assets and fixed layout for Ceres destruction and Zebes approach.</summary>
public static class CeresDestructionRomData
{
    /// <summary>$8B:C11B: <c>LDX #8</c> ... <c>DEX : BPL</c> around WaitForNMI, before scene setup.</summary>
    public const int InitialNmiWaits = 9;

    /// <summary>Native palette-effect instruction-list identities used by the sequence.</summary>
    public static class PaletteFx
    {
        /// <summary>$8D:E1A8, PaletteFXObjects_CutsceneGunshipEngineFlicker;
        /// spawned by $8B:C784 during Zebes approach setup.</summary>
        public const ushort EngineFlicker = 0xe1a8;
    }

    /// <summary>Cartridge addresses of compressed Ceres and Zebes artwork.</summary>
    public static class Assets
    {
        /// <summary>SNES address of the compressed Ceres scene tilemaps.</summary>
        public const int CeresTilemaps = 0x96fe69;
        /// <summary>SNES address of the compressed Zebes approach tilemap.</summary>
        public const int ZebesTilemap = 0x978adb;
        /// <summary>SNES address of the compressed Zebes character graphics.</summary>
        public const int ZebesCharacters = 0x96ec76;
    }

    /// <summary>Music selectors used by the Ceres destruction scene.</summary>
    public static class Music
    {
        /// <summary>Music-data index loaded for Ceres destruction.</summary>
        public const byte CeresDataIndex = 0x2d;
        /// <summary>$8B:C2D9: state $22's Ceres track (state $25 selects 8 at $8B:C2CE).</summary>
        public const byte CeresTrack = 7;
        /// <summary>Native delayed-music argument used when the Ceres track starts.</summary>
        public const ushort DelayArgument = 0x000e;
    }

    /// <summary>VRAM transfer sizes, destinations, and Mode 7 map layout.</summary>
    public static class Vram
    {
        /// <summary>Byte capacity of the Mode 7 character lane.</summary>
        public const int Mode7CharacterBytes = 0x4000;
        /// <summary>Byte capacity of the Mode 7 map lane.</summary>
        public const int Mode7MapCapacityBytes = 0x4000;
        /// <summary>Maximum decompressed byte count accepted for a scene tilemap.</summary>
        public const int CompressedTilemapLimit = 0x1000;
        /// <summary>Minimum decompressed size required of the combined Ceres tilemaps.</summary>
        public const int CeresMinimumTilemapBytes = 0x0f00;
        /// <summary>Byte offset of the active Ceres scene tilemap in the combined resource.</summary>
        public const int CeresSceneTilemapOffset = 0x0600;
        /// <summary>Byte count copied for the active Ceres scene tilemap.</summary>
        public const int CeresSceneTilemapBytes = 0x0600;
        /// <summary>Byte count of one retained half of the Mode 7 map.</summary>
        public const int MapHalfBytes = 0x0300;
        /// <summary>Source byte offset of the blank map half used to clear the scene.</summary>
        public const int ClearMapSourceOffset = 0x0c00;
        /// <summary>Mode 7 map word destination of the cleared lower half.</summary>
        public const ushort ClearMapDestinationWord = 0x0300;
        /// <summary>VRAM byte destination for shared cinematic OBJ characters.</summary>
        public const int ObjectCharacterDestinationByte = 0xc000;
        /// <summary>Byte count of the shared cinematic OBJ character transfer.</summary>
        public const int SharedObjectCharacterBytes = 0x1a00;
        /// <summary>Initial character index written across the Mode 7 map.</summary>
        public const byte InitialMode7MapCharacter = 0x8c;
        /// <summary>Minimum decompressed byte count required of the Zebes tilemap.</summary>
        public const int ZebesTilemapMinimumBytes = 0x0800;
        /// <summary>VRAM byte destination for the Zebes Mode 1 tilemap.</summary>
        public const int ZebesTilemapDestinationByte = 0xb800;
    }

    /// <summary>Mode 7 centers and Mode 1 layer configuration for the two scenes.</summary>
    public static class Rendering
    {
        /// <summary>M7X center used by the Ceres explosion composition before its fade-out.</summary>
        public const short CeresCenterX = 52;
        /// <summary>M7Y center used by the Ceres explosion composition before its fade-out.</summary>
        public const short CeresCenterY = 48;
        /// <summary>M7X center used by the following Zebes approach composition.</summary>
        public const short ZebesCenterX = 56;
        /// <summary>M7Y center used by the following Zebes approach composition.</summary>
        public const short ZebesCenterY = 24;
        /// <summary>VRAM word base of the Mode 1 Zebes tilemap.</summary>
        public const ushort Mode1TilemapWord = 0x5c00;
        /// <summary>VRAM word base of the Mode 1 Zebes character graphics.</summary>
        public const ushort Mode1CharacterWord = 0x6000;
        /// <summary>Mask that wraps cinematic world X coordinates to nine bits.</summary>
        public const ushort WorldXMask = 0x01ff;
    }

    /// <summary>Authored holds and mosaic fade parameters measured in gameplay updates.</summary>
    public static class Timing
    {
        /// <summary>Updates that the completed Ceres explosion remains visible.</summary>
        public const ushort ExplosionHoldFrames = 0x00c0;
        /// <summary>Updates that the completed Zebes approach remains visible.</summary>
        public const ushort ZebesHoldFrames = 0x0040;
        /// <summary>Initial MOSAIC register value enabling a size-eight effect on BG1.</summary>
        public const byte InitialMosaicRegister = 0x81;
        /// <summary>MOSAIC register increment applied by each fade step.</summary>
        public const byte MosaicFadeStep = 0x10;
        /// <summary>Mask selecting the mosaic-size field.</summary>
        public const byte MosaicSizeMask = 0xf0;
    }

    /// <summary>Mode 7 scale thresholds and fixed-point scene velocities.</summary>
    public static class Motion
    {
        /// <summary>Mode 7 identity scale in native 8.8 units.</summary>
        public const int IdentityScale = 0x0100;
        /// <summary>Scale threshold that ends the initial Ceres zoom.</summary>
        public const int CeresZoomLimit = 0x0280;
        /// <summary>Scale used when the Ceres explosion composition begins.</summary>
        public const int CeresExplosionScale = 0x0300;
        /// <summary>Minimum nonzero Mode 7 scale retained during the sequence.</summary>
        public const int MinimumScale = 0x0010;
        /// <summary>Scale threshold that ends the near Zebes approach.</summary>
        public const int ZebesApproachScaleLimit = 0x0480;
        /// <summary>Scale threshold separating the far Zebes approach phase.</summary>
        public const int FarZebesScaleLimit = 0x2000;
        /// <summary>Slow per-update change in native 8.8 scale units.</summary>
        public const int SlowScaleStep = 0x0010;
        /// <summary>Fast per-update change in native 8.8 scale units.</summary>
        public const int FastScaleStep = 0x0020;
        /// <summary>Positive one-eighth-pixel velocity in signed 16.16 units.</summary>
        public const int EighthPixel16Point16 = 0x0000_2000;
        /// <summary>Negative one-half-pixel velocity in signed 16.16 units.</summary>
        public const int NegativeHalfPixel16Point16 = unchecked((int)0xffff_8000);
        /// <summary>Positive one-sixteenth-pixel velocity in signed 16.16 units.</summary>
        public const int SixteenthPixel16Point16 = 0x0000_1000;
        /// <summary>Negative one-quarter-pixel velocity in signed 16.16 units.</summary>
        public const int NegativeQuarterPixel16Point16 = unchecked((int)0xffff_c000);
        /// <summary>Fractional horizontal velocity word assigned to the gunship.</summary>
        public const ushort GunshipXSubvelocity = 0x4000;
        /// <summary>Signed integer vertical velocity word assigned to the gunship.</summary>
        public const ushort GunshipYVelocity = 0xffff;
        /// <summary>Fractional vertical velocity word assigned to the gunship.</summary>
        public const ushort GunshipYSubvelocity = 0xf000;
        /// <summary>Mode 7 X position that completes the gunship exit.</summary>
        public const ushort Mode7ExitX = 0x003e;
        /// <summary>Gets the native table-index angle used during the Zebes approach.</summary>
        public static SnesAngle ApproachAngle => SnesAngle.FromTableIndex(0x20);
    }

    /// <summary>Reserved cinematic sprite slots, palettes, and initial coordinates.</summary>
    public static class Sprites
    {
        /// <summary>$8B:C27F: first automatic allocation owns native byte index $1E (slot 15).</summary>
        public const int AsteroidSlot = 15;
        /// <summary>$8B:C285/C28A: explicit byte index $02 reserves slot 1 for small asteroids.</summary>
        public const int SmallAsteroidSlot = 1;
        /// <summary>$8B:C290/C295: explicit byte index $00 reserves slot 0 for the vortex.</summary>
        public const int VortexSlot = 0;
        /// <summary>$8B:C29B: second automatic allocation reserves slot 14 for invisible CF33.</summary>
        public const int SpawnerSlot = 14;
        /// <summary>OBJ palette used by the Ceres explosion sprites.</summary>
        public static readonly SnesObjAttributeWord ExplosionPalette = SnesObjPalettes.Index5;
        /// <summary>Initial horizontal background coordinate of the Zebes approach.</summary>
        public const ushort ZebesInitialBackgroundX = 0x0080;
    }
}
