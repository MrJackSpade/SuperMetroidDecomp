namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One cue of Zoa's shooting animation: the speed its launch moves at from then on (#1841).</summary>
/// <param name="LaunchSpeed">The launch's speed in pixels per frame.</param>
internal readonly record struct ZoaCue(SignedSixteenSixteen LaunchSpeed);
