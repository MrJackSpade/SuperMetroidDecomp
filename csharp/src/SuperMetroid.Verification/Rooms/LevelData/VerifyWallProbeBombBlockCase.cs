using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Rooms.LevelData;

/// <summary>The first and second of one verify wall probe bomb block case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyWallProbeBombBlockCase(SamusPoseId First, bool Second);
