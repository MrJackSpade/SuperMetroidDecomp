namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge definitions consumed by Samus movement and pose selection.</summary>
/// <remarks>
/// Addresses are grouped by their native subsystem rather than by the C# class that happens
/// to read them. Full 24-bit addresses use <see cref="int"/>; values stored natively as
/// same-bank pointers use <see cref="ushort"/> so callers must explicitly supply the bank.
/// Runtime velocity, timers, and selected pointers do not belong in this catalog.
/// </remarks>
public static class SamusMovementRomData
{
    /// <summary>$91:F1FC/$F25E ball-landing handlers: signed whole-speed threshold for the first automatic bounce.</summary>
    public const short FirstBallBounceMinimumSpeed = 3;

    /// <summary>$90:A3C1, SamusMovement_Standing: held-shot animation timer before the animation tick.</summary>
    public const ushort StandingShotAnimationTimer = 16;

    /// <summary>$90:A58D, Samus_Movement_06_Falling: minimum signed whole speed selecting fast-fall art.</summary>
    public const short FastFallAnimationSpeed = 5;
    /// <summary>$90:A58D falling handler: first fast-fall animation frame.</summary>
    public const ushort FastFallAnimationFrame = 5;
    /// <summary>$90:A58D falling handler: fast-fall timer before the animation tick.</summary>
    public const ushort FastFallAnimationTimer = 8;

    /// <summary>Native ROM banks shared by the movement table families.</summary>
    public static class Banks
    {
        /// <summary>Bank containing physics constants, speed tables, and effects metadata.</summary>
        public const int Movement = 0x900000;

        /// <summary>Bank containing pose, animation, and transition tables.</summary>
        public const int Pose = 0x910000;
    }

    /// <summary>Pose definitions, animation lists, and controller-transition streams.</summary>
    public static class Poses
    {
        /// <summary><c>$91:B629 Samus pose definitions</c>, eight bytes per pose.</summary>
        public const int Definitions = 0x91b629;

        /// <summary>Native byte width of one pose-definition record.</summary>
        public const int DefinitionByteCount = 8;

        /// <summary>
        /// $FF in pose-definition byte two: $91:82F9 retains the current pose after
        /// input lookup failure. This is metadata, not an installed Samus pose ID.
        /// </summary>
        public const SamusPoseId RetainCurrentPoseFallback = (SamusPoseId)0xff;

        /// <summary>
        /// $FF in pose-definition byte three: the pose has no shot direction. Hurt and
        /// damage-boost art store it, and $91:E95D lands them through the facing pair.
        /// </summary>
        public const byte NoShotDirection = 0xff;
    }

    /// <summary>Horizontal acceleration, Speed Booster, and animation cadence tables.</summary>
    public static class HorizontalMotion
    {
        /// <summary><c>$90:9F49 kSamusSpeedTable_Normal_X</c>.</summary>
        public const ushort NormalSpeedTable = 0x9f49;

        /// <summary>First ordinary-air movement-type record after the standalone entry.</summary>
        public const ushort NormalAirSpeedTable = NormalSpeedTable + SpeedTableEntry.ByteCount;

        /// <summary>Bank-$90 base selected below water without Gravity Suit.</summary>
        public const ushort WaterSpeedTable = 0xa08d;

        /// <summary>Bank-$90 base selected below lava/acid without Gravity Suit.</summary>
        public const ushort LavaAcidSpeedTable = 0xa1dd;

        /// <summary>High-nibble stage mask used by setup <c>$84:CDEA</c>.</summary>
        public const ushort SpeedBoostStageMask = 0x0f00;

        /// <summary>Stage-four value that marks an actively speed-boosting Samus.</summary>
        public const ushort ActiveSpeedBoostStage = 0x0400;
    }

    /// <summary>Liquid damage, splashes, footsteps, and atmospheric-effect animation data.</summary>
    public static class Environment
    {
        /// <summary>Direct OAM attribute-list pointers indexed by atmospheric-effect type.</summary>
        public const int AtmosphericSpriteAttributeListPointers = 0x908bff;

        /// <summary>Four words per authored direct small-OBJ effect list at $90:8C0F/$90:8C17.</summary>
        public const int DirectAtmosphericFrameCount = 4;

        /// <summary>Retail type-one direct small-OBJ attribute list at $90:8C0F.</summary>
        public const ushort TypeOneAtmosphericAttributes = 0x8c0f;

        /// <summary>Retail shared type-four/six/seven attribute list at $90:8C17.</summary>
        public const ushort SharedAtmosphericAttributes = 0x8c17;
    }
}

/// <summary>Standalone bank-$90 horizontal speed records selected by literal address rather than movement type.</summary>
public enum SamusStandaloneSpeedRecord
{
    /// <summary>$90:9F25 XAccelSpeeds_DiagonalBombJump, used by diagonal bomb jumps.</summary>
    DiagonalBombJump = 0x909f25,
    /// <summary>$90:9F31 XAccelSpeeds_DisconnectGrappleInAir.</summary>
    GrappleReleaseAir = 0x909f31,
    /// <summary>$90:9F3D XAccelSpeeds_DisconnectGrappleInWater.</summary>
    GrappleReleaseWater = 0x909f3d,
    /// <summary>$90:9F49 XAccelSpeeds_DisconnectGrappleInLavaAcid.</summary>
    GrappleReleaseLavaAcid = 0x909f49,
}
