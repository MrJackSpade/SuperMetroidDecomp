namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed cartridge data ranges consumed by translated enemy logic. These are deliberately
/// separate from AI and instruction addresses: every member names data shape and ownership.
/// </summary>
internal static class EnemyRomTablePointers
{
    /// <summary>Ceres Ridley, destruction, and door animation data.</summary>
    public static class Ceres
    {
        /// <summary>Sixteen three-color Ceres Ridley eye-fade palette rows at $A6:E2AA.</summary>
        public const int RidleyEyeFadePaletteRows = 0xa6e2aa;
    }

    /// <summary>Kraid palettes, hitboxes, growth, and projectile motion data.</summary>
    public static class Kraid
    {
        /// <summary>Kraid room-background target palette at $A7:86C7 (32 bytes).</summary>
        public const int RoomBackgroundPaletteWords = 0xa786c7;
        /// <summary>Kraid health palette source at $A7:B3D3.</summary>
        public const int HealthPaletteWords = 0xa7b3d3;
        /// <summary>Kraid secondary palette source at $A7:B513.</summary>
        public const int SecondaryPaletteWords = 0xa7b513;
        /// <summary>Kraid death arm palette at $A7:B4F3 (32 bytes).</summary>
        public const int DeathArmPaletteWords = 0xa7b4f3;
    }

    /// <summary>Norfair Ridley movement and health-scaling tables.</summary>
    public static class Ridley
    {
        /// <summary>Thirty-two initial body/tail palette words at $A6:E1CF (64 bytes).</summary>
        public const int InitialPaletteWords = 0xa6e1cf;
        /// <summary>Area-two reveal-palette source-pointer words at $A6:A4EB.</summary>
        public const int RevealPaletteSourcePointers = 0xa6a4eb;
    }

    /// <summary>Tourian entrance statue palettes and instruction tables.</summary>
    public static class TourianStatue
    {
        /// <summary>Sixteen statue palette words at $AA:D765 (32 bytes).</summary>
        public const int StatuePaletteWords = 0xaad765;
        /// <summary>Sixteen base-decoration palette words at $AA:D785 (32 bytes).</summary>
        public const int BaseDecorationPaletteWords = 0xaad785;
    }

    /// <summary>Wrecked Ship work-robot instruction-list selection data.</summary>
    public static class WorkRobot
    {
        /// <summary>Six four-color/timer palette records at $A8:CCC1 (60 bytes).</summary>
        public const int PaletteAnimationRecords = 0xa8ccc1;
    }
}
