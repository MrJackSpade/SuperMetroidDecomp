namespace SuperMetroid.Core.Gameplay;

/// <summary>Who owns Samus's drawing in an actor pass.</summary>
internal enum SamusDrawOwner
{
    /// <summary>Ordinary gameplay draws Samus, her projectiles and the enemy layers.</summary>
    Gameplay,

    /// <summary>The death sequence owns Samus; projectiles, explosions and enemies are not drawn.</summary>
    DeathSequence,
}
