namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge and fixed-layout definitions for the X-ray Scope.</summary>
/// <remarks>
/// The room state's X-ray pointer remains room-authored data. This catalog owns the shared
/// palette, angle bands, tangent table, and HDMA-window units used by every room.
/// </remarks>
public static class SamusXrayRomData
{
    /// <summary>Palette words copied into Samus's visor colors while X-ray is active.</summary>
    public static class Palette
    {
        /// <summary><c>$9B:A3C0</c>, X-ray visor palette words.</summary>
        public const int VisorWords = 0x9ba3c0;
        /// <summary><c>$91:D727</c>, suit-indexed normal Samus palette pointers.</summary>
        public const int NormalSuitPointers = 0x91d727;
        /// <summary>Bank expanded around a normal-suit palette pointer.</summary>
        public const int PaletteBank = 0x9b0000;
        /// <summary>First CGRAM color occupied by Samus's OBJ palette.</summary>
        public const int SamusCgramIndex = 192;
        /// <summary>CGRAM color within Samus's palette occupied by the visor.</summary>
        public const int VisorCgramIndex = SamusCgramIndex + 4;
        /// <summary>Number of colors copied when restoring the normal suit palette.</summary>
        public const int SuitColorCount = 16;
        /// <summary>Final visor word offset retained while the beam widens.</summary>
        public const ushort WideningFinalWordOffset = 4;
        /// <summary>First visor word offset in the full-beam cycle.</summary>
        public const ushort FullCycleFirstWordOffset = 6;
        /// <summary>Exclusive visor word offset ending the full-beam cycle.</summary>
        public const ushort FullCycleEndWordOffset = 12;
        /// <summary>Frames between visor color advances.</summary>
        public const ushort FrameDelay = 5;
    }

    /// <summary>Angle bands selecting the five standing/crouching X-ray animation frames.</summary>
    public static class AnimationAngles
    {
        /// <summary>Right-facing boundary between frames zero and one.</summary>
        public const byte RightFrameOne = 0x19;
        /// <summary>Right-facing boundary between frames one and two.</summary>
        public const byte RightFrameTwo = 0x32;
        /// <summary>Right-facing boundary between frames two and three.</summary>
        public const byte RightFrameThree = 0x4b;
        /// <summary>Right-facing boundary between frames three and four.</summary>
        public const byte RightFrameFour = 0x64;
        /// <summary>Left-facing boundary between frames four and three.</summary>
        public const byte LeftFrameFour = 0x99;
        /// <summary>Left-facing boundary between frames three and two.</summary>
        public const byte LeftFrameThree = 0xb2;
        /// <summary>Left-facing boundary between frames two and one.</summary>
        public const byte LeftFrameTwo = 0xcb;
        /// <summary>Left-facing boundary between frames one and zero.</summary>
        public const byte LeftFrameOne = 0xe4;
    }

    /// <summary>Fixed-point and ROM-table definitions for the X-ray color-math window.</summary>
    public static class Window
    {
        /// <summary><c>$91:C9D4</c>, 129 absolute tangent words in unsigned 8.8 format.</summary>
        public const int AbsoluteTangentTable = 0x91c9d4;
        /// <summary>Number of words in the inclusive quarter-turn tangent table.</summary>
        public const int AbsoluteTangentWordCount = 129;
        /// <summary>One whole unit in the native signed 8.8 direction representation.</summary>
        public const int UnitVector = 0x0100;
        /// <summary>One-half unit used by the upper/lower edge accumulator.</summary>
        public const int HalfUnit = 0x0080;
        /// <summary>Per-frame 16.16 angular-width growth applied while widening.</summary>
        public const uint WideningStep = 0x00000800u;
    }
}
