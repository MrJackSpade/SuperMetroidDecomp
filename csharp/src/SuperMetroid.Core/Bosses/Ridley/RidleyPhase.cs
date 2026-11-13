namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The phases of Ridley's AI shared by both encounters. The state stores the member; its values are arbitrary, and
/// verification's catalog relates each member to its source (#1413).
/// </summary>
/// <remarks>
/// Lower Norfair Ridley begins at <see cref="NorfairEnterArena"/> after the shared reveal sequence. The Ceres fake
/// retreat phases from <see cref="CeresFakeRetreatMoveToPosition"/> on are shared with the Baby retrieval route,
/// and the real fight selects them only while Ridley has grabbed Samus.
/// </remarks>
public enum RidleyPhase
{
    /// <summary>Clears shared motion before returning to the flying dispatcher.</summary>
    ClearVelocity = 1,
    /// <summary>Waits for the room-entry door transition before starting Ridley's reveal.</summary>
    WaitForDoorTransition,
    /// <summary>Counts the initial reveal delay before fading the eyes.</summary>
    InitialDelay,
    /// <summary>Advances the shared eye-palette fade before body reveal.</summary>
    FadeInEyes,
    /// <summary>Advances the body-palette fade and prepares the roar.</summary>
    FadeInBody,
    /// <summary>Waits before selecting the pre-flight roar animation.</summary>
    WaitBeforeRoar,
    /// <summary>Holds the pre-flight roar before encounter-specific liftoff.</summary>
    WaitBeforeLiftoff,
    /// <summary>Accelerates the Ceres body upward during liftoff.</summary>
    CeresLiftoffAccelerating,
    /// <summary>Decelerates Ceres liftoff into the hovering phase.</summary>
    CeresLiftoffDecelerating,
    /// <summary>Runs Ceres hovering and chooses its next scripted attack.</summary>
    CeresHovering,
    /// <summary>Moves the Ceres body to its fireball firing position.</summary>
    CeresFireballMoveToPosition,
    /// <summary>Maintains the Ceres fireball attack before returning to hover.</summary>
    CeresFireballShooting,
    /// <summary>Initializes the Ceres lunge movement and phase timer.</summary>
    CeresLungeSetup,
    /// <summary>Advances the Ceres lunge until recovery.</summary>
    CeresLungeMain,
    /// <summary>Initializes the Ceres downward-aiming swoop.</summary>
    CeresSwoopSetup,
    /// <summary>Steers Ceres Ridley to the swoop start position.</summary>
    CeresSwoopMoveToPosition,
    /// <summary>Descends through the downward-aiming portion of the Ceres swoop.</summary>
    CeresSwoopDescendingAimingDown,
    /// <summary>Turns the descending Ceres swoop toward a leftward aim.</summary>
    CeresSwoopDescendingAimingLeft,
    /// <summary>Starts the upward-aiming ascent of the Ceres swoop.</summary>
    CeresSwoopAscendingAimingUp,
    /// <summary>Continues the faster upward-aiming Ceres swoop ascent.</summary>
    CeresSwoopAscendingAimingUpFaster,
    /// <summary>Raises Ceres Ridley beyond the room for the real getaway.</summary>
    CeresRealRetreatRising,
    /// <summary>Prepares retreat palettes and walls while waiting to publish the getaway.</summary>
    CeresRetreatDelay,
    /// <summary>Publishes the Ceres turning-floor getaway handoff.</summary>
    CeresPublishEscapeHandoff,
    /// <summary>The inactive Ceres body entry while its getaway presentation owns the scene.</summary>
    CeresInactive,
    /// <summary>Advances the Ceres self-destruct graphics, countdown setup, and optional Japanese typewriter phases.</summary>
    CeresActivateSelfDestruct,
    /// <summary>Retains the Ceres alarm-palette update after self-destruct setup.</summary>
    CeresSelfDestructPaletteOnly,

    /// <summary>Moves Lower Norfair Ridley into the arena after the shared reveal.</summary>
    NorfairEnterArena,
    /// <summary>Selects Lower Norfair Ridley's next action from combat state.</summary>
    NorfairSelectAttack,
    /// <summary>The missed-lunge setup entry, preparing the hover recovery.</summary>
    NorfairHoverSetup,
    /// <summary>The missed-lunge move-to-position entry, advancing hover recovery.</summary>
    NorfairHover,
    /// <summary>Initializes the Lower Norfair swoop phase countdown and motion.</summary>
    NorfairSwoopSetup,
    /// <summary>Moves Lower Norfair Ridley to its swoop start.</summary>
    NorfairSwoopMoveToStart,
    /// <summary>Descends through the downward-aiming Lower Norfair swoop phase.</summary>
    NorfairSwoopAimDown,
    /// <summary>Rotates the descending Lower Norfair swoop toward horizontal aim.</summary>
    NorfairSwoopAimSideways,
    /// <summary>Transitions the Lower Norfair swoop into upward aim.</summary>
    NorfairSwoopAimUp,
    /// <summary>Accelerates the upward-aiming Lower Norfair swoop ascent.</summary>
    NorfairSwoopClimb,
    /// <summary>Decelerates the swoop and chooses attack selection or grab approach.</summary>
    NorfairSwoopRecover,
    /// <summary>The hover-action entry used by the pogo setup dispatcher.</summary>
    NorfairPogoSetup,
    /// <summary>The hover move-to-wall-behind entry used by the descending pogo phase.</summary>
    NorfairPogoDescending,
    /// <summary>The hover move-to-wall-in-front entry used by the ascending pogo phase.</summary>
    NorfairPogoAscending,
    /// <summary>The pogo fly-to-position entry, initializing the ground-attack sequence.</summary>
    NorfairFireballSetup,
    /// <summary>The ground-attack side-position phase.</summary>
    NorfairFireballMoveToSide,
    /// <summary>The ground-attack height-position phase.</summary>
    NorfairFireballMoveToHeight,
    /// <summary>The pogo descending entry used by the ground-attack dispatcher.</summary>
    NorfairFireballAttack,
    /// <summary>The ground-attack recovery phase before choosing the next action.</summary>
    NorfairFireballRecover,
    /// <summary>The lunge action used to approach and grab Samus.</summary>
    NorfairGrabApproach,
    /// <summary>Initializes the Lower Norfair carry state after grabbing Samus.</summary>
    NorfairCarrySetup,
    /// <summary>Moves the grabbed Samus toward the carry anchor.</summary>
    NorfairCarryMoveToAnchor,
    /// <summary>Rises while carrying Samus until the release timer expires.</summary>
    NorfairCarryRise,
    /// <summary>Recovers after releasing Samus and restores ordinary tail spacing.</summary>
    NorfairCarryRelease,
    /// <summary>Recover from a lunge while a bomb is armed.</summary>
    NorfairReturnToArena,

    /// <summary>Steers the fake Ceres retreat to its start; the shared entry can also be selected during a Lower Norfair grab.</summary>
    CeresFakeRetreatMoveToPosition,
    /// <summary>Raises Ceres Ridley until the Baby starts falling; shared with the grab route.</summary>
    CeresFakeRetreatRising,
    /// <summary>Starts the Baby's fall and waits before the Ceres retrieval animation.</summary>
    CeresWaitBeforeRetrievingBaby,
    /// <summary>Steers Ridley's hand toward the falling Baby and reacquires it.</summary>
    CeresRetrieveBaby,

    /// <summary>Moves zero-health Ridley to the death spot while retaining any grabbed Samus.</summary>
    NorfairReleaseSamus,
    /// <summary>Selects the death-roar animation and starts its thirty-two-frame phase.</summary>
    NorfairDeathStart,
    /// <summary>Continues death-spot convergence during the roar, then stops motion and starts acid drainage.</summary>
    NorfairDeathExplosions,
    /// <summary>Emits small explosions while acid drains; expiry releases Samus and allocates breakup actors.</summary>
    NorfairDeathFall,
    /// <summary>Hides the intact body and disables shared wing/tail animation after breakup allocation.</summary>
    NorfairDeathImpact,
    /// <summary>Counts the deliberate thirty-two-frame wait before the terminal death delay.</summary>
    NorfairDeathWait,
    /// <summary>Completes the terminal wait, publishes boss persistence, scatters drops, requests music, and deletes the body.</summary>
    NorfairDeathFinish,
    /// <summary>Terminal empty phase selected after drops, music and deletion.</summary>
    NorfairDeathComplete,
}
