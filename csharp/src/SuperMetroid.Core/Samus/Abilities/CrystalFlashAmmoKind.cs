namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>
/// The ammunition Crystal Flash is currently converting into energy. The drain empties ten of each kind in this
/// order, missiles first and power bombs last, and finishes after the tenth power bomb.
/// </summary>
public enum CrystalFlashAmmoKind
{
    /// <summary>Ten missiles are consumed first.</summary>
    Missiles,

    /// <summary>Ten super missiles follow the missiles.</summary>
    SuperMissiles,

    /// <summary>Ten power bombs are consumed last; their tenth decrement ends the drain.</summary>
    PowerBombs,
}
