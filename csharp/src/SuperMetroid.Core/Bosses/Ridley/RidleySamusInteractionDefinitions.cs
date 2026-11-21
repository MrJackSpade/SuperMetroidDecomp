using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Movement policies selected by Samus's morphed, spin-jumping or damage-boosting movement.</summary>
/// <remarks>Each movement decides two things only: whether Ridley may grab Samus, and how long she stays out of reach after he releases her.</remarks>
internal static partial class RidleySamusInteractionDefinitions
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

    /// <summary>Classifies each of Samus's 26 movement types by grab policy.</summary>
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
        [SamusMovementType.TurningRisingOrAimedCrouched] = GrabPolicy.Ordinary,
        [SamusMovementType.TurningWhileFalling] = GrabPolicy.Ordinary,
        [SamusMovementType.Special] = GrabPolicy.Ordinary,
        [SamusMovementType.SpinJumping] = GrabPolicy.Immune,
        [SamusMovementType.Grappling] = GrabPolicy.Immune,
        [SamusMovementType.DamageBoost] = GrabPolicy.Immune,
        [SamusMovementType.DraygonHeld] = GrabPolicy.Immune,
    };

    /// <summary>How long Samus stays out of Ridley's reach after he releases her in ball form: six frames.</summary>
    internal const int MorphedReleaseIntangibilityFrames = 6;

    /// <summary>How long Samus stays out of Ridley's reach after he releases her in any other form: ten frames.</summary>
    internal const int OrdinaryReleaseIntangibilityFrames = 10;
}
