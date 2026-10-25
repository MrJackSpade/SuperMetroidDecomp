namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases of Ridley's AI shared by both encounters. Each member names the bank-$A6 routine it ports; the state
/// stores the member, not the routine's address, and a native trace translates through verification's native-word
/// table (#1413).
/// </summary>
/// <remarks>
/// Lower Norfair Ridley begins at <see cref="NorfairEnterArena"/> after the shared reveal sequence. The Ceres fake
/// retreat functions from <see cref="CeresFakeRetreatMoveToPosition"/> on are shared with the Baby retrieval route,
/// and the real fight selects them only while Ridley has grabbed Samus.
/// </remarks>
public enum RidleyAiFunction : ushort
{
    /// <summary>$A6:A354: clears shared motion before returning to the flying dispatcher.</summary>
    ClearVelocity = 1,
    /// <summary>$A6:A35B: waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition,
    /// <summary>$A6:A377: counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay,
    /// <summary>$A6:A389: advances the shared eye-palette fade before body reveal.</summary>
    FadeInEyes,
    /// <summary>$A6:A3DF: advances the body-palette fade and prepares the roar.</summary>
    FadeInBody,
    /// <summary>$A6:A455: waits before selecting the pre-flight roar animation.</summary>
    WaitBeforeRoar,
    /// <summary>$A6:A478: holds the pre-flight roar before encounter-specific liftoff.</summary>
    WaitBeforeLiftoff,
    /// <summary>$A6:A6AF: accelerates the Ceres body upward during liftoff.</summary>
    CeresLiftoffAccelerating,
    /// <summary>$A6:A6C8: decelerates Ceres liftoff into the hovering phase.</summary>
    CeresLiftoffDecelerating,
    /// <summary>$A6:A6E8: runs Ceres hovering and chooses its next scripted attack.</summary>
    CeresHovering,
    /// <summary>$A6:A782: moves the Ceres body to its fireball firing position.</summary>
    CeresFireballMoveToPosition,
    /// <summary>$A6:A7F9: maintains the Ceres fireball attack before returning to hover.</summary>
    CeresFireballShooting,
    /// <summary>$A6:A83C: initializes the Ceres lunge movement and phase timer.</summary>
    CeresLungeSetup,
    /// <summary>$A6:A84E: advances the Ceres lunge until recovery.</summary>
    CeresLungeMain,
    /// <summary>$A6:A88D: initializes the Ceres downward-aiming swoop.</summary>
    CeresSwoopSetup,
    /// <summary>$A6:A8A4: steers Ceres Ridley to the swoop start position.</summary>
    CeresSwoopMoveToPosition,
    /// <summary>$A6:A8D4: descends through the downward-aiming portion of the Ceres swoop.</summary>
    CeresSwoopDescendingAimingDown,
    /// <summary>$A6:A8F8: turns the descending Ceres swoop toward a leftward aim.</summary>
    CeresSwoopDescendingAimingLeft,
    /// <summary>$A6:A923: starts the upward-aiming ascent of the Ceres swoop.</summary>
    CeresSwoopAscendingAimingUp,
    /// <summary>$A6:A947: continues the faster upward-aiming Ceres swoop ascent.</summary>
    CeresSwoopAscendingAimingUpFaster,
    /// <summary>$A6:A971: raises Ceres Ridley beyond the room for the real getaway.</summary>
    CeresRealRetreatRising,
    /// <summary>$A6:A9A0: prepares retreat palettes and walls while waiting to publish the getaway.</summary>
    CeresRetreatDelay,
    /// <summary>$A6:AA11: publishes the Ceres Mode-7 getaway handoff.</summary>
    CeresPublishEscapeHandoff,
    /// <summary>$A6:AA1B: the inactive Ceres body entry while its getaway presentation owns the scene.</summary>
    CeresInactive,
    /// <summary>$A6:C04E: advances Ceres self-destruct transfers, countdown setup, and optional Japanese typewriter phases.</summary>
    CeresActivateSelfDestruct,
    /// <summary>$A6:AA50: retains the Ceres alarm-palette update after self-destruct setup.</summary>
    CeresSelfDestructPaletteOnly,

    /// <summary>$A6:B2F3: moves Lower Norfair Ridley into the arena after the shared reveal.</summary>
    NorfairEnterArena,
    /// <summary>$A6:B321: selects Lower Norfair Ridley's next action from combat state.</summary>
    NorfairSelectAttack,
    /// <summary>$A6:B3EC: the native missed-lunge setup entry, preparing the translated hover recovery.</summary>
    NorfairHoverSetup,
    /// <summary>$A6:B3F8: the native missed-lunge move-to-position entry, advancing translated hover recovery.</summary>
    NorfairHover,
    /// <summary>$A6:B441: initializes the Lower Norfair swoop phase countdown and motion.</summary>
    NorfairSwoopSetup,
    /// <summary>$A6:B455: moves Lower Norfair Ridley to its swoop start.</summary>
    NorfairSwoopMoveToStart,
    /// <summary>$A6:B493: descends through the downward-aiming Lower Norfair swoop phase.</summary>
    NorfairSwoopAimDown,
    /// <summary>$A6:B4D1: rotates the descending Lower Norfair swoop toward horizontal aim.</summary>
    NorfairSwoopAimSideways,
    /// <summary>$A6:B516: transitions the Lower Norfair swoop into upward aim.</summary>
    NorfairSwoopAimUp,
    /// <summary>$A6:B554: accelerates the upward-aiming Lower Norfair swoop ascent.</summary>
    NorfairSwoopClimb,
    /// <summary>$A6:B594: decelerates the swoop and chooses attack selection or grab approach.</summary>
    NorfairSwoopRecover,
    /// <summary>$A6:B5C4: the native hover-action entry used by the translated pogo setup dispatcher.</summary>
    NorfairPogoSetup,
    /// <summary>$A6:B5E5: the native hover move-to-wall-behind entry used by the translated descending pogo phase.</summary>
    NorfairPogoDescending,
    /// <summary>$A6:B613: the native hover move-to-wall-in-front entry used by the translated ascending pogo phase.</summary>
    NorfairPogoAscending,
    /// <summary>$A6:B68B: the native pogo fly-to-position entry, initializing the translated ground-attack sequence.</summary>
    NorfairFireballSetup,
    /// <summary>$A6:B6A7: the translated ground-attack side-position phase.</summary>
    NorfairFireballMoveToSide,
    /// <summary>$A6:B6DD: the translated ground-attack height-position phase.</summary>
    NorfairFireballMoveToHeight,
    /// <summary>$A6:B70E: the native pogo descending entry used by the translated ground-attack dispatcher.</summary>
    NorfairFireballAttack,
    /// <summary>$A6:B7B9: the translated ground-attack recovery phase before choosing the next action.</summary>
    NorfairFireballRecover,
    /// <summary>$A6:BAB7: the native lunge action used to approach and grab Samus.</summary>
    NorfairGrabApproach,
    /// <summary>$A6:BB8F: initializes the Lower Norfair carry state after grabbing Samus.</summary>
    NorfairCarrySetup,
    /// <summary>$A6:BBC4: moves the grabbed Samus toward the carry anchor.</summary>
    NorfairCarryMoveToAnchor,
    /// <summary>$A6:BBF1: rises while carrying Samus until the release timer expires.</summary>
    NorfairCarryRise,
    /// <summary>$A6:BC2E: recovers after releasing Samus and restores ordinary tail spacing.</summary>
    NorfairCarryRelease,
    /// <summary>$A6:BD4E, Function_Ridley_DodgingPowerBomb: recover from a lunge while a bomb is armed.</summary>
    NorfairReturnToArena,

    /// <summary>$A6:BD9A: steers the fake Ceres retreat to its start; the shared entry can also be selected during a Lower Norfair grab.</summary>
    CeresFakeRetreatMoveToPosition,
    /// <summary>$A6:BDBC: raises Ceres Ridley until the Baby starts falling; shared with the grab route.</summary>
    CeresFakeRetreatRising,
    /// <summary>$A6:BDF2: starts the Baby's fall and waits before the Ceres retrieval animation.</summary>
    CeresWaitBeforeRetrievingBaby,
    /// <summary>$A6:BE03: steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    CeresRetrieveBaby,

    /// <summary>$A6:C538: moves zero-health Ridley to the death spot while retaining any grabbed Samus.</summary>
    NorfairReleaseSamus,
    /// <summary>$A6:C53E: selects the death-roar instruction list and starts its thirty-two-frame phase.</summary>
    NorfairDeathStart,
    /// <summary>$A6:C551: continues death-spot convergence during the roar, then stops motion and starts acid drainage.</summary>
    NorfairDeathExplosions,
    /// <summary>$A6:C588: emits small explosions while acid drains; expiry releases Samus and allocates breakup actors.</summary>
    NorfairDeathFall,
    /// <summary>$A6:C5A8: hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    NorfairDeathImpact,
    /// <summary>$A6:C5C8: counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    NorfairDeathWait,
    /// <summary>$A6:C5DA: completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    NorfairDeathFinish,
    /// <summary>$A6:C600: terminal RTS selected by $C5FD after drops, music and deletion.</summary>
    NorfairDeathComplete,
}
