using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
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
    private readonly EnemyPassEvents _passEvents;

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Botwoon], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new BotwoonEntity(this, place, common));

    /// <summary>Active room services and RNG used by path and drop decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Botwoon's body actors (<see cref="BotwoonFightDefinitions.BodySegments"/>) and spit projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns the wall-phase explosion sprites.</summary>
    private readonly RoomEffectActors _effectActors;
    /// <summary>Runs common touch behavior before Botwoon's own death begins.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns the special drops (<see cref="BotwoonDeathDefinitions.WallDropCount"/>) from the chance table.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Botwoon's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds Botwoon's head, body, colours, collision, and drop services.</summary>
    internal BotwoonBehavior(BotwoonBehaviorDependencies dependencies)
        : base(StartingMotions)
    {
        _passEvents = dependencies.PassEvents;
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
        // The setup installs the hidden animation: the head starts hidden in its hole.
        state.HeadPlayback = EnemyAnimationPlayback.Start(head, BotwoonAnimations.HeadStartingWith(BotwoonHeadAnimationSegment.Hidden));

        if (defeated)
        {
            _passEvents.Publish(new BotwoonWallChange(BotwoonWallMutation.Clear));
            head.Presence = head.Presence.With(EnemyPresence.Deleted);
            return;
        }

        state.OpeningWait.LastFor(BotwoonFightDefinitions.OpeningDelay);
        state.SpeedStage = BotwoonSpeedStageDefinitions.Full;
        state.InsideHole = true;
        state.PreviousInsideHole = true;
        state.InitialAction = true;
        state.TargetHole = BotwoonHole.Left;
        state.StartingHealth = head.Health;
        // The setup makes the head intangible: it starts hidden in its hole, out of every collision pass.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);

        for (int history = 0; history < state.HeadHistory.Length; history++)
        {
            state.HeadHistory[history] = head.Position.Whole;
        }
        // His trail is not filled from his head: until his head has moved far enough to fill it, a body segment
        // trailing past its written places stands at the room's origin, (0,0).
    }

    /// <summary>What <c>RunBotwoonMain</c> does for each <see cref="BotwoonPhase"/> member, each member's own arm.</summary>
    private IdentityTable<BotwoonPhase, Action<RoomEnemyPlace, BotwoonEnemyState, SamusState>> RunBotwoonMainForBotwoonPhase => field ??= new(new Dictionary<BotwoonPhase, Action<RoomEnemyPlace, BotwoonEnemyState, SamusState>>
    {
        [BotwoonPhase.InitialDelay] = (head, state, samus) =>
        {
            if (state.OpeningWait.StepRunsOut())
                state.Phase = BotwoonPhase.ChooseNextAction;
        },
        [BotwoonPhase.ChooseNextAction] = (head, state, samus) => RunBotwoonActionSelector(head, state, samus),
        [BotwoonPhase.FollowAuthoredPath] = (head, state, samus) => RunBotwoonAuthoredPathState(head, state, samus),
        [BotwoonPhase.SpitWhileHidden] = (head, state, samus) => RunBotwoonHiddenSpitState(head, state, samus),
        [BotwoonPhase.DeathDelay] = (head, state, samus) =>
        {
            if (state.PreFallWait.StepRunsOut())
                state.Phase = BotwoonPhase.HeadFalling;
        },
        [BotwoonPhase.HeadFalling] = (head, state, samus) => RunBotwoonHeadFall(head, state),
        [BotwoonPhase.WaitForBody] = (head, state, samus) =>
        {
            if (state.LastBodySegmentLanded)
                BeginBotwoonWallExplosions(head, state, samus);
        },
        [BotwoonPhase.WallExplosions] = (head, state, samus) => RunBotwoonWallExplosions(head, state),
    });

    /// <summary>Runs Botwoon's main AI and its movement-phase step for one update.</summary>
    private void RunBotwoonMain(RoomEnemyPlace head, BotwoonEnemyState state, SamusState samus)
    {
        // A fatal hit does not delete Botwoon. He keeps following his current path until
        // the tail has crossed the next hole, then the synchronized head/body death begins.
        if (state.FatallyHit && state.ExitTransitionComplete)
        {
            state.BodyDeathStarted = true;
            state.Phase = BotwoonPhase.DeathDelay;
            state.PreFallWait.LastFor(BotwoonDeathDefinitions.PreFallWaitUpdates);
            state.FatallyHit = false;
            state.ExitTransitionComplete = false;
        }

        RunBotwoonMainForBotwoonPhase[state.Phase](head, state, samus);

        UpdateBotwoonHealthBand(head, state);
    }

    /// <summary>Chooses an authored traversal or hidden spit action after the current path completes.</summary>
    private void RunBotwoonActionSelector(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.Movement != BotwoonMovement.Finished)
        {
            RunBotwoonMovingFrame(head, state, samus);
            DetectBotwoonHole(head, state);
            return;
        }

        bool followsPath = state.InsideHole
            || state.InitialAction
            || state.SpeedStage != BotwoonSpeedStageDefinitions.Full
            || BotwoonNavigationDefinitions.FollowsAuthoredPath.IsTakenBy(_room.Random.Generate());
        state.InitialAction = false;

        if (followsPath)
        {
            state.Phase = BotwoonPhase.FollowAuthoredPath;
            state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
            ChooseBotwoonPath(head, state);
        }
        else
        {
            state.Phase = BotwoonPhase.SpitWhileHidden;
            state.HeadAction = BotwoonHeadAction.AimAtSamus;
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
        if (state.Movement == BotwoonMovement.Finished)
        {
            state.Phase = BotwoonPhase.ChooseNextAction;
            state.Movement = BotwoonMovement.MoveTowardHole;
            if (state.InsideHole)
                state.HoleLatch = false;
            return;
        }

        RunBotwoonMovingFrame(head, state, samus);
    }

    /// <summary>
    /// Runs his head through its aim, volley and cooldown until the cooldown returns it to following his movement, then on
    /// the next update chooses whether the next path begins inside a hole.
    /// </summary>
    private void RunBotwoonHiddenSpitState(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        if (state.HeadAction != BotwoonHeadAction.AnimateFromMovement)
        {
            RunBotwoonHeadAction(head, state, samus);
            return;
        }

        state.Phase = BotwoonPhase.FollowAuthoredPath;

        state.InsideHole = !state.FatallyHit && BotwoonNavigationDefinitions.StartsInsideHole.IsTakenBy(_room.Random.Generate());
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

    /// <summary>What <c>RunBotwoonMovement</c> does for each <see cref="BotwoonMovement"/> member, each member's own arm.</summary>
    private static IdentityTable<BotwoonMovement, Action<RoomEnemyPlace, BotwoonEnemyState>> RunBotwoonMovementForBotwoonMovement => field ??= new(new Dictionary<BotwoonMovement, Action<RoomEnemyPlace, BotwoonEnemyState>>
    {
        [BotwoonMovement.MoveTowardHole] = static (head, state) => MoveBotwoonTowardHole(head, state),
        [BotwoonMovement.FollowAuthoredPath] = static (head, state) => FollowBotwoonPath(head, state),
        [BotwoonMovement.Finished] = static (head, state) =>
        {
            throw new InvalidOperationException("Botwoon moves after finishing his movement, before choosing what follows.");
        },
    });

    /// <summary>Moves Botwoon toward a hole or along one of his authored paths, as his movement says.</summary>
    private static void RunBotwoonMovement(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        RunBotwoonMovementForBotwoonMovement[state.Movement](head, state);
    }

    /// <summary>Steers the head toward its selected hole until inside/outside state changes.</summary>
    private static void MoveBotwoonTowardHole(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigation.Hole(state.TargetHole);
        Offset16<RoomPixels> delta = hole.Target - head.Position.Whole;
        TrigonometryAngle aim = EnemyMath.CalculateAimAngle(new(
            ClampBotwoonTargetDelta(delta.X), ClampBotwoonTargetDelta(delta.Y)));
        state.MovementAngle = aim.ToAngle();

        if (state.InsideHole == state.PreviousInsideHole)
        {
            head.Position = head.Position.Advance(
                EnemyMath.VelocityAlong(state.MovementAngle.ToTrigonometryAngle(), state.SpeedStage.MovementSpeed));
        }
        else
        {
            state.PreviousInsideHole = state.InsideHole;
            state.Movement = BotwoonMovement.Finished;
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
    /// Refreshes the speed stage, then draws where the next walk from his hole leads, in the open or inside the wall,
    /// and starts it.
    /// </summary>
    private void ChooseBotwoonPath(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        UpdateBotwoonSpeedStage(head, state);
        StartBotwoonWalk(state, BotwoonNavigation.WalkFrom(state.TargetHole, state.InsideHole, _room.Random.Generate()));
    }

    /// <summary>
    /// Outside a hole, takes the speed stage his health has reached: below half his starting health he speeds up, and below
    /// a quarter of it again; at no health at all he moves at full speed.
    /// </summary>
    private static void UpdateBotwoonSpeedStage(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.InsideHole)
            return;

        state.SpeedStage = head.Health == 0 || head.Health >= state.HalfHealth ? BotwoonSpeedStageDefinitions.Full
            : head.Health >= state.QuarterHealth ? BotwoonSpeedStageDefinitions.BelowHalf
            : BotwoonSpeedStageDefinitions.BelowQuarter;
    }

    /// <summary>
    /// Starts <paramref name="walk"/>: its path, the way it is walked, its first step and the hole it reaches. The game
    /// loaded the walk on the next movement update; nothing reads the walk or the target hole before then, so loading it
    /// as it is drawn moves him the same (#1868).
    /// </summary>
    private static void StartBotwoonWalk(BotwoonEnemyState state, BotwoonPathWalk walk)
    {
        state.PathPosition = BotwoonPathPosition.StartOf(walk);
        state.PathTraversal = walk.Traversal;
        state.TargetHole = walk.Destination;
        state.Movement = BotwoonMovement.FollowAuthoredPath;
    }

    /// <summary>
    /// Takes as many path steps as the current speed allows. Walking off either end of the path completes it; the
    /// steps already taken on that update are then discarded, so the head does not move on its final update.
    /// </summary>
    private static void FollowBotwoonPath(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        BotwoonPathPosition position = state.PathPosition ?? throw new InvalidOperationException("Botwoon follows a path before choosing one.");
        Offset16<RoomPixels> total = Offset16<RoomPixels>.Zero;
        for (int taken = 0; taken < state.SpeedStage.MovementSpeed; taken++)
        {
            if (!position.TryTake(state.PathTraversal, out Offset16<RoomPixels> movement, out position))
            {
                state.Movement = BotwoonMovement.Finished;
                return;
            }
            total += movement;
            state.PathPosition = position;
        }

        head.Position += AlongTraversal(state.PathTraversal, total);
    }

    /// <summary>
    /// <paramref name="amount"/> taken the way <paramref name="traversal"/> walks the path (#2569): as it is forward, reversed
    /// backward.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="traversal"/> is undefined.</exception>
    private static T AlongTraversal<T>(BotwoonPathTraversal traversal, T amount) where T : System.Numerics.IUnaryNegationOperators<T, T> =>
        traversal switch
        {
            BotwoonPathTraversal.Forward => amount,
            BotwoonPathTraversal.Backward => -amount,
            _ => throw new ArgumentOutOfRangeException(nameof(traversal), traversal, $"Undefined {nameof(BotwoonPathTraversal)}."),
        };

    /// <summary>Writes the head's whole-pixel position into the head-position history ring.</summary>
    private static void RecordBotwoonHeadPosition(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        state.History[state.RingIndex] = head.Position.Whole;
    }

    /// <summary>Spaces the body projectiles along older head-history samples.</summary>
    private static void PositionBotwoonBody(BotwoonEnemyState state)
    {
        int spacing = state.SpeedStage.SegmentSpacingEntries;
        int historyIndex = BotwoonTrailRing.PlaceAt(state.RingIndex - spacing);
        for (int segmentIndex = state.Body.Count - 1; segmentIndex >= 0; segmentIndex--)
        {
            BotwoonSegmentState segment = state.Body[segmentIndex];

            ToggleBotwoonSegmentAtHole(state, segment, historyIndex);
            segment.Projectile.Position = segment.Projectile.Position.WithWhole(state.History[historyIndex]);
            historyIndex = BotwoonTrailRing.PlaceAt(historyIndex - spacing);
        }
    }

    /// <summary>Changes a segment's collision mode when the point of its trail it follows crosses a hole.</summary>
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
            segment.Pose = BotwoonBodyAnimationSegment.Toward(kind, EnemyMath.CalculateAimAngle(toLeader));
        }
    }

    /// <summary>Latches the first hole rectangle entered by the head and marks its history offset.</summary>
    private static void DetectBotwoonHole(RoomEnemyPlace head, BotwoonEnemyState state)
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

    /// <summary>What <c>RunBotwoonHeadAction</c> does for each <see cref="BotwoonHeadAction"/> member, each member's own arm.</summary>
    private IdentityTable<BotwoonHeadAction, Action<RoomEnemyPlace, BotwoonEnemyState, SamusState>> RunBotwoonHeadActionForBotwoonHeadAction => field ??= new(new Dictionary<BotwoonHeadAction, Action<RoomEnemyPlace, BotwoonEnemyState, SamusState>>
    {
        [BotwoonHeadAction.AnimateFromMovement] = (head, state, samus) => AnimateBotwoonHeadFromMovement(head, state),
        [BotwoonHeadAction.AimAtSamus] = (head, state, samus) => AimBotwoonAtSamus(head, state, samus),
        [BotwoonHeadAction.WaitForSpitFrame] = (head, state, samus) => { },
        [BotwoonHeadAction.SpitAimedVolley] = (head, state, samus) => SpawnBotwoonSpitVolley(head, state, BotwoonFightDefinitions.AimedVolley),
        [BotwoonHeadAction.SpawnImmediateVolley] = (head, state, samus) => SpawnBotwoonSpitVolley(head, state, BotwoonFightDefinitions.ImmediateVolley),
        [BotwoonHeadAction.AttackCooldown] = (head, state, samus) =>
        {
            if (state.SpitCooldown.StepRunsOut())
                state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
        },
    });

    /// <summary>Runs Botwoon's head action: animating, aiming, waiting for the spit frame, an immediate volley or the cooldown.</summary>
    private void RunBotwoonHeadAction(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        RunBotwoonHeadActionForBotwoonHeadAction[state.HeadAction](head, state, samus);
    }

    /// <summary>Uses recent head displacement to select visible facing or hidden hole animation.</summary>
    private static void AnimateBotwoonHeadFromMovement(RoomEnemyPlace head, BotwoonEnemyState state)
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
                segment = BotwoonHeadAnimationSegment.MovingToward(EnemyMath.CalculateAimAngle(moved));
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
    private void AimBotwoonAtSamus(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        TrigonometryAngle aim = EnemyMath.CalculateAimAngle(
            new(samus.Position.X.Whole - head.Position.X.Whole, samus.Position.Y.Whole - head.Position.Y.Whole));
        StartBotwoonHeadAnimation(head, state, BotwoonHeadAnimationSegment.SpittingToward(aim));
        state.SpitAngle = aim.ToAngle();
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
    internal void AdvanceBotwoonTypedAnimation(RoomEnemyPlace place)
    {
        AdvanceBotwoonHeadAnimation(place, place.RequireSpeciesState<BotwoonEnemyState>());
    }

    /// <summary>
    /// Runs Botwoon's head animation where the shared enemy animation runs: the frame
    /// expires on the update its timer would, and the clock notes each newly shown frame
    /// as it does a timed frame.
    /// </summary>
    internal void AdvanceBotwoonHeadAnimation(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (!head.AnimationClock.Expire())
            return;

        AnimationPlace place = state.HeadPlayback.Place;
        AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame> advance = BotwoonAnimations.HeadSegments.PlayToNextFrame(
            state.HeadPlayback.Animation, ref place, cue => ApplyBotwoonHeadCue(head, state, cue));
        state.HeadPlayback = state.HeadPlayback with { Place = place };
        // A hold keeps the last frame shown and stops the clock (#2454): the head waits on it until a new animation is
        // installed. Every outcome takes the shared enemy animation's meaning.
        EnemyAnimationPlayback.Show(head, advance, static frame => BotwoonVisualDefinitions.HeadFrames.Of(frame));
    }

    /// <summary>What <c>ApplyBotwoonHeadCue</c> does for each <see cref="BotwoonHeadCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<BotwoonHeadCueKind, Action<BotwoonEnemyState>> ApplyBotwoonHeadCueForBotwoonHeadCueKind => field ??= new(new Dictionary<BotwoonHeadCueKind, Action<BotwoonEnemyState>>
    {
        [BotwoonHeadCueKind.QueueSpitSound] = state => RequestBotwoonSound(BotwoonSoundDefinitions.Spit),
        [BotwoonHeadCueKind.ReachSpitFrame] = state =>
        {
            // Only his aim starts the spitting animation, and it waits for the spit, so the spit always finds the
            // head waiting; the volley follows on his next head step.
            state.HeadAction = BotwoonHeadAction.SpitAimedVolley;
        },
    });

    /// <summary>Applies frame cues for facing radius, spit audio, or the volley trigger.</summary>
    private void ApplyBotwoonHeadCue(RoomEnemyPlace head, BotwoonEnemyState state, BotwoonHeadCue cue)
    {
        cue.React(ApplyBotwoonHeadCueReactions, new ApplyBotwoonHeadCueContext(head, state, this));
    }

    /// <summary>What <c>ApplyBotwoonHeadCue</c> does on each <see cref="BotwoonHeadCue"/>, built once (#2553).</summary>
    private static BotwoonHeadCueReactions<ApplyBotwoonHeadCueContext> ApplyBotwoonHeadCueReactions { get; } = new(
        ApplyFacingRadius: static (radius, context) => context.Head.Radius = BotwoonAnimations.HeadRadius(radius.Facing),
        Signal: static (signal, context) =>
        {
            context.Owner.ApplyBotwoonHeadCueForBotwoonHeadCueKind[signal.Kind](context.State);
        });

    /// <summary>What <c>ApplyBotwoonHeadCue</c>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Head">The <c>head</c> <c>ApplyBotwoonHeadCue</c> was handed.</param>
    /// <param name="State">The <c>state</c> <c>ApplyBotwoonHeadCue</c> was handed.</param>
    /// <param name="Owner">The <see cref="BotwoonBehavior"/> reacting.</param>
    private readonly record struct ApplyBotwoonHeadCueContext(RoomEnemyPlace Head, BotwoonEnemyState State, BotwoonBehavior Owner);

    /// <summary>Emits a volley's spit projectiles, fanned out across the aimed direction, and starts his head's cooldown after it.</summary>
    private void SpawnBotwoonSpitVolley(
        RoomEnemyPlace head,
        BotwoonEnemyState state,
        BotwoonSpitVolley volley)
    {
        int speed = state.SpeedStage.SpitSpeed;
        TrigonometryAngle angle = state.SpitAngle.TurnedBySteps(volley.FirstOffsetSteps).ToTrigonometryAngle();
        for (int projectile = 0; projectile < volley.Count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = angle.TurnedBySteps(BotwoonFightDefinitions.VolleySpreadSteps);
        }
        state.SpitCooldown.LastFor(volley.CooldownUpdates);
        state.HeadAction = BotwoonHeadAction.AttackCooldown;
    }

    /// <summary>
    /// Shows the next health band's colours once Botwoon's health has fallen below its threshold (#1512, #2266), at most
    /// one band an update: his head and body take the import's shade of the band.
    /// </summary>
    private static void UpdateBotwoonHealthBand(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (BotwoonHealthBand.After(state.HealthBand) is not { } next || head.Health >= next.HealthBelow)
            return;
        state.HealthBand = next;
        head.Shading = state.BodyShading;
    }

    /// <summary>Accelerates the defeated head down to the floor, then hides it and waits for body landing.</summary>
    private void RunBotwoonHeadFall(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        SignedSixteenSixteen displacement = state.FallPlace.Speed.Positive;
        head.Position = head.Position.AdvanceY(displacement);
        if (head.Position.Y.Whole < BotwoonFightDefinitions.FloorY)
        {
            state.FallPlace += BotwoonDeathDefinitions.FallAcceleration;
            return;
        }

        head.Position = head.Position.WithWholeY(BotwoonFightDefinitions.FloorY);
        state.Phase = BotwoonPhase.WaitForBody;
        _projectiles.SpawnDustOrExplosion(head.Position.Whole, DustExplosionAnimation.BigExplosion);
        RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Requests the crumble room object and the RNG-positioned drops before wall explosions.</summary>
    private void BeginBotwoonWallExplosions(RoomEnemyPlace head, BotwoonEnemyState state, SamusState samus)
    {
        state.Phase = BotwoonPhase.WallExplosions;
        _passEvents.Publish(new BotwoonWallChange(BotwoonWallMutation.Crumble));

        // His death leaves his wall drops. One random number places each, across the drop strip and down the drop band
        // (DropX, DropY), and each becomes a pickup at once, chosen by his drop chances.
        for (int dropIndex = 0; dropIndex < BotwoonDeathDefinitions.WallDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            var position = new Position16<RoomPixels>(
                BotwoonDeathDefinitions.DropX.Of(random) + BotwoonDeathDefinitions.DropLeft,
                BotwoonDeathDefinitions.DropY.Of(random) + BotwoonDeathDefinitions.DropTop);
            _deathsAndDrops.SpawnEnemyDropFromDefinition(position, head.DefinitionId, samus);
        }
        state.WallSequence = UpdateCountdown.Lasting(BotwoonDeathDefinitions.WallExplosionDuration);
        state.LargeExplosionWait.LastFor(1);
        state.SmallExplosionWait.LastFor(1);
    }

    /// <summary>
    /// Runs the wall's explosions for their <see cref="BotwoonDeathDefinitions.WallExplosionDuration"/>, then removes
    /// Botwoon, records the boss's defeat and asks for the elevator music.
    /// </summary>
    private void RunBotwoonWallExplosions(RoomEnemyPlace head, BotwoonEnemyState state)
    {
        if (state.WallSequence.IsOver)
        {
            head.Presence = head.Presence.With(EnemyPresence.Deleted);
            _room.Services.Bosses.SetDefeated(DefeatedBosses.AreaMiniBoss);
            // The elevator music is asked for, at the shortest delay, once the wall's explosions have run their whole course,
            // not when his health first runs out.
            _audio.QueueEnemyMusic(new DelayedMusicRequest(
                MusicRequest.SelectTrack(MusicTrack.Elevator),
                MusicQueueDelay.Shortest));
            return;
        }

        if (state.WallSequence.Age >= BotwoonDeathDefinitions.WallExplosionsBegin)
        {
            if (state.LargeExplosionWait.StepRunsOut())
            {
                state.LargeExplosionWait.LastFor(BotwoonDeathDefinitions.LargeExplosionInterval);
                _effectActors.SpawnRoomEffectActor(
                    new(_room.Random.Next(BotwoonDeathDefinitions.LargeExplosionX) + BotwoonDeathDefinitions.LargeExplosionLeft,
                        WallExplosionBandTop(state) + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter)),
                    RoomEffectActorKind.BigExplosion,
                    graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
                RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
            }

            if (state.SmallExplosionWait.StepRunsOut())
            {
                state.SmallExplosionWait.LastFor(BotwoonDeathDefinitions.SmallExplosionInterval);
                for (int explosion = 0; explosion < BotwoonDeathDefinitions.SmallExplosionsPerBurst; explosion++)
                {
                    _effectActors.SpawnRoomEffectActor(
                        new(_room.Random.Next(BotwoonDeathDefinitions.SmallExplosionX) + BotwoonDeathDefinitions.SmallExplosionLeft,
                            WallExplosionBandTop(state) + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter)),
                        RoomEffectActorKind.SmokePuff,
                        graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
                }
            }
        }
        state.WallSequence.Count();
    }

    /// <summary>
    /// The top of the band the wall explosions burst in, before each one's jitter (#2464): it moves down the wall
    /// <see cref="BotwoonDeathDefinitions.ExplosionBandDescent"/> each update of the sequence from
    /// <see cref="BotwoonDeathDefinitions.ExplosionRise"/> above the room's top.
    /// </summary>
    private static int WallExplosionBandTop(BotwoonEnemyState state) =>
        state.WallSequence.Age * BotwoonDeathDefinitions.ExplosionBandDescent - BotwoonDeathDefinitions.ExplosionRise;

    /// <summary>
    /// Finishes Botwoon's contact, shot and
    /// power-bomb reactions. All three take their damage without the ordinary death animation: zero health is only a
    /// request to finish the
    /// current traversal. The head and the body actors remain alive until the tail has
    /// crossed the next hole, as his death requires.
    /// </summary>
    private static void ResolveBotwoonCombatAfterCommon(RoomEnemyPlace head)
    {
        if (head.Health != 0)
            return;

        BotwoonEnemyState state = head.RequireSpeciesState<BotwoonEnemyState>();
        state.FatallyHit = true;

        // The dying head becomes intangible: it leaves the
        // projectile, bomb and touch passes, so a later hit cannot interrupt its fall.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
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
    private EnemyProjectileBehavior BodyAndSpit => this;

    /// <summary>What each of Botwoon's body segments keeps: his head, its place in his body, its pose and phase.</summary>
    private EnemyProjectileStates<BotwoonSegmentState> Segments { get; } = new();

    /// <summary>The heading and speed each of his spits flies along, given as he spits it.</summary>
    private EnemyProjectileStates<AngledFlightState> Spits { get; } = new();

    /// <summary>A body segment and a spit move from their spawn.</summary>
    private static readonly Dictionary<RoomEnemyProjectileKind, BotwoonProjectileMotion?> StartingMotions = new()
    {
        [RoomEnemyProjectileKind.BotwoonBody] = BotwoonProjectileMotion.BodySegment,
        [RoomEnemyProjectileKind.BotwoonSpit] = BotwoonProjectileMotion.SpitFlying,
    };

    /// <summary>What <c>Move</c> does for each <see cref="BotwoonProjectileMotion"/> member, each member's own arm.</summary>
    private IdentityTable<BotwoonProjectileMotion, Action<EnemyProjectileMotionContext>> MoveForBotwoonProjectileMotion => field ??= new(new Dictionary<BotwoonProjectileMotion, Action<EnemyProjectileMotionContext>>
    {
        [BotwoonProjectileMotion.BodySegment] = context => MoveBotwoonBodySegment(context.Projectile, context.ShortCounter),
        [BotwoonProjectileMotion.SpitFlying] = context => FlyBotwoonSpit(context.Projectile, context.Camera),
    });

    /// <summary>
    /// Moves a body segment, oriented and coloured with his hurt and run through its death, or a spit flying until it
    /// leaves the camera.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every motion.</param>
    /// <param name="motion">The motion the projectile holds.</param>
    internal override void Move(EnemyProjectileMotionContext context, BotwoonProjectileMotion motion)
    {
        MoveForBotwoonProjectileMotion[motion](context);
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

    /// <summary>Resolves Samus contact, leaving Botwoon at zero health to die his own way.</summary>
    internal void ReactToBotwoonTouch(EnemyTouchContext context)
    {
        _collision.ResolveOrdinaryTouch(context, TouchDeathAnimation.Skip);
        ResolveBotwoonCombatAfterCommon(context.Place);
    }
    /// <summary>
    /// Spawns Botwoon's body, from the segment behind the head down to the tail. Each segment takes the room's next free
    /// enemy-projectile place, so a segment's death delay follows its place while its body spacing follows its segment
    /// index; both orders are kept.
    /// </summary>
    /// <param name="head">Botwoon's head, whose position and graphics the segments start with.</param>
    /// <returns>The segments from the tail to the segment behind the head.</returns>
    private BotwoonSegmentState[] SpawnBotwoonBody(RoomEnemyPlace head)
    {
        var body = new BotwoonSegmentState[BotwoonFightDefinitions.BodySegments];
        for (int segmentIndex = body.Length - 1; segmentIndex >= 0; segmentIndex--)
            body[segmentIndex] = SpawnBotwoonBodySegment(head, segmentIndex);
        return body;
    }

    /// <summary>Spawns one of Botwoon's body segments.</summary>
    /// <param name="head">Botwoon's head, whose position and graphics the segment starts with.</param>
    /// <param name="segmentIndex">The segment's place in the body, from zero (the tail) to the last, behind the head.</param>
    /// <returns>The segment, alive and inside the wall.</returns>
    private BotwoonSegmentState SpawnBotwoonBodySegment(RoomEnemyPlace head, int segmentIndex)
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
        var animation = new BotwoonSegmentAnimation(BotwoonBodyAnimationSegment.Hidden);
        projectile.StartAnimation(animation);
        var segment = new BotwoonSegmentState(head, segmentIndex, projectile, animation);
        Segments.Start(projectile, segment);
        projectile.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        projectile.CanDamageSamus = false;
        return segment;
    }

    /// <summary>Moves a body segment for one update through its four movement states.</summary>
    private void MoveBotwoonBodySegment(
        EnemyProjectile projectile,
        int hurtFlashCadence)
    {
        BotwoonSegmentState segment = Segments.Of(projectile);
        if (segment.Phase == BotwoonBodyPhase.Main && Encounter(segment).BodyDeathStarted)
            segment.Phase = BotwoonBodyPhase.BeginDying;

        BotwoonBodyPhase phase = segment.Phase;
        if (phase == BotwoonBodyPhase.Main)
        {
            AnimateBotwoonBodySegment(projectile, hurtFlashCadence);
            return;
        }
        if (phase == BotwoonBodyPhase.Falling)
        {
            RunBotwoonBodyFall(segment, hurtFlashCadence);
            return;
        }
        // A landed segment rests where it fell.
        if (phase == BotwoonBodyPhase.Landed)
            return;
        if (phase == BotwoonBodyPhase.BeginDying)
        {
            // The wait follows the segment's projectile place, not its place in the body, and its first update is this
            // one. The dying segment holds the picture it shows until it lands.
            segment.FallWait.LastFor(BotwoonSegmentFallDelay.WaitUpdates(projectile.Place));
            segment.Phase = BotwoonBodyPhase.DyingDelay;
            projectile.StopFrameHold();
        }
        // The dying wait, from the update that began it.
        if (!segment.FallWait.StepKeepsRunning())
            segment.Phase = BotwoonBodyPhase.Falling;
        ShowBotwoonBodyHurtFlash(projectile, hurtFlashCadence);
    }

    /// <summary>The state of the Botwoon whose body <paramref name="segment"/> is part of: his head's.</summary>
    private static BotwoonEnemyState Encounter(BotwoonSegmentState segment) =>
        segment.Head.RequireSpeciesState<BotwoonEnemyState>();

    /// <summary>Installs a segment animation for its angle and applies the head's hurt flash.</summary>
    private void AnimateBotwoonBodySegment(
        EnemyProjectile segment,
        int hurtFlashCadence)
    {
        BotwoonSegmentState state = Segments.Of(segment);
        if (state.Animation.Install(state.Pose))
            segment.StartAnimation(state.Animation);
        ShowBotwoonBodyHurtFlash(segment, hurtFlashCadence);
    }

    /// <summary>Shows his body in his health band's colours, or in the hurt flash on the updates his flashing head shows it.</summary>
    private void ShowBotwoonBodyHurtFlash(
        EnemyProjectile projectile,
        int hurtFlashCadence)
    {
        BotwoonSegmentState segment = Segments.Of(projectile);
        projectile.Graphics = projectile.Graphics.In(segment.Head.HurtFlash.IsRunning && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(hurtFlashCadence)
            ? EnemyShades.HurtFlashLook
            : Encounter(segment).BodyShading);
    }

    /// <summary>Accelerates a dying segment to the floor (<see cref="BotwoonFightDefinitions.FloorY"/>) and marks the tail cap's final landing.</summary>
    private void RunBotwoonBodyFall(
        BotwoonSegmentState segment,
        int hurtFlashCadence)
    {
        EnemyProjectile projectile = segment.Projectile;
        projectile.Position = projectile.Position.AdvanceY(segment.FallPlace.Speed.Positive);
        if (projectile.Position.Y.Whole < BotwoonFightDefinitions.FloorY)
        {
            // The segment moves on along the speed curve; it lands well before the curve ends.
            segment.FallPlace += BotwoonDeathDefinitions.FallAcceleration;
            ShowBotwoonBodyHurtFlash(projectile, hurtFlashCadence);
            return;
        }

        projectile.Position = projectile.Position.WithWholeY(BotwoonFightDefinitions.FloorY);
        segment.Phase = BotwoonBodyPhase.Landed;
        // The landed segment plays the shared big-explosion animation.
        projectile.StartAnimation(new DustExplosionProjectileAnimation(DustExplosionAnimation.BigExplosion));
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
    private void SpawnBotwoonSpit(RoomEnemyPlace head, TrigonometryAngle angle, int speed)
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
        spit.StartAnimation(new BotwoonSpitAnimation());

        // The spit flies along its heading at its speed, split across and down, keeping each step's fraction of a pixel
        // so a shallow flight keeps its slow drift.
        Spits.Start(spit, AngledFlightState.Launch(angle, speed));
    }

    /// <summary>Runs a spit's per-frame flight.</summary>
    internal void FlyBotwoonSpit(
        EnemyProjectile spit,
        Position16<RoomPixels> camera)
    {
        Spits.Of(spit).Advance(spit);

        EnemyView.DeleteOutside(spit, camera);
    }

    /// <summary>Queues one of Botwoon's sounds as Botwoon makes it, after the update's earlier requests.</summary>
    private void RequestBotwoonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);
}
