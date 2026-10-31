namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Equipment-screen categories: the low byte of the selector index.</summary>
public enum PauseEquipmentCategory
{
    /// <summary>Reserve-tank controls, selector byte zero.</summary>
    Reserves = 0,
    /// <summary>Weapons category, selector byte one.</summary>
    Beams = 1,
    /// <summary>Suit category, selector byte two.</summary>
    Suits = 2,
    /// <summary>Boots category, selector byte three.</summary>
    Boots = 3,
}
