namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy wrote the camera's X position in the pass (#2571), before the update's ordinary scrolling.</summary>
/// <param name="CameraX">The camera X in room pixels.</param>
public sealed record EnemyCameraXWrite(int CameraX) : EnemyPassEvent;
