namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's encounter-mode word at <c>$7E:7802</c> (<c>Ridley.fightMode</c>), shared by the
/// Ceres and Lower Norfair encounters. Native code tests it with <c>BEQ</c>/<c>BMI</c> and
/// <c>CMP #$0002</c>; only these four values are ever stored.
/// </summary>
public enum RidleyFightMode : ushort
{
    /// <summary>Zero: reveal, retreat, or any phase outside active combat (<c>$A6:A148</c>, <c>$A6:A6FC</c>).</summary>
    NotFighting = 0x0000,

    /// <summary>One: ordinary combat (<c>$A6:A6E3</c>, <c>$A6:B317</c>, <c>$A6:BD86</c>).</summary>
    Fighting = 0x0001,

    /// <summary>Two: power-bomb dodge, which suppresses repeated combat interruption (<c>$A6:BD56</c>).</summary>
    PowerBombDodge = 0x0002,

    /// <summary>$FFFF: Lower Norfair death sequence started (<c>$A6:DFC5</c>); the only negative value.</summary>
    Dying = 0xFFFF,
}
