using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified cartridge assets and fixed layout for Ceres destruction and Zebes approach.</summary>
public static class CeresDestructionRomData
{
    public static class Assets
    {
        public const int Palette = 0x8ce5e9;
        public const int Mode7Characters = 0x95a82f;
        public const int CeresTilemaps = 0x96fe69;
        public const int CeresObjectCharacters = 0x96d10a;
        public const int ZebesTilemap = 0x978adb;
        public const int ZebesCharacters = 0x96ec76;
        public const int SignedSineTable = 0xa0b443;
        public const int SharedObjectCharacters = 0x9ad200;
    }

    public static class Music
    {
        public const byte CeresDataIndex = 0x2d;
        public const byte CeresTrack = 8;
        public const byte ZebesDataIndex = 0x33;
        public const byte ZebesTrack = 5;
        public const ushort DelayArgument = 0x000e;
    }

    public static class Vram
    {
        public const int Mode7CharacterBytes = 0x4000;
        public const int Mode7MapCapacityBytes = 0x4000;
        public const int CompressedTilemapLimit = 0x1000;
        public const int CeresMinimumTilemapBytes = 0x0f00;
        public const int CeresSceneTilemapOffset = 0x0600;
        public const int CeresSceneTilemapBytes = 0x0600;
        public const int MapHalfBytes = 0x0300;
        public const int ClearMapSourceOffset = 0x0c00;
        public const ushort ClearMapDestinationWord = 0x0300;
        public const int ObjectCharacterDestinationByte = 0xc000;
        public const int SharedObjectCharacterBytes = 0x1a00;
        public const byte InitialMode7MapCharacter = 0x8c;
        public const int ZebesTilemapMinimumBytes = 0x0800;
        public const int ZebesTilemapDestinationByte = 0xb800;
    }

    public static class Rendering
    {
        public const ushort Mode1TilemapWord = 0x5c00;
        public const ushort Mode1CharacterWord = 0x6000;
        public const ushort WorldXMask = 0x01ff;
    }

    public static class Timing
    {
        public const int FirstExplosionFrame = 0x0080;
        public const int SecondaryExplosionFirstFrame = 0x0081;
        public const int SecondaryExplosionLastFrame = 0x00d0;
        public const int SecondaryExplosionPeriod = 12;
        public const int FinalExplosionFrame = 0x0110;
        public const ushort ExplosionHoldFrames = 0x00c0;
        public const ushort ZebesHoldFrames = 0x0040;
        public const byte InitialMosaicRegister = 0x81;
        public const byte MosaicFadeStep = 0x10;
        public const byte MosaicSizeMask = 0xf0;
    }

    public static class Motion
    {
        public const int IdentityScale = 0x0100;
        public const int CeresZoomLimit = 0x0280;
        public const int CeresExplosionScale = 0x0300;
        public const int MinimumScale = 0x0010;
        public const int ZebesApproachScaleLimit = 0x0480;
        public const int FarZebesScaleLimit = 0x2000;
        public const int SlowScaleStep = 0x0010;
        public const int FastScaleStep = 0x0020;
        public const int EighthPixel16Point16 = 0x0000_2000;
        public const int NegativeHalfPixel16Point16 = unchecked((int)0xffff_8000);
        public const int SixteenthPixel16Point16 = 0x0000_1000;
        public const int NegativeQuarterPixel16Point16 = unchecked((int)0xffff_c000);
        public const ushort ExplosionFastXSubvelocity = 0x4000;
        public const ushort ExplosionSlowXSubvelocity = 0x0800;
        public const ushort GunshipXSubvelocity = 0x4000;
        public const ushort GunshipYVelocity = 0xffff;
        public const ushort GunshipYSubvelocity = 0xf000;
        public const ushort PlanetEightEightAcceleration = 0x0040;
        public const ushort StarEightEightAcceleration = 0x0020;
        public const ushort Mode7ExitX = 0x003e;
        public static SnesAngle ApproachAngle => SnesAngle.FromTableIndex(0x20);
    }

    public static class Sprites
    {
        public static readonly SnesObjAttributeWord ScenePalette = new(0x0800);
        public static readonly SnesObjAttributeWord ExplosionPalette = new(0x0a00);
        public static readonly SnesObjAttributeWord PlanetPalette = new(0x0e00);
        public const ushort InitialExplosionList = 0xccdb;
        public const ushort SecondaryExplosionList = 0xccf5;
        public const ushort FinalExplosionList = 0xcd1b;
        public const ushort GunshipList = 0xce1b;
        public const ushort PlanetList = 0xccab;
        public const ushort PlanetTitleList = 0xccbb;
        public static readonly CeresCinematicActorDefinition InitialAsteroids =
            new(0x0050, 0x009f, ScenePalette.Raw, 0xcc3f);
        public static readonly CeresCinematicActorDefinition InitialSmallAsteroids =
            new(0x0080, 0x0060, ScenePalette.Raw, 0xcc4f);
        public static readonly CeresCinematicActorDefinition InitialVortex =
            new(0x0070, 0x0057, ScenePalette.Raw, 0xcc57);
        public static readonly CeresCinematicActorDefinition ZebesPlanet =
            new(0x0088, 0x006f, PlanetPalette.Raw, PlanetList);
        public static readonly CeresCinematicActorDefinition UpperLeftStar =
            new(0x0030, 0x002f, ScenePalette.Raw, 0xcd83);
        public static readonly CeresCinematicActorDefinition UpperRightStar =
            new(0x00d0, 0x002f, ScenePalette.Raw, 0xcd8b);
        public static readonly CeresCinematicActorDefinition LowerLeftStar =
            new(0x0030, 0x00cf, ScenePalette.Raw, 0xcd93);
        public static readonly CeresCinematicActorDefinition LowerRightStar =
            new(0x00d0, 0x00cf, ScenePalette.Raw, 0xcd9b);
        public static readonly CeresCinematicActorDefinition PlanetTitle =
            new(0x0080, 0x00ba, 0, PlanetTitleList);
        public const ushort ZebesInitialBackgroundX = 0x0080;
    }
}

/// <summary>Immutable constructor arguments for one bank-$8B cinematic sprite object.</summary>
public readonly record struct CeresCinematicActorDefinition(
    ushort X,
    ushort Y,
    ushort PaletteBits,
    ushort InstructionPointer);
