namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>A drawn Botwoon projectile frame: kind, facing (body and tail) and phase (body and spit).</summary>
internal readonly record struct BotwoonProjectileFrame(BotwoonProjectileFrameKind Kind, BotwoonFacing Facing, int Phase);
