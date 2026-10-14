namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Ordered spawner events; repeating and final waves may occur on the same call.</summary>
internal readonly record struct CeresExplosionSpawnEvents(bool Initial, bool Repeating, bool Final);
