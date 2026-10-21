using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus;

/// <summary>The source, prospective pose and facing of one verify samus gun extended movement case.</summary>
/// <param name="Source">The source.</param>
/// <param name="ProspectivePose">The prospective pose.</param>
/// <param name="Facing">The facing.</param>
internal readonly record struct VerifySamusGunExtendedMovementCase(SamusPoseId Source, SamusPoseId ProspectivePose, string Facing);
