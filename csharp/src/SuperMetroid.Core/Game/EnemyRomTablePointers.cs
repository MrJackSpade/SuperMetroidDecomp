namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed cartridge data ranges consumed by translated enemy logic. These are deliberately
/// separate from AI and instruction addresses: every member names data shape and ownership.
/// </summary>
internal static class EnemyRomTablePointers
{
    /// <summary>Movement and collision tables shared across enemy banks.</summary>
    public static class Common
    {
        /// <summary>256 signed 16-bit sine/cosine samples at $A0:B443 (512 bytes).</summary>
        public const int SignedSineCosineWords = 0xa0b443;
        /// <summary>Sixteen 32-byte slope-height profiles at $94:8B2B (512 bytes).</summary>
        public const int SlopeHeightBytes = 0x948b2b;
    }

    /// <summary>Bomb/Golden Torizo actor and projectile tables.</summary>
    public static class Torizo
    {
        /// <summary>Two Golden Torizo super-missile list pointers at $86:B209 (4 bytes).</summary>
        public const int SuperMissileInstructionPointers = 0x86b209;
        /// <summary>Two Bomb Torizo wake positions at $AA:C95F (4 bytes).</summary>
        public const int WakeXPositions = 0xaac95f;
        /// <summary>Two Bomb Torizo wake positions at $AA:C963 (4 bytes).</summary>
        public const int WakeYPositions = 0xaac963;
        /// <summary>Two Bomb Torizo wake instruction lists at $AA:C967 (4 bytes).</summary>
        public const int WakeInstructionLists = 0xaac967;
        /// <summary>Two Bomb Torizo wake property masks at $AA:C96B (4 bytes).</summary>
        public const int WakePropertyMasks = 0xaac96b;
        /// <summary>Two Bomb Torizo wake X radii at $AA:C96F (4 bytes).</summary>
        public const int WakeXRadii = 0xaac96f;
        /// <summary>Two Bomb Torizo wake Y radii at $AA:C973 (4 bytes).</summary>
        public const int WakeYRadii = 0xaac973;
        /// <summary>Twenty Golden Torizo walking displacement words at $AA:D59A (40 bytes).</summary>
        public const int WalkHorizontalVelocityWords = 0xaad59a;
        /// <summary>Bomb Torizo palette-FX color words at $84:8032.</summary>
        public const int BodyPaletteFxColors = 0x848032;
        /// <summary>Bomb Torizo belly palette-FX color words at $84:8132.</summary>
        public const int BellyPaletteFxColors = 0x848132;
    }

    /// <summary>Chozo-statue palette and motion tables in bank $AA.</summary>
    public static class ChozoStatue
    {
        /// <summary>Sixteen statue palette words at $AA:E2DD (32 bytes).</summary>
        public const int PaletteWords = 0xaae2dd;
        /// <summary>Thirty-two signed statue movement velocities at $AA:E630 (64 bytes).</summary>
        public const int CarryVelocityWords = 0xaae630;
        /// <summary>Thirty-two carried-Samus X offsets at $AA:E670 (64 bytes).</summary>
        public const int CarriedSamusXOffsetWords = 0xaae670;
        /// <summary>Thirty-two carried-Samus Y offsets at $AA:E6B0 (64 bytes).</summary>
        public const int CarriedSamusYOffsetWords = 0xaae6b0;
    }

    /// <summary>Ceres Ridley, destruction, and door animation data.</summary>
    public static class Ceres
    {
        /// <summary>Two Ceres door VRAM-transfer list pointers at $A6:F900 (4 bytes).</summary>
        public const int DoorTransferPointers = 0xa6f900;
        /// <summary>Ridley tail-tip spritemap pointers at $A6:DCBA.</summary>
        public const int TailTipSpritemapPointers = 0xa6dcba;
        /// <summary>Ridley wing spritemap pointers at $A6:DB02.</summary>
        public const int WingSpritemapPointers = 0xa6db02;
        /// <summary>Ridley rotation divisors at $A6:D712.</summary>
        public const int RidleyRotationDivisorBytes = 0xa6d712;
        /// <summary>Ceres Ridley fade component steps at $A6:E269.</summary>
        public const int RidleyFadeComponentBytes = 0xa6e269;
    }

    /// <summary>Crocomire death graphics-transfer tables.</summary>
    public static class Crocomire
    {
        /// <summary>Seven VRAM destination offsets at $A4:99CB (14 bytes).</summary>
        public const int DeathVramDestinationWords = 0xa499cb;
        /// <summary>Seven source pointers at $A4:99D9 (14 bytes).</summary>
        public const int DeathGraphicsSourceWords = 0xa499d9;
    }

    /// <summary>Dead Sidehopper launch tables.</summary>
    public static class DeadSidehopper
    {
        /// <summary>Four signed vertical velocity words at $A9:D951 (8 bytes).</summary>
        public const int VerticalVelocityWords = 0xa9d951;
        /// <summary>Four signed horizontal velocity words at $A9:D959 (8 bytes).</summary>
        public const int HorizontalVelocityWords = 0xa9d959;
    }

    /// <summary>Landing Site gunship graphics-transfer tables.</summary>
    public static class Gunship
    {
        /// <summary>Six signed liftoff dust X-offset words at $86:A2D6 (12 bytes).</summary>
        public const int DustXOffsetWords = 0x86a2d6;
        /// <summary>Six liftoff dust instruction-list pointers at $86:A2E2 (12 bytes).</summary>
        public const int DustInstructionPointers = 0x86a2e2;
        /// <summary>Five graphics source pointers at $A2:AC07 (10 bytes).</summary>
        public const int LiftoffGraphicsSourceWords = 0xa2ac07;
        /// <summary>Five VRAM destination words at $A2:AC11 (10 bytes).</summary>
        public const int LiftoffVramDestinationWords = 0xa2ac11;
    }

    /// <summary>Bank-$86 falling-spark randomization data.</summary>
    public static class FallingSpark
    {
        /// <summary>Two interleaved initial-Y words at $86:F3D4.</summary>
        public const int InitialYWords = 0x86f3d4;
        /// <summary>Two interleaved initial-X words at $86:F3D6.</summary>
        public const int InitialXWords = 0x86f3d6;
    }

    /// <summary>KiHunter distance and orbit-radius data.</summary>
    public static class KiHunter
    {
        /// <summary>Attack trigger distance word at $A8:F180.</summary>
        public const int TriggerDistanceWord = 0xa8f180;
        /// <summary>Attack ellipse Y-radius word at $A8:F182.</summary>
        public const int AttackYRadiusWord = 0xa8f182;
        /// <summary>Attack ellipse X-radius word at $A8:F184.</summary>
        public const int AttackXRadiusWord = 0xa8f184;
        /// <summary>Wingless hop radius byte at $A8:F186.</summary>
        public const int WinglessHopRadiusByte = 0xa8f186;
    }

    /// <summary>Kraid palettes, hitboxes, growth, and projectile motion data.</summary>
    public static class Kraid
    {
        /// <summary>Initial nail spritemap pointer word at $A7:8B0C.</summary>
        public const int InitialNailSpritemapWord = 0xa78b0c;
        /// <summary>Kraid roar/growth initial timer words at $A7:96D2.</summary>
        public const int InitialTimerWords = 0xa796d2;
        /// <summary>Kraid death initial timer word at $A7:9764.</summary>
        public const int DeathInitialTimerWord = 0xa79764;
        /// <summary>Kraid combat timer word at $A7:974A.</summary>
        public const int CombatTimerWord = 0xa7974a;
        /// <summary>Kraid room-background target palette at $A7:86C7 (32 bytes).</summary>
        public const int RoomBackgroundPaletteWords = 0xa786c7;
        /// <summary>Kraid ceiling-rock X-position words at $A7:ACB3.</summary>
        public const int CeilingRockXWords = 0xa7acb3;
        /// <summary>Kraid hitbox left-coordinate records at $A7:B163.</summary>
        public const int HitboxLeftWords = 0xa7b163;
        /// <summary>Kraid hitbox top-coordinate records at $A7:B165.</summary>
        public const int HitboxTopWords = 0xa7b165;
        /// <summary>Kraid health palette source at $A7:B3D3.</summary>
        public const int HealthPaletteWords = 0xa7b3d3;
        /// <summary>Kraid secondary palette source at $A7:B513.</summary>
        public const int SecondaryPaletteWords = 0xa7b513;
        /// <summary>Kraid death arm palette at $A7:B4F3 (32 bytes).</summary>
        public const int DeathArmPaletteWords = 0xa7b4f3;
        /// <summary>Kraid spat-rock X velocity words at $A7:BC65.</summary>
        public const int RockXVelocityWords = 0xa7bc65;
        /// <summary>Kraid second-phase movement record table at $A7:BA7D.</summary>
        public const int SecondPhaseMovementRecords = 0xa7ba7d;
        /// <summary>Kraid fingernail position-offset words at $A7:BF1D.</summary>
        public const int NailPositionOffsetWords = 0xa7bf1d;
        /// <summary>Kraid death explosion Y/function records at $A7:C5E7.</summary>
        public const int DeathExplosionRecords = 0xa7c5e7;
    }

    /// <summary>Phantoon movement, timing, palette, and flame-pattern data.</summary>
    public static class Phantoon
    {
        /// <summary>Eight first-round hiding timer words at $A7:CD41 (16 bytes).</summary>
        public const int FirstRoundHidingTimerWords = 0xa7cd41;
        /// <summary>Eight eye-closed timer words at $A7:CD53 (16 bytes).</summary>
        public const int EyeClosedTimerWords = 0xa7cd53;
        /// <summary>Figure-eight speed and limit words at $A7:CD73.</summary>
        public const int FigureEightMotionWords = 0xa7cd73;
        /// <summary>Eight random direction bytes at $A7:CDA5.</summary>
        public const int RandomDirectionBytes = 0xa7cda5;
        /// <summary>Mouth flame-pattern pointer words at $A7:CCFD.</summary>
        public const int MouthPatternPointerWords = 0xa7ccfd;
        /// <summary>Phantoon flame random-angle bytes at $86:98B4.</summary>
        public const int FlameAngleBytes = 0x8698b4;
        /// <summary>Phantoon flame-rain X-position bytes at $86:98F7.</summary>
        public const int FlameRainXBytes = 0x8698f7;
        /// <summary>Phantoon spiral-flame angle bytes at $86:9979.</summary>
        public const int SpiralAngleBytes = 0x869979;
    }

    /// <summary>Norfair Ridley movement and health-scaling tables.</summary>
    public static class Ridley
    {
        /// <summary>Thirty-two initial body/tail palette words at $A6:E1CF (64 bytes).</summary>
        public const int InitialPaletteWords = 0xa6e1cf;
        /// <summary>Area-two reveal-palette source-pointer words at $A6:A4EB.</summary>
        public const int RevealPaletteSourcePointers = 0xa6a4eb;
        /// <summary>Descending pogo target-X words at $A6:B60D.</summary>
        public const int DescendingPogoTargetXWords = 0xa6b60d;
        /// <summary>Ascending pogo target-X words at $A6:B63B.</summary>
        public const int AscendingPogoTargetXWords = 0xa6b63b;
        /// <summary>Ground-attack target-X words at $A6:B6C8.</summary>
        public const int GroundAttackTargetXWords = 0xa6b6c8;
        /// <summary>Pogo upward acceleration words at $A6:B94D.</summary>
        public const int PogoUpwardAccelerationWords = 0xa6b94d;
        /// <summary>Pogo downward acceleration words at $A6:B959.</summary>
        public const int PogoDownwardAccelerationWords = 0xa6b959;
        /// <summary>Pogo horizontal-path pointer words at $A6:B965.</summary>
        public const int PogoHorizontalPathPointers = 0xa6b965;
        /// <summary>Pogo vertical-path pointer words at $A6:B96D.</summary>
        public const int PogoVerticalPathPointers = 0xa6b96d;
        /// <summary>Claw X-offset words selected by facing at $A6:B9D5.</summary>
        public const int ClawXOffsetWords = 0xa6b9d5;
        /// <summary>Claw Y-offset words selected by foot separation at $A6:B9DB.</summary>
        public const int ClawYOffsetWords = 0xa6b9db;
        /// <summary>Health-stage movement-divisor indexes at $A6:BB4E.</summary>
        public const int HealthMovementDivisorIndexWords = 0xa6bb4e;
        /// <summary>Carry-anchor X-position words selected by facing at $A6:BBEB.</summary>
        public const int CarryAnchorXWords = 0xa6bbeb;
        /// <summary>Carry-release X-position words selected by facing at $A6:BC62.</summary>
        public const int CarryReleaseXWords = 0xa6bc62;
        /// <summary>Tail rotation divisor bytes at $A6:D61F.</summary>
        public const int TailRotationDivisorBytes = 0xa6d61f;
        /// <summary>Four health-stage tail instruction words at $A6:B439.</summary>
        public const int TailInstructionWords = 0xa6b439;
        /// <summary>Three fourteen-color health-palette records at $A6:E46A (84 bytes).</summary>
        public const int HealthPaletteWords = 0xa6e46a;
    }

    /// <summary>Tourian entrance statue palettes and instruction tables.</summary>
    public static class TourianStatue
    {
        /// <summary>Statue instruction-list table at $AA:D810.</summary>
        public const int InstructionListWords = 0xaad810;
        /// <summary>Sixteen statue palette words at $AA:D765 (32 bytes).</summary>
        public const int StatuePaletteWords = 0xaad765;
        /// <summary>Sixteen base-decoration palette words at $AA:D785 (32 bytes).</summary>
        public const int BaseDecorationPaletteWords = 0xaad785;
    }

    /// <summary>Wrecked Ship work-robot instruction-list selection data.</summary>
    public static class WorkRobot
    {
        /// <summary>Parameterized instruction-list pointer words at $A8:CC30.</summary>
        public const int InitialInstructionListWords = 0xa8cc30;
    }
}
