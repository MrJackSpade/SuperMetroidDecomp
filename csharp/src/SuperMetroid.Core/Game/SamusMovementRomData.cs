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
        public const byte RetainCurrentPoseFallback = 0xff;

        /// <summary><c>$91:B010</c>, one bank-$91 animation-delay-list pointer per pose.</summary>
        public const int AnimationDelayListPointers = 0x91b010;

        /// <summary><c>$91:B5D1</c>, pointer to the shared ordinary-running delay list.</summary>
        public const int DefaultRunningAnimationDelayListPointer = 0x91b5d1;

        /// <summary><c>$91:9EE2</c>, one bank-$91 input-transition-list pointer per pose.</summary>
        public const int TransitionListPointers = 0x919ee2;
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

        /// <summary><c>$91:B61F</c>, initial/next low byte for each Speed Booster stage.</summary>
        public const int SpeedBoostCounterLowBytes = 0x91b61f;

        /// <summary><c>$91:B5DE</c>, animation-delay-list pointer for each boost stage.</summary>
        public const int SpeedBoostAnimationDelayListPointers = 0x91b5de;

        /// <summary>High-nibble stage mask used by setup <c>$84:CDEA</c>.</summary>
        public const ushort SpeedBoostStageMask = 0x0f00;

        /// <summary>Stage-four value that marks an actively speed-boosting Samus.</summary>
        public const ushort ActiveSpeedBoostStage = 0x0400;
    }

    /// <summary>Vertical launch velocities and gravity constants in bank $90.</summary>
    public static class VerticalMotion
    {
        /// <summary>Normal-jump speed selected by the current liquid medium.</summary>
        public const int NormalJumpSpeeds = 0x909eb9;

        /// <summary>Normal-jump subspeed selected by the current liquid medium.</summary>
        public const int NormalJumpSubspeeds = 0x909ebf;

        /// <summary>Hi-Jump speed selected by the current liquid medium.</summary>
        public const int HiJumpSpeeds = 0x909ec5;

        /// <summary>Hi-Jump subspeed selected by the current liquid medium.</summary>
        public const int HiJumpSubspeeds = 0x909ecb;

        /// <summary>Wall-jump speed selected by the current liquid medium.</summary>
        public const int WallJumpSpeeds = 0x909ed1;

        /// <summary>Wall-jump subspeed selected by the current liquid medium.</summary>
        public const int WallJumpSubspeeds = 0x909ed7;

        /// <summary>Hi-Jump wall-jump speed selected by the current liquid medium.</summary>
        public const int HiWallJumpSpeeds = 0x909edd;

        /// <summary>Hi-Jump wall-jump subspeed selected by the current liquid medium.</summary>
        public const int HiWallJumpSubspeeds = 0x909ee3;

        /// <summary>Knockback launch speed selected by the current liquid medium.</summary>
        public const int KnockbackSpeeds = 0x909ee9;

        /// <summary>Knockback launch subspeed selected by the current liquid medium.</summary>
        public const int KnockbackSubspeeds = 0x909eef;

        /// <summary>Bomb-jump launch speed selected by the current liquid medium.</summary>
        public const int BombJumpSpeeds = 0x909ef5;

        /// <summary>Bomb-jump launch subspeed selected by the current liquid medium.</summary>
        public const int BombJumpSubspeeds = 0x909efb;

        /// <summary>Dry-air, water, and lava/acid fractional gravity words.</summary>
        public const int GravitySubaccelerations = 0x909ea1;

        /// <summary>Dry-air, water, and lava/acid whole gravity words.</summary>
        public const int GravityAccelerations = 0x909ea7;

        /// <summary>Initial falling speed used by unmorph and Spring Ball transitions.</summary>
        public const int FallingTransitionSpeed = 0x909eb5;

        /// <summary>Initial falling subspeed used by unmorph and Spring Ball transitions.</summary>
        public const int FallingTransitionSubspeed = 0x909eb7;

        /// <summary>Standalone horizontal speed record used by diagonal bomb jumps.</summary>
        public const int DiagonalBombJumpHorizontalSpeed = 0x909f25;

        /// <summary>Grapple-release horizontal speed records for air.</summary>
        public const int GrappleReleaseAirSpeed = 0x909f31;

        /// <summary>Grapple-release horizontal speed records for water.</summary>
        public const int GrappleReleaseWaterSpeed = 0x909f3d;

        /// <summary>Grapple-release horizontal speed records for lava and acid.</summary>
        public const int GrappleReleaseLavaAcidSpeed = 0x909f49;
    }

    /// <summary>Liquid damage, splashes, footsteps, and atmospheric-effect animation data.</summary>
    public static class Environment
    {
        /// <summary>Running animation frames that emit a footstep atmospheric effect.</summary>
        public const int RunningFootstepFrames = 0x90a424;

        /// <summary>Movement-type table selecting grounded versus airborne water splashes.</summary>
        public const int WaterSplashTypes = 0x9081a4;

        /// <summary>Crateria room-index table selecting special footstep types.</summary>
        public const int CrateriaFootstepTypes = 0x90edc9;

        /// <summary>Per-frame fractional lava damage.</summary>
        public const int LavaSubdamagePerFrame = 0x909e8b;

        /// <summary>Per-frame whole lava damage.</summary>
        public const int LavaDamagePerFrame = 0x909e8d;

        /// <summary>Per-frame fractional acid damage.</summary>
        public const int AcidSubdamagePerFrame = 0x909e8f;

        /// <summary>Per-frame whole acid damage.</summary>
        public const int AcidDamagePerFrame = 0x909e91;

        /// <summary>Room-index flags controlling rain and other atmospheric effects.</summary>
        public const int RoomAtmosphericEffectFlags = 0x91f0f3;

        /// <summary>Animation-timer-list pointers indexed by atmospheric-effect type.</summary>
        public const int AtmosphericAnimationTimerListPointers = 0x908b93;

        /// <summary>Animation frame counts indexed by atmospheric-effect type.</summary>
        public const int AtmosphericAnimationFrameCounts = 0x908bef;

        /// <summary>Direct OAM attribute-list pointers indexed by atmospheric-effect type.</summary>
        public const int AtmosphericSpriteAttributeListPointers = 0x908bff;
    }

    /// <summary>Bank-$94 slope response and alignment tables shared by movement/collision.</summary>
    public static class Slopes
    {
        /// <summary>Non-square-slope horizontal velocity multipliers.</summary>
        public const int HorizontalMultipliers = 0x948586;

        /// <summary>Sixteen-pixel height profiles indexed by non-square slope shape.</summary>
        public const int AlignmentHeights = 0x948b2b;
    }
}
