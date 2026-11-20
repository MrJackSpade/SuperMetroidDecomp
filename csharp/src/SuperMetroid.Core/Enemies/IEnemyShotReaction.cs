namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy that may react to Samus's shots.</summary>
internal interface IEnemyShotReaction
{
    /// <summary>
    /// The stages of the enemy's reaction to Samus's beams and missiles now: its own, the shared one, or the reaction of an
    /// enemy shots pass (<see cref="EnemyShotReaction.ShotsPass"/>).
    /// </summary>
    EnemyShotReaction ShotReaction { get; }
}
