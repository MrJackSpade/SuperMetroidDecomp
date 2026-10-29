namespace SuperMetroid.Core.Hud;

/// <summary>What the HUD's AUTO reserve indicator shows once something has drawn it (#1466).</summary>
public enum HudReserveIndicator
{
    /// <summary>AUTO with its arrows, with energy in the reserve tanks.</summary>
    Filled,

    /// <summary>AUTO with its arrows, with the reserve tanks empty.</summary>
    Empty,

    /// <summary>Cleared by switching the reserve tanks to manual.</summary>
    Cleared,
}
