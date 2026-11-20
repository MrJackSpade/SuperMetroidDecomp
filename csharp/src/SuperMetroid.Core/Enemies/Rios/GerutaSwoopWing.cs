using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One wing pose of a Geruta's swoop start (#2557), with where its flames sit and which way they burn.</summary>
/// <param name="Wings">The wing pose shown.</param>
/// <param name="FlameOffset">How far the flames sit from the Geruta, in pixels.</param>
/// <param name="Flames">Which way the flames burn.</param>
internal readonly record struct GerutaSwoopWing(EnemyFrameId Wings, int FlameOffset, GerutaFlames Flames);
