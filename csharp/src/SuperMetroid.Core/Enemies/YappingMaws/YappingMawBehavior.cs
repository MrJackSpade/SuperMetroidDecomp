using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The Yapping Maw species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class YappingMawBehavior : IEnemySpecies, IEnemySpawnFactory<YappingMawSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.YappingMaw];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, YappingMawSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new YappingMawEntity(this, occupant, place, common, settings));

    /// <summary>Creates the four auxiliary body-link projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Owns the root effect actor accompanying the body links.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Caps camera movement while Samus is held at the Maw's mouth.</summary>
    private readonly SamusScrollCap _scrollCap;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Maw AI to room, body-link, effect-actor, and Samus camera services.</summary>
    internal YappingMawBehavior(EnemyProjectileEngine projectiles, RoomEffectActors effectActors, SamusScrollCap scrollCap, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
        _effectActors = effectActors;
        _scrollCap = scrollCap;
    }

    /// <summary>What <see cref="ApplyYappingMawCue"/> does for each <see cref="YappingMawCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<YappingMawCueKind, Action<YappingMawEnemyState>> ApplyYappingMawCueForYappingMawCueKind => field ??= new(new Dictionary<YappingMawCueKind, Action<YappingMawEnemyState>>
    {
        [YappingMawCueKind.PlayBiteSound] = state => PlayYappingMawAttackSound(state),
    });

    /// <summary>Carries out one cue of the Yapping Maw's animations: holding a grabbed Samus at a mouth offset, or the bite sound.</summary>
    private void ApplyYappingMawCue(YappingMawEnemyState state, YappingMawCue cue)
    {
        cue.React(ApplyYappingMawCueReactions, new ApplyYappingMawCueContext(state, this));
    }

    /// <summary>What <see cref="ApplyYappingMawCue"/> does on each <see cref="YappingMawCue"/>, built once (#2553).</summary>
    private static YappingMawCueReactions<ApplyYappingMawCueContext> ApplyYappingMawCueReactions { get; } = new(
        HoldSamus: static (hold, context) => SetYappingMawHeldOffset(context.State, hold.Aim),
        Signal: static (signal, context) =>
        {
            context.Owner.ApplyYappingMawCueForYappingMawCueKind[signal.Kind](context.State);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Owner">The <see cref="YappingMawBehavior"/> reacting.</param>
    private readonly record struct ApplyYappingMawCueContext(YappingMawEnemyState State, YappingMawBehavior Owner);

    /// <summary>Initializes the Yapping Maw.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal YappingMawEnemyState InitializeYappingMaw(RoomEnemyOccupant occupant, YappingMawSpawnSettings yappingMawSpawnSettings, RoomEnemyPlace place)
    {
        // The body links are created mouth end first and kept root to mouth.
        var bodies = new EnemyProjectile?[YappingMawAttackDefinitions.TongueLinks];
        for (int link = bodies.Length - 1; link >= 0; link--)
        {
            // A full pool spawns no body, and the link shows nothing.
            if (_projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.YappingMawBody,
                    EnemyProjectileBehavior.Still, SpriteGraphics.SpawnedBy(occupant, place.Shading),
                    new YappingMawBodyProjectileAnimation(YappingMawReach.Of(yappingMawSpawnSettings.Attack).Body)) is not { } body)
                continue;

            body.Position = body.Position.WithWhole(place.Position.Whole);
            bodies[link] = body;
        }

        var state = new YappingMawEnemyState
        {
            Action = YappingMawAction.WaitingForSamus,
            Origin = place.Position.Whole,
            RetractedDelayTimer = UpdateCountdown.Lasting(YappingMawAttackDefinitions.RetractedHold),
            Links = [.. bodies.Select(body => new YappingMawLink(body))],
            // The root's kind follows the Maw's attack direction; the Maw holds the root it spawned.
            RootEffectActor = _effectActors.SpawnRoomEffectActor(
                new(place.Position.X.Whole, place.Position.Y.Whole + (YappingMawAttackDefinitions.RootDistance.Toward(yappingMawSpawnSettings.Attack.Reversed()))),
                YappingMawReach.Of(yappingMawSpawnSettings.Attack).Root,
                SpriteGraphics.SpawnedBy(occupant, place.Shading)),
        };

        state.Playback = EnemyAnimationPlayback.Start(place, YappingMawAnimations.StartingWith(
            YappingMawReach.Of(yappingMawSpawnSettings.Attack).Bite));
        return state;
    }

    /// <summary>What <see cref="RunYappingMawMain"/> does for each <see cref="YappingMawAction"/> member, each member's own arm.</summary>
    private IdentityTable<YappingMawAction, Action<YappingMawSpawnSettings, RoomEnemyPlace, YappingMawEnemyState, SamusState>> RunYappingMawMainForYappingMawAction => field ??= new(new Dictionary<YappingMawAction, Action<YappingMawSpawnSettings, RoomEnemyPlace, YappingMawEnemyState, SamusState>>
    {
        [YappingMawAction.WaitingForSamus] = (settings, place, state, samus) => WaitForSamusWithYappingMaw(settings, state, samus),
        [YappingMawAction.BeginExtension] = (_, place, state, samus) => BeginYappingMawExtension(place, state),
        [YappingMawAction.ExtendingOrRetracting] = (settings, place, state, samus) => AdvanceYappingMawExtension(settings, place, state, samus),
        [YappingMawAction.RetractedDelay] = (_, place, state, samus) => WaitAfterYappingMawRetraction(place, state, samus),
    });

    /// <summary>Runs the Maw's main behaviour, then places its four links.</summary>
    private void RunYappingMawMain(
        YappingMawSpawnSettings yappingMawSpawnSettings, RoomEnemyPlace place,
        YappingMawEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera)
    {
        state.GrabCooldown = state.GrabCooldown.Counted();
        state.IsOffScreen = IsYappingMawPointOffScreen(place, camera);

        RunYappingMawMainForYappingMawAction[state.Action](yappingMawSpawnSettings, place, state, samus);

        // The mouth owns the final point. The four body links own the root, quarter, half and three-quarter points of
        // the tongue; they do nothing of their own but show their picture where the Maw places them each update.
        PositionYappingMawBodyProjectile(state.Links[0], state.Origin);
        PositionYappingMawBodyProjectile(state.Links[1], YappingMawLinkPosition(state, 0));
        PositionYappingMawBodyProjectile(state.Links[2], YappingMawLinkPosition(state, 1));
        PositionYappingMawBodyProjectile(state.Links[3], YappingMawLinkPosition(state, 2));
    }

    /// <summary>Waits for Samus, including the deliberately approximate distance check.</summary>
    private static void WaitForSamusWithYappingMaw(
        YappingMawSpawnSettings yappingMawSpawnSettings,
        YappingMawEnemyState state,
        SamusState samus)
    {
        (int distance, TrigonometryAngle bearing) = CalculateYappingMawTarget(
            state.Origin,
            samus.Position.Whole);
        state.DistanceToSamus = distance;

        if (distance < YappingMawAttackDefinitions.GrabSafetyDistance)
        {
            // A target already inside the safety distance triggers no extension, and starts the grab cooldown again on every update.
            state.GrabCooldown = YappingMawGrabCooldown.Started(YappingMawAttackDefinitions.GrabCooldown);
            return;
        }

        // An attack starts only strictly inside the placement's activation distance (#2450).
        if (distance >= yappingMawSpawnSettings.ActivationDistance)
            return;

        // The tongue reaches a nearer target where it is and a farther one only as far as its longest reach.
        if (state.DistanceToSamus >= YappingMawAttackGeometry.MaximumTargetDistance)
            state.DistanceToSamus = YappingMawAttackGeometry.MaximumTargetDistance;

        state.AimAngle = bearing;
        state.Action = YappingMawAction.BeginExtension;
    }

    /// <summary>Runs the one-frame setup action.</summary>
    private static void BeginYappingMawExtension(RoomEnemyPlace place, YappingMawEnemyState state)
    {
        state.Extension = YappingMawCurl.None;
        state.SpeedCurvePlace = QuadraticSpeedCurvePlace.Start;
        state.CurlPhase = YappingMawCurlPhase.Extending;
        state.SegmentRadius = state.DistanceToSamus / 2;
        state.CurlAngle = state.AimAngle.ToAngle();

        state.Baseline = new Offset16<RoomPixels>(
            CalculateYappingMawX(TrigonometryAngle.HalfTurn, state.SegmentRadius),
            CalculateYappingMawY(TrigonometryAngle.HalfTurn, state.SegmentRadius / 2));

        YappingMawAim aim = AimOf(state.AimAngle);
        state.Aim = aim;
        state.Playback = EnemyAnimationPlayback.Start(place, YappingMawAnimations.StartingWith(YappingMawBites.AttackForDirection(aim)));

        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(aim);
        state.Action = YappingMawAction.ExtendingOrRetracting;
    }

    /// <summary>
    /// The angle steps each link of the chain bends by for a curl of <paramref name="curl"/> whole angle units. The
    /// extension only runs while the curl is 0 through <see cref="YappingMawAttackDefinitions.LargestCurl"/>: setup starts it at zero, the
    /// clamp caps it, and the update that carries it negative leaves the extension; a curl outside that range is refused.
    /// </summary>
    private static int BendStepOf(int curl) => Ensure.BetweenInclusive(0, YappingMawAttackDefinitions.LargestCurl.WholeUnits, curl) / YappingMawAttackDefinitions.CurlPerBendStep;

    /// <summary>Runs the curved four-link solver and state transition.</summary>
    private void AdvanceYappingMawExtension(
        YappingMawSpawnSettings yappingMawSpawnSettings, RoomEnemyPlace place,
        YappingMawEnemyState state,
        SamusState samus)
    {
        int bendStep = BendStepOf(state.Extension.WholeUnits);
        // The bend corrects the eight offsets in one chain, link by link and X before Y (YappingMawBendChain).
        var chain = new YappingMawBendChain();
        for (int segment = 0; segment < state.Links.Count; segment++)
        {
            int multiple = bendStep * (segment + 1);
            TrigonometryAngle linkAngle = state.MirroredBend
                ? TrigonometryAngle.HalfTurn.TurnedBySteps(multiple)
                : TrigonometryAngle.HalfTurn.TurnedBySteps(-multiple);

            int rawX = YappingMawBendChain.Held(CalculateYappingMawX(linkAngle, state.SegmentRadius) - state.Baseline.X);
            int rawY = YappingMawBendChain.Held(CalculateYappingMawY(linkAngle, state.SegmentRadius / 2) - state.Baseline.Y);

            // The curl rotates a correction whose length is the link's X offset for both components, which gives the
            // curve its lopsided shape.
            int correctionX = YappingMawBendChain.Held(
                CalculateYappingMawX(state.CurlAngle.ToTrigonometryAngle(), rawX) -
                CalculateYappingMawX(RotationAngle.Zero.ToTrigonometryAngle(), rawX));
            int correctionY = YappingMawBendChain.Held(
                CalculateYappingMawY(state.CurlAngle.ToTrigonometryAngle(), rawX) -
                CalculateYappingMawY(RotationAngle.Zero.ToTrigonometryAngle(), rawX));
            int segmentX = chain.Corrected(rawX, correctionX);
            int segmentY = chain.Corrected(rawY, correctionY);
            state.Links[segment].Offset = new Offset16<RoomPixels>(
                YappingMawBendChain.Offset(segmentX), YappingMawBendChain.Offset(segmentY));
        }

        place.Position = place.Position.WithWhole(YappingMawLinkPosition(state, 3));
        AddYappingMawQuadraticSpeed(state);

        if (!state.Extension.IsNegative)
        {
            if (state.Extension.WholeUnits >= YappingMawAttackDefinitions.LargestCurl.WholeUnits)
            {
                state.Extension = YappingMawAttackDefinitions.LargestCurl;
                // The turn-around steps half a mark further than the update's own advance: from extending at a mark's
                // speed to retracting at the same mark's, or from retracting on to extending at the next mark's.
                if (state.CurlPhase == YappingMawCurlPhase.Retracting)
                {
                    state.SpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
                    state.CurlPhase = YappingMawCurlPhase.Extending;
                }
                else
                {
                    state.CurlPhase = YappingMawCurlPhase.Retracting;
                }
            }

            if (state.HasGrabbedSamus)
                PositionSamusInYappingMaw(place, state, samus);
            return;
        }

        state.Action = YappingMawAction.RetractedDelay;
        state.GrabCooldown = YappingMawGrabCooldown.Started(YappingMawAttackDefinitions.GrabCooldown);

        // The six retracted animations are not one per aim. The right and left sectors have
        // dedicated art, while every other sector shares its attack direction's animation.
        VerticalDirection reach = yappingMawSpawnSettings.Attack;
        state.Playback = new(YappingMawAnimations.StartingWith(YappingMawReach.Of(reach).CooldownAfter(state.Aim)), AnimationPlace.StartOfAnimation);
        place.AnimationClock.Restart();

        if (state.HasGrabbedSamus &&
            YappingMawRetractionHold.CarriesHeldSamus(reach, bitStraightAcross: state.Aim is YappingMawAim.Right or YappingMawAim.Left))
            PositionSamusInYappingMaw(place, state, samus);
    }

    /// <summary>Adds the current quadratic speed to the curl, carrying the fraction, then advances its place along the curve.</summary>
    private static void AddYappingMawQuadraticSpeed(YappingMawEnemyState state)
    {
        state.Extension = state.Extension.MovedBy(state.CurlPhase.CurlChange(state.SpeedCurvePlace.Speed.Speed));
        state.SpeedCurvePlace += QuadraticSpeedCurvePlace.OneMark;
    }

    /// <summary>Waits out the hold after retraction (<see cref="YappingMawAttackDefinitions.RetractedHold"/>), then releases Samus unless she is dying.</summary>
    private void WaitAfterYappingMawRetraction(
        RoomEnemyPlace mouth,
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (state.HasGrabbedSamus)
            PositionSamusInYappingMaw(mouth, state, samus);

        // A hold that has run out stays at zero while Samus's death keeps the Maw waiting.
        if (!state.RetractedDelayTimer.Step() || samus.DeathSequence.IsActive)
            return;

        samus.Control.Unlock();
        state.HasGrabbedSamus = false;
        state.GrabCooldown = YappingMawGrabCooldown.Started(YappingMawAttackDefinitions.GrabCooldown);
        state.RetractedDelayTimer = UpdateCountdown.Lasting(YappingMawAttackDefinitions.RetractedHold);
        state.Action = YappingMawAction.WaitingForSamus;
    }

    /// <summary>Holds Samus in the mouth, ending her grapple or her spin or wall jump, and caps her scrolling.</summary>
    private void PositionSamusInYappingMaw(
        RoomEnemyPlace place,
        YappingMawEnemyState state,
        SamusState samus)
    {
        // The hold drops an active grapple; otherwise a spin or wall jump becomes her standing pose, committed before
        // the Maw releases her input. Her other poses stay as they are.
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
        {
            samus.Grapple.Phase = GrapplePhase.Dropped;
        }
        else
        {
            SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
            if (movement is SamusMovementType.SpinJumping or SamusMovementType.WallJumping)
            {
                // She stands facing the way she faced, with the standing pose's collision radii and animation.
                samus.ChangePose(SamusPoseMetadata.IsFacing(samus, SamusFacingDirection.Left)
                    ? SamusPoseId.FacingLeftNormalPose
                    : SamusPoseId.FacingRightNormalPose);
                SamusPoseMetadata.RefreshCollisionRadii(samus);
                samus.CommitPoseHistory();
            }
        }

        samus.Position = samus.Position.WithWhole(place.Position.Whole + state.HeldSamusOffset);
        _scrollCap.CapSamusScrollingSpeed(samus);
    }

    /// <summary>Custom touch reaction: grab, never ordinary contact damage.</summary>
    private static void ResolveYappingMawTouch(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (!state.GrabCooldown.AllowsGrab || state.HasGrabbedSamus)
            return;

        state.GrabCooldown = YappingMawGrabCooldown.Spent;
        state.HasGrabbedSamus = true;
        samus.Control.LockOutPlayer();
    }

    /// <summary>
    /// Runs after a hit's common damage on the Maw it struck. Ice releases a living Maw's captive; death additionally
    /// destroys all four body-link projectiles and the root effect actor, which the killed Maw's state still names.
    /// </summary>
    private static void ResolveYappingMawShotAfterCommon(
        RoomEnemyPlace place,
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (place.Health != 0)
        {
            if (place.Freeze.IsOver)
                return;

            ReleaseSamusFromYappingMaw(state, samus);
            return;
        }

        foreach (YappingMawLink link in state.Links)
            link.Body?.Delete();
        state.RootEffectActor.End();
        ReleaseSamusFromYappingMaw(state, samus);
    }

    /// <summary>Unlocks Samus input after retraction unless her death sequence owns it.</summary>
    private static void ReleaseSamusFromYappingMaw(
        YappingMawEnemyState state,
        SamusState samus)
    {
        if (!samus.DeathSequence.IsActive)
            samus.Control.Unlock();
        state.HasGrabbedSamus = false;
    }

    /// <summary>Shows a frozen Maw's colours on its four body links and its root.</summary>
    internal static void RunYappingMawFrozen(
        RoomEnemyPlace place,
        YappingMawEnemyState state)
    {
        PictureShading shading = SelectYappingMawFrozenShading(
            place.Shading,
            place.Freeze.UpdatesLeft);
        foreach (YappingMawLink link in state.Links)
            if (link.Body is { } body)
                body.Graphics = body.Graphics.In(shading);

        state.RootEffectActor.Graphics = state.RootEffectActor.Graphics.In(shading);
    }

    /// <summary>Flashes the frozen colours against the Maw's own colours once the freeze passes its steady stretch (<see cref="EnemyDrawPhaseDefinitions.FrozenColoursSteadyUpdates"/>).</summary>
    private static PictureShading SelectYappingMawFrozenShading(
        PictureShading originalShading,
        int frozenTimer)
    {
        if (frozenTimer == 0)
            return originalShading;

        // The frozen colours flash back to the actor's own on alternating pairs only while fewer frozen updates are left
        // than the steady stretch's.
        if (frozenTimer < EnemyDrawPhaseDefinitions.FrozenColoursSteadyUpdates && !EnemyDrawPhaseDefinitions.FrozenColoursShown.IsInSecondHalfAt(frozenTimer))
            return originalShading;
        return EnemyShades.FrozenLook;
    }

    /// <summary>Holds a grabbed Samus at the mouth offset of <paramref name="aim"/>, as the hold cue asks.</summary>
    private static void SetYappingMawHeldOffset(
        YappingMawEnemyState state,
        YappingMawAim aim)
    {
        state.HeldSamusOffset = YappingMawAttackGeometry.HeldSamusOffset(aim);
    }

    /// <summary>Publishes the attack sound only when this Maw is visible.</summary>
    private void PlayYappingMawAttackSound(YappingMawEnemyState state)
    {
        if (!state.IsOffScreen)
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.YappingMawAttackSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Moves one tongue link's body projectile to its solved point.</summary>
    private static void PositionYappingMawBodyProjectile(YappingMawLink link, Position16<RoomPixels> position)
    {
        if (link.Body is { } body)
            body.Position = body.Position.WithWhole(position);
    }

    /// <summary>The root plus one solved link's signed far-end offset, wrapping around the coordinate range.</summary>
    private static Position16<RoomPixels> YappingMawLinkPosition(YappingMawEnemyState state, int segment) =>
        state.Origin + state.Links[segment].Offset;

    /// <summary>Whether the Maw lies outside the view (<see cref="EnemyView.Contains"/>).</summary>
    private static bool IsYappingMawPointOffScreen(
        RoomEnemyPlace place,
        Position16<RoomPixels> camera) =>
        !EnemyView.Contains(place.Position.Whole, camera);

    /// <summary>The Maw's distance and bearing to Samus.</summary>
    private static YappingMawTarget CalculateYappingMawTarget(
        Position16<RoomPixels> origin,
        Position16<RoomPixels> samus)
    {
        int deltaX = samus.X - origin.X;
        int absoluteX = Math.Abs(deltaX);
        if (YappingMawDistanceReach.MeasuresByAxisAlone(absoluteX))
            return new YappingMawTarget(absoluteX, TrigonometryAngle.QuarterTurn);

        int deltaY = samus.Y - origin.Y;
        int absoluteY = Math.Abs(deltaY);
        if (YappingMawDistanceReach.MeasuresByAxisAlone(absoluteY))
            return new YappingMawTarget(absoluteY, TrigonometryAngle.QuarterTurn);

        TrigonometryAngle positiveQuadrantAngle = EnemyMath.CalculateAimAngle(new(absoluteX, absoluteY));
        int distance =
            Math.Abs(EnemyMath.CircleSinePixels(positiveQuadrantAngle - TrigonometryAngle.QuarterTurn, absoluteX)) +
            Math.Abs(EnemyMath.CircleSinePixels(positiveQuadrantAngle, absoluteY));
        return new YappingMawTarget(distance, EnemyMath.CalculateAimAngle(new(deltaX, deltaY)));
    }

    /// <summary>
    /// Which of the eight aims the bearing <paramref name="aim"/> points in: eight equal sectors in the aims' clockwise
    /// order from up, each centred on its direction, so the aim is turned on half a sector before its sector is read.
    /// </summary>
    private static YappingMawAim AimOf(TrigonometryAngle aim) => YappingMawAnimationDefinitions.Aims.At(
        Array.IndexOf(TrigonometryAngle.DirectionsClockwise, aim.NearestDirection()));

    /// <summary>X is the Y offset a quarter turn behind.</summary>
    private static int CalculateYappingMawX(TrigonometryAngle angle, int length) =>
        CalculateYappingMawY(angle - TrigonometryAngle.QuarterTurn, length);

    /// <summary>
    /// The offset of a link <paramref name="length"/> along the turn <paramref name="angle"/>, -254 through 254 pixels: the
    /// reach of the length the short product uses (<see cref="ShortCircleProduct.RadiusInUse"/>) along the fine sine read
    /// coarsely (<see cref="UnitComponent.CoarseFineSineOf"/>), in whole pairs of pixels rounded down
    /// (<see cref="UnitComponent.ReachRoundedDownToPixelPairsOf"/>), so a point on the negative side short of a pair lies a
    /// whole pair further out (#1620, #2453); its consumers wrap it onto the room's axis.
    /// </summary>
    private static int CalculateYappingMawY(TrigonometryAngle angle, int length) =>
        // The game samples the angle reflected below zero, whose sine is this angle's sine reversed.
        UnitComponent.CoarseFineSineOf(angle).Reversed.ReachRoundedDownToPixelPairsOf(ShortCircleProduct.RadiusInUse(length));

    /// <summary>Routes bomb hits through the Maw's shared shot reaction.</summary>
    /// <param name="state">The state of the enemy the reaction is built for, which it holds from its setup.</param>
    internal static EnemyBombReaction YappingMawBombReaction(YappingMawEnemyState state) => new()
    {
        AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, state, c.Samus),
    };

    /// <summary>Runs YappingMaw's main AI.</summary>
    internal void RunYappingMawMainAi(YappingMawSpawnSettings yappingMawSpawnSettings, RoomEnemyPlace place, YappingMawEnemyState yappingMawEnemyState, IEnemyAiFrame frame)
    {
        RunYappingMawMain(
            yappingMawSpawnSettings, place,
            yappingMawEnemyState,
            frame.Samus,
            frame.Camera);
    }

    /// <summary>Advances YappingMaw's typed animation.</summary>
    internal void AdvanceYappingMawTypedAnimation(RoomEnemyPlace place, YappingMawEnemyState yappingMawEnemyState) =>
        EnemyAnimationPlayback.Advance(place, yappingMawEnemyState, YappingMawAnimations.Segments,
            static (YappingMawEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                ApplyYappingMawCue(state, cue);
                return null;
            });

    /// <summary>Handles frozen release and death cleanup after common shot damage.</summary>
    /// <param name="state">The state of the enemy the reaction is built for, which it holds from its setup.</param>
    internal static EnemyShotReaction YappingMawShotReaction(YappingMawEnemyState state) => new()
    {
        AfterCommon = c => ResolveYappingMawShotAfterCommon(c.Enemy, state, c.Samus),
    };

    /// <summary>Runs contact as a grab that locks Samus without ordinary touch damage.</summary>
    internal static void ReactToYappingMawTouch(EnemyTouchContext context, YappingMawEnemyState yappingMawEnemyState) =>
        // The Maw's touch deals no contact damage: once its grab cooldown has run out, a touch catches Samus,
        // taking her input and holding her in the mouth.
        ResolveYappingMawTouch(yappingMawEnemyState, context.Samus);
}
