using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Movement policies selected by CheckIfSamusMorphedSpinJumpingDamageBoosting at $A6:BCF1.</summary>
/// <remarks>
/// The BIT operand uses the high byte of $A6:BD04..BD1F: bit7 controls grab eligibility,
/// bit6 controls morphed release timing. Those are the only consumed fields in the two
/// managed callers. No lower flag bits or adjacent-table reads are exposed by their contract.
/// </remarks>
internal static class RidleySamusInteractionDefinitions
{
    /// <summary>Separates movement handlers by Ridley's grab eligibility and release delay.</summary>
    internal enum GrabPolicy
    {
        /// <summary>Ridley's contact cannot start a grab in this movement handler.</summary>
        Immune,
        /// <summary>A non-ball grab uses the longer post-release intangibility interval.</summary>
        Ordinary,
        /// <summary>A ball-form grab uses the shorter post-release intangibility interval.</summary>
        Morphed
    }

    /// <summary>$A6:BD04..BD1F classifies each of the 28 Samus movement types by grab policy.</summary>
    internal static readonly Dictionary<SamusMovementType, GrabPolicy> GrabPolicies = new()
    {
        [SamusMovementType.MorphBallGround] = GrabPolicy.Morphed,
        [SamusMovementType.UnusedGlitchBall] = GrabPolicy.Morphed,
        [SamusMovementType.MorphBallFalling] = GrabPolicy.Morphed,
        [SamusMovementType.UnusedGlitchBallAlternate] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallGround] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallInAir] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallFalling] = GrabPolicy.Morphed,
        [SamusMovementType.Standing] = GrabPolicy.Ordinary,
        [SamusMovementType.Running] = GrabPolicy.Ordinary,
        [SamusMovementType.NormalJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.Crouching] = GrabPolicy.Ordinary,
        [SamusMovementType.Falling] = GrabPolicy.Ordinary,
        [SamusMovementType.Knockback] = GrabPolicy.Ordinary,
        [SamusMovementType.Unused0D] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningOnGround] = GrabPolicy.Ordinary,
        [SamusMovementType.PostureTransition] = GrabPolicy.Ordinary,
        [SamusMovementType.Moonwalking] = GrabPolicy.Ordinary,
        [SamusMovementType.WallJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.RanIntoWall] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningWhileJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningWhileFalling] = GrabPolicy.Ordinary,
        [SamusMovementType.Special] = GrabPolicy.Ordinary,
        [SamusMovementType.SpinJumping] = GrabPolicy.Immune,
        [SamusMovementType.Unused0B] = GrabPolicy.Immune,
        [SamusMovementType.Unused0C] = GrabPolicy.Immune,
        [SamusMovementType.Grappling] = GrabPolicy.Immune,
        [SamusMovementType.DamageBoost] = GrabPolicy.Immune,
        [SamusMovementType.DraygonHeld] = GrabPolicy.Immune,
    };

    /// <summary>$A6:BC98 loads6 frames for morphed overflow.</summary>
    internal const ushort MorphedReleaseIntangibilityFrames = 6;

    /// <summary>$A6:BC9D otherwise loads10.</summary>
    internal const ushort OrdinaryReleaseIntangibilityFrames = 10;
}
