using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>The pose and direction of one create space jump and screw attack cartridge case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct CreateSpaceJumpAndScrewAttackCartridgeCase(SamusPoseId Pose, byte Direction);
