using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Samus;

/// <summary>
/// The installed frame-handler pair and the control setups that replace it, both through
/// scripted control setups and through the direct handler installs elsewhere. Every change to who controls Samus goes through one of these
/// operations; gates read <see cref="Mode"/>.
/// </summary>
internal sealed class SamusControl : IRoomSamusControl
{
    /// <summary>The installed alpha and beta handlers.</summary>
    public SamusControlMode Mode { get; private set; } = SamusControlMode.Normal;

    /// <summary>Whether a locked alpha is installed, so none of the normal alpha's input owners run.</summary>
    public bool AlphaLocked => Mode.Alpha == SamusAlphaHandler.Locked;

    /// <summary>
    /// The stationary lock: installs the locked alpha and the stationary beta, retaining the existing pose and animation
    /// cursor. Do not reset the pose to standing: a script can legitimately freeze a running or airborne frame. The
    /// stationary beta updates contact and minimap state but does not animate Samus.
    /// </summary>
    public void LockStationary() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.ScriptLocked);

    /// <summary>Restores Samus's control: every install of the normal pair.</summary>
    public void Unlock() => Mode = SamusControlMode.Normal;

    /// <summary>
    /// A locked pair whose beta no gate distinguishes: elevator rides, cinematics, face-forward, death and load
    /// appearances. A pair that is already locked stays installed, so a script lock survives an overlapping gate.
    /// </summary>
    public void LockHandlers()
    {
        if (!AlphaLocked)
            Install(SamusAlphaHandler.Locked, SamusBetaHandler.Locked);
    }

    /// <summary>
    /// The station lock: the locked alpha with an empty beta, which neither moves nor
    /// animates Samus. A map station keeps it through the pause menu until the unpause command restores the normal pair.
    /// </summary>
    public void LockIntoRefillStation() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.RefillStation);

    /// <summary>
    /// The handler part of the unpause command: whenever the beta is the station lock's empty one, it
    /// restores the normal pair. Any station that locked Samus, map or recharge, therefore lets go when the pause menu
    /// closes.
    /// </summary>
    public void ReleaseRefillStationOnUnpause()
    {
        if (Mode.Beta == SamusBetaHandler.RefillStation)
            Unlock();
    }

    /// <summary>The suit-pickup command: the inert pair of the suit transformation.</summary>
    public void LockForSuitPickup() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.SuitPickup);

    /// <summary>
    /// The gunship command after the face-forward lock: a beta which checks low health despite the
    /// locked alpha, until the ship restores the normal pair.
    /// </summary>
    public void EnterGunship() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.Gunship);

    /// <summary>Mother Brain's rainbow-beam setup: the locked alpha with another empty beta.</summary>
    public void LockForRainbowBeam() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.RainbowBeam);

    /// <summary>Installs <paramref name="alpha"/> and <paramref name="beta"/>, replacing the pair.</summary>
    private void Install(SamusAlphaHandler alpha, SamusBetaHandler beta) => Mode = new SamusControlMode(alpha, beta);
}
