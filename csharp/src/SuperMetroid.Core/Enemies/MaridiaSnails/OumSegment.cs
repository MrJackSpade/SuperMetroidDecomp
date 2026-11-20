namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// Oum's idle, attacking and rolling animations (#641): one segment per animation a spawn, branch or wake enters, in authored order.
/// Their steps are <see cref="OumAnimations"/>.
/// </summary>
public enum OumSegment
{
    /// <summary>Oum facing left, upright, held.</summary>
    FacingLeftIdle,
    /// <summary>
    /// Oum facing left opening its vent with a splash and jetting water out longer and shorter again, before the attack
    /// ends.
    /// </summary>
    FacingLeftAttacking,
    /// <summary>
    /// Oum facing left rolling forward an eighth of a turn each pose from its vent-open pose, repeating.
    /// </summary>
    FacingLeftRollingForwards,
    /// <summary>
    /// Oum facing left rolling backward an eighth of a turn each pose to its vent-open pose, repeating.
    /// </summary>
    FacingLeftRollingBackwards,
    /// <summary>Oum facing right, upright, held.</summary>
    FacingRightIdle,
    /// <summary>
    /// Oum facing right opening its vent with a splash and jetting water out longer and shorter again, before the
    /// attack ends.
    /// </summary>
    FacingRightAttacking,
    /// <summary>
    /// Oum facing right rolling forward an eighth of a turn each pose from its vent-open pose, repeating.
    /// </summary>
    FacingRightRollingForwards,
    /// <summary>
    /// Oum facing right rolling backward an eighth of a turn each pose to its vent-open pose, repeating.
    /// </summary>
    FacingRightRollingBackwards,
}
