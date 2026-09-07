using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>Verified bank-$8B/$8C data used by the ending, credits, and result screen.</summary>
public static class EndingCreditsRomData
{
    public static class Assets
    {
        public const int EscapePalette = 0x8cede9;
        public const int PostCreditsPalette = 0x8cefe9;
        public const int CreditsPalette = 0x8ce9e9;
        public const int ExplosionPalette = 0x8cebe9;
        public const int EscapeMapA = 0x98bcd6;
        public const int EscapeMapB = 0x98ed4f;
        public const int ExplosionMap = 0x999101;
        public const int EscapeCharactersA = 0x99d17e;
        public const int EscapeCharactersB = 0x99d65b;
        public const int ExplosionCharacters = 0x99d932;
        public const int EndingObjectCharacters = 0x99d17e;
        public const int EndingObjectCharacters70 = 0x98b5c1;
        public const int EndingObjectCharacters74 = 0x98b857;
        public const int EndingObjectCharacters78 = 0x98baed;
        public const int EndingObjectCharacters7C = 0x98bccd;
        public const int EndingFontCharacters = 0x97e7de;
        public const int CreditsTilemap = 0x97eeff;
        public const int WaitingForCreditsCharacters = 0x979803;
        public const int SuitlessSamusCharacters = 0x97b957;
        public const int ShootingScreenCharacters = 0x97d7fc;
        public const int WaitingForCreditsTilemap = 0x9796f4;
        public const int PostCreditsMode7Characters = 0x97f987;
        public const int PostCreditsTileFragmentA = 0x99da9f;
        public const int PostCreditsTileFragmentB = 0x99dab1;
        public const int SignedSineTable = 0xa0b443;
    }

    public static class Instructions
    {
        public static readonly SnesAddress Bank = new(0x8c, 0);
        public const ushort CreditsInitial = 0xd91b;
        public const ushort ResultPanel = 0xdc9b;
        public const ushort ItemPercentagePanel = 0xdedb;
        public const ushort ItemPercentageText = 0xdfdb;
        public const ushort SeeYouNextMissionText = 0xe0af;
        public const ushort JapaneseItemPercentageSubtitle = 0xdf5b;
    }

    public static class Rendering
    {
        /// <summary>Ending bank-$8B Mode-7 projection center X, in physical map pixels.</summary>
        public const short Mode7CenterX = 56;
        /// <summary>Ending bank-$8B Mode-7 projection center Y, in physical map pixels.</summary>
        public const short Mode7CenterY = 24;
        /// <summary>Ending OBSEL=$02 selects the escape/explosion OBJ character base.</summary>
        public const byte EscapeObjectSelection = 2;
        /// <summary>Post-credits OBSEL=$03 selects the reward OBJ character base.</summary>
        public const byte RewardObjectSelection = 3;
        public const ushort BlankTile = 0x007f;
        public const int TilemapWidth = 32;
        public const int TilemapHeight = 32;
        public const int TilemapWords = TilemapWidth * TilemapHeight;
        public const int CreditsSourceBytes = 0x2000;
        public const ushort CreditsTilemapWord = 0x4800;
        public const ushort CreditsCharacterWord = 0x4000;
        public const ushort PostCreditsTilemapWord = 0x4c00;
        public const ushort PostCreditsCharacterWord = 0x5000;
        public const int Mode7Bytes = 0x4000;
        public const int DecompressionLimit = 0x8000;
        public const int ObjectCharactersDestination = 0x8000;
        public const int FontCharactersDestination = 0xa000;
        public const int PostCreditsObjectDestination = 0xc000;
        public const int WaitingTilemapDestination = 0x9800;
        public const int PostCreditsFragmentADestination = 0x4000;
        public const int PostCreditsFragmentBDestination = 0x4800;
        public const int ObjectFragmentBytes = 0x0800;
        public const int ObjectFragmentLimit = 0x1000;
        public const int FontCharacterBytes = 0x1000;
        public const int WaitingCharacterBytes = 0x2000;
        public const int ShootingCharacterBytes = 0x4000;
        public const int WaitingTilemapBytes = 0x0800;
        public const int PostCreditsFragmentABytes = 0x0100;
        public const int Fragment70Destination = 0xe000;
        public const int Fragment74Destination = 0xe800;
        public const int Fragment78Destination = 0xf000;
        public const int Fragment7CDestination = 0xf800;
        public const int PaletteSecondHalfOffset = 0x0100;
        public const int PaletteHalfBytes = 128;
        public const int PostCreditsPaletteSourceOffset = 8;
        public const int PostCreditsPaletteBytes = 252;
        public const int PostCreditsPaletteDestination = 4;
        public const ushort WhiteColor = 0x7fff;
    }

    public static class Music
    {
        public const byte EscapeDataIndex = 0x33;
        public const byte EndingDataIndex = 0x3c;
        public const byte Track = 5;
        public const ushort DelayArgument = 0x000e;
    }

    public static class Rewards
    {
        public const ushort SuitlessMaximumHoursExclusive = 3;
        public const ushort HelmetlessMaximumHoursExclusive = 10;
    }

    public static class Motion
    {
        public const ushort IdentityScale = 0x0100;
        public const ushort EscapeInitialX = 0x0020;
        public const ushort EscapeInitialY = 0x0040;
        public const ushort EscapeEndY = 0x0180;
        public const ushort CloudMotionStartY = 0x0060;
        public const ushort CloudSceneBScaleLimit = 0x00b0;
        public const ushort PlanetEscapeInitialScale = 0x0c00;
        public const ushort PlanetFastEndScale = 0x05b0;
        public const ushort PlanetSlowEndScale = 0x04a0;
        public const ushort PlanetTurnStartScale = 0x0180;
        public const ushort PlanetExitScale = 0x0020;
        public const ushort InitialAccelerationFraction = 0x8000;
        public const int AccelerationDelta16Point16 = 0x0000_0100;
        public const uint CreditsScrollDelta16Point16 = 0x0000_8000;
        public static SnesAngle PlanetSlowTargetAngle => SnesAngle.FromTableIndex(0xe0);
        public static SnesAngle PlanetExitTargetAngle => SnesAngle.FromTableIndex(0x10);
        public static ReadOnlySpan<int> PlanetFastPattern =>
        [
            0x0000_8000, 0x0000_8000, 0x0000_8000, 0x0000_8000,
            unchecked((int)0xffff_8000), unchecked((int)0xffff_8000), 0x0000_8000, 0x0000_8000,
            0x0000_8000, unchecked((int)0xffff_8000), unchecked((int)0xffff_8000), 0x0000_8000,
            0x0000_8000, 0x0000_8000, unchecked((int)0xffff_8000), unchecked((int)0xffff_8000),
        ];
        public static ReadOnlySpan<int> PlanetSlowPattern =>
        [
            0x0001_0000, 0x0001_0000, 0x0001_0000, unchecked((int)0xffff_0000),
            unchecked((int)0xffff_0000), 0x0001_0000, 0x0001_0000, unchecked((int)0xffff_0000),
        ];
    }

    public static class Text
    {
        public const int ResultPanelDestination = 288;
        public const int ResultPanelWords = 288;
        public const int ItemPercentagePanelDestination = 384;
        public const int ItemPercentagePanelWords = 64;
        public const int JapaneseSubtitleDestination = 736;
        public const int JapaneseSubtitleWords = 64;
        public const int PercentageHundredsTopIndex = 462;
        public const ushort DigitTopTile = 0x3860;
        public const ushort DigitBottomTile = 0x3870;
        public const ushort PercentTopTile = 0x386a;
        public const ushort PercentBottomTile = 0x387a;
        public const ushort CollectibleItemMask = 0xf32f;
        public const ushort CollectibleBeamMask = 0x100f;
        public const ushort PackedPositionXMask = 0x00ff;
        public const ushort HoursTensX = 0x009c;
        public const ushort HoursUnitsX = 0x00a4;
        public const ushort MinutesTensX = 0x00b4;
        public const ushort MinutesUnitsX = 0x00bc;
    }

    public static class Sprites
    {
        public static readonly SnesObjAttributeWord TextPalette = SnesObjPalettes.Index2;
        public static readonly SnesObjAttributeWord TimeLabelPalette = SnesObjPalettes.Index1;
        public static readonly SnesObjAttributeWord ScenePalette = SnesObjPalettes.Index5;
        public static readonly SnesObjAttributeWord AlternatePalette = SnesObjPalettes.Index6;
        public static readonly SnesObjAttributeWord PlanetPalette = SnesObjPalettes.Index7;
        public static readonly EndingSpriteDefinition EscapeACloudRightTop =
            new(0x0140, 0x00c0, ScenePalette, 0xed0d);
        public static readonly EndingSpriteDefinition EscapeACloudLeftTop =
            new(0xffc0, 0x0040, ScenePalette, 0xed15);
        public static readonly EndingSpriteDefinition EscapeACloudRightBottom =
            new(0x0140, 0x01c0, ScenePalette, 0xed0d);
        public static readonly EndingSpriteDefinition EscapeACloudLeftBottom =
            new(0xffc0, 0xff40, ScenePalette, 0xed15);
        public static readonly EndingSpriteDefinition EscapeBCloudTopA =
            new(0xffa0, 0x0080, ScenePalette, 0xeced);
        public static readonly EndingSpriteDefinition EscapeBCloudTopB =
            new(0xffa0, 0x00c0, ScenePalette, 0xecf5);
        public static readonly EndingSpriteDefinition EscapeBCloudBottomA =
            new(0x0120, 0x0120, ScenePalette, 0xecfd);
        public static readonly EndingSpriteDefinition EscapeBCloudBottomB =
            new(0x0120, 0x0160, ScenePalette, 0xed05);
        public static readonly EndingSpriteDefinition ExplodingZebes =
            new(0x0080, 0x0080, PlanetPalette, 0xeb0f);
        public static readonly EndingSpriteDefinition ExplosionLava =
            new(0x0080, 0x0080, ScenePalette, 0xeb59);
        public static readonly EndingSpriteDefinition ExplosionGlow =
            new(0x0080, 0x0080, PlanetPalette, 0xeb3d);
        public static readonly EndingSpriteDefinition ExplosionStars =
            new(0x0080, 0x0080, PlanetPalette, 0xeb51);
        public static readonly EndingSpriteDefinition ExplosionSilhouette =
            new(0x0080, 0x0080, ScenePalette, 0xeb69);
        public static readonly EndingSpriteDefinition ExplosionStarsRight =
            new(0x0080, 0x0080, PlanetPalette, 0xeb71);
        public static readonly EndingSpriteDefinition ExplosionStarsLeft =
            new(0xff80, 0x0080, PlanetPalette, 0xeb81);
        public static readonly EndingSpriteDefinition ExplosionAfterglow =
            new(0x0080, 0x0080, AlternatePalette, 0xeb89);
        public static readonly EndingSpriteDefinition OperationWasText =
            new(0x0080, 0x0060, TextPalette, 0xeb91);
        public static readonly EndingSpriteDefinition CompletedSuccessfullyText =
            new(0x0080, 0x0060, TextPalette, 0xebd7);
        public static readonly EndingSpriteDefinition ClearTimeText =
            new(0x0080, 0x00a0, TimeLabelPalette, 0xec35);
        public static readonly EndingSpriteDefinition ClearTimeColon =
            new(0x00ac, 0x00a0, new SnesObjAttributeWord(0), 0xecd1);
        public const ushort ClearTimeDigitY = 0x00a0;
        public const ushort ClearTimeDigitInstructionBase = 0xec81;
        public const int ClearTimeDigitInstructionStride = 8;
        public static readonly EndingSpriteDefinition SuitlessRewardBody =
            new(0x0078, 0x0088, ScenePalette, 0xed1d);
        public static readonly EndingSpriteDefinition SuitlessRewardHead =
            new(0x0078, 0x0088, ScenePalette, 0xed25);
        public static readonly EndingSpriteDefinition ArmoredRewardBody =
            new(0x0078, 0x0098, AlternatePalette, 0xedb1);
        public static readonly EndingSpriteDefinition HelmetlessRewardHead =
            new(0x0079, 0x006b, ScenePalette, 0xedc1);
        public static readonly EndingSpriteDefinition ArmoredRewardHead =
            new(0x007c, 0x006c, AlternatePalette, 0xedb9);
    }
}

/// <summary>Immutable bank-$8B ending-sprite constructor data.</summary>
public readonly record struct EndingSpriteDefinition(
    ushort X,
    ushort Y,
    SnesObjAttributeWord Attributes,
    ushort InstructionPointer);
