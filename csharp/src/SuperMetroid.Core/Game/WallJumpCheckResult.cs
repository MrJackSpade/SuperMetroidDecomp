namespace SuperMetroid.Core.Game;

/// <summary>Wall probe outcome including contact distance even when the launch chord does not trigger.</summary>
internal readonly record struct WallJumpCheckResult(
    bool Triggered,
    bool Contact,
    ushort Distance);
