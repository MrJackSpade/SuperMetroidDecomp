using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>Verified bank-$85 message-box tables, routines, layout, and PPU values.</summary>
public static class GameplayMessageRomData
{
    /// <summary>Cartridge addresses of message definitions, borders, and selection tilemaps.</summary>
    public static class Assets
    {
        /// <summary>$85:869B, 29 setup/draw/content definition triples.</summary>
        public const int DefinitionTable = 0x85869b;
        /// <summary>$85:8000, tilemap words forming the large message-box border.</summary>
        public const int LargeBorder = 0x858000;
        /// <summary>$85:8040, tilemap words forming the small message-box border.</summary>
        public const int SmallBorder = 0x858040;
        /// <summary>$85:9581, YES/NO selection rows used by the save prompt.</summary>
        public const int SaveSelectionTilemap = 0x859581;
        /// <summary>SNES base address of bank $85 for bank-relative message pointers.</summary>
        public const int BankBase = 0x850000;
    }

    /// <summary>Bank-$85 routine identities stored in the message definition table.</summary>
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

    /// <summary>Message tilemap dimensions, transfer offsets, and window placement.</summary>
    public static class Layout
    {
        /// <summary>Native byte width of one setup/draw/content definition triple.</summary>
        public const int DefinitionBytes = 6;
        /// <summary>Width of the message BG3 tilemap in tile words.</summary>
        public const int TilemapWidth = 32;
        /// <summary>Number of rows occupied by the upper and lower borders together.</summary>
        public const int BorderRows = 2;
        /// <summary>Minimum supported message-box height in tile rows.</summary>
        public const int MinimumRows = 3;
        /// <summary>Maximum supported message-box height in tile rows.</summary>
        public const int MaximumRows = 6;
        /// <summary>Width and height of one BG tile in pixels.</summary>
        public const int TilePixels = 8;
        /// <summary>Destination tilemap word index of the save-selection rows.</summary>
        public const int SaveSelectionDestinationWord = 128;
        /// <summary>Word count copied for one save-selection row.</summary>
        public const int SaveSelectionRowWords = 32;
        /// <summary>Source word index of the selected-YES row.</summary>
        public const int SaveSelectionYesSourceWord = 32;
        /// <summary>Source word index of the selected-NO row.</summary>
        public const int SaveSelectionNoSourceWord = 64;
        /// <summary>VRAM word base of the message-box BG3 character graphics.</summary>
        public const ushort CharacterBaseWord = 0x4000;
        /// <summary>Mask removing the native VRAM increment-mode bit from a destination word.</summary>
        public const int VramWordMask = 0x7fff;
        /// <summary>Vertical screen coordinate around which the message window expands.</summary>
        public const int WindowCenterY = 124;
    }

    /// <summary>Authored lag waits, window-animation steps, and minimum display durations.</summary>
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
        /// <summary>Maximum half-height reached by the expanding message window.</summary>
        public const int MaximumRadiusPixels = 24;
        /// <summary>Per-step change in message-window radius.</summary>
        public const int RadiusStepPixels = 2;
        /// <summary>Minimum updates an item-acquisition message remains displayed.</summary>
        public const int ItemMinimumDisplayFrames = 360;
        /// <summary>Minimum updates a station message remains displayed.</summary>
        public const int StationMinimumDisplayFrames = 10;
    }

    /// <summary>Temporary CGRAM entries installed while a message box is active.</summary>
    public static class Palette
    {
        /// <summary>CGRAM color index replaced by the light message color.</summary>
        public const int TemporaryLightIndex = 25;
        /// <summary>SNES BGR555 light color installed for the message box.</summary>
        public static Bgr555 TemporaryLightColor => Bgr555.FromWord(0x0bb1);
        /// <summary>CGRAM color index replaced by the dark message color.</summary>
        public const int TemporaryDarkIndex = 26;
        /// <summary>SNES BGR555 dark color installed for the message box.</summary>
        public static Bgr555 TemporaryDarkColor => new(31, 0, 0);
    }

    /// <summary>Controller-binding glyph selection and message-relative patch offsets.</summary>
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
