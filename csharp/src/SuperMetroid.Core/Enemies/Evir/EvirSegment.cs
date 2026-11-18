namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The Evir body's and arms' facing loops and its projectile's spit and regeneration animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="EvirAnimations"/>.
/// </summary>
internal enum EvirSegment
{
    /// <summary>
    /// The body facing left, its legs tucking, curling, lowering, hanging, stretching and lifting, repeating.
    /// </summary>
    BodyFacingLeft,
    /// <summary>
    /// One wave of the arms facing left: reaching, curling, folding and spreading. Their animation waves four times,
    /// then rests folded, repeating.
    /// </summary>
    ArmsFacingLeft,
    /// <summary>The arms facing left resting folded after their waves.</summary>
    ArmsFoldedFacingLeft,
    /// <summary>
    /// The body facing right, its legs tucking, curling, lowering, hanging, stretching and lifting, repeating.
    /// </summary>
    BodyFacingRight,
    /// <summary>
    /// One wave of the arms facing right: reaching, curling, folding and spreading. Their animation waves four times,
    /// then rests folded, repeating.
    /// </summary>
    ArmsFacingRight,
    /// <summary>The arms facing right resting folded after their waves.</summary>
    ArmsFoldedFacingRight,
    /// <summary>Its spine ball, held while it flies.</summary>
    ProjectileNormal,
    /// <summary>A regenerating spit's start at the mouth, its entry cue.</summary>
    ProjectileRegenerating,
    /// <summary>A regenerating spit's sound, once it starts back at the mouth.</summary>
    ProjectileRegeneratingSound,
    /// <summary>One step of a regenerating spit's regrowth out from the mouth, which its regeneration repeats.</summary>
    ProjectileRegrowing,
    /// <summary>The regenerated spit, whole again.</summary>
    ProjectileRegrown,
}
