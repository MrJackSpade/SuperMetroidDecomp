namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Bank-$A6 function pointers used by both Ridley encounters. Keeping the original
/// addresses in debugger-visible state makes every transition directly comparable with
/// a WRAM trace and prevents host-only ordinals from obscuring shared cartridge code.
/// </summary>
public enum RidleyAiFunction : ushort
{
    /// <summary>$A6:A354: clears shared motion before returning to the flying dispatcher.</summary>
    ClearVelocity = 0xa354,
    /// <summary>$A6:A35B: waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition = 0xa35b,
    /// <summary>$A6:A377: counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay = 0xa377,
    /// <summary>$A6:A389: advances the shared eye-palette fade before body reveal.</summary>
    FadeInEyes = 0xa389,
    /// <summary>$A6:A3DF: advances the body-palette fade and prepares the roar.</summary>
    FadeInBody = 0xa3df,
    /// <summary>$A6:A455: waits before selecting the pre-flight roar animation.</summary>
    WaitBeforeRoar = 0xa455,
    /// <summary>$A6:A478: holds the pre-flight roar before encounter-specific liftoff.</summary>
    WaitBeforeLiftoff = 0xa478,
    /// <summary>$A6:A6AF: accelerates the Ceres body upward during liftoff.</summary>
    CeresLiftoffAccelerating = 0xa6af,
    /// <summary>$A6:A6C8: decelerates Ceres liftoff into the hovering phase.</summary>
    CeresLiftoffDecelerating = 0xa6c8,
    /// <summary>$A6:A6E8: runs Ceres hovering and chooses its next scripted attack.</summary>
    CeresHovering = 0xa6e8,
    /// <summary>$A6:A782: moves the Ceres body to its fireball firing position.</summary>
    CeresFireballMoveToPosition = 0xa782,
    /// <summary>$A6:A7F9: maintains the Ceres fireball attack before returning to hover.</summary>
    CeresFireballShooting = 0xa7f9,
    /// <summary>$A6:A83C: initializes the Ceres lunge movement and phase timer.</summary>
    CeresLungeSetup = 0xa83c,
    /// <summary>$A6:A84E: advances the Ceres lunge until recovery.</summary>
    CeresLungeMain = 0xa84e,
    /// <summary>$A6:A88D: initializes the Ceres downward-aiming swoop.</summary>
    CeresSwoopSetup = 0xa88d,
    /// <summary>$A6:A8A4: steers Ceres Ridley to the swoop start position.</summary>
    CeresSwoopMoveToPosition = 0xa8a4,
    /// <summary>$A6:A8D4: descends through the downward-aiming portion of the Ceres swoop.</summary>
    CeresSwoopDescendingAimingDown = 0xa8d4,
    /// <summary>$A6:A8F8: turns the descending Ceres swoop toward a leftward aim.</summary>
    CeresSwoopDescendingAimingLeft = 0xa8f8,
    /// <summary>$A6:A923: starts the upward-aiming ascent of the Ceres swoop.</summary>
    CeresSwoopAscendingAimingUp = 0xa923,
    /// <summary>$A6:A947: continues the faster upward-aiming Ceres swoop ascent.</summary>
    CeresSwoopAscendingAimingUpFaster = 0xa947,
    /// <summary>$A6:A971: raises Ceres Ridley beyond the room for the real getaway.</summary>
    CeresRealRetreatRising = 0xa971,
    /// <summary>$A6:A9A0: prepares retreat palettes and walls while waiting to publish the getaway.</summary>
    CeresRetreatDelay = 0xa9a0,
    /// <summary>$A6:AA11: publishes the Ceres Mode-7 getaway handoff.</summary>
    CeresPublishEscapeHandoff = 0xaa11,
    /// <summary>$A6:AA1B: the inactive Ceres body entry while its getaway presentation owns the scene.</summary>
    CeresInactive = 0xaa1b,
    /// <summary>$A6:C04E: advances Ceres self-destruct transfers, countdown setup, and optional Japanese typewriter phases.</summary>
    CeresActivateSelfDestruct = 0xc04e,
    /// <summary>$A6:AA50: retains the Ceres alarm-palette update after self-destruct setup.</summary>
    CeresSelfDestructPaletteOnly = 0xaa50,

    // Lower Norfair Ridley begins at $B2F3 after the shared reveal sequence. These names
    // describe the native movement/attack phases; their values are not host inventions.
    /// <summary>$A6:B2F3: moves Lower Norfair Ridley into the arena after the shared reveal.</summary>
    NorfairEnterArena = 0xb2f3,
    /// <summary>$A6:B321: selects Lower Norfair Ridley's next action from combat state.</summary>
    NorfairSelectAttack = 0xb321,
    /// <summary>$A6:B3EC: the native missed-lunge setup entry, preparing the translated hover recovery.</summary>
    NorfairHoverSetup = 0xb3ec,
    /// <summary>$A6:B3F8: the native missed-lunge move-to-position entry, advancing translated hover recovery.</summary>
    NorfairHover = 0xb3f8,
    /// <summary>$A6:B441: initializes the Lower Norfair swoop phase countdown and motion.</summary>
    NorfairSwoopSetup = 0xb441,
    /// <summary>$A6:B455: moves Lower Norfair Ridley to its swoop start.</summary>
    NorfairSwoopMoveToStart = 0xb455,
    /// <summary>$A6:B493: descends through the downward-aiming Lower Norfair swoop phase.</summary>
    NorfairSwoopAimDown = 0xb493,
    /// <summary>$A6:B4D1: rotates the descending Lower Norfair swoop toward horizontal aim.</summary>
    NorfairSwoopAimSideways = 0xb4d1,
    /// <summary>$A6:B516: transitions the Lower Norfair swoop into upward aim.</summary>
    NorfairSwoopAimUp = 0xb516,
    /// <summary>$A6:B554: accelerates the upward-aiming Lower Norfair swoop ascent.</summary>
    NorfairSwoopClimb = 0xb554,
    /// <summary>$A6:B594: decelerates the swoop and chooses attack selection or grab approach.</summary>
    NorfairSwoopRecover = 0xb594,
    /// <summary>$A6:B5C4: the native hover-action entry used by the translated pogo setup dispatcher.</summary>
    NorfairPogoSetup = 0xb5c4,
    /// <summary>$A6:B5E5: the native hover move-to-wall-behind entry used by the translated descending pogo phase.</summary>
    NorfairPogoDescending = 0xb5e5,
    /// <summary>$A6:B613: the native hover move-to-wall-in-front entry used by the translated ascending pogo phase.</summary>
    NorfairPogoAscending = 0xb613,
    /// <summary>$A6:B68B: the native pogo fly-to-position entry, initializing the translated ground-attack sequence.</summary>
    NorfairFireballSetup = 0xb68b,
    /// <summary>$A6:B6A7: the translated ground-attack side-position phase.</summary>
    NorfairFireballMoveToSide = 0xb6a7,
    /// <summary>$A6:B6DD: the translated ground-attack height-position phase.</summary>
    NorfairFireballMoveToHeight = 0xb6dd,
    /// <summary>$A6:B70E: the native pogo descending entry used by the translated ground-attack dispatcher.</summary>
    NorfairFireballAttack = 0xb70e,
    /// <summary>$A6:B7B9: the translated ground-attack recovery phase before choosing the next action.</summary>
    NorfairFireballRecover = 0xb7b9,
    /// <summary>$A6:BAB7: the native lunge action used to approach and grab Samus.</summary>
    NorfairGrabApproach = 0xbab7,
    /// <summary>$A6:BB8F: initializes the Lower Norfair carry state after grabbing Samus.</summary>
    NorfairCarrySetup = 0xbb8f,
    /// <summary>$A6:BBC4: moves the grabbed Samus toward the carry anchor.</summary>
    NorfairCarryMoveToAnchor = 0xbbc4,
    /// <summary>$A6:BBF1: rises while carrying Samus until the release timer expires.</summary>
    NorfairCarryRise = 0xbbf1,
    /// <summary>$A6:BC2E: recovers after releasing Samus and restores ordinary tail spacing.</summary>
    NorfairCarryRelease = 0xbc2e,
    /// <summary>$A6:BD4E, Function_Ridley_DodgingPowerBomb: recover from a lunge while a bomb is armed.</summary>
    NorfairReturnToArena = 0xbd4e,

    // The following functions are shared with Ceres's Baby retrieval route but are only
    // selected during the real fight when Ridley has grabbed Samus.
    /// <summary>$A6:BD9A: steers the fake Ceres retreat to its start; the shared entry can also be selected during a Lower Norfair grab.</summary>
    CeresFakeRetreatMoveToPosition = 0xbd9a,
    /// <summary>$A6:BDBC: raises Ceres Ridley until the Baby starts falling; shared with the grab route.</summary>
    CeresFakeRetreatRising = 0xbdbc,
    /// <summary>$A6:BDF2: starts the Baby's fall and waits before the Ceres retrieval animation.</summary>
    CeresWaitBeforeRetrievingBaby = 0xbdf2,
    /// <summary>$A6:BE03: steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    CeresRetrieveBaby = 0xbe03,

    /// <summary>$A6:C538: moves zero-health Ridley to the death spot while retaining any grabbed Samus.</summary>
    NorfairReleaseSamus = 0xc538,
    /// <summary>$A6:C53E: selects the death-roar instruction list and starts its thirty-two-frame phase.</summary>
    NorfairDeathStart = 0xc53e,
    /// <summary>$A6:C551: continues death-spot convergence during the roar, then stops motion and starts acid drainage.</summary>
    NorfairDeathExplosions = 0xc551,
    /// <summary>$A6:C588: emits small explosions while acid drains; expiry releases Samus and allocates breakup actors.</summary>
    NorfairDeathFall = 0xc588,
    /// <summary>$A6:C5A8: hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    NorfairDeathImpact = 0xc5a8,
    /// <summary>$A6:C5C8: counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    NorfairDeathWait = 0xc5c8,
    /// <summary>$A6:C5DA: completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    NorfairDeathFinish = 0xc5da,
    /// <summary>$A6:C600: terminal RTS selected by $C5FD after drops, music and deletion.</summary>
    NorfairDeathComplete = 0xc600,
}
