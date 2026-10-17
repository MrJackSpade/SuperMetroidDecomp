namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How one of Torizo's body rectangles reacts when a Samus projectile hits it.</summary>
internal enum TorizoHitboxShot
{
    /// <summary>The active body: the shared shot reaction that Golden Torizo extends with its catch and counterattack.</summary>
    Body = 1,
    /// <summary>The awakening body while standing up or sitting down: inert for Bomb Torizo, guarded direct damage for Golden Torizo.</summary>
    StandUpSitDown,
}
