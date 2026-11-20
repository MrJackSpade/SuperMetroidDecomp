namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>The eye that cast the Morph Ball eye beam (#2554): its place, whose position the cone starts from, and its state, whose aim it follows.</summary>
/// <param name="Body">The eye body's place.</param>
/// <param name="Eye">The eye's state.</param>
internal sealed record MorphBallEyeBeamCaster(RoomEnemyPlace Body, MorphBallEyeEnemyState Eye);
