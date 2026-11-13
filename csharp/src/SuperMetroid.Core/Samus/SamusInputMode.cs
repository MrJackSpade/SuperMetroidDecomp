namespace SuperMetroid.Core.Samus;

/// <summary>
/// How the input phase of Samus's frame, the first of its two, runs: whether her input is handled. Samus's control setups
/// select it.
/// </summary>
internal enum SamusInputMode
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
