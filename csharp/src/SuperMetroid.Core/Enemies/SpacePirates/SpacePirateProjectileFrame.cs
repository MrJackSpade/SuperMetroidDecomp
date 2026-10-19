namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>A laser or claw pose: the left laser's thirteen, the right laser's thirteen, then each claw's eight.</summary>
internal readonly record struct SpacePirateProjectileFrame(int Pose);
