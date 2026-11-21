namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The belly glow a Torizo's fight start lights (#1700, #2553): each watches the Torizo it lights and ends with its defeat
/// (<see cref="ColourEffectCondition.EndsWhenTorizoDies"/>).
/// </summary>
public enum TorizoBellyGlow
{
    /// <summary>The Bomb Torizo's belly glow.</summary>
    BombTorizo,

    /// <summary>The Golden Torizo's belly glow.</summary>
    GoldenTorizo,
}
