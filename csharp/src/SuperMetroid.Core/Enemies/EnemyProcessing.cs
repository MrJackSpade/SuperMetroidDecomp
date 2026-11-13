namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the room's enemy passes run for an enemy: whether its AI and animation run while it is off screen, and whether
/// its timed animation runs at all. Its population entry starts it and its behaviour toggles it; each is an
/// independent fact.
/// </summary>
[Flags]
public enum EnemyProcessing
{
    /// <summary>The enemy runs only while on screen and its animation does not run.</summary>
    None = 0,

    /// <summary>Allows enemy AI and animation to run when the enemy lies outside the active screen bounds.</summary>
    OffScreen = 1 << 0,

    /// <summary>Plays the enemy's animation each update.</summary>
    Animates = 1 << 1,
}
