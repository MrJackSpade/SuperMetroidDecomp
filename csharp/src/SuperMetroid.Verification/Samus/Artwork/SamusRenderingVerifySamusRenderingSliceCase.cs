using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The pose, movement type, frame, expected sprites and name of one samus rendering verify samus rendering slice case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="MovementType">The movement type.</param>
/// <param name="Frame">The frame.</param>
/// <param name="ExpectedSprites">The expected sprites.</param>
/// <param name="Name">The name.</param>
internal readonly record struct SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId Pose, byte MovementType, ushort Frame, int ExpectedSprites, string Name);
