namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The setup a palette effect runs when it starts.
/// Its values are arbitrary; verification's view names each member's original identity (#1415).
/// </summary>
public enum PaletteFxSetup
{
    /// <summary>No setup.</summary>
    Null = 1,
    /// <summary>The intro's setup.</summary>
    Intro,
    /// <summary>Norfair's setup, which chooses the heat program for Samus's suit.</summary>
    Norfair,
    /// <summary>Brinstar's setup.</summary>
    Brinstar,
}
