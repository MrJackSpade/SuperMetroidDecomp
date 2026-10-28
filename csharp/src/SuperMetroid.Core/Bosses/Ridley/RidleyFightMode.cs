namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's encounter mode, shared by the Ceres and Lower Norfair encounters: whether he is outside combat, fighting,
/// dodging a power bomb or dying, which decides whether hits interrupt him, his tail attacks and his death starts. The
/// cartridge's word for each mode is verification's <c>RidleyNativeWords.FightModes</c>.
/// </summary>
public enum RidleyFightMode
{
    /// <summary>Reveal, retreat, or any phase outside active combat.</summary>
    NotFighting,

    /// <summary>Ordinary combat.</summary>
    Fighting,

    /// <summary>Power-bomb dodge, which suppresses repeated combat interruption.</summary>
    PowerBombDodge,

    /// <summary>The Lower Norfair death sequence has started.</summary>
    Dying,
}
