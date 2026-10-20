namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Ordered spawner events; repeating and final waves may occur on the same call.</summary>
[IndependentFlag("The initial, repeating and final spawns are separate schedule checks on one frame; any subset can fire together.")]
internal readonly record struct CeresExplosionSpawnEvents(bool Initial, bool Repeating, bool Final);
