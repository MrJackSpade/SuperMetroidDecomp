using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// The Botwoon species owner: its per-room state, behaviour and the interactions it registers. Botwoon runs his own body
/// segments and spit (#1801): it is their projectile behaviour, since a segment is a piece of Botwoon run from his state.
/// </summary>
[RoomLifetime]
internal sealed class BotwoonBehavior : EnemyProjectileBehavior, IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Botwoon];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new BotwoonEntity(this, slot, common);

    /// <summary>Active room services and RNG used by path and drop decisions.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates Botwoon's thirteen body actors and spit projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Spawns the wall-phase explosion sprites.</summary>
    private readonly RoomSpriteObjects _spriteObjects;
    /// <summary>Runs common touch behavior before Botwoon's special death tail.</summary>
    private readonly EnemyCollision _collision;
    /// <summary>Spawns the sixteen special drops from the chance table.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;
    /// <summary>Receives Botwoon's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;


    /// <summary>Binds Botwoon's head, body, colours, collision, and drop services.</summary>
    internal BotwoonBehavior(BotwoonBehaviorDependencies dependencies)
    {
        _frameResults = dependencies.FrameResults;
        _room = dependencies.Room;
        _projectiles = dependencies.Projectiles;
        _spriteObjects = dependencies.SpriteObjects;
        _collision = dependencies.Collision;
        _deathsAndDrops = dependencies.DeathsAndDrops;
        _audio = dependencies.Audio;
    }

    /// <summary>Number of independent chance-table pickups requested when the wall crumbles.</summary>
    private const int BotwoonSpecialDropCount = 16;

    /// <summary>Per-room movement, head-animation, body, and wall-collapse state.</summary>
    internal BotwoonEnemyState? BotwoonState { get; private set; }

    /// <summary>Botwoon's head, the enemy that owns <see cref="BotwoonState"/>; null until it initializes.</summary>
    private RoomEnemySlot? _head;

    /// <summary>Attaches <paramref name="state"/> as the state of Botwoon, whose head is <paramref name="head"/>.</summary>
    internal void AttachBotwoon(RoomEnemySlot head, BotwoonEnemyState state)
    {
        _head = Ensure.NotNull(head);
        BotwoonState = Ensure.NotNull(state);
    }

    /// <summary>The value the pre-fall death count starts from once the tail completes its hole transition.</summary>
    private const int BotwoonDeathDelayStart = 240;
    /// <summary>The pre-fall death count at which the head starts falling.</summary>
    private const int BotwoonDeathDelayEnd = 256;
    /// <summary>
    /// Fall acceleration the dying head and each falling body segment add each airborne update: three quarters of a
    /// quadratic-speed record.
    /// </summary>
    private static readonly EnemyQuadraticSpeedIndex BotwoonDeathFallAcceleration = EnemyQuadraticSpeedIndex.FromTwoHundredFiftySixthsOfRow(192);
    /// <summary>Updates the wall-explosion phase lasts before the boss is removed.</summary>
    private const int BotwoonWallExplosionDuration = 192;
    /// <summary>Wall-explosion update from which the explosion timers run.</summary>
    private const int BotwoonWallExplosionsBegin = 64;

    /// <summary>Sets Botwoon up when the room spawns it.</summary>
    internal void InitializeBotwoon(RoomEnemySlot head)
    {
        var state = new BotwoonEnemyState();
        AttachBotwoon(head, state);
        head.SetSpeciesState(state);
        // The initializer installs the hidden animation: the head starts hidden in its hole.
        state.HeadPlayback = EnemyAnimationPlayback.Start(head, BotwoonAnimations.HeadStartingWith(BotwoonHeadAnimationSegment.Hidden));

        // A previously defeated Botwoon does not allocate body actors. The room loader
        // installs the permanent open wall and makes both one-screen scroll entries blue.
        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaMiniBoss))
        {
            _frameResults.BotwoonWall = BotwoonWallMutation.Clear;
            head.Presence = head.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // Segments spawn from the one behind the head down to the tail. The projectile allocator hands out slots in
        // its own order, so a segment's death delay follows its projectile place while its body spacing follows its
        // segment index; both orders are kept.
        for (int segmentIndex = state.BodySegments.Length - 1; segmentIndex >= 0; segmentIndex--)
            SpawnBotwoonBodySegment(head, state, segmentIndex);

        state.Phase = BotwoonPhase.InitialDelay;
        state.Movement = BotwoonMovement.MoveTowardHole;
        state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
        state.InitialDelayTimer = 256;
        var initialSpeed = BotwoonSpeeds.ForHealthPhase(0);
        state.Speed = initialSpeed.MovementSpeed;
        state.SegmentSpacingEntries = initialSpeed.SegmentSpacingEntries;
        state.InsideHole = true;
        state.PreviousInsideHole = true;
        state.InitialAction = true;
        state.TargetHoleIndex = 0;
        state.HalfHealth = head.Health >> 1;
        state.QuarterHealth = head.Health >> 2;
        // The initializer makes the head intangible: it starts hidden in its hole, out of every collision pass.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);

        for (int history = 0; history < 4; history++)
        {
            state.HeadHistory[history] = head.Position.Whole;
        }
        // Botwoon's position history is never seeded by Botwoon's initializer. It
        // lies inside the enemy state the room's enemy setup clears on room
        // load, so body segments read (0,0) until the ring has been written that far; the
        // freshly constructed state's arrays are that cleared state.
    }

    /// <summary>Runs Botwoon's per-frame behaviour and its movement-phase step.</summary>
    private void RunBotwoonMain(RoomEnemySlot head, BotwoonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Botwoon main AI requires the active Samus actor.");

        // A fatal hit does not delete Botwoon. He keeps following his current path until
        // the tail has crossed the next hole, then the synchronized head/body death begins.
        if (state.PendingDeath && state.ExitTransitionComplete)
        {
            state.BodyDeathStarted = true;
            state.Phase = BotwoonPhase.DeathDelay;
            state.DeathTimer = BotwoonDeathDelayStart;
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
                if (state.DeathTimer >= BotwoonDeathDelayEnd)
                    state.Phase = BotwoonPhase.HeadFalling;
                break;

            case BotwoonPhase.HeadFalling:
                RunBotwoonHeadFall(head, state);
                break;

            case BotwoonPhase.WaitForBody:
                if (state.LastBodySegmentLanded)
                    BeginBotwoonWallExplosions(head, state);
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
        RoomEnemySlot head,
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
            state.Movement = BotwoonMovement.LoadAuthoredPath;
            state.AttackTimer = 0;
            state.HeadAction = BotwoonHeadAction.AnimateFromMovement;
            ChooseBotwoonPath(head, state);
        }
        else
        {
            state.Phase = BotwoonPhase.SpitWhileHidden;
            state.HeadAction = BotwoonHeadAction.AimAtSamus;
            state.AttackTimer = 48;
            // The spit step makes the spitting head tangible again.
            head.Contact = head.Contact.Without(EnemyContact.IgnoresSamus);
        }
    }

    /// <summary>Finishes a path at its terminal marker, then returns to action selection.</summary>
    private void RunBotwoonAuthoredPathState(
        RoomEnemySlot head,
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
        RoomEnemySlot head,
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
        state.Movement = BotwoonMovement.LoadAuthoredPath;
        state.HeadAction = BotwoonHeadAction.AnimateFromMovement;

        state.InsideHole = !state.PendingDeath && BotwoonNavigationDefinitions.StartsInsideHole.IsSetIn(_room.Random.Generate());
        state.PreviousInsideHole = state.InsideHole;
        if (!state.InsideHole)
        {
            state.PathChoice = 0;
        }
        else
        {
            state.HoleLatch = false;
            state.SavedHoleRingIndex = null;
        }
        ChooseBotwoonPath(head, state);
    }

    /// <summary>Moves the head, writes position history, places and orients body segments, then tests holes.</summary>
    private void RunBotwoonMovingFrame(
        RoomEnemySlot head,
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

    /// <summary>Dispatches movement toward a hole or through an authored path descriptor.</summary>
    internal static void RunBotwoonMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        switch (state.Movement)
        {
            case BotwoonMovement.MoveTowardHole:
                MoveBotwoonTowardHole(head, state);
                return;
            case BotwoonMovement.LoadAuthoredPath:
                LoadBotwoonPathDescriptor(state);
                FollowBotwoonPath(head, state);
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
    private static void MoveBotwoonTowardHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        BotwoonHoleDefinition hole =
            BotwoonNavigation.HoleAt(state.TargetHoleIndex);
        Offset16<RoomPixels> delta = hole.Target - head.Position.Whole;
        Bearing bearing = EnemyMath.CalculateBearing(new(
            ClampBotwoonTargetDelta(delta.X), ClampBotwoonTargetDelta(delta.Y)));
        state.MovementAngle = bearing.ToTableAngle().ToAngle();

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
    /// beyond <see cref="HoleBearingReach"/> pixels either way it reads as that reach, except that a distance of exactly
    /// 256 pixels upward or leftward is kept as it is. The bearing toward his hole, and so his path, follows these values.
    /// </summary>
    private static int ClampBotwoonTargetDelta(int value) =>
        value < -(HoleBearingReach + 1) ? -HoleBearingReach : value > HoleBearingReach ? HoleBearingReach : value;

    /// <summary>The farthest a hole's distance on either axis is measured for its bearing, in pixels.</summary>
    private const int HoleBearingReach = 255;

    /// <summary>Uses health phase, current hole, and RNG to choose an authored path.</summary>
    private void ChooseBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        UpdateBotwoonHealthPhase(head, state);
        int hiddenBase = state.InsideHole ? BotwoonNavigationDefinitions.HiddenPathChoiceBase : 0;
        int variant = _room.Random.Next(BotwoonNavigationDefinitions.PathChoice);
        state.PathChoice = variant + hiddenBase + BotwoonNavigationDefinitions.PathVariantsPerHole * state.TargetHoleIndex;
    }

    /// <summary>Changes traversal speed and body spacing at half and quarter health outside holes.</summary>
    internal static void UpdateBotwoonHealthPhase(RoomEnemySlot head, BotwoonEnemyState state)
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

    /// <summary>Loads the chosen path, the way it is walked, its first step and the destination hole.</summary>
    private static void LoadBotwoonPathDescriptor(BotwoonEnemyState state)
    {
        BotwoonPathDescriptorDefinition descriptor =
            BotwoonNavigation.PathForChoice(state.PathChoice);
        state.PathIndex = descriptor.Path;
        state.PathTraversal = descriptor.Traversal;
        state.PathStep = descriptor.FirstStep;
        state.TargetHoleIndex = descriptor.TargetHoleIndex;
        state.PathComplete = false;
        state.Movement = BotwoonMovement.FollowAuthoredPath;
    }

    /// <summary>
    /// Takes as many path steps as the current speed allows. Walking off either end of the path completes it; the
    /// steps already taken on that update are then discarded, so the head does not move on its final update.
    /// </summary>
    private static void FollowBotwoonPath(RoomEnemySlot head, BotwoonEnemyState state)
    {
        bool backward = state.PathTraversal == BotwoonPathTraversal.Backward;
        Offset16<RoomPixels> total = Offset16<RoomPixels>.Zero;
        for (int taken = 0; taken < state.Speed; taken++)
        {
            if (!BotwoonNavigation.HasStep(state.PathIndex, state.PathStep))
            {
                state.PathComplete = true;
                return;
            }
            total += BotwoonNavigation.Step(state.PathIndex, state.PathStep);
            state.PathStep += backward ? -1 : 1;
        }

        head.Position += backward ? -total : total;
    }

    /// <summary>Writes the head's whole-pixel position into the head-position history ring.</summary>
    private static void RecordBotwoonHeadPosition(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.History[state.RingIndex] = head.Position.Whole;
    }

    /// <summary>Spaces thirteen body projectiles along older head-history samples.</summary>
    private static void PositionBotwoonBody(BotwoonEnemyState state)
    {
        int historyIndex = BotwoonTrailRing.PlaceAt(state.RingIndex - state.SegmentSpacingEntries);
        for (int segmentIndex = state.BodySegments.Length - 1; segmentIndex >= 0; segmentIndex--)
        {
            EnemyProjectile segment = state.Segment(segmentIndex);

            ToggleBotwoonSegmentAtHole(state, segmentIndex, historyIndex);
            segment.Position = segment.Position.WithWhole(state.History[historyIndex]);
            historyIndex = BotwoonTrailRing.PlaceAt(historyIndex - state.SegmentSpacingEntries);
        }
    }

    /// <summary>Changes a segment's collision mode when its delayed history cursor crosses a hole.</summary>
    private static void ToggleBotwoonSegmentAtHole(
        BotwoonEnemyState state,
        int segmentIndex,
        int historyIndex)
    {
        if (state.SavedHoleRingIndex != historyIndex)
            return;

        EnemyProjectile segment = state.Segment(segmentIndex);
        bool wasInside = state.SegmentInsideHole[segmentIndex];
        state.SegmentInsideHole[segmentIndex] = !wasInside;
        segment.CanDamageSamus = wasInside;
        segment.CollisionOption = wasInside ? EnemyProjectileCollisionOption.IndestructibleDud : EnemyProjectileCollisionOption.NoCollision;

        if (segmentIndex != 0)
            return;
        state.HoleLatch = false;
        state.ExitTransitionComplete = !state.InsideHole;
        state.SavedHoleRingIndex = null;
    }

    /// <summary>
    /// Faces each body sprite toward the segment ahead of it, or the head: a segment inside the wall is hidden, the
    /// tail segment (index zero) shows the tail cap and every other segment a body link.
    /// </summary>
    private static void OrientBotwoonBody(RoomEnemySlot head, BotwoonEnemyState state)
    {
        int lastSegment = state.BodySegments.Length - 1;
        for (int segmentIndex = lastSegment; segmentIndex >= 0; segmentIndex--)
        {
            EnemyProjectile segment = state.Segment(segmentIndex);
            EnemyProjectile? leader = segmentIndex == lastSegment ? null : state.Segment(segmentIndex + 1);
            Offset16<RoomPixels> toLeader = (leader?.Position.Whole ?? head.Position.Whole) - segment.Position.Whole;
            BotwoonBodyAnimationSegmentKind kind = state.SegmentInsideHole[segmentIndex] ? BotwoonBodyAnimationSegmentKind.Hidden
                : segmentIndex == 0 ? BotwoonBodyAnimationSegmentKind.Tail
                : BotwoonBodyAnimationSegmentKind.Body;
            segment.StateOf<BotwoonSegmentState>().Pose =
                BotwoonBodyAnimationSegment.Toward(kind, EnemyMath.CalculateBearing(toLeader));
        }
    }

    /// <summary>Latches the first hole rectangle entered by the head and marks its history offset.</summary>
    internal static void DetectBotwoonHole(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HoleLatch)
            return;

        // The game tests the holes from the last to the first and latches the first it finds.
        for (int holeIndex = BotwoonNavigationDefinitions.HoleCount - 1; holeIndex >= 0; holeIndex--)
        {
            BotwoonHoleDefinition hole = BotwoonNavigation.HoleAt(holeIndex);
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

    /// <summary>Dispatches head animation, aim, spit-frame wait, immediate volley, or cooldown.</summary>
    private void RunBotwoonHeadAction(
        RoomEnemySlot head,
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
                    SpawnBotwoonSpitVolley(head, state, count: 5, startOffset: -32);
                    state.SpitFrameReached = false;
                    state.HeadAction = BotwoonHeadAction.AttackCooldown;
                }
                return;
            case BotwoonHeadAction.SpawnImmediateVolley:
                SpawnBotwoonSpitVolley(head, state, count: 3, startOffset: -16);
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
    internal static void AnimateBotwoonHeadFromMovement(RoomEnemySlot head, BotwoonEnemyState state)
    {
        Offset16<RoomPixels> moved = head.Position.Whole - state.HeadHistory[3];
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

        for (int index = 3; index > 0; index--)
        {
            state.HeadHistory[index] = state.HeadHistory[index - 1];
        }
        state.HeadHistory[0] = head.Position.Whole;
    }

    /// <summary>Aims the spit animation at Samus and selects delayed or immediate volley timing.</summary>
    internal void AimBotwoonAtSamus(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        SamusState samus)
    {
        Bearing bearing = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(samus.Position.X.Whole, head.Position.X.Whole), CoordinateAxis.Difference(samus.Position.Y.Whole, head.Position.Y.Whole)));
        StartBotwoonHeadAnimation(head, state, BotwoonHeadAnimationSegment.SpittingToward(bearing));
        state.SpitAngle = bearing.ToTableAngle().ToAngle();
        state.HeadAction = state.Phase == BotwoonPhase.SpitWhileHidden
            ? BotwoonHeadAction.WaitForSpitFrame
            : BotwoonHeadAction.SpawnImmediateVolley;
        RunBotwoonHeadAction(head, state, samus);
    }

    /// <summary>Restarts the head animation timer only when its selected animation changes.</summary>
    private static void StartBotwoonHeadAnimation(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        BotwoonHeadAnimationSegment segment)
    {
        if (segment == state.HeadAnimation)
            return;
        state.HeadPlayback = EnemyAnimationPlayback.Start(head, BotwoonAnimations.HeadStartingWith(segment));
    }

    /// <summary>Advances Botwoon's typed animation.</summary>
    internal void AdvanceBotwoonTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        AdvanceBotwoonHeadAnimation(slot, RequireBotwoonState(slot));
    }

    /// <summary>
    /// Runs Botwoon's head animation where the shared enemy animation runs: the frame
    /// expires on the update its timer would, and a newly shown frame raises the
    /// new-frame flag exactly as a timed frame does.
    /// </summary>
    internal void AdvanceBotwoonHeadAnimation(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (!head.AnimationClock.Expire())
            return;

        AnimationPlace place = state.HeadPlayback.Place;
        // No head animation counts its plays, so nothing reads or writes this counter.
        WrappingCountdown uncounted = default;
        AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame> advance = BotwoonAnimations.HeadSegments.PlayToNextFrame(
            state.HeadPlayback.Animation, ref place, ref uncounted, cue => ApplyBotwoonHeadCue(head, state, cue));
        state.HeadPlayback = state.HeadPlayback with { Place = place };
        // A hold is the script's sleep: the timer wraps from zero and the last frame stays.
        if (advance is AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame>.Held)
            return;
        if (advance is not AnimationShown<BotwoonHeadAnimationSegment, BotwoonHeadFrame>.Frame shown)
            throw new InvalidOperationException($"Botwoon's head animation ended an update with {advance}.");
        head.Frame = BotwoonVisualDefinitions.HeadFrames.Of(shown.Shown);
        head.AnimationClock.Show(shown.Duration);
    }

    /// <summary>Applies frame cues for facing radius, spit audio, or the volley trigger.</summary>
    private void ApplyBotwoonHeadCue(RoomEnemySlot head, BotwoonEnemyState state, BotwoonHeadCue cue)
    {
        switch (cue)
        {
            case BotwoonHeadCue.ApplyFacingRadius:
                head.Radius = BotwoonAnimations.HeadRadius(state.HeadAnimation.Facing);
                return;
            case BotwoonHeadCue.QueueSpitSound:
                RequestBotwoonSound(BotwoonSoundDefinitions.Spit);
                return;
            case BotwoonHeadCue.ReachSpitFrame:
                state.SpitFrameReached = true;
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BotwoonHeadCue)} {cue}.");
        }
    }

    /// <summary>Emits three or five spit projectiles at sixteen-angle increments around the aimed direction.</summary>
    private void SpawnBotwoonSpitVolley(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        int count,
        int startOffset)
    {
        int speed = BotwoonSpeeds.ForHealthPhase(state.HealthPhase).SpitSpeed;
        TrigonometryAngle angle = state.SpitAngle.TurnedBySteps(startOffset).ToTrigonometryAngle();
        for (int projectile = 0; projectile < count; projectile++)
        {
            SpawnBotwoonSpit(head, angle, speed);
            angle = angle.TurnedBySteps(16);
        }
    }

    /// <summary>
    /// Shows the next health band's colours when Botwoon's health crosses its threshold (#1512): his head and body take
    /// the import's shade of the band.
    /// </summary>
    internal static void UpdateBotwoonHealthBand(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.HealthBandsPassed == BotwoonHealthBandDefinitions.BandCount)
            return;
        if (!BotwoonHealthBands.ShouldAdvance(state.HealthBandsPassed, head.Health))
            return;
        state.HealthBandsPassed++;
        head.Shading = state.BodyShading;
    }

    /// <summary>Accelerates the defeated head down to Y=200, then hides it and waits for body landing.</summary>
    private void RunBotwoonHeadFall(RoomEnemySlot head, BotwoonEnemyState state)
    {
        SignedSixteenSixteen displacement = state.DeathFallAccumulator.Row.Positive;
        head.Position = head.Position.AdvanceY(displacement);
        if (CoordinateAxis.Difference(head.Position.Y.Whole, 200) < 0)
        {
            state.DeathFallAccumulator += BotwoonDeathFallAcceleration;
            return;
        }

        head.Position = head.Position.WithWholeY(200);
        state.Phase = BotwoonPhase.WaitForBody;
        _projectiles.SpawnRoomGraphicsDustExplosion(head.Position.Whole, MiscDustAnimation.BigExplosion);
        RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
        head.Presence = head.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Requests the crumble room object and sixteen RNG-positioned drops before wall explosions.</summary>
    private void BeginBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        state.Phase = BotwoonPhase.WallExplosions;
        _frameResults.BotwoonWall = BotwoonWallMutation.Crumble;

        // Botwoon's death drop emits sixteen independent pickup
        // requests. Each position uses both halves of one RNG result: X is the sample modulo 128
        // and Y the sample divided by 256, modulo 64. Each pickup is materialized immediately as
        // a real pickup actor from the head definition's item chances.
        for (int dropIndex = 0; dropIndex < BotwoonSpecialDropCount; dropIndex++)
        {
            int random = _room.Random.Generate();
            var position = new Position16<RoomPixels>(
                BotwoonDeathDefinitions.DropX.Of(random) + 64,
                BotwoonDeathDefinitions.DropY.Of(random) + 128);
            _deathsAndDrops.SpawnEnemyDropFromChanceTable(
                position,
                head.Definition.DropChances);
        }
        state.WallExplosionFrame = 0;
        state.LargeExplosionTimer = 0;
        state.SmallExplosionTimer = 0;
    }

    /// <summary>Runs the 192-frame wall sequence, then records the boss's defeat and queues delayed music.</summary>
    private void RunBotwoonWallExplosions(RoomEnemySlot head, BotwoonEnemyState state)
    {
        if (state.WallExplosionFrame >= BotwoonWallExplosionDuration)
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

        if (state.WallExplosionFrame >= BotwoonWallExplosionsBegin)
        {
            if (--state.LargeExplosionTimer < 0)
            {
                state.LargeExplosionTimer = 12;
                _spriteObjects.SpawnRoomSpriteObject(
                    new(_room.Random.Next(BotwoonDeathDefinitions.LargeExplosionX) + 232, state.WallExplosionFrame + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter) - 8),
                    RoomSpriteObjectKind.BotwoonLargeExplosion,
                    graphics: SpriteGraphics.SharedIn(EnemyShades.InProjectileLook));
                RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);
            }

            if (--state.SmallExplosionTimer < 0)
            {
                state.SmallExplosionTimer = 4;
                for (int explosion = 0; explosion < 2; explosion++)
                {
                    _spriteObjects.SpawnRoomSpriteObject(
                        new(_room.Random.Next(BotwoonDeathDefinitions.SmallExplosionX) + 224, state.WallExplosionFrame + _room.Random.Next(BotwoonDeathDefinitions.ExplosionYJitter) - 8),
                        RoomSpriteObjectKind.BotwoonSmallExplosion,
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
    internal void ResolveBotwoonCombatAfterCommon(RoomEnemySlot head)
    {
        if (head.Health != 0)
            return;

        BotwoonEnemyState state = RequireBotwoonState(head);
        state.PendingDeath = true;

        // The dying head becomes intangible: it leaves the
        // projectile, bomb and touch passes, so a later hit cannot interrupt its fall.
        head.Contact = head.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Applies the angle table's signed X/Y displacement to the head.</summary>
    private static void AddBotwoonAngleVector(
        RoomEnemySlot head,
        TrigonometryAngle angle,
        int magnitude)
    {
        SignedSixteenSixteen xMagnitude = EnemyMath.AbsoluteCosineSpeed(angle, magnitude).AsDisplacement();
        SignedSixteenSixteen yMagnitude = EnemyMath.AbsoluteSineSpeed(angle, magnitude).AsDisplacement();
        head.Position = head.Position.AdvanceX(EnemyMath.CosineIsNegative(angle) ? -xMagnitude : xMagnitude);
        head.Position = head.Position.AdvanceY(EnemyMath.SineIsNegative(angle) ? -yMagnitude : yMagnitude);
    }

    /// <summary>Requires the state of the initialized Botwoon whose head is <paramref name="head"/>.</summary>
    internal BotwoonEnemyState RequireBotwoonState(RoomEnemySlot head) =>
        BotwoonState is not null && ReferenceEquals(head, _head)
            ? BotwoonState
            : throw new InvalidOperationException(
                $"Enemy slot {head.SlotIndex} has no initialized Botwoon state.");

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

    /// <summary>
    /// Moves a body segment, oriented and coloured with his hurt and run through its death, or a spit flying until it
    /// leaves the camera.
    /// </summary>
    /// <param name="context">The projectile and what its pass hands every frame behaviour.</param>
    private protected override void Move(EnemyProjectileFrameBehaviorContext context)
    {
        EnemyProjectileFrameBehavior motion = context.Projectile.FrameBehavior;
        if (motion == EnemyProjectileFrameBehavior.BotwoonBodySegment)
        {
            RunBotwoonBodyFrameBehavior(context.Projectile, context.ShortCounter);
        }
        else if (motion == EnemyProjectileFrameBehavior.BotwoonSpitFlight)
        {
            RunBotwoonSpitFrameBehavior(context.Projectile, context.Camera);
        }
        else
        {
            base.Move(context);
        }
    }

    /// <summary>Runs Botwoon's main AI.</summary>
    internal void RunBotwoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBotwoonMain(slot, RequireBotwoonState(slot), frame.Samus);
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
        _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus, deathAnimation: TouchDeathAnimation.Skip);
        ResolveBotwoonCombatAfterCommon(context.Slot);
    }
    /// <summary>Spawns one of Botwoon's body segments.</summary>
    /// <param name="head">Botwoon's head, whose position and graphics the segment starts with.</param>
    /// <param name="state">Botwoon's state, which records the segment.</param>
    /// <param name="segmentIndex">The segment's place in the body, zero (the tail) through twelve (behind the head).</param>
    internal void SpawnBotwoonBodySegment(
        RoomEnemySlot head,
        BotwoonEnemyState state,
        int segmentIndex)
    {
        EnemyProjectile? segment = _projectiles.AllocateEnemyProjectile();
        if (segment is null)
        {
            throw new InvalidOperationException(
                "Botwoon requires thirteen free enemy-projectile slots during room load.");
        }

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            segment,
            RoomEnemyProjectileKind.BotwoonBody,
            BodyAndSpit,
            SpriteGraphics.SpawnedBy(head));
        segment.Position = FixedPosition<RoomPixels>.FromWhole(head.Position.Whole);

        // Every segment, the tail cap included, starts hidden inside the wall.
        BotwoonBodyAnimationSegment pose = BotwoonBodyAnimationSegment.Hidden;
        segment.StartAnimation(BotwoonProjectileAnimation.ForSegment(pose));
        segment.State = new BotwoonSegmentState(pose);

        state.BodySegments[segmentIndex] = segment;
        state.SegmentPhases[segmentIndex] = BotwoonBodyPhase.Main;
        state.SegmentInsideHole[segmentIndex] = true;
        segment.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        segment.CanDamageSamus = false;
    }

    /// <summary>Runs a body segment's per-frame behaviour through its four movement states.</summary>
    internal void RunBotwoonBodyFrameBehavior(
        EnemyProjectile segment,
        int randomEnemyCounter)
    {
        BotwoonEnemyState state = BotwoonState ?? throw new InvalidOperationException(
            "A live Botwoon body segment has no owning head state.");
        int segmentIndex = Array.IndexOf(state.BodySegments, segment);
        if (segmentIndex < 0)
            throw new InvalidOperationException($"The enemy projectile in place {segment.Place} is not one of Botwoon's body segments.");
        BotwoonBodyPhase phase = state.SegmentPhases[segmentIndex];
        if (state.BodyDeathStarted && phase == BotwoonBodyPhase.Main)
        {
            phase = BotwoonBodyPhase.BeginDying;
            state.SegmentPhases[segmentIndex] = phase;
        }

        switch (phase)
        {
            case BotwoonBodyPhase.Main:
                AnimateBotwoonBodySegment(segment, randomEnemyCounter);
                return;

            case BotwoonBodyPhase.BeginDying:
                // The delay follows the segment's projectile place, not its place in the body.
                segment.StateOf<BotwoonSegmentState>().DeathDelay = BotwoonSegmentFallDelay.Start(segment.Place);
                state.SegmentPhases[segmentIndex] = BotwoonBodyPhase.DyingDelay;
                goto case BotwoonBodyPhase.DyingDelay;

            case BotwoonBodyPhase.DyingDelay:
            {
                // The delay starts below the fall count and ends on reaching it.
                BotwoonSegmentState dying = segment.StateOf<BotwoonSegmentState>();
                dying.DeathDelay++;
                if (dying.DeathDelay >= BotwoonSegmentState.FallAt)
                    state.SegmentPhases[segmentIndex] = BotwoonBodyPhase.Falling;
                segment.FrameHold = WrappingCountdown.Start(0);
                ShowBotwoonBodyHurtFlash(segment, randomEnemyCounter);
                return;
            }

            case BotwoonBodyPhase.Falling:
                RunBotwoonBodyFall(segment, segmentIndex, state, randomEnemyCounter);
                return;

            case BotwoonBodyPhase.Landed:
                return;

            default:
                throw new InvalidOperationException($"Botwoon's body has no behaviour for phase {phase}.");
        }
    }

    /// <summary>Installs a segment animation for its angle and applies the head's hurt flash.</summary>
    internal void AnimateBotwoonBodySegment(
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
    private void ShowBotwoonBodyHurtFlash(
        EnemyProjectile segment,
        int randomEnemyCounter)
    {
        RoomEnemySlot head = _head ?? throw new InvalidOperationException("Botwoon's body draws once Botwoon is loaded.");
        segment.Graphics = segment.Graphics.In(head.FlashTimer != 0 && EnemyDrawPhaseDefinitions.HurtFlashShown.IsSetIn(randomEnemyCounter)
            ? EnemyShades.HurtFlashLook
            : (BotwoonState ?? throw new InvalidOperationException("Botwoon's body draws once Botwoon is loaded.")).BodyShading);
    }

    /// <summary>Accelerates a dying segment to Y=200 and marks the tail cap's final landing.</summary>
    private void RunBotwoonBodyFall(
        EnemyProjectile segment,
        int segmentIndex,
        BotwoonEnemyState state,
        int randomEnemyCounter)
    {
        BotwoonSegmentState falling = segment.StateOf<BotwoonSegmentState>();
        segment.Position = segment.Position.AdvanceY(falling.FallPlace.Row.Positive);
        if (CoordinateAxis.Difference(segment.Position.Y.Whole, 200) < 0)
        {
            // A place past the table's last row fails when its row is read, long before the place could grow large.
            falling.FallPlace += BotwoonDeathFallAcceleration;
            segment.FrameHold = WrappingCountdown.Start(0);
            ShowBotwoonBodyHurtFlash(segment, randomEnemyCounter);
            return;
        }

        segment.Position = segment.Position.WithWholeY(200);
        state.SegmentPhases[segmentIndex] = BotwoonBodyPhase.Landed;
        // The landed segment plays the shared big-explosion animation.
        segment.StartAnimation(new MiscDustProjectileAnimation(MiscDustProjectileSegment.BigExplosion));
        segment.Graphics = SpriteGraphics.SharedIn(EnemyShades.InProjectileLook);
        segment.CanDamageSamus = false;
        segment.CollisionOption = EnemyProjectileCollisionOption.NoCollision;
        RequestBotwoonSound(BotwoonSoundDefinitions.Explosion);

        // The tail segment owns the synchronization flag.
        if (ReferenceEquals(state.BodySegments[0], segment))
            state.LastBodySegmentLanded = true;
    }

    /// <summary>Spawns one of Botwoon's spits.</summary>
    /// <param name="head">Botwoon's head.</param>
    /// <param name="angle">The spit's direction.</param>
    /// <param name="speed">The spit's speed.</param>
    internal void SpawnBotwoonSpit(RoomEnemySlot head, TrigonometryAngle angle, int speed)
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
    internal static void RunBotwoonSpitFrameBehavior(
        EnemyProjectile spit,
        Position16<RoomPixels> camera)
    {
        spit.StateOf<AngledFlightState>().Advance(spit);

        if (CoordinateAxis.Difference(spit.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.AsSigned(camera.X + 256 - spit.Position.X.Whole) < 0 ||
            CoordinateAxis.Difference(spit.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.AsSigned(camera.Y + 256 - spit.Position.Y.Whole) < 0)
        {
            spit.Delete();
        }
    }

    /// <summary>Queues one of Botwoon's sounds as Botwoon makes it, after the update's earlier requests.</summary>
    private void RequestBotwoonSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);
}
