namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>
/// One platform's two native dispatcher indexes: the patrol direction
/// (<c>Platform.XMovementFunctionIndex</c>, <c>$7E:7804,x</c>) and the rider-selected vertical
/// movement (<c>Platform.YMovementFunctionIndex</c>, <c>$7E:7800,x</c>).
/// </summary>
public readonly record struct PlatformMovementFunctions(
    PlatformHorizontalMovement Horizontal,
    PlatformVerticalMovement Vertical);
