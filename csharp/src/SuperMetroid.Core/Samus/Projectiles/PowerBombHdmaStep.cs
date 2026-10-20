namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether a bomb frame advances the power-bomb HDMA explosion.</summary>
public enum PowerBombHdmaStep
{
    /// <summary>The explosion advances.</summary>
    Advance,

    /// <summary>Another owner advances it this frame.</summary>
    Skip,
}
