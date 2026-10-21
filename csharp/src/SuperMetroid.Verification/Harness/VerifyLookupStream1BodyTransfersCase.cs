using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Harness;

/// <summary>The pose and phase of one verify lookup stream1 body transfers case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Phase">The phase.</param>
internal readonly record struct VerifyLookupStream1BodyTransfersCase(SamusPoseId Pose, ushort Phase);
