namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A generic spin jump and the spins it becomes when Screw Attack or Space Jump is equipped, facing the same way.</summary>
/// <param name="Generic">The generic spin jump.</param>
/// <param name="ScrewAttack">The Screw Attack spin.</param>
/// <param name="SpaceJump">The Space Jump spin.</param>
internal readonly record struct SamusEquippedSpinPoses(SamusPoseId Generic, SamusPoseId ScrewAttack, SamusPoseId SpaceJump);
