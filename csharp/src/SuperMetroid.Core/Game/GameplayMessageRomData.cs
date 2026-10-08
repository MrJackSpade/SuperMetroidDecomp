using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Verified bank-$85 message-box tables, routines, layout, and PPU values.</summary>
public static class GameplayMessageRomData
{
    public static class Assets
    {
        /// <summary>$85:869B, 29 setup/draw/content definition triples.</summary>
        public const int DefinitionTable = 0x85869b;
        public const int LargeBorder = 0x858000;
        public const int SmallBorder = 0x858040;
        public const int SaveSelectionTilemap = 0x859581;
        public const int BankBase = 0x850000;
    }

    public static class Routines
    {
        /// <summary>$85:825A, writes the large message-box border layout.</summary>
        public const ushort DrawLargeTilemap = 0x825a;
        /// <summary>$85:8289, writes the small message-box border layout.</summary>
        public const ushort DrawSmallTilemap = 0x8289;
        /// <summary>$85:83C5, patches the configured shoot-button glyph and selects the large box.</summary>
        public const ushort PatchShootButton = 0x83c5;
        /// <summary>$85:83CC, patches the configured run-button glyph and selects the large box.</summary>
        public const ushort PatchRunButton = 0x83cc;
        /// <summary>$85:8436, configures PPU state for a small message box.</summary>
        public const ushort SetupSmall = 0x8436;
        /// <summary>$85:8441, configures PPU state for a large message box.</summary>
        public const ushort SetupLarge = 0x8441;
    }

    public static class Layout
    {
        /// <summary>Native byte width of one setup/draw/content definition triple.</summary>
        public const int DefinitionBytes = 6;
        public const int TilemapWidth = 32;
        public const int BorderRows = 2;
        public const int MinimumRows = 3;
        public const int MaximumRows = 6;
        public const int TilePixels = 8;
        public const int SaveSelectionDestinationWord = 128;
        public const int SaveSelectionRowWords = 32;
        public const int SaveSelectionYesSourceWord = 32;
        public const int SaveSelectionNoSourceWord = 64;
        public const ushort CharacterBaseWord = 0x4000;
        public const int VramWordMask = 0x7fff;
        public const int WindowCenterY = 124;
    }

    public static class Timing
    {
        /// <summary>$85:8122: Play_Saving_Sound_Effect waits 160 accepted lag frames.</summary>
        public const int GunshipSavingSoundFrames = 160;
        /// <summary>
        /// $85:808D-$8096: lag frames before Open_MessageBox. Initialise_PPU_for_MessageBoxes 2,
        /// Clear_MessageBox_BG3Tilemap 1, Write_Message_Tilemap 1, Setup_PPU_for_Active_MessageBox
        /// and its HDMA table 2, Play_2_Lag_Frames_of_Music_and_Sound_Effects 2.
        /// </summary>
        public const int PreOpenLagFrames = 8;
        /// <summary>$85:80D9-$80DC: the gunship's second box, without PPU setup or clear.</summary>
        public const int ReopenLagFrames = 5;
        /// <summary>$85:81F3: Clear_MessageBox_BG3Tilemap waits one lag frame.</summary>
        public const int ClearTilemapLagFrames = 1;
        /// <summary>$85:80AA-$80B4: tilemap clear 1, Restore_PPU 2, music/sound 2.</summary>
        public const int RestoreLagFrames = 5;
        /// <summary>$85:84BC: the save selector waits two lag frames per ReadControllerInput.</summary>
        public const int SaveSelectionReadLagFrames = 2;
        public const int MaximumRadiusPixels = 24;
        public const int RadiusStepPixels = 2;
        public const int ItemMinimumDisplayFrames = 360;
        public const int StationMinimumDisplayFrames = 10;
    }

    public static class Palette
    {
        public const int TemporaryLightIndex = 25;
        public const ushort TemporaryLightColor = 0x0bb1;
        public const int TemporaryDarkIndex = 26;
        public const ushort TemporaryDarkColor = 0x001f;
    }

    public static class Buttons
    {
        /// <summary>$85:8426, DrawSpecialButton_SetupPPUForLargeMessageBox.buttons: A glyph.</summary>
        private const ushort A = 0x28e0;
        /// <summary>$85:8428, native buttons row: B glyph.</summary>
        private const ushort B = 0x3ce1;
        /// <summary>$85:842A, native buttons row: X glyph.</summary>
        private const ushort X = 0x2cf7;
        /// <summary>$85:842C, native buttons row: Y glyph.</summary>
        private const ushort Y = 0x38f8;
        /// <summary>$85:842E, native buttons row: Select glyph.</summary>
        private const ushort Select = 0x38d0;
        /// <summary>$85:8430, native buttons row: L glyph.</summary>
        private const ushort L = 0x38eb;
        /// <summary>$85:8432, native buttons row: R glyph.</summary>
        private const ushort R = 0x38f1;
        /// <summary>$85:8434, native buttons row: blank fallback when no supported binding bit matches.</summary>
        public static readonly SnesBgTilemapWord UnknownGlyph = new(0x284e);

        /// <summary>$85:83D1..8409 BIT-test order; the first matching button wins even for a malformed multibit binding.</summary>
        public static ushort ResolveGlyphWord(ushort binding) => binding switch
        {
            var bits when (bits & (ushort)SnesButton.A) != 0 => A,
            var bits when (bits & (ushort)SnesButton.B) != 0 => B,
            var bits when (bits & (ushort)SnesButton.X) != 0 => X,
            var bits when (bits & (ushort)SnesButton.Y) != 0 => Y,
            var bits when (bits & (ushort)SnesButton.Select) != 0 => Select,
            var bits when (bits & (ushort)SnesButton.L) != 0 => L,
            var bits when (bits & (ushort)SnesButton.R) != 0 => R,
            _ => UnknownGlyph.Raw,
        };

        /// <summary>$85:8749..877E Special_Button_Tilemap_Offsets, ID minus one; 1B is the native dummy boundary.</summary>
        public static ushort SpecialGlyphByteOffset(GameplayMessageId messageId)
        {
            if ((uint)((byte)messageId - 1) >= 27) throw new IndexOutOfRangeException();
            return messageId switch
            {
                GameplayMessageId.MissileTank or GameplayMessageId.SuperMissileTank or GameplayMessageId.Bombs => 0x12a,
                GameplayMessageId.PowerBombTank or GameplayMessageId.GrappleBeam or GameplayMessageId.XrayScope => 0x12c,
                GameplayMessageId.SpeedBooster => 0x120,
                _ => 0,
            };
        }
    }
}
