namespace SuperMetroid.Core.Samus.Movement;

/// <summary>How the way Samus moves takes a hit's knockback (#2445). Its values are arbitrary.</summary>
internal enum SamusKnockbackResponse
{
    /// <summary>An upright Samus takes the knockback pose facing her way, and its direction from the hit's side and held forward.</summary>
    Upright,
    /// <summary>A Morph or Spring Ball keeps its pose and is knocked up and back the way it faces, whatever the hit's side.</summary>
    Ball,
    /// <summary>The glitch ball takes the glitch ball's knockback pose and is then knocked back as an upright Samus is.</summary>
    GlitchBall,
    /// <summary>The movement keeps the hit's hurt countdown but refuses its knockback: turns, posture changes, the Grapple and the like.</summary>
    Refused,
}
