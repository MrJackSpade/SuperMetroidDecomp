namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>How an Etecoon starts: its heading, the hops it teaches and its route (#1472, #1519).</summary>
/// <param name="Heading">The way it first heads.</param>
/// <param name="TeachingHops">The teaching hops it counts as already taken.</param>
/// <param name="Route">The route it takes after its first landing.</param>
/// <param name="QuakePauseFrames">Updates it starts paused for, as an earthquake pauses it.</param>
internal sealed record EtecoonSpawnSettings(HorizontalDirection Heading, int TeachingHops, EtecoonRoute Route, int QuakePauseFrames) : EnemySpawnSettings;
