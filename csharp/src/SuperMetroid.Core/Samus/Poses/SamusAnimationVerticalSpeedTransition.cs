namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Publishes one of two transitional poses, chosen by whether Samus is moving vertically. No retail animation holds it as
/// a step of its own: the animation reads it only where a frame lands on an operand whose value selects it (#1535).
/// </summary>
/// <param name="Grounded">The pose taken while not moving vertically.</param>
/// <param name="Airborne">The pose taken while moving vertically.</param>
internal sealed record SamusAnimationVerticalSpeedTransition(SamusPoseId Grounded, SamusPoseId Airborne) : SamusAnimationCue;
