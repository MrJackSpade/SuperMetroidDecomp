using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus;

/// <summary>The pose, stream and bytes of one animation.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="Stream">The stream.</param>
/// <param name="Bytes">The bytes.</param>
internal readonly record struct Animation(SamusPoseId Pose, ushort Stream, byte[] Bytes);
