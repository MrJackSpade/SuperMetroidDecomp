namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The movement the pose movement dispatch selects for Samus's pose by its movement type (#2553), handed to the step that
/// serves it. Each movement is its own type, so a step is handed only a movement it serves and reads the movement type
/// from it, never checking Samus's pose again for a selection the dispatch already made.
/// </summary>
internal abstract class SamusMovement
{
    /// <summary>Only the movements declared here exist.</summary>
    /// <param name="type">The movement type the movement is.</param>
    private SamusMovement(SamusMovementType type) => Type = type;

    /// <summary>The movement type the movement is.</summary>
    internal SamusMovementType Type { get; }

    /// <summary>The normal jump's movement.</summary>
    internal static NormalJumpingMovement NormalJumping { get; } = new();

    /// <summary>The spin jump's movement.</summary>
    internal static SpinJumpingMovement SpinJumping { get; } = new();

    /// <summary>The wall jump's movement.</summary>
    internal static WallJumpingMovement WallJumping { get; } = new();

    /// <summary>The damage boost's movement.</summary>
    internal static DamageBoostMovement DamageBoost { get; } = new();

    /// <summary>The fall's movement.</summary>
    internal static FallingMovement Falling { get; } = new();

    /// <summary>The moonwalk's movement.</summary>
    internal static MoonwalkingMovement Moonwalking { get; } = new();

    /// <summary>
    /// The rising or aimed crouched turn's movement in the air: a jumping turn, or an aimed crouched turn moving up or down.
    /// </summary>
    internal static AerialTurnMovement TurningRisingOrAimedCrouched { get; } = new(SamusMovementType.TurningRisingOrAimedCrouched);

    /// <summary>The falling turn's movement.</summary>
    internal static AerialTurnMovement TurningWhileFalling { get; } = new(SamusMovementType.TurningWhileFalling);

    /// <summary>The normal jump's movement (<see cref="SamusMovementType.NormalJumping"/>).</summary>
    internal sealed class NormalJumpingMovement : SamusMovement
    {
        /// <summary>The one normal jump's movement.</summary>
        internal NormalJumpingMovement()
            : base(SamusMovementType.NormalJumping)
        {
        }
    }

    /// <summary>The spin jump's movement (<see cref="SamusMovementType.SpinJumping"/>).</summary>
    internal sealed class SpinJumpingMovement : SamusMovement
    {
        /// <summary>The one spin jump's movement.</summary>
        internal SpinJumpingMovement()
            : base(SamusMovementType.SpinJumping)
        {
        }
    }

    /// <summary>The wall jump's movement (<see cref="SamusMovementType.WallJumping"/>), which only wall-jump poses move by.</summary>
    internal sealed class WallJumpingMovement : SamusMovement
    {
        /// <summary>The one wall jump's movement.</summary>
        internal WallJumpingMovement()
            : base(SamusMovementType.WallJumping)
        {
        }
    }

    /// <summary>The damage boost's movement (<see cref="SamusMovementType.DamageBoost"/>), which only damage-boost poses move by.</summary>
    internal sealed class DamageBoostMovement : SamusMovement
    {
        /// <summary>The one damage boost's movement.</summary>
        internal DamageBoostMovement()
            : base(SamusMovementType.DamageBoost)
        {
        }
    }

    /// <summary>The fall's movement (<see cref="SamusMovementType.Falling"/>).</summary>
    internal sealed class FallingMovement : SamusMovement
    {
        /// <summary>The one fall's movement.</summary>
        internal FallingMovement()
            : base(SamusMovementType.Falling)
        {
        }
    }

    /// <summary>The moonwalk's movement (<see cref="SamusMovementType.Moonwalking"/>), which only the stable moonwalk poses move by.</summary>
    internal sealed class MoonwalkingMovement : SamusMovement
    {
        /// <summary>The one moonwalk's movement.</summary>
        internal MoonwalkingMovement()
            : base(SamusMovementType.Moonwalking)
        {
        }
    }

    /// <summary>
    /// A turn's movement in the air: <see cref="SamusMovementType.TurningRisingOrAimedCrouched"/>, whose poses are the
    /// jumping turns and the aimed crouched turns, or <see cref="SamusMovementType.TurningWhileFalling"/>, whose poses are
    /// the falling turns.
    /// </summary>
    internal sealed class AerialTurnMovement : SamusMovement
    {
        /// <summary>The turn's movement in the air of <paramref name="type"/>.</summary>
        /// <param name="type">One of the two turning movement types that turn in the air.</param>
        internal AerialTurnMovement(SamusMovementType type)
            : base(type)
        {
        }
    }
}
