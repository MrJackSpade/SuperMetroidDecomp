using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Movement policies selected by Samus's morphed, spin-jumping or damage-boosting movement.</summary>
/// <remarks>
/// Each movement's flags: one controls grab eligibility,
/// the other controls morphed release timing. Those are the only consumed fields in the two
/// managed callers. No other flags or adjacent-table reads are exposed by their contract.
/// </remarks>
internal static class RidleySamusInteractionDefinitions
{
    /// <summary>Separates Samus's movement owners by Ridley's grab eligibility and release delay.</summary>
    internal enum GrabPolicy
    {
        /// <summary>Ridley's contact cannot start a grab under this movement owner.</summary>
        Immune,
        /// <summary>A non-ball grab uses the longer post-release intangibility interval.</summary>
        Ordinary,
        /// <summary>A ball-form grab uses the shorter post-release intangibility interval.</summary>
        Morphed
    }

    /// <summary>Classifies each of the 28 Samus movement types by grab policy.</summary>
    internal static readonly Dictionary<SamusMovementType, GrabPolicy> GrabPolicies = new()
    {
        [SamusMovementType.MorphBallGround] = GrabPolicy.Morphed,
        [SamusMovementType.GlitchBall] = GrabPolicy.Morphed,
        [SamusMovementType.MorphBallFalling] = GrabPolicy.Morphed,
        [SamusMovementType.GlitchBallKnockback] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallGround] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallInAir] = GrabPolicy.Morphed,
        [SamusMovementType.SpringBallFalling] = GrabPolicy.Morphed,
        [SamusMovementType.Standing] = GrabPolicy.Ordinary,
        [SamusMovementType.Running] = GrabPolicy.Ordinary,
        [SamusMovementType.NormalJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.Crouching] = GrabPolicy.Ordinary,
        [SamusMovementType.Falling] = GrabPolicy.Ordinary,
        [SamusMovementType.Knockback] = GrabPolicy.Ordinary,
        [SamusMovementType.CompactAirborne] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningOnGround] = GrabPolicy.Ordinary,
        [SamusMovementType.PostureTransition] = GrabPolicy.Ordinary,
        [SamusMovementType.Moonwalking] = GrabPolicy.Ordinary,
        [SamusMovementType.WallJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.RanIntoWall] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningWhileJumping] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningWhileFalling] = GrabPolicy.Ordinary,
        [SamusMovementType.Special] = GrabPolicy.Ordinary,
        [SamusMovementType.SpinJumping] = GrabPolicy.Immune,
        [SamusMovementType.Grappling] = GrabPolicy.Immune,
        [SamusMovementType.DamageBoost] = GrabPolicy.Immune,
        [SamusMovementType.DraygonHeld] = GrabPolicy.Immune,
    };

    /// <summary>The release loads6 frames for morphed overflow.</summary>
    internal const int MorphedReleaseIntangibilityFrames = 6;

    /// <summary>The release otherwise loads10.</summary>
    internal const int OrdinaryReleaseIntangibilityFrames = 10;
}
