namespace SuperMetroid.Core.Samus.Poses;

/// <summary>The spins a generic spin jump becomes when Screw Attack or Space Jump is equipped, facing the same way.</summary>
/// <param name="ScrewAttack">The Screw Attack spin.</param>
/// <param name="SpaceJump">The Space Jump spin.</param>
internal readonly record struct SamusEquippedSpinPoses(SamusPoseId ScrewAttack, SamusPoseId SpaceJump);
