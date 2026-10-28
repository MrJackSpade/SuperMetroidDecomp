namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Native equipment-screen categories: the low byte of the selector index ($82:AC58 dispatch).</summary>
public enum PauseEquipmentCategory
{
    /// <summary>Reserve-tank controls, selector byte zero.</summary>
    Reserves = 0,
    /// <summary>$82:AFBE Weapons category, selector byte one.</summary>
    Beams = 1,
    /// <summary>$82:B0C2 Suit category, selector byte two.</summary>
    Suits = 2,
    /// <summary>$82:B150 Boots category, selector byte three.</summary>
    Boots = 3,
}
