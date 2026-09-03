namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge data and fixed hardware layout shared by Samus palettes.</summary>
/// <remarks>
/// Palette state machines retain their mutable timers and phase transitions. This catalog
/// names the ROM tables, object-program words, CGRAM ranges, and fixed-color endpoints those
/// machines consume, so a hexadecimal address never masquerades as gameplay behavior.
/// </remarks>
public static class SamusPaletteRomData
{
    /// <summary>Native banks containing Samus palette pointers, colors, and FX programs.</summary>
    public static class Banks
    {
        /// <summary>Bank <c>$91</c>, which owns Samus palette pointer lists.</summary>
        public const int Movement = 0x910000;
        /// <summary>Bank <c>$9B</c>, which owns Samus BGR555 color data.</summary>
        public const int Palette = 0x9b0000;
        /// <summary>Bank <c>$8D</c>, which owns generic palette-FX programs.</summary>
        public const int PaletteFx = 0x8d0000;
    }

    /// <summary>Shared full-body OBJ palette layout and suit selection table.</summary>
    public static class Common
    {
        /// <summary><c>$91:D727</c>, Power/Varia/Gravity normal-palette pointers.</summary>
        public const int NormalSuitPointers = 0x91d727;
        /// <summary>First CGRAM color of Samus OBJ palette four.</summary>
        public const int SamusObjPaletteStart = 192;
        /// <summary>First CGRAM color of suitless Samus OBJ palette seven.</summary>
        public const int SuitlessObjPaletteStart = 240;
        /// <summary>Number of colors in one complete SNES palette.</summary>
        public const int ColorsPerObjPalette = 16;
        /// <summary>Samus-palette-relative visor color index.</summary>
        public const int VisorColorOffset = 4;
    }

    /// <summary>Ordinary damage-flash and intro-restoration palettes.</summary>
    public static class HurtFlash
    {
        /// <summary><c>$9B:A380</c>, the sixteen-color yellow hurt palette.</summary>
        public const int Colors = 0x9ba380;
        /// <summary><c>$9B:A3A0</c>, the sixteen-color cinematic Samus palette.</summary>
        public const int IntroColors = 0x9ba3a0;
    }

    /// <summary>Shared visor colors used by room animation and the X-ray Scope.</summary>
    public static class Visor
    {
        /// <summary><c>$9B:A3C0</c>, six widening/cycling visor colors.</summary>
        public const int Colors = 0x9ba3c0;
        /// <summary>Packed offset-six/timer-one reset used outside animated rooms.</summary>
        public const ushort NormalRoomReset = 0x0601;
        /// <summary>Timer reloaded after one visor color is copied.</summary>
        public const ushort FrameDelay = 5;
        /// <summary>First byte offset in the three-color steady visor cycle.</summary>
        public const byte CycleFirstByteOffset = 6;
        /// <summary>Exclusive byte offset ending the three-color steady visor cycle.</summary>
        public const byte CycleEndByteOffset = 12;
    }

    /// <summary>Pointer tables used by special full-body palette handlers.</summary>
    public static class FullBodyCycles
    {
        /// <summary><c>$91:D998</c>, suit-indexed Speed Booster flash palettes.</summary>
        public const int SpeedBoostPointers = 0x91d998;
        /// <summary><c>$91:DA4A</c>, suit-indexed Screw Attack palette lists.</summary>
        public const int ScrewAttackLists = 0x91da4a;
        /// <summary><c>$91:DAA9</c>, suit-indexed active Speed Booster palette lists.</summary>
        public const int SpeedBoosterLists = 0x91daa9;
        /// <summary><c>$91:DB10</c>, suit-indexed stored-shine palette lists.</summary>
        public const int StoredShineLists = 0x91db10;
        /// <summary><c>$91:DB75</c>, suit-indexed active-shinespark palette lists.</summary>
        public const int ActiveShinesparkLists = 0x91db75;
        /// <summary><c>$91:D99E</c>, ten full-body Hyper Beam palette pointers.</summary>
        public const int HyperBeamPointers = 0x91d99e;
        /// <summary>Number of full-body Hyper Beam palettes.</summary>
        public const int HyperBeamPaletteCount = 10;
    }

    /// <summary>Bank-$8D palette object spawned with the Hyper Beam.</summary>
    public static class HyperBeamFx
    {
        /// <summary><c>$8D:E1F0</c>, two-word Hyper Beam palette-FX definition.</summary>
        public const int ObjectDefinition = 0x8de1f0;
        /// <summary>Expected no-op setup callback in the object definition.</summary>
        public const ushort SetupCallback = PaletteFxSetupCodes.Null;
        /// <summary>Initial instruction list stored by the object definition.</summary>
        public const ushort InitialList = 0xd900;
        /// <summary>First timed color record after the destination-selection command.</summary>
        public const ushort FirstFrame = 0xd904;
        /// <summary>Palette-buffer byte index selecting OBJ palette six, color one.</summary>
        public const ushort DestinationByteIndex = 0x01c2;
        /// <summary>Instruction <c>$C655</c>: select palette-buffer byte index from Y.</summary>
        public const ushort SetColorIndex = PaletteFxInstructionCodes.SetColorIndex;
        /// <summary>Instruction <c>$C595</c>: finish the current timed palette record.</summary>
        public const ushort Done = PaletteFxInstructionCodes.Wait;
        /// <summary>Instruction <c>$C61E</c>: jump to the instruction pointer in Y.</summary>
        public const ushort Goto = PaletteFxInstructionCodes.Goto;
        /// <summary>Number of timed color records in the loop.</summary>
        public const int FrameCount = 10;
        /// <summary>Number of colors written by each record.</summary>
        public const int ColorsPerFrame = 8;
        /// <summary>Bytes occupied by a duration, eight colors, and the done opcode.</summary>
        public const int FrameByteCount = 20;
    }

    /// <summary>Palette tables consumed by the fatal-damage sequence.</summary>
    public static class Death
    {
        /// <summary><c>$9B:B7D3</c>, three families of ten suited palette pointers.</summary>
        public const int SuitPointers = 0x9bb7d3;
        /// <summary><c>$9B:B80F</c>, ten suitless palette pointers.</summary>
        public const int SuitlessPointers = 0x9bb80f;
        /// <summary><c>$9B:B823</c>, nine interleaved timer/palette-index records.</summary>
        public const int ExplosionTimingAndPaletteIndices = 0x9bb823;
        /// <summary><c>$9B:B835</c>, 22 whiteout shades from black through white.</summary>
        public const int WhiteoutShades = 0x9bb835;
        /// <summary>Number of suited and suitless explosion palette variants.</summary>
        public const int PaletteCount = 10;
        /// <summary>Number of shades in the inclusive whiteout ramp.</summary>
        public const int WhiteoutShadeCount = 22;
    }

    /// <summary>Two independently timed palette streams used by Crystal Flash.</summary>
    public static class CrystalFlash
    {
        /// <summary><c>$90:C3C9</c>, twelve beam-loadout palette pointers.</summary>
        public const int BeamPalettePointers = 0x90c3c9;
        /// <summary>Bank <c>$90</c>, containing the restored projectile palettes.</summary>
        public const int BeamPaletteBank = 0x900000;
        /// <summary><c>$91:DC00</c>, ten body-palette pointer/timer records.</summary>
        public const int BodyRecords = 0x91dc00;
        /// <summary><c>$91:DC28</c>, six bubble-palette pointers.</summary>
        public const int BubblePointers = 0x91dc28;
        /// <summary>Number of beam-loadout palette pointers.</summary>
        public const int BeamPaletteCount = 12;
        /// <summary>Number of Crystal Flash body records.</summary>
        public const int BodyRecordCount = 10;
        /// <summary>Bytes occupied by one body pointer/timer record.</summary>
        public const int BodyRecordByteCount = 4;
        /// <summary>Number of bubble palette pointers.</summary>
        public const int BubblePaletteCount = 6;
        /// <summary>Number of colors copied for the body portion.</summary>
        public const int BodyColorCount = 10;
        /// <summary>Number of colors copied for the bubble portion.</summary>
        public const int BubbleColorCount = 6;
        /// <summary>First CGRAM color of OBJ palette six.</summary>
        public const int BodyCgramStart = 0xe0;
        /// <summary>First CGRAM color of the bubble portion in OBJ palette six.</summary>
        public const int BubbleCgramStart = 0xea;
    }

    /// <summary>Raw COLDATA endpoints used by the shared Varia/Gravity pickup sequence.</summary>
    public static class SuitPickup
    {
        /// <summary>Initial red byte shared by both transformations.</summary>
        public const byte InitialRed = 48;
        /// <summary>Initial/terminal Varia green byte.</summary>
        public const byte VariaGreen = 80;
        /// <summary>Initial/terminal Gravity green byte.</summary>
        public const byte GravityGreen = 73;
        /// <summary>Initial Varia blue component-enable byte.</summary>
        public const byte VariaBlue = 0x80;
        /// <summary>Initial/terminal Gravity blue component-enable byte.</summary>
        public const byte GravityBlue = 0x90;
        /// <summary>White-ramp red endpoint.</summary>
        public const byte WhiteRed = 63;
        /// <summary>White-ramp green endpoint.</summary>
        public const byte WhiteGreen = 95;
        /// <summary>White-ramp blue endpoint including component enable.</summary>
        public const byte WhiteBlue = 0x9f;
        /// <summary>Varia orange-ramp green endpoint.</summary>
        public const byte VariaOrangeGreen = 77;
        /// <summary>Varia orange-ramp blue endpoint including component enable.</summary>
        public const byte VariaOrangeBlue = 0x83;
        /// <summary>Final fixed-color register reset red byte.</summary>
        public const byte ResetRed = 32;
        /// <summary>Final fixed-color register reset green byte.</summary>
        public const byte ResetGreen = 64;
        /// <summary>Final fixed-color register reset blue byte.</summary>
        public const byte ResetBlue = 0x80;
    }

    /// <summary>Fixed-color tables used by Power Bomb and Crystal Flash HDMA.</summary>
    public static class PowerBomb
    {
        /// <summary><c>$88:9079</c>, sixteen RGB triplets for the pre-explosion.</summary>
        public const int PreExplosionColors = 0x889079;
        /// <summary><c>$88:8D85</c>, radius-indexed RGB triplets for the explosion.</summary>
        public const int ExplosionColors = 0x888d85;
        /// <summary>Each fixed-color record stores red, green, and blue bytes.</summary>
        public const int BytesPerColor = 3;
        /// <summary>Five-bit component payload mask in a COLDATA byte.</summary>
        public const byte ComponentMask = 0x1f;
        /// <summary>Number of pre-explosion fixed-color triplets.</summary>
        public const int PreExplosionColorCount = 16;
        /// <summary>Number of explosion fixed-color triplets addressed by radius.</summary>
        public const int ExplosionColorCount = 32;
    }
}
