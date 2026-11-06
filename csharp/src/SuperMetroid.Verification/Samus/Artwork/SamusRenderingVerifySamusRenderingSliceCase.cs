using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The pose, movement type, frame, expected body halves and name of one Samus body-half routing case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="MovementType">The movement type.</param>
/// <param name="Frame">The frame.</param>
/// <param name="ExpectedHalves">The body halves the frame draws: one for the top half alone, two for the top and bottom halves.</param>
/// <param name="Name">The name.</param>
internal readonly record struct SamusRenderingVerifySamusRenderingSliceCase(SamusPoseId Pose, byte MovementType, ushort Frame, int ExpectedHalves, string Name);
