using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Samus;

/// <summary>
/// The installed frame-handler pair and the Samus commands that replace it, the port of <c>Run_Samus_Command</c>
/// ($90:F084) and the direct handler installs elsewhere. Every change to who controls Samus goes through one of these
/// operations; gates read <see cref="Mode"/>.
/// </summary>
internal sealed class SamusControl : IRoomSamusControl
{
    /// <summary>The installed alpha and beta handlers.</summary>
    public SamusControlMode Mode { get; private set; } = SamusControlMode.Normal;

    /// <summary>Whether a locked alpha is installed, so none of the normal alpha's input owners run.</summary>
    public bool AlphaLocked => Mode.Alpha == SamusAlphaHandler.Locked;

    /// <summary>
    /// Samus command zero ($90:F109): installs alpha $E713 and beta $E8DC, retaining the existing pose and animation
    /// cursor. Do not reset the pose to standing: a script can legitimately freeze a running or airborne frame. The
    /// stationary beta updates contact and minimap state but does not animate Samus.
    /// </summary>
    public void LockStationary() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.ScriptLocked);

    /// <summary>Samus command one ($90:F117) and every other install of the normal pair.</summary>
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
    /// Samus command six ($90:F1AA): locked alpha $E713 with the bare <c>RTL</c> beta $90:E8D6, which neither moves nor
    /// animates Samus. A map station keeps it through the pause menu until command $0C restores the normal pair.
    /// </summary>
    public void LockIntoRefillStation() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.RefillStation);

    /// <summary>
    /// The handler part of unpause command $0C ($90:F2A2): whenever the beta is command six's bare <c>RTL</c>, it
    /// restores the normal pair. Any station that locked Samus, map or recharge, therefore lets go when the pause menu
    /// closes.
    /// </summary>
    public void ReleaseRefillStationOnUnpause()
    {
        if (Mode.Beta == SamusBetaHandler.RefillStation)
            Unlock();
    }

    /// <summary>Suit command $15: the inert pair of the suit transformation.</summary>
    public void LockForSuitPickup() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.SuitPickup);

    /// <summary>
    /// Gunship command $1A ($90:F409) after the face-forward lock: beta $90:E902, which checks low health despite the
    /// locked alpha, until the ship restores the normal pair.
    /// </summary>
    public void EnterGunship() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.Gunship);

    /// <summary>Mother Brain's rainbow-beam setup ($90:F394): the locked alpha with the bare <c>RTL</c> beta $90:E8D9.</summary>
    public void LockForRainbowBeam() => Install(SamusAlphaHandler.Locked, SamusBetaHandler.RainbowBeam);

    /// <summary>Installs <paramref name="alpha"/> and <paramref name="beta"/>, replacing the pair.</summary>
    private void Install(SamusAlphaHandler alpha, SamusBetaHandler beta) => Mode = new SamusControlMode(alpha, beta);
}
