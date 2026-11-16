using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The Botwoon species owner: its per-room state, behaviour and the interactions it registers. Botwoon runs his own body
/// segments and spit (#1801): it is their projectile behaviour, since a segment is a piece of Botwoon run from his state.
/// </summary>
[RoomLifetime]
internal sealed class BotwoonBehavior : EnemyProjectileMotionBehavior<BotwoonProjectileMotion>, IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Botwoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new BotwoonEntity(this, place, common);

    /// <summary>Active room services and RNG used by path and drop decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Botwoon's thirteen body actors and spit projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns the wall-phase explosion sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Runs common touch behavior before Botwoon's special death tail.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns the sixteen special drops from the chance table.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Botwoon's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Binds Botwoon's head, body, colours, collision, and drop services.</summary>
    internal BotwoonBehavior(BotwoonBehaviorDependencies dependencies)
        : base(StartingMotions)
    {
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _effectActors = dependencies.EffectActors;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _audio = dependencies.Audio;
    }

    /// <summary>Sets Botwoon up when the room spawns it.</summary>
    internal void InitializeBotwoon(RoomEnemyPlace head)
    {
        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and opens both of its screens to the camera, aligned to their top.
        bool defeated = _room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss);
        var state = new BotwoonEnemyState(defeated ? [] : SpawnBotwoonBody(head));
        head.SetSpeciesState(state);
        // The initializer installs the hidden animation: the head starts hidden in its hole.
        state.HeadPlayback = EnemyAnimationPlayback.Start(head, BotwoonAnimations.HeadStartingWith(BotwoonHeadAnimationSegment.Hidden));

        if (defeated)
        {
            _frameResults.BotwoonWall = BotwoonWallMutation.Clear;
            head.Presence = head.Presence.With(EnemyPresence.Deleted);
            return;
        }

        state.InitialDelayTimer = BotwoonFightDefinitions.OpeningDelay;
        var initialSpeed = BotwoonSpeeds.ForHealthPhase(0);
        state.Speed = initialSpeed.MovementSpeed;
        state.SegmentSpacingEntries = initialSpeed.SegmentSpacingEntries;
        state.InsideHole = true;
        state.PreviousInsideHole = true;
        state.InitialAction = true;
        state.TargetHole = BotwoonHole.Left;
        state.HalfHealth = head.Health / BotwoonFightDefinitions.FasterBelowShareOfHealth;
        state.QuarterHealth = head.Health / BotwoonFightDefinitions.FastestBelowShareOfHealth;
        // The initializer makes the head intangible: it starts hidden in its hole, out of every collision pass.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);

        for (int history = 0; history < state.HeadHistory.Length; history++)
        {
            state.HeadHistory[history] = head.Position.Whole;
        }
        // Botwoon's position history is never seeded by Botwoon's initializer. It
        // lies inside the enemy state the room's enemy setup clears on room
        // load, so body segments read (0,0) until the ring has been written that far; the
        // freshly constructed state's arrays are that cleared state.
    }

    /// <summary>Runs Botwoon's main AI and its movement-phase step for one update.</summary>
    private void RunBotwoonMain(RoomEnemyPlace head, BotwoonEnemyState state, SamusState samus)
    {
        // A fatal hit does not delete Botwoon. He keeps following his current path until
        // the tail has crossed the next hole, then the synchronized head/body death begins.
        if (state.PendingDeath && state.ExitTransitionComplete)
        {
            state.BodyDeathStarted = true;
            state.Phase = BotwoonPhase.DeathDelay;
            state.DeathTimer = BotwoonDeathDefinitions.DeathDelayStart;
            state.PendingDeath = false;
            state.ExitTransitionComplete = false;
        }

        switch (state.Phase)
        {
            case BotwoonPhase.InitialDelay:
                if (--state.InitialDelayTimer == 0)
                    state.Phase = BotwoonPhase.ChooseNextAction;
                break;

            case BotwoonPhase.ChooseNextAction:
                RunBotwoonActionSelector(head, state, samus);
                break;

            case BotwoonPhase.FollowAuthoredPath:
                RunBotwoonAuthoredPathState(head, state, samus);
                break;

            case BotwoonPhase.SpitWhileHidden:
                RunBotwoonHiddenSpitState(head, state, samus);
                break;

            case BotwoonPhase.DeathDelay:
                state.DeathTimer++;
                if (state.DeathTimer >= BotwoonDeathDefinitions.DeathDelayEnd)
                    state.Phase = BotwoonPhase.HeadFalling;
                break;

            case BotwoonPhase.HeadFalling:
                RunBotwoonHeadFall(head, state);
                break;

            case BotwoonPhase.WaitForBody:
                if (state.LastBodySegmentLanded)
                    BeginBotwoonWallExplosions(head, state, samus);
                break;

            case BotwoonPhase.WallExplosions:
                RunBotwoonWallExplosions(head, state);
                break;

            default:
                throw new InvalidDataException(
                    $"Botwoon has no behaviour for phase {state.Phase}.");
        }

        UpdateBotwoonHealthBand(head, state);
    }

    /// <summary>Chooses an authored traversal or hidden spit action after the current path completes.</summary>
    private void RunBotwoonActionSelector(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (!state.PathComplete)
        {
            RunBotwoonMovingFrame(head, state, samus);
            DetectBotwoonHole(head, state);
            return;
        }

        state.PathComplete = false;
        int choice = 0;
        if (!state.InsideHole && !state.InitialAction && state.HealthPhase == 0)
            choice = _room.Random.Next(BotwoonNavigationDefinitions.ActionChoice);
        state.InitialAction = false;

        if (choice < BotwoonNavigationDefinitions.AuthoredPathActions)
        {
            state.Phase = BotwoonPhase.FollowAuthoredPath;
            state.AttackTimer = 0;
            state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
            ChooseBotwoonPath(head, state);
        }
        else
        {
            state.Phase = BotwoonPhase.SpitWhileHidden;
            state.HeadAction = BotwoonHeadAction.AimAtSamus;
            state.AttackTimer = BotwoonFightDefinitions.HiddenSpitCooldown;
            // The spit step makes the spitting head tangible again.
            head.Contact = head.Contact.Without(EnemyContact.IgnoresSamus);
        }
    }

    /// <summary>Finishes a path at its terminal marker, then returns to action selection.</summary>
    private void RunBotwoonAuthoredPathState(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.PathComplete)
        {
            state.PathComplete = false;
            state.Phase = BotwoonPhase.ChooseNextAction;
            state.Movement = BotwoonMovement.MoveTowardHole;
            if (state.InsideHole)
                state.HoleLatch = false;
            return;
        }

        RunBotwoonMovingFrame(head, state, samus);
    }

    /// <summary>Waits for the spit timer, then chooses whether the next path begins inside a hole.</summary>
    private void RunBotwoonHiddenSpitState(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.AttackTimer != 0)
        {
            RunBotwoonHeadAction(head, state, samus);
            return;
        }

        state.PathComplete = false;
        state.Phase = BotwoonPhase.FollowAuthoredPath;
        state.HeadAction = BotwoonHeadAction.AnimateFromMovement;

        state.InsideHole = !state.PendingDeath && BotwoonNavigationDefinitions.StartsInsideHole.IsSetIn(_room.Random.Generate());
        state.PreviousInsideHole = state.InsideHole;
        if (state.InsideHole)
        {
            state.HoleLatch = false;
            state.SavedHoleRingIndex = null;
        }
        ChooseBotwoonPath(head, state);
    }

    /// <summary>Moves the head, writes position history, places and orients body segments, then tests holes.</summary>
    private void RunBotwoonMovingFrame(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        RunBotwoonMovement(head, state);
        RecordBotwoonHeadPosition(head, state);
        PositionBotwoonBody(state);
        RunBotwoonHeadAction(head, state, samus);
        OrientBotwoonBody(head, state);
        state.RingIndex = BotwoonTrailRing.PlaceAt(state.RingIndex + 1);
    }

    /// <summary>Moves Botwoon toward a hole or along one of his authored paths, as his movement says.</summary>
    internal static void RunBotwoonMovement(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        switch (state.Movement)
        {
            case BotwoonMovement.MoveTowardHole:
                MoveBotwoonTowardHole(head, state);
                return;
            case BotwoonMovement.FollowAuthoredPath:
                FollowBotwoonPath(head, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Botwoon has no behaviour for movement {state.Movement}.");
        }
    }

    /// <summary>Steers the head toward its selected hole until inside/outside state changes.</summary>
    private static void MoveBotwoonTowardHole(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigation.Hole(state.TargetHole);
        Offset16<RoomPixels> delta = hole.Target - head.Position.Whole;
        Bearing bearing = EnemyMath.CalculateBearing(new(
            ClampBotwoonTargetDelta(delta.X), ClampBotwoonTargetDelta(delta.Y)));
        state.MovementAngle = bearing.ToTrigonometryAngle().ToAngle();

        if (state.InsideHole == state.PreviousInsideHole)
        {
            AddBotwoonAngleVector(head, state.MovementAngle.ToTrigonometryAngle(), state.Speed);
        }
        else
        {
            state.PreviousInsideHole = state.InsideHole;
            state.PathComplete = true;
        }
    }

    /// <summary>
    /// Restricts target distance to the range the signed bearing supports, exactly as the game clamps it (#1627):
    /// beyond <see cref="BotwoonNavigationDefinitions.HoleBearingReach"/> pixels either way it reads as that reach, except that a distance of exactly
    /// 256 pixels upward or leftward is kept as it is. The bearing toward his hole, and so his path, follows these values.
    /// </summary>
    private static int ClampBotwoonTargetDelta(int value) =>
        value < -(BotwoonNavigationDefinitions.HoleBearingReach + 1) ? -BotwoonNavigationDefinitions.HoleBearingReach : value > BotwoonNavigationDefinitions.HoleBearingReach ? BotwoonNavigationDefinitions.HoleBearingReach : value;

    /// <summary>
    /// Refreshes the health phase, then draws where the next walk from his hole leads, in the open or inside the wall,
    /// and starts it.
    /// </summary>
    private void ChooseBotwoonPath(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        UpdateBotwoonHealthPhase(head, state);
        int draw = _room.Random.Next(BotwoonNavigationDefinitions.PathDraw);
        StartBotwoonWalk(state, BotwoonNavigation.WalkFrom(state.TargetHole, state.InsideHole, draw));
    }

    /// <summary>Changes traversal speed and body spacing at half and quarter health outside holes.</summary>
    internal static void UpdateBotwoonHealthPhase(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.InsideHole)
            return;

        int phase = 0;
        if (head.Health != 0 && head.Health < state.HalfHealth)
            phase = head.Health < state.QuarterHealth ? 2 : 1;
        state.HealthPhase = phase;
        var speed = BotwoonSpeeds.ForHealthPhase(phase);
        state.Speed = speed.MovementSpeed;
        state.SegmentSpacingEntries = speed.SegmentSpacingEntries;
    }

    /// <summary>
    /// Starts <paramref name="walk"/>: its path, the way it is walked, its first step and the hole it reaches. The game
    /// loaded the walk on the next movement update; nothing reads the walk or the target hole before then, so loading it
    /// as it is drawn moves him the same (#1868).
    /// </summary>
    internal static void StartBotwoonWalk(BotwoonEnemyState state, BotwoonPathWalk walk)
    {
        state.Path = walk.Path;
        state.PathTraversal = walk.Traversal;
        state.PathStep = walk.FirstStep;
        state.TargetHole = walk.Destination;
        state.PathComplete = false;
        state.Movement = BotwoonMovement.FollowAuthoredPath;
    }

    /// <summary>
    /// Takes as many path steps as the current speed allows. Walking off either end of the path completes it; the
    /// steps already taken on that update are then discarded, so the head does not move on its final update.
    /// </summary>
    private static void FollowBotwoonPath(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        BotwoonPathDefinition path = state.Path ?? throw new InvalidOperationException("Botwoon follows a path before choosing one.");
        bool backward = state.PathTraversal == BotwoonPathTraversal.Backward;
        Offset16<RoomPixels> total = Offset16<RoomPixels>.Zero;
        for (int taken = 0; taken < state.Speed; taken++)
        {
            if (!BotwoonNavigation.HasStep(path, state.PathStep))
            {
                state.PathComplete = true;
                return;
            }
            total += BotwoonNavigation.Step(path, state.PathStep);
            state.PathStep += backward ? -1 : 1;
        }

        head.Position += backward ? -total : total;
    }

    /// <summary>Writes the head's whole-pixel position into the head-position history ring.</summary>
    private static void RecordBotwoonHeadPosition(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        state.History[state.RingIndex] = head.Position.Whole;
    }

    /// <summary>Spaces thirteen body projectiles along older head-history samples.</summary>
    private static void PositionBotwoonBody(BotwoonEnemyState state)
    {
        int historyIndex = BotwoonTrailRing.PlaceAt(state.RingIndex - state.SegmentSpacingEntries);
        for (int segmentIndex = state.Body.Count - 1; segmentIndex >= 0; segmentIndex--)
        {
            BotwoonSegmentState segment = state.Body[segmentIndex];

            ToggleBotwoonSegmentAtHole(state, segment, historyIndex);
            segment.Projectile.Position = segment.Projectile.Position.WithWhole(state.History[historyIndex]);
            historyIndex = BotwoonTrailRing.PlaceAt(historyIndex - state.SegmentSpacingEntries);
        }
    }

    /// <summary>Changes a segment's collision mode when its delayed history cursor crosses a hole.</summary>
    private static void ToggleBotwoonSegmentAtHole(
        BotwoonEnemyState state,
        BotwoonSegmentState segment,
        int historyIndex)
    {
        if (state.SavedHoleRingIndex != historyIndex)
            return;

        bool wasInside = segment.InsideHole;
        segment.InsideHole = !wasInside;
        segment.Projectile.CanDamageSamus = wasInside;
        segment.Projectile.CollisionOption = wasInside ? EnemyProjectileCollisionOption.IndestructibleDud : EnemyProjectileCollisionOption.NoCollision;

        if (!segment.IsTail)
            return;
        state.HoleLatch = false;
        state.ExitTransitionComplete = !state.InsideHole;
        state.SavedHoleRingIndex = null;
    }

    /// <summary>
    /// Faces each body sprite toward the segment ahead of it, or the head: a segment inside the wall is hidden, the
    /// tail segment (index zero) shows the tail cap and every other segment a body link.
    /// </summary>
    private static void OrientBotwoonBody(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        int lastSegment = state.Body.Count - 1;
        for (int segmentIndex = lastSegment; segmentIndex >= 0; segmentIndex--)
        {
            BotwoonSegmentState segment = state.Body[segmentIndex];
            Position16<RoomPixels> leader = segmentIndex == lastSegment
                ? head.Position.Whole
                : state.Body[segmentIndex + 1].Projectile.Position.Whole;
            Offset16<RoomPixels> toLeader = leader - segment.Projectile.Position.Whole;
            BotwoonBodyAnimationSegmentKind kind = segment.InsideHole ? BotwoonBodyAnimationSegmentKind.Hidden
                : segment.IsTail ? BotwoonBodyAnimationSegmentKind.Tail
                : BotwoonBodyAnimationSegmentKind.Body;
            segment.Pose = BotwoonBodyAnimationSegment.Toward(kind, EnemyMath.CalculateBearing(toLeader));
        }
    }

    /// <summary>Latches the first hole rectangle entered by the head and marks its history offset.</summary>
    internal static void DetectBotwoonHole(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        // The game tests the holes from the last to the first and latches the first it finds.
        foreach (BotwoonHoleDefinition hole in Enumerable.Reverse(BotwoonNavigationDefinitions.Holes))
        {
            Offset16<RoomPixels> fromTopLeft = head.Position.Whole - hole.TopLeft;
            Offset16<RoomPixels> fromBottomRight = head.Position.Whole - hole.BottomRight;
            if (fromTopLeft.X >= 0 && fromBottomRight.X < 0 &&
                fromTopLeft.Y >= 0 && fromBottomRight.Y < 0)
            {
                state.HoleLatch = true;
                state.InsideHole = !state.InsideHole;
                state.SavedHoleRingIndex = state.RingIndex;
                return;
            }
        }
        state.HoleLatch = false;
    }

    /// <summary>Runs Botwoon's head action: animating, aiming, waiting for the spit frame, an immediate volley or the cooldown.</summary>
    private void RunBotwoonHeadAction(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        switch (state.HeadAction)
        {
            case BotwoonHeadAction.AnimateFromMovement:
                AnimateBotwoonHeadFromMovement(head, state);
                return;
            case BotwoonHeadAction.AimAtSamus:
                AimBotwoonAtSamus(head, state, samus);
                return;
            case BotwoonHeadAction.WaitForSpitFrame:
                if (state.SpitFrameReached)
                {
                    SpawnBotwoonSpitVolley(head, state, BotwoonFightDefinitions.AimedVolley);
                    state.SpitFrameReached = false;
                    state.HeadAction = BotwoonHeadAction.AttackCooldown;
                }
                return;
            case BotwoonHeadAction.SpawnImmediateVolley:
                SpawnBotwoonSpitVolley(head, state, BotwoonFightDefinitions.ImmediateVolley);
                state.HeadAction = BotwoonHeadAction.AttackCooldown;
                return;
            case BotwoonHeadAction.AttackCooldown:
                if (--state.AttackTimer < 0)
                {
                    state.AttackTimer = 0;
                    state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Undefined Botwoon head behaviour {state.HeadAction}.");
        }
    }

    /// <summary>Uses recent head displacement to select visible facing or hidden hole animation.</summary>
    internal static void AnimateBotwoonHeadFromMovement(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> moved = head.Position.Whole - state.HeadHistory[^1];
        if (moved.X != 0 || moved.Y != 0)
        {
            BotwoonHeadAnimationSegment segment;
            if (state.InsideHole)
            {
                // The head becomes intangible while it travels inside the wall.
                head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
                segment = BotwoonHeadAnimationSegment.Hidden;
            }
            else
            {
                // Collision is restored once the head is outside.
                head.Contact = head.Contact.Without(EnemyContact.IgnoresSamus);
                segment = BotwoonHeadAnimationSegment.MovingToward(EnemyMath.CalculateBearing(moved));
            }
            StartBotwoonHeadAnimation(head, state, segment);
        }

        for (int index = state.HeadHistory.Length - 1; index > 0; index--)
        {
            state.HeadHistory[index] = state.HeadHistory[index - 1];
        }
        state.HeadHistory[0] = head.Position.Whole;
    }

    /// <summary>Aims the spit animation at Samus and selects delayed or immediate volley timing.</summary>
    internal void AimBotwoonAtSamus(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        Bearing bearing = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, head.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, head.Position.Y.Whole)));
        StartBotwoonHeadAnimation(head, state, BotwoonHeadAnimationSegment.SpittingToward(bearing));
        state.SpitAngle = bearing.ToTrigonometryAngle().ToAngle();
        state.HeadAction = state.Phase == BotwoonPhase.SpitWhileHidden
            ? BotwoonHeadAction.WaitForSpitFrame
            : BotwoonHeadAction.SpawnImmediateVolley;
        RunBotwoonHeadAction(head, state, samus);
    }

    /// <summary>Starts the head animation beginning at <paramref name="segment"/> unless it plays, restarting its frame timer.</summary>
    private static void StartBotwoonHeadAnimation(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        BotwoonHeadAnimationSegment segment) =>
        state.HeadPlayback = EnemyAnimationPlayback.StartUnlessPlaying(head, state.HeadPlayback, BotwoonAnimations.HeadStartingWith(segment));

    /// <summary>Advances Botwoon's typed animation.</summary>
    internal void AdvanceBotwoonTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        AdvanceBotwoonHeadAnimation(place, place.RequireSpeciesState<BotwoonEnemyState>());
    }

    /// <summary>
    /// Runs Botwoon's head animation where the shared enemy animation runs: the frame
    /// expires on the update its timer would, and a newly shown frame raises the
    /// new-frame flag exactly as a timed frame does.
    /// </summary>
    internal void AdvanceBotwoonHeadAnimation(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (!head.AnimationClock.Expire())
            return;

        AnimationPlace place = state.HeadPlayback.Place;
        AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame> advance = BotwoonAnimations.HeadSegments.PlayToNextFrame(
            state.HeadPlayback.Animation, ref place, cue => ApplyBotwoonHeadCue(head, state, cue));
        state.HeadPlayback = state.HeadPlayback with { Place = place };
        // A hold keeps the last frame shown: the head waits on it until a new animation is installed.
        if (advance is AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame>.Held)
            return;
        if (advance is not AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame>.Frame shown)
            throw new InvalidOperationException($"Botwoon's head animation ended an update with {advance}.");
        head.Frame = BotwoonVisualDefinitions.HeadFrames.Of(shown.Shown);
        head.AnimationClock.Show(shown.Duration);
    }

    /// <summary>Applies frame cues for facing radius, spit audio, or the volley trigger.</summary>
    private void ApplyBotwoonHeadCue(RoomEnemyPlace head, BotwoonEnemyState state, BotwoonHeadCue cue)
    {
        switch (cue)
        {
            case BotwoonHeadCue.ApplyFacingRadius radius:
                head.Radius = BotwoonAnimations.HeadRadius(radius.Facing);
                return;
            case BotwoonHeadCue.Signal { Kind: BotwoonHeadCueKind.QueueSpitSound }:
                RequestBotwoonSound(BotwoonSoundDefinitions.Spit);
                return;
            case BotwoonHeadCue.Signal { Kind: BotwoonHeadCueKind.ReachSpitFrame }:
                state.SpitFrameReached = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonHeadCue)} {cue}.");
        }
    }

    /// <summary>Emits a volley's spit projectiles, fanned out across the aimed direction.</summary>
    private void SpawnBotwoonSpitVolley(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        BotwoonSpitVolley volley)
    {
        int speed = BotwoonSpeeds.ForHealthPhase(state.HealthPhase).SpitSpeed;
        TrigonometryAngle angle = state.SpitAngle.TurnedBySteps(volley.FirstOffsetSteps).ToTrigonometryAngle();
        for (int projectile = 0; projectile < volley.Count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = angle.TurnedBySteps(BotwoonFightDefinitions.VolleySpreadSteps);
        }
    }

    /// <summary>
    /// Shows the next health band's colours when Botwoon's health crosses its threshold (#1512): his head and body take
    /// the import's shade of the band.
    /// </summary>
    internal static void UpdateBotwoonHealthBand(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.HealthBandsPassed == BotwoonHealthBandDefinitions.BandCount)
            return;
        if (!BotwoonHealthBands.ShouldAdvance(state.HealthBandsPassed, head.Health))
            return;
        state.HealthBandsPassed++;
        head.Shading = state.BodyShading;
    }

    /// <summary>Accelerates the defeated head down to the floor, then hides it and waits for body landing.</summary>
    private void RunBotwoonHeadFall(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        SignedSixteenSixteen displacement = state.DeathFallAccumulator.Speed.Positive;
        head.Position = head.Position.AdvanceY(displacement);
        if (CoordinateAxis.Difference(head.Position.Y.Whole, BotwoonFightDefinitions.FloorY) < 0)
        {
            state.DeathFallAccumulator += BotwoonDeathDefinitions.FallAcceleration;
            return;
        }

        head.Position = head.Position.WithWholeY(BotwoonFightDefinitions.FloorY);
        state.Phase = BotwoonPhase.WaitForBody;
        _projectiles.SpawnDustOrExplosion(head.Position.Whole, MiscDustAnimation.BigExplosion);
        RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Requests the crumble room object and sixteen RNG-positioned drops before wall explosions.</summary>
    private void BeginBotwoonWallExplosions(RoomEnemyPlace head, BotwoonEnemyState state, SamusState samus)
    {
        state.Phase = BotwoonPhase.WallExplosions;
        _frameResults.BotwoonWall = BotwoonWallMutation.Crumble;

        // Botwoon's death drop emits sixteen independent pickup
        // requests. Each position uses both halves of one RNG result: X is the sample modulo 128
        // and Y the sample divided by 256, modulo 64. Each pickup is materialized immediately as
        // a real pickup actor from the head definition's item chances.
        for (int dropIndex = 0; dropIndex < BotwoonDeathDefinitions.WallDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            var position = new Position16<RoomPixels>(
                BotwoonDeathDefinitions.DropX.Of(random) + BotwoonDeathDefinitions.DropLeft,
                BotwoonDeathDefinitions.DropY.Of(random) + BotwoonDeathDefinitions.DropTop);
            _deathsAndDrops.SpawnEnemyDropFromChanceTable(
                position,
                head.Definition.DropChances,
                samus);
        }
        state.WallExplosionFrame = 0;
        state.LargeExplosionTimer = 0;
        state.SmallExplosionTimer = 0;
    }

    /// <summary>Runs the 192-frame wall sequence, then records the boss's defeat and queues delayed music.</summary>
    private void RunBotwoonWallExplosions(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.WallExplosionFrame >= BotwoonDeathDefinitions.WallExplosionDuration)
        {
            head.Presence = head.Presence.With(EnemyPresence.Deleted);
            _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
            // The death sequence invokes the shortest-delay request for the elevator song only after the complete 192-frame
            // wall-explosion phase, not when health first reaches zero.
            _audio.QueueEnemyMusic(new DelayedMusicRequest(
                MusicRequest.SelectTrack(MusicTrack.Elevator),
                MusicQueueDelay.Shortest));
            return;
        }

        if (state.WallExplosionFrame >= BotwoonDeathDefinitions.WallExplosionsBegin)
        {
            if (--state.LargeExplosionTimer < 0)
            {
                state.LargeExplosionTimer = BotwoonDeathDefinitions.LargeExplosionInterval;
                _effectActors.SpawnRoomEffectActor(
                    new(_room.Random.Next(BotwoonDeathDefinitions.LargeExplosionX) + BotwoonDeathDefinitions.LargeExplosionLeft,
                        state.WallExplosionFrame + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter) - BotwoonDeathDefinitions.ExplosionRise),
                    RoomEffectActorKind.BigExplosion,
                    graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
                RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
            }

            if (--state.SmallExplosionTimer < 0)
            {
                state.SmallExplosionTimer = BotwoonDeathDefinitions.SmallExplosionInterval;
                for (int explosion = 0; explosion < BotwoonDeathDefinitions.SmallExplosionsPerBurst; explosion++)
                {
                    _effectActors.SpawnRoomEffectActor(
                        new(_room.Random.Next(BotwoonDeathDefinitions.SmallExplosionX) + BotwoonDeathDefinitions.SmallExplosionLeft,
                            state.WallExplosionFrame + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter) - BotwoonDeathDefinitions.ExplosionRise),
                        RoomEffectActorKind.SmokePuff,
                        graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
                }
            }
        }
        state.WallExplosionFrame++;
    }

    /// <summary>
    /// Runs the private tail shared by Botwoon's contact, shot and
    /// power-bomb reactions. All three reactions deliberately use a common damage helper
    /// that skips the ordinary death animation: zero health is only a request to finish the
    /// current traversal. The head and thirteen body actors remain alive until the tail has
    /// crossed the next hole, exactly as the death tail requires.
    /// </summary>
    internal static void ResolveBotwoonCombatAfterCommon(RoomEnemyPlace head)
    {
        if (head.Health != 0)
            return;

        BotwoonEnemyState state = head.RequireSpeciesState<BotwoonEnemyState>();
        state.PendingDeath = true;

        // The dying head becomes intangible: it leaves the
        // projectile, bomb and touch passes, so a later hit cannot interrupt its fall.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Moves the head by <paramref name="magnitude"/> along <paramref name="angle"/>.</summary>
    private static void AddBotwoonAngleVector(
        RoomEnemyPlace head,
        TrigonometryAngle angle,
        int magnitude)
    {
        SignedSixteenSixteen xMagnitude = EnemyMath.AbsoluteCosineSpeed(angle, magnitude).AsDisplacement();
        SignedSixteenSixteen yMagnitude = EnemyMath.AbsoluteSineSpeed(angle, magnitude).AsDisplacement();
        head.Position = head.Position.AdvanceX(EnemyMath.CosineIsNegative(angle) ? -xMagnitude : xMagnitude);
        head.Position = head.Position.AdvanceY(EnemyMath.SineIsNegative(angle) ? -yMagnitude : yMagnitude);
    }

    /// <summary>Routes bomb hits through the shared shot reaction without generic death animation.</summary>
    internal EnemyBombReaction BotwoonBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        // Botwoon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveBotwoonCombatAfterCommon(c.Enemy),
    };

    /// <summary>Botwoon's body segments and spit (#1760): their spawners start their animations, and Botwoon runs them.</summary>
    internal EnemyProjectileBehavior BodyAndSpit => this;

    /// <summary>A body segment and a spit move from their spawn.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, BotwoonProjectileMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.BotwoonBody] = BotwoonProjectileMotion.BodySegment,
        [RoomEnemyProjectileKind.BotwoonSpit] = BotwoonProjectileMotion.SpitFlying,
    };

    /// <summary>
    /// Moves a body segment, oriented and coloured with his hurt and run through its death, or a spit flying until it
    /// leaves the camera.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    private protected override void Move(EnemyProjectileMotionContext context, BotwoonProjectileMotion motion)
    {
        switch (motion)
        {
            case BotwoonProjectileMotion.BodySegment:
                MoveBotwoonBodySegment(context.Projectile, context.ShortCounter);
                break;
            case BotwoonProjectileMotion.SpitFlying:
                FlyBotwoonSpit(context.Projectile, context.Camera);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonProjectileMotion)} {motion}.");
        }
    }

    /// <summary>Runs Botwoon's main AI.</summary>
    internal void RunBotwoonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBotwoonMain(place, place.RequireSpeciesState<BotwoonEnemyState>(), frame.Samus);
    }

    /// <summary>Applies power-bomb damage while preserving Botwoon's delayed death traversal.</summary>
    internal EnemyPowerBombReaction BotwoonPowerBombReaction => field ??= new()
    {
        ExemptFromGenericDeath = true,
        AfterCommon = c => ResolveBotwoonCombatAfterCommon(c.Enemy),
    };

    /// <summary>Resolves the health bands and skips generic death on shot hits.</summary>
    internal EnemyShotReaction BotwoonShotReaction => field ??= new()
    {
        ExemptFromGenericDeath = true,
        // Botwoon's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        AfterCommon = c => ResolveBotwoonCombatAfterCommon(c.Enemy),
    };

    /// <summary>Resolves Samus contact while preserving Botwoon's special zero-health tail.</summary>
    internal void ReactToBotwoonTouch(EnemyTouchContext context)
    {
        _collision.ResolveNormalEnemyTouch(context, TouchDeathAnimation.Skip);
        ResolveBotwoonCombatAfterCommon(context.Place);
    }
    /// <summary>
    /// Spawns Botwoon's body, from the segment behind the head down to the tail. The projectile allocator hands out places
    /// in its own order, so a segment's death delay follows its projectile place while its body spacing follows its
    /// segment index; both orders are kept.
    /// </summary>
    /// <param name="head">Botwoon's head, whose position and graphics the segments start with.</param>
    /// <returns>The segments from the tail to the segment behind the head.</returns>
    internal IReadOnlyList<BotwoonSegmentState> SpawnBotwoonBody(RoomEnemyPlace head)
    {
        var body = new BotwoonSegmentState[BotwoonFightDefinitions.BodySegments];
        for (int segmentIndex = body.Length - 1; segmentIndex >= 0; segmentIndex--)
            body[segmentIndex] = SpawnBotwoonBodySegment(head, segmentIndex);
        return body;
    }

    /// <summary>Spawns one of Botwoon's body segments.</summary>
    /// <param name="head">Botwoon's head, whose position and graphics the segment starts with.</param>
    /// <param name="segmentIndex">The segment's place in the body, zero (the tail) through twelve (behind the head).</param>
    /// <returns>The segment, alive and inside the wall.</returns>
    internal BotwoonSegmentState SpawnBotwoonBodySegment(RoomEnemyPlace head, int segmentIndex)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
        {
            throw new InvalidOperationException(
                "Botwoon requires thirteen free enemy-projectile places during room load.");
        }

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.BotwoonBody,
            BodyAndSpit,
            SpriteGraphics.SpawnedBy(head));
        projectile.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);

        // Every segment, the tail cap included, starts hidden inside the wall.
        BotwoonBodyAnimationSegment pose = BotwoonBodyAnimationSegment.Hidden;
        projectile.StartAnimation(BotwoonProjectileAnimation.ForSegment(pose));
        var segment = new BotwoonSegmentState(head, segmentIndex, projectile, pose);
        projectile.State = segment;
        projectile.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        projectile.CanDamageSamus = false;
        return segment;
    }

    /// <summary>Moves a body segment for one update through its four movement states.</summary>
    internal void MoveBotwoonBodySegment(
        EnemyProjectile projectile,
        int randomEnemyCounter)
    {
        BotwoonSegmentState segment = projectile.StateOf<BotwoonSegmentState>();
        if (segment.Phase == BotwoonBodyPhase.Main && Encounter(segment).BodyDeathStarted)
            segment.Phase = BotwoonBodyPhase.BeginDying;

        switch (segment.Phase)
        {
            case BotwoonBodyPhase.Main:
                AnimateBotwoonBodySegment(projectile, randomEnemyCounter);
                return;

            case BotwoonBodyPhase.BeginDying:
                // The delay follows the segment's projectile place, not its place in the body.
                segment.DeathDelay = BotwoonSegmentFallDelay.Start(projectile.Place);
                segment.Phase = BotwoonBodyPhase.DyingDelay;
                goto case BotwoonBodyPhase.DyingDelay;

            case BotwoonBodyPhase.DyingDelay:
                // The delay starts below the fall count and ends on reaching it.
                segment.DeathDelay++;
                if (segment.DeathDelay >= BotwoonSegmentState.FallAt)
                    segment.Phase = BotwoonBodyPhase.Falling;
                projectile.FrameHold = WrappingCountdown.Start(0);
                ShowBotwoonBodyHurtFlash(projectile, randomEnemyCounter);
                return;

            case BotwoonBodyPhase.Falling:
                RunBotwoonBodyFall(segment, randomEnemyCounter);
                return;

            case BotwoonBodyPhase.Landed:
                return;

            default:
                throw new InvalidOperationException($"Botwoon's body has no behaviour for phase {segment.Phase}.");
        }
    }

    /// <summary>The state of the Botwoon whose body <paramref name="segment"/> is part of: his head's.</summary>
    private static BotwoonEnemyState Encounter(BotwoonSegmentState segment) =>
        segment.Head.RequireSpeciesState<BotwoonEnemyState>();

    /// <summary>Installs a segment animation for its angle and applies the head's hurt flash.</summary>
    internal static void AnimateBotwoonBodySegment(
        EnemyProjectile segment,
        int randomEnemyCounter)
    {
        BotwoonProjectileAnimation animation = segment.Animation as BotwoonProjectileAnimation
            ?? throw new InvalidOperationException($"Botwoon body segment {segment.Place} has no segment animation.");
        if (animation.Install(segment.StateOf<BotwoonSegmentState>().Pose))
            segment.FrameHold = WrappingCountdown.Start(1);
        ShowBotwoonBodyHurtFlash(segment, randomEnemyCounter);
    }

    /// <summary>Shows his body in his health band's colours, or in the hurt flash on the updates his flashing head shows it.</summary>
    private static void ShowBotwoonBodyHurtFlash(
        EnemyProjectile projectile,
        int randomEnemyCounter)
    {
        BotwoonSegmentState segment = projectile.StateOf<BotwoonSegmentState>();
        projectile.Graphics = projectile.Graphics.In(segment.Head.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(randomEnemyCounter)
            ? EnemyShades.HurtFlashLook
            : Encounter(segment).BodyShading);
    }

    /// <summary>Accelerates a dying segment to Y=200 and marks the tail cap's final landing.</summary>
    private void RunBotwoonBodyFall(
        BotwoonSegmentState segment,
        int randomEnemyCounter)
    {
        EnemyProjectile projectile = segment.Projectile;
        projectile.Position = projectile.Position.AdvanceY(segment.FallPlace.Speed.Positive);
        if (CoordinateAxis.Difference(projectile.Position.Y.Whole, BotwoonFightDefinitions.FloorY) < 0)
        {
            // A place past the table's last row fails when its row is read, long before the place could grow large.
            segment.FallPlace += BotwoonDeathDefinitions.FallAcceleration;
            projectile.FrameHold = WrappingCountdown.Start(0);
            ShowBotwoonBodyHurtFlash(projectile, randomEnemyCounter);
            return;
        }

        projectile.Position = projectile.Position.WithWholeY(BotwoonFightDefinitions.FloorY);
        segment.Phase = BotwoonBodyPhase.Landed;
        // The landed segment plays the shared big-explosion animation.
        projectile.StartAnimation(new MiscDustProjectileAnimation(MiscDustAnimation.BigExplosion));
        projectile.Graphics = SpriteGraphics.SharedIn(EnemyShades.InProjectileLook);
        projectile.CanDamageSamus = false;
        projectile.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);

        // The tail segment owns the synchronization flag.
        if (segment.IsTail)
            Encounter(segment).LastBodySegmentLanded = true;
    }

    /// <summary>Spawns one of Botwoon's spits.</summary>
    /// <param name="head">Botwoon's head.</param>
    /// <param name="angle">The spit's direction.</param>
    /// <param name="speed">The spit's speed.</param>
    internal void SpawnBotwoonSpit(RoomEnemyPlace head, TrigonometryAngle angle, int speed)
    {
        EnemyProjectile? spit = _projectiles.AllocateEnemyProjectile();
        if (spit is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            spit,
            RoomEnemyProjectileKind.BotwoonSpit,
            BodyAndSpit,
            SpriteGraphics.SpawnedBy(head));
        spit.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);
        spit.StartAnimation(BotwoonProjectileAnimation.Spit());

        // The spit keeps unsigned quarter-circle speeds, signed by its heading every update, with their fractions so
        // shallow trajectories do not lose their subpixel motion.
        spit.State = AngledFlightState.Launch(angle, speed);
    }

    /// <summary>Runs a spit's per-frame flight.</summary>
    internal static void FlyBotwoonSpit(
        EnemyProjectile spit,
        Position16<RoomPixels> camera)
    {
        spit.StateOf<AngledFlightState>().Advance(spit);

        EnemyView.DeleteOutside(spit, camera);
    }

    /// <summary>Queues one of Botwoon's sounds as Botwoon makes it, after the update's earlier requests.</summary>
    private void RequestBotwoonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);
}
