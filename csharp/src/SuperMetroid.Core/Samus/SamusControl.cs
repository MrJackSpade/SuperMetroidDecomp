using SuperMetroid.Core.Rooms;
namespace SuperMetroid.Core.Samus;

/// <summary>
/// The installed input and movement modes and the control setups that replace them, both through
/// scripted control setups and through the direct mode installs elsewhere. Every change to who controls Samus goes through one of these
/// operations; gates read <see cref="Mode"/>.
/// </summary>
internal sealed class SamusControl : IRoomSamusControl
{
    /// <summary>The installed input and movement modes.</summary>
    public SamusControlMode Mode { get; private set; } = SamusControlMode.Normal;

    /// <summary>Whether the locked input mode is installed, so none of the normal input phase's input owners run.</summary>
    public bool InputLocked => Mode.Input == SamusInputMode.Locked;

    /// <summary>
    /// The stationary lock: installs the locked input mode and the stationary movement mode, retaining the existing pose
    /// and animation cursor. Do not reset the pose to standing: a script can legitimately freeze a running or airborne
    /// frame. The stationary movement mode updates contact and minimap state but does not animate Samus.
    /// </summary>
    public void LockStationary() => Install(SamusInputMode.Locked, SamusMovementMode.ScriptLocked);

    /// <summary>Restores Samus's control: every install of the normal modes.</summary>
    public void Unlock() => Mode = SamusControlMode.Normal;

    /// <summary>
    /// Locked modes whose movement mode no gate distinguishes: elevator rides, cinematics, face-forward, death and load
    /// appearances. Modes that are already locked stay installed, so a script lock survives an overlapping gate.
    /// </summary>
    public void LockOutPlayer()
    {
        if (!InputLocked)
            Install(SamusInputMode.Locked, SamusMovementMode.Locked);
    }

    /// <summary>
    /// The station lock: the locked input mode with an empty movement mode, which neither moves nor
    /// animates Samus. A map station keeps it through the pause menu until closing the pause menu restores the normal modes.
    /// </summary>
    public void LockAtStation() => Install(SamusInputMode.Locked, SamusMovementMode.RefillStation);

    /// <summary>
    /// The station lock's release on unpause: whenever the movement mode is the station lock's empty one, it
    /// restores the normal modes. Any station that locked Samus, map or recharge, therefore lets go when the pause menu
    /// closes.
    /// </summary>
    public void ReleaseRefillStationOnUnpause()
    {
        if (Mode.Movement == SamusMovementMode.RefillStation)
            Unlock();
    }

    /// <summary>The suit-pickup lock: the inert modes of the suit transformation.</summary>
    public void LockForSuitPickup() => Install(SamusInputMode.Locked, SamusMovementMode.SuitPickup);

    /// <summary>
    /// The gunship setup after the face-forward lock: a movement mode which checks low health despite the
    /// locked input mode, until the ship restores the normal modes.
    /// </summary>
    public void EnterGunship() => Install(SamusInputMode.Locked, SamusMovementMode.Gunship);

    /// <summary>Mother Brain's rainbow-beam setup: the locked input mode with another empty movement mode.</summary>
    public void LockForRainbowBeam() => Install(SamusInputMode.Locked, SamusMovementMode.RainbowBeam);

    /// <summary>Installs <paramref name="input"/> and <paramref name="movement"/>, replacing both modes.</summary>
    private void Install(SamusInputMode input, SamusMovementMode movement) => Mode = new SamusControlMode(input, movement);
}
