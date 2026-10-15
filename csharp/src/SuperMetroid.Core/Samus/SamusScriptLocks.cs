using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Samus;

/// <summary>Script ownership of Samus's movement: the stationary script control lock, the refill-station lock and the Rainbow Beam handlers.</summary>
internal sealed class SamusScriptLocks
{
    /// <summary>Samus owner whose handler and input-lock state the script commands update.</summary>
    private readonly SamusState _samus;

    /// <summary>Binds script lock commands to the active Samus actor.</summary>
    internal SamusScriptLocks(SamusState owner)
    {
        _samus = owner;
    }
    /// <summary>
    /// Command-zero ownership of the stationary alpha/beta pair. This is distinct
    /// from input suppression by moving cinematic, elevator, or enemy handlers.
    /// The stationary beta updates contact/minimap state but does not animate Samus.
    /// </summary>
    public bool StationaryScriptControlLocked { get; internal set; }

    /// <summary>
    /// Applies the paired lock/unlock commands at $90:F109/$90:F117. Command zero
    /// installs alpha $E713 and beta $E8DC, retaining the existing pose and animation
    /// cursor; command one restores the normal handlers. Do not reset the pose to
    /// standing: a script can legitimately freeze a running or airborne frame.
    /// </summary>
    public void SetStationaryScriptControlLock(bool locked)
    {
        _samus.InputLocked = locked;
        StationaryScriptControlLocked = locked;
        RefillStationLocked = false;
    }

    /// <summary>
    /// Command six's handler pair ($90:F1AA): locked alpha $E713 with the bare <c>RTL</c>
    /// beta $90:E8D6. That beta neither moves nor animates Samus. A map station keeps it
    /// through the pause menu until command $0C ($90:F29E) restores the normal pair.
    /// </summary>
    public bool RefillStationLocked { get; internal set; }

    /// <summary>
    /// Whether Mother Brain's rainbow-beam setup ($90:F394) owns the handler pair: its new-state
    /// handler is the bare <c>RTL</c> at $90:E8D9 until command one restores the normal pair.
    /// </summary>
    public bool RainbowBeamHandlersInstalled =>
        _samus.Drained.Phase == DrainedSamusPhase.RainbowBeamLocked && _samus.InputLocked;

    /// <summary>Applies <c>SamusCommand_6_LockSamusIntoRefillStation</c>.</summary>
    public void LockIntoRefillStation()
    {
        _samus.InputLocked = true;
        StationaryScriptControlLocked = false;
        RefillStationLocked = true;
    }

    /// <summary>
    /// The handler part of unpause command $0C ($90:F2A2): whenever the new-state handler is
    /// command six's bare <c>RTL</c>, it restores the normal pair. Any station that locked
    /// Samus, map or recharge, therefore lets go when the pause menu closes.
    /// </summary>
    public void ReleaseRefillStationLockOnUnpause()
    {
        if (RefillStationLocked)
            _samus.InputLocked = false;
    }
}
