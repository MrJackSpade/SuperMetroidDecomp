using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The Baby Metroid moved the view to a camera X during the enemy pass, before the ordinary scrolling later in the update
/// (#2562); the camera and the foreground take it without losing the camera's fraction of a pixel or its ideal position.
/// </summary>
/// <param name="CameraX">The camera X in room pixels.</param>
public sealed record BabyMetroidCameraMove(int CameraX) : EnemyPassEvent;
