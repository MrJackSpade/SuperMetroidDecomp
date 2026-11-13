namespace SuperMetroid.Core.Samus;

/// <summary>The first half of Samus's per-frame work, which Samus's control setups select: whether her input is handled.</summary>
internal enum SamusAlphaHandler
{
    /// <summary>
    /// Ordinary control: her radius, HUD selection, block-inside reactions, pose input, bombs and projectiles.
    /// </summary>
    Normal,

    /// <summary>
    /// Locked, as while a suit is collected or Samus dies: none of the ordinary input handling runs.
    /// </summary>
    Locked,
}
