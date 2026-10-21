namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The shoot frame, aim frame and release left of one verify gate jump traces case.</summary>
/// <param name="ShootFrame">The shoot frame.</param>
/// <param name="AimFrame">The aim frame.</param>
/// <param name="ReleaseLeft">The release left.</param>
internal readonly record struct VerifyGateJumpTracesCase(int ShootFrame, int AimFrame, bool ReleaseLeft);
