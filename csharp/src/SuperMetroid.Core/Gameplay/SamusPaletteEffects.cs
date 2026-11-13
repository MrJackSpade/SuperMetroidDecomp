namespace SuperMetroid.Core.Gameplay;

/// <summary>Whether an actor pass advances Samus's palette effects.</summary>
internal enum SamusPaletteEffects
{
    /// <summary>The palette effects advance.</summary>
    Advance,

    /// <summary>The palette effects hold, as under a movement mode that does not dispatch them.</summary>
    Hold,
}
