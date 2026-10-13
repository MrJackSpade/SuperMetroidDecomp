// Retail ROM addresses and field offsets that only verification oracles read. Each class
// was a nested constants group of the production catalog named by its prefix.

namespace SuperMetroid.Core.Audio
{
    /// <summary>Retail music command used by the title sequence oracle.</summary>
    internal static class AudioRomDataMusicTracks
    {
        /// <summary>Title-screen track command for the music bank uploaded from <see cref="AudioUploadAddresses.TitleSequence"/>.</summary>
        public const byte Title = 0x05;
    }
}

namespace SuperMetroid.Core.Frontend
{
    /// <summary>Word offsets in a DemoSetDef record at bank $91.</summary>
    internal static class AttractDemoRomDataEquipmentFields
    {
        /// <summary>Collected item-bit word at the start of an equipment record.</summary>
        public const int Items = 0;
        /// <summary>Missile count word after the item bits.</summary>
        public const int Missiles = 2;
        /// <summary>Super Missile count word.</summary>
        public const int SuperMissiles = 4;
        /// <summary>Power Bomb count word.</summary>
        public const int PowerBombs = 6;
        /// <summary>Current health word.</summary>
        public const int Health = 8;
        /// <summary>Collected beam-bit word, separate from equipped beams.</summary>
        public const int CollectedBeams = 10;
        /// <summary>Equipped beam-bit word.</summary>
        public const int EquippedBeams = 12;
        /// <summary>Bank-91 demo input-object pointer word.</summary>
        public const int InputObject = 14;
    }

    /// <summary>Bank-$91 input-object identities for the two grapple demonstrations.</summary>
    internal static class AttractDemoRomDataInputObjects
    {
        /// <summary>$91:9EB2, DemoInputObjects_Title_GrappleBeam, ordinary grapple demonstration.</summary>
        public const ushort GrappleBeam = 0x9eb2;
        /// <summary>$91:9EC4, DemoInputObjects_Title_AdvancedGrappleBeam, advanced grapple demonstration.</summary>
        public const ushort AdvancedGrappleBeam = 0x9ec4;
    }

    /// <summary>Word offsets in a DemoRoomData record at bank $82.</summary>
    internal static class AttractDemoRomDataRoomFields
    {
        /// <summary>Room identity word in a demo room record.</summary>
        public const int Room = 0;
        /// <summary>Entry door pointer word.</summary>
        public const int Door = 2;
        /// <summary>Door slot index word.</summary>
        public const int DoorSlot = 4;
        /// <summary>Initial camera X word.</summary>
        public const int CameraX = 6;
        /// <summary>Initial camera Y word.</summary>
        public const int CameraY = 8;
        /// <summary>Samus's vertical offset from the screen top.</summary>
        public const int SamusYFromTop = 10;
        /// <summary>Samus's horizontal offset from the screen center.</summary>
        public const int SamusXFromCenter = 12;
        /// <summary>Demo duration word.</summary>
        public const int Duration = 14;
        /// <summary>Demo setup routine pointer word.</summary>
        public const int Setup = 16;
    }

    /// <summary>Named bank-$8C indirect-data records compared by cinematic draw code.</summary>
    internal static class CinematicCodePointersIndirectData
    {
        /// <summary>Bank-8C blank-space list at $8C:D67D used by intro text rendering.</summary>
        public const ushort IntroTextSpace = 0xd67d;
        /// <summary>Bank-8C first intro story-page background list at $8C:C383.</summary>
        public const ushort IntroTextPage1 = 0xc383;
        /// <summary>Bank-8C second intro story-page background list at $8C:C797.</summary>
        public const ushort IntroTextPage2 = 0xc797;
        /// <summary>Bank-8C third intro story-page background list at $8C:CB45.</summary>
        public const ushort IntroTextPage3 = 0xcb45;
        /// <summary>Bank-8C fourth intro story-page background list at $8C:CE33.</summary>
        public const ushort IntroTextPage4 = 0xce33;
        /// <summary>Bank-8C fifth intro story-page background list at $8C:D15D.</summary>
        public const ushort IntroTextPage5 = 0xd15d;
        /// <summary>Bank-8C sixth intro story-page background list at $8C:D511.</summary>
        public const ushort IntroTextPage6 = 0xd511;
    }
}

namespace SuperMetroid.Core.Game
{
    /// <summary>Chozo-statue palette and motion tables in bank $AA.</summary>
    internal static class EnemyRomTablePointersChozoStatue
    {
        /// <summary>Thirty-two signed statue movement velocities at $AA:E630 (64 bytes).</summary>
        public const int CarryVelocityWords = 0xaae630;
        /// <summary>Thirty-two carried-Samus X offsets at $AA:E670 (64 bytes).</summary>
        public const int CarriedSamusXOffsetWords = 0xaae670;
        /// <summary>Thirty-two carried-Samus Y offsets at $AA:E6B0 (64 bytes).</summary>
        public const int CarriedSamusYOffsetWords = 0xaae6b0;
    }

    /// <summary>Movement and collision tables shared across enemy banks.</summary>
    internal static class EnemyRomTablePointersCommon
    {
        /// <summary>256 signed 16-bit sine/cosine samples at $A0:B443 (512 bytes).</summary>
        /// <remarks>
        /// Physical view of <see cref="EnemyTrigonometryTables.SignedSine"/>
        /// within the 320-word prefix/full-cycle region. Proof: #625 / #910.
        /// </remarks>
        public const int SignedSineCosineWords = 0xa0b443;
        /// <summary>Thirty-two 16-byte slope-height profiles at $94:8B2B (512 bytes).</summary>
        /// <remarks>Physical alias of <see cref="SlopeHeightDefinitions.Read"/>. Proof: #625 / #914.</remarks>
        public const int SlopeHeightBytes = 0x948b2b;
    }

    /// <summary>Crocomire death graphics-transfer tables.</summary>
    internal static class EnemyRomTablePointersCrocomire
    {
        /// <summary>Seven VRAM destination offsets at $A4:99CB (14 bytes).</summary>
        public const int DeathVramDestinationWords = 0xa499cb;
        /// <summary>Seven source pointers at $A4:99D9 (14 bytes).</summary>
        public const int DeathGraphicsSourceWords = 0xa499d9;
    }

    /// <summary>Dead Sidehopper launch tables.</summary>
    internal static class EnemyRomTablePointersDeadSidehopper
    {
        /// <summary>Four signed vertical velocity words at $A9:D951 (8 bytes).</summary>
        public const int VerticalVelocityWords = 0xa9d951;
        /// <summary>Four signed horizontal velocity words at $A9:D959 (8 bytes).</summary>
        public const int HorizontalVelocityWords = 0xa9d959;
    }

    /// <summary>Bank-$86 falling-spark randomization data.</summary>
    internal static class EnemyRomTablePointersFallingSpark
    {
        /// <summary>Falling-spark horizontal whole-velocity words at $86:F3D4, four-byte stride; seven authored records plus an eighth native overread.</summary>
        public const int HorizontalWholeWords = 0x86f3d4;
        /// <summary>Falling-spark horizontal fractional-velocity words at $86:F3D6, four-byte stride.</summary>
        public const int HorizontalFractionWords = 0x86f3d6;
    }

    /// <summary>Landing Site gunship graphics-transfer tables.</summary>
    internal static class EnemyRomTablePointersGunship
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

    /// <summary>Phantoon movement, timing, palette, and flame-pattern data.</summary>
    internal static class EnemyRomTablePointersPhantoon
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

    /// <summary>Retail populations used to validate Ripper-family OAM mode.</summary>
    internal static class EnemyRomTablePointersRipper
    {
        /// <summary>GRipper room-population record at $A1:B16D.</summary>
        public const int GRipperPopulationRecord = 0xa1b16d;
        /// <summary>Ripper II room-population record at $A1:A48B.</summary>
        public const int Ripper2PopulationRecord = 0xa1a48b;
        /// <summary>Ordinary Ripper room-population record at $A1:9452.</summary>
        public const int RipperPopulationRecord = 0xa19452;
    }

    /// <summary>Bomb/Golden Torizo actor and projectile tables.</summary>
    internal static class EnemyRomTablePointersTorizo
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

    /// <summary>Pose tables selected when an interrupted grapple drops Samus.</summary>
    internal static class SamusGrappleRomDataRelease
    {
        /// <summary>Ten direction-indexed standing release poses.</summary>
        public const int StandingPoseTable = 0x9bc9ba;
        /// <summary>Ten direction-indexed crouching release poses.</summary>
        public const int CrouchingPoseTable = 0x9bc9c4;
    }

    /// <summary>Bank-$94 slope response and alignment tables shared by movement/collision.</summary>
    internal static class SamusMovementRomDataSlopes
    {
        /// <summary>Non-square-slope horizontal velocity multipliers.</summary>
        public const int HorizontalMultipliers = 0x948586;

        /// <summary>Sixteen-pixel height profiles indexed by non-square slope shape.</summary>
        /// <remarks>Physical alias of <see cref="SlopeHeightDefinitions.Read"/>. Proof: #625 / #914.</remarks>
        public const int AlignmentHeights = 0x948b2b;
    }

    /// <summary>Projectile collision geometry read from bank $94.</summary>
    internal static class SamusProjectileRomDataCollision
    {
        /// <summary>Sixteen height bytes for each non-square slope shape.</summary>
        /// <remarks>Physical alias of <see cref="SlopeHeightDefinitions.Read"/>. Proof: #625 / #914.</remarks>
        public const int NonSquareSlopeDefinitions = 0x948b2b;
        /// <summary>Four-quadrant solidity bytes for square slope shapes.</summary>
        /// <remarks>Physical bank-$94 alias of <see cref="SquareSlopeDefinitions.ReadSamusQuadrant"/>. Proof: #625 / #915.</remarks>
        public const int SquareSlopeDefinitions = 0x948e54;
    }
}

namespace SuperMetroid.Core.Input
{
    /// <summary>Bank-91 demo input identities for the intro Mother Brain sequence.</summary>
    internal static class DemoInputRomDataIntroMotherBrain
    {
        /// <summary>Bank-91 intro Mother Brain input object at $91:8784.</summary>
        public const ushort Object = 0x8784;
        /// <summary>Bank-91 input list at $91:8694 consumed by the object.</summary>
        public const ushort InputList = 0x8694;
        /// <summary>Bank-91 record following that input list at $91:86B8.</summary>
        public const ushort NextRecord = 0x86b8;
    }
}
