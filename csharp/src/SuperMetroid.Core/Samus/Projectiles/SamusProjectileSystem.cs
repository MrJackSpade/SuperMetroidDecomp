using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// Samus's ordinary shots in their places (see <see cref="SamusShotPool"/> for the place order).
/// </summary>
/// <remarks>Firing, damage and animation, and terrain movement each keep their own seam.</remarks>
[RoomLifetime]
public sealed class SamusProjectileSystem
{
    /// <summary>Tracks held charge, flare components, and charge glow state across room transitions.</summary>
    internal BeamChargeState Charge { get; }

    /// <summary>Holds the five shot places and the shot count.</summary>
    internal SamusShotPool Pool { get; }

    /// <summary>Collects impact-driven earthquake requests from projectile and terrain reactions.</summary>
    internal ProjectileEarthquakes Earthquakes { get; }

    /// <summary>The Power Bomb fuse the Chainsaw shot runs over itself.</summary>
    internal ChainsawFuse Chainsaw { get; }

    /// <summary>Queues the sound selected for projectile contact with terrain or enemies.</summary>
    internal ProjectileImpactAudio ImpactAudio { get; }

    /// <summary>
    /// The update's audio requests the projectiles' firing, combination and impact sounds join as they are made: the
    /// runtime's, or a cinematic's (#2057).
    /// </summary>
    internal GameplaySoundRequests AudioRequests { get; }

    /// <summary>Owns the projectile visuals emitted by Shinespark echoes.</summary>
    internal ShinesparkEchoProjectile Shinespark { get; }

    /// <summary>Starts and runs the special beam attacks, whose particles take Samus's first shot places.</summary>
    internal BeamCombos Combos { get; }

    /// <summary>Plays the shots' animations and draws their pictures.</summary>
    private ProjectileAnimation Animation { get; }

    /// <summary>Applies HUD, cooldown, equipment, and charge rules when allocating a shot.</summary>
    internal BeamFiring Firing { get; }

    /// <summary>Advances live projectile positions, camera culling, and trails.</summary>
    internal ProjectileMotion Motion { get; }

    /// <summary>Resolves projectile impact with blocks and breakable terrain.</summary>
    private ProjectileTerrainCollision TerrainCollision { get; }

    /// <summary>Routes ordinary shots into enemy collision and damage reactions.</summary>
    internal ProjectileEnemyImpacts EnemyImpacts { get; }

    /// <summary>The trails moving projectiles leave, which run and show until both their sparkles end.</summary>
    private ProjectileTrails Trails { get; }

    /// <summary>
    /// Whether the posture-transition admission admits weapons for a pose outside its table, which the beam producer and
    /// the Grapple admission consult.
    /// </summary>
    internal PostureAdmissionBeyondTransitions<SamusPoseId> PostureAdmission { get; }

    /// <summary>The angles a Shinespark crash finishing in a drained pose releases its echoes into these places at.</summary>
    internal DrainedCrashEchoDepartures<SamusPoseId, ShinesparkDepartureAngles> DrainedCrashDepartures { get; }

    /// <summary>Creates one room's five projectile places, its trails and the owners that run them.</summary>
    /// <param name="audioRequests">The update's audio requests the projectiles' sounds join.</param>
    /// <param name="charge">The held beam charge, flare and charge glow, which outlive each room's projectiles.</param>
    /// <param name="samusOverreads">The installed services reproducing Samus's glitch reads; the projectiles consult their own group of them.</param>
    /// <param name="leftBeamStart">
    /// What each of the room's shot places keeps for its next projectile's horizontal fraction of a pixel, created by the
    /// room's builder with the room's shot pool.
    /// </param>
    /// <param name="shotCount">How many shots Samus has out as firing counts them, created by the room's builder with the room's shot pool.</param>
    /// <param name="artwork">The Samus artwork whose projectile frames the shots draw, rebound with the host's artwork.</param>
    /// <param name="earthquake">The room's earthquake, which a Super Missile's impact starts, composed with the room's enemies.</param>
    internal SamusProjectileSystem(GameplaySoundRequests audioRequests, BeamChargeState charge, SamusOverreadServices samusOverreads,
        LeftBeamStartFromLastShot leftBeamStart, SamusShotCount shotCount, SamusArtworkBinding artwork, RoomEarthquake earthquake)
    {
        AudioRequests = Ensure.NotNull(audioRequests);
        SamusProjectileOverreads overreads = Ensure.NotNull(samusOverreads).Projectiles;
        Charge = Ensure.NotNull(charge);
        PostureAdmission = overreads.PostureAdmission;
        DrainedCrashDepartures = overreads.DrainedCrashDepartures;
        Pool = new SamusShotPool(leftBeamStart, shotCount);
        Earthquakes = new ProjectileEarthquakes(earthquake);
        ImpactAudio = new ProjectileImpactAudio(AudioRequests);
        Animation = new ProjectileAnimation(Charge, Pool, Ensure.NotNull(artwork));
        TerrainCollision = new ProjectileTerrainCollision(Animation, Earthquakes, ImpactAudio, samusOverreads.StraddledTerrain);
        EnemyImpacts = new ProjectileEnemyImpacts(Animation, Earthquakes, Pool, TerrainCollision, overreads.UnauthoredBeamFiring);
        Trails = new ProjectileTrails();
        Chainsaw = new ChainsawFuse(Animation, overreads.ChainsawFuseShift);
        Shinespark = new ShinesparkEchoProjectile(Animation, Pool);
        Combos = new BeamCombos(Animation, Charge, Pool, Trails, overreads.SpaceTimeOverwrite);
        Firing = new BeamFiring(Charge, Combos, Pool, TerrainCollision, overreads.UnauthoredBeamFiring);
        Motion = new ProjectileMotion(Animation, Pool, TerrainCollision, Trails);
    }

    /// <summary>
    /// Starts each shot place keeping the horizontal fraction of a pixel the room this one replaces left
    /// (<see cref="RoomHandOver"/>, <see cref="LeftBeamStartFromLastShot"/>), before any shot is fired.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver) => Pool.LeftBeamStart.TakeOverFrom(handOver);

    /// <summary>
    /// Leaves for the room that replaces this one the horizontal fraction of a pixel each shot place keeps as the room's
    /// load clears Samus's shots (<see cref="RoomHandOver"/>, <see cref="LeftBeamStartFromLastShot"/>).
    /// </summary>
    internal void HandOverTo(RoomHandOver handOver) =>
        Pool.LeftBeamStart.HandOverTo(handOver, [.. Pool.Shots.Select(shot => shot.IsOccupied ? shot.Position.X.FractionOfPixel : (PixelFraction?)null)]);

    /// <summary>How many places Samus's shot pool has: how many ordinary shots she can have out at once; bombs have their own places.</summary>
    public const int PlaceCount = 5;

    /// <summary>Each shot place's projectile, in place order: the live one, or the last one that ended there.</summary>
    public IReadOnlyList<SamusProjectile> Shots => Pool.Shots;

    /// <summary>
    /// Shows the equipped beams' colours: the beams show the shade of the combination they select (#1512). No pictures
    /// change: each projectile shows the sheet of the beams it is pictured with (#1586), and the installed sheets are
    /// pre-coloured in every combination's shade.
    /// </summary>
    public static void ShowBeamShade(SamusColours colours, SuperMetroid.Core.Progression.SamusBeamFlags equippedBeams) =>
        ShowBeamShade(colours, SamusBeamCombinations.Of(equippedBeams));

    /// <summary>
    /// Shows a beam combination's shade as the beams' colours: every combination the pause screen can equip, the glitched
    /// Spazer-with-Plasma ones among them, shows one (<see cref="SamusShades.Beam"/>).
    /// </summary>
    private static void ShowBeamShade(SamusColours colours, SamusBeamCombination selection) =>
        Ensure.NotNull(colours).ShowBeams(SamusShades.Beam(selection));

    /// <summary>
    /// Runs the beam charge colours on
    /// Samus's suit shade.
    /// </summary>
    /// <remarks>
    /// This belongs to Samus's suit colouring, not charge-flare drawing: the charge colours are written at this phase of
    /// the update, and the next displayed frame shows them.
    /// The runtime shows the special Speed Booster, Screw Attack, Shinespark, Crystal Flash and X-ray colouring afterward,
    /// except on the update the glow ends and restores the suit: that update shows no special colouring.
    /// </remarks>
    /// <param name="colours">Samus's shown colours.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="roomVisorAnimation">Whether the room animates Samus's visor (<see cref="RoomFxOverlayState.VisorAnimation"/>).</param>
    /// <returns>
    /// Whether a charged shot's glow ended this update and restored the suit's shade, which keeps Samus's special
    /// colourings from running this update.
    /// </returns>
    public bool UpdateBeamChargeGlow(
        SamusColours colours,
        SamusState samus,
        RoomVisorAnimation roomVisorAnimation)
    {
        Ensure.NotNull(colours);
        Ensure.NotNull(samus);

        if (Charge.ChargedShotGlow is not { } glow)
        {
            // The charge glow admits the continuously cycling charge glow only while
            // grapple is inactive and the charge, a count that never passes its clamp or the Hyper Beam's
            // flare, has reached a full charge (BeamChargeDefinitions.FullyChargedCounter).
            bool chargeGlowActive =
                samus.Grapple.Phase == GrapplePhase.Inactive &&
                Charge.HasCharged(BeamChargeDefinitions.FullyChargedCounter);
            if (chargeGlowActive)
            {
                bool pseudoScrew = samus.HorizontalSpeed.BodyAttack == SamusBodyAttack.PseudoScrew;
                Charge.PlayChargePulse(
                    pseudoScrew ? SamusChargeKind.PseudoScrew : SamusChargeKind.ChargedBeam,
                    samus.EquippedItems.ShownSuit(), colours);
                return false;
            }

            // The charge glow restarts its pulse whenever charge is below threshold or grapple
            // beam is in use. This guarantees the next eligible update starts at
            // the pulse's first shade rather than resuming a partially completed pulse.
            Charge.RestartChargePulse();
            samus.VisorShades.Update(
                colours,
                samus.SpecialColouring.Holder,
                roomVisorAnimation);
            return false;
        }

        // A charged shot's glow plays its sequence, painting each shade as it shows it: an ordinary shot's flash paints the
        // yellow glow on each of its updates; the Hyper Beam shot's glow shows each frame for its hold.
        // On the update after its last shade the glow ends and the suit's own shade returns.
        PictureShade? shown = glow.PlayUpdate(out bool ended);
        if (ended)
        {
            Charge.ChargedShotGlow = null;
            SamusSuitShade.Show(colours, samus.EquippedItems);
            return true;
        }
        if (shown is { } shade)
            colours.ShowSuit(shade);
        return false;
    }

    /// <summary>
    /// Runs the power-beam HUD producer and then handles every shot: <see cref="FireFromHud"/>, then
    /// <see cref="HandleShots"/>.
    /// </summary>
    /// <param name="terrain">The room terrain the shots collide with and react to.</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment, charge, and HUD selection.</param>
    /// <param name="buttons">The actions Samus's projectile producer reads: held actions, this frame's new presses, and the previous frame's new presses (#633).</param>
    /// <param name="cameraPosition">Horizontal and vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="samusBombs">
    /// Samus's bombs, carrying the shared projectile cooldown. The runtime steps that
    /// clock once, immediately before calling this method; this method may replace it after
    /// a successful shot just as the game does.
    /// </param>
    /// <param name="progression">The progression a Spacetime shot's overwrite turns on and off.</param>
    internal void StepFrame(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, Position16<RoomPixels> cameraPosition, SamusBombProjectileSystem samusBombs, SharedGameState progression)
    {
        FireFromHud(terrain, samus, buttons, samusBombs, projectileProducerEnabled: true);
        HandleShots(terrain, samus, cameraPosition, samusBombs, progression);
    }

    /// <summary>
    /// Runs the power-beam HUD producer, the beam and missile share of Samus's HUD item step, which the game runs before it
    /// handles any projectile: the producer fires and queues its firing sound as it fires (#1620), so the sound is tested
    /// against the power bomb as it stood before the update's bombs ran.
    /// </summary>
    /// <param name="terrain">The room terrain the shots collide with and react to.</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment, charge, and HUD selection.</param>
    /// <param name="buttons">The actions Samus's projectile producer reads: held actions, this frame's new presses, and the previous frame's new presses (#633).</param>
    /// <param name="samusBombs">Samus's bombs, carrying the shared projectile cooldown, which a successful shot replaces.</param>
    /// <param name="projectileProducerEnabled">Whether to admit the HUD weapon producer; the shots are handled whether or not it runs.</param>
    internal void FireFromHud(SamusTerrain terrain, SamusState samus, ProjectileButtonInput buttons, SamusBombProjectileSystem samusBombs, bool projectileProducerEnabled)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(samusBombs);

        SoundRequest? queued = null;

        // Her standing form's producer is selected by the live HUD item. The beam and Power Bombs
        // share the beam producer; Missiles and Super Missiles reach the missile producer. Ball poses still
        // go to the bombs instead, and while her input is locked no producer runs.
        // A forward-facing pose fires nothing and goes straight on to the shots already out. In particular an elevator
        // may keep accepting physical controller samples without allowing them to charge or fire a weapon.
        // Spin, wall-jump, hurt and special movement use the jumping HUD selection mode, not
        // the normal beam producer. With grapple inactive that mode does
        // nothing: held/released Shoot must leave the charge untouched.
        // Existing projectiles still advance below. Pose input can leave spin
        // first, making the normal producer eligible on the following input phase.
        SamusHudSelectionMode hudMode = SamusHudObservations.SelectionMode(SamusPoseMetadata.ReadMovementType(samus));
        bool preservesJumpCharge = samus.Grapple.Phase == GrapplePhase.Inactive &&
            hudMode == SamusHudSelectionMode.Jump;
        // The turning HUD step waits for an explicit pose-setup muzzle handoff.
        // With no handoff it neither increments charge nor releases it on a Shoot edge.
        bool preservesTurnCharge = hudMode == SamusHudSelectionMode.Turning &&
            samus.TransitionShot.Handed is null;
        // The posture-transition HUD step preserves the carried charge during morph art;
        // ordinary crouch/stand transitions still run their beam producer.
        bool preservesPostureCharge = hudMode == SamusHudSelectionMode.Transition &&
            !SamusHudInput.PostureTransitionAdmitsWeapons(samus.Pose,
                grappleActive: samus.Grapple.Phase != GrapplePhase.Inactive, PostureAdmission);
        if (projectileProducerEnabled && !preservesJumpCharge && !preservesTurnCharge && !preservesPostureCharge && !SamusPoseMetadata.IsForwardFacingPose(samus.Pose) &&
            !SamusPoseMetadata.IsStableBallPose(samus.Pose))
        {
            // The X-ray HUD item fires the normal beam when selected X-ray's Run
            // action is released. Holding Run owns this step even if setup is
            // rejected; selection alone must not consume Shoot. The bindings are read as actions
            // at the latch, so Dash honors remapped bindings.
            bool xrayBeamFallback = samus.SelectedHudItem == HudItem.XRay &&
                (buttons.ControllerInput & GameplayAction.Dash) == GameplayAction.None;
            if (samus.SelectedHudItem is HudItem.Beam or HudItem.PowerBomb || xrayBeamFallback)
            {
                queued = Firing.HandleBeamInput(
                    terrain,
                    samus,
                    buttons.ControllerInput,
                    // Uncharged firing tests both current and prior new-input latches.
                    buttons.ControllerNewInput | buttons.ControllerPreviousNewInput,
                    samusBombs).Request;
            }
            else if (samus.SelectedHudItem is HudItem.Missile or HudItem.SuperMissile)
            {
                queued = BeamFiringResult.Of(Firing.TryFireMissile(
                    samus,
                    buttons.ControllerNewInput,
                    buttons.ControllerPreviousNewInput,
                    samusBombs)?.Sound, SoundRequestPriority.Normal).Request;
            }
        }
        // Firing queues its sound as it fires, before any projectile moves.
        if (queued is { } firing)
            AudioRequests.QueueSound(firing.SoundEffect, firing.Priority);
    }

    /// <summary>
    /// Runs every shot Samus has in flight, in update order, after the bombs (<see cref="SamusBombProjectileSystem.HandleBombs"/>):
    /// the shots' share of the game's projectile handling.
    /// </summary>
    /// <param name="terrain">The room terrain the shots collide with and react to.</param>
    /// <param name="samus">Mutable player owner supplying pose, equipment and the X-ray freeze.</param>
    /// <param name="cameraPosition">Horizontal and vertical camera origin in whole room pixels, used by projectile offscreen handling.</param>
    /// <param name="samusBombs">Samus's bombs, carrying the shared cooldown and the power bomb the shots read.</param>
    /// <param name="progression">The progression a Spacetime shot's overwrite turns on and off.</param>
    internal void HandleShots(SamusTerrain terrain, SamusState samus, Position16<RoomPixels> cameraPosition, SamusBombProjectileSystem samusBombs, SharedGameState progression)
    {
        Ensure.NotNull(terrain.Level);
        Ensure.NotNull(samus);
        Ensure.NotNull(samusBombs);
        Ensure.NotNull(progression);

        // Bombs were handled by the companion class first; the shots run in update order here because a delete cue
        // changes the counter. The HUD producer can activate X-ray during this pass, so the freeze is checked
        // before each shot, not only at entry. A shot with no animation is never updated, so its behaviour never runs;
        // an explosion has no behaviour and only plays its animation.
        foreach (SamusProjectile shot in Pool.InUpdateOrder)
        {
            if (samus.Xray.TimeIsFrozen)
                break;
            if (shot.Playback is null)
                continue;

            // A projectile is given its behaviour only once it holds its place (SamusProjectile.Install).
            if (shot.Behavior is { } behavior && shot.Occupant is { } held)
                behavior.Run(new SamusShotUpdate(held, this, terrain, samus, cameraPosition, samusBombs, progression));

            // Killing a projectile replaces rather than clears a live beam. Consequently the
            // explosion's first frame is selected in this same projectile pass.
            if (shot.Occupant is { Playback: not null } playing)
                Animation.AdvanceProjectileAnimation(playing);
        }
    }

    /// <summary>
    /// Advances the flare draw's three charge-flare animation components as a simulation phase and reports which
    /// components this frame shows, in order. Nothing here depends on the display.
    /// </summary>
    public ChargeFlareComponents AdvanceChargeFlare(SamusState samus)
    {
        Ensure.NotNull(samus);

        if (!Charge.IsCharging)
            return ChargeFlareComponents.None;

        if (samus.HyperBeamEnabled)
        {
            // Hyper Beam replaces the ordinary flare's animations with a backward count through the components'
            // pictures, fast sparks first, then slow sparks, then the main flare. The fast sparks reaching their first
            // picture end the whole flare.
            ChargeFlareComponents hyper = ChargeFlareComponents.HyperBeamFlare;
            foreach (ChargeFlareComponent component in hyper.DrawOrder)
            {
                if (Charge.PlayHyperBeamFlareBackward(component) && component == ChargeFlareComponent.FastSparks)
                    Charge.ClearCount();
            }
            // Every component is shown, the fast spark first, even on the frame that ends the flare.
            return hyper;
        }

        // Updates before the main flare shows start or keep its hidden animation; from then the central
        // flare is drawn, and from the sparks' count both orbiting spark components too. The Hyper Beam's flare counts
        // no updates, so without the Hyper Beam equipped it shows nothing here.
        if (Charge.UpdatesHeld == 1)
        {
            Charge.StartChargeFlare();
        }
        if (Charge.UpdatesHeld < ChargeFlareAnimationDefinitions.MainFlareShownFrom)
            return ChargeFlareComponents.None;

        ChargeFlareComponents shown = Charge.UpdatesHeld < ChargeFlareAnimationDefinitions.SparksShownFrom ? ChargeFlareComponents.MainFlareOnly : ChargeFlareComponents.FlareAndSparks;
        foreach (ChargeFlareComponent component in shown.DrawOrder)
            Animation.AdvanceFlareComponent(component);
        return shown;
    }

    /// <summary>Publishes the charge-flare components <see cref="AdvanceChargeFlare"/> reported, before Samus.</summary>
    public void DrawChargeFlare(
        SpriteFrameBuffer sprites,
        SamusState samus,
        Position16<RoomPixels> cameraPosition,
        ChargeFlareComponents components,
        TurningFloorTransform? turningFloor,
        ChargeFlareArtwork artwork)
    {
        Ensure.NotNull(sprites);
        Ensure.NotNull(samus);
        foreach (ChargeFlareComponent component in components.DrawOrder)
            Animation.DrawFlareComponent(sprites, samus, cameraPosition, component, turningFloor, artwork);
    }

    /// <summary>
    /// Draws the live ordinary-projectile subset after Samus, every live shot on every update, uncharged beams included
    /// (#1683).
    /// </summary>
    public void DrawLiveProjectiles(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusProjectile shot in Pool.InUpdateOrder)
        {
            if (!shot.IsActive ||
                shot.Occupant is not { } held ||
                held.Type.Family.IsBombOrExplosion)
                continue;

            // Unlike a bomb or an explosion, a live shot is drawn wherever it lies, offscreen included; the display clips it.
            if (shot.Sprite is not null)
                ProjectileAnimation.DrawShot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile),
                    shot, cameraPosition, artwork);
        }
    }

    /// <summary>
    /// Advances every trail one update, after the ordinary projectile pass, and removes the trails that have ended. Each
    /// side owns its own timer, animation position and position.
    /// </summary>
    /// <param name="timeIsFrozen">
    /// Whether time is frozen. Frozen trails keep their current picture and are still drawn;
    /// their timers and position steps do not advance.
    /// </param>
    public void AdvanceTrails(bool timeIsFrozen) => Trails.Advance(timeIsFrozen);

    /// <summary>Publishes every live trail side, the newest trail first.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="cameraPosition">Whole-pixel room camera origin subtracted from trail coordinates.</param>
    /// <param name="sheets">The projectile sheets the trail poses are drawn from.</param>
    public void DrawTrails(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork sheets) => Trails.Draw(sprites, cameraPosition, sheets);

    /// <summary>
    /// Draws beam explosions in enemy collision's earlier bomb/explosion phase.
    /// </summary>
    public void DrawExplosions(
        SpriteFrameBuffer sprites,
        Position16<RoomPixels> cameraPosition,
        ProjectileDrawArtwork artwork)
    {
        Ensure.NotNull(sprites);

        foreach (SamusProjectile shot in Pool.InUpdateOrder)
        {
            if (!shot.IsActive || shot.Occupant is not { } held || held.Type.Family is not (
                    SamusProjectileFamily.BeamExplosion or
                    SamusProjectileFamily.MissileExplosion))
                continue;
            _ = held.Type.Family;

            ProjectileAnimation.DrawShot(sprites, new SpriteSource(SpriteSourceKind.SamusProjectile),
                shot, cameraPosition, artwork);
        }
    }

    /// <summary>
    /// Clears every shot, trail and the held charge's flare and glow for forced owners such as
    /// elevator boarding and the intro flashback. A room's load builds a new system instead,
    /// whose held charge is the session's.
    /// </summary>
    public void Reset()
    {
        ClearProjectileData();
        Charge.ClearCount();
        Charge.PreviousCharge = BeamChargeLevel.None;
        Charge.ChargedShotGlow = null;
        Charge.ClearFlare();
        Charge.RestartChargePulse();
    }

    /// <summary>
    /// Clears every shot and trail, without the flare
    /// counter, flare animation state or charge glow state, which <see cref="Reset"/> clears after it.
    /// </summary>
    private void ClearProjectileData()
    {
        Pool.ClearAll();
        Trails.Clear();
        Pool.ShotCount.Clear();
        Firing.InteractionImmunity = UpdateCountdown.Ended();
    }

}
