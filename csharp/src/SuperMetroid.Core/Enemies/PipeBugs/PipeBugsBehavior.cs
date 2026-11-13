using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The pipe bugs species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class PipeBugsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PipeBugsPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PipeBugsEntity(this, slot, common);

    /// <summary>The room's enemy places, where each Norfair formation leader finds its formation when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>
    /// The five-bug formation each Norfair leader found when it was placed: itself, then the four bugs its room lists
    /// right after it.
    /// </summary>
    private readonly EnemyPartLinks<RoomEnemySlot[]> _formations = new();
    /// <summary>
    /// The places set up as Norfair Pipe Bugs this room. A formation member killed and not yet respawned holds no state,
    /// and its place is still the formation's.
    /// </summary>
    private readonly HashSet<RoomEnemySlot> _norfairPlaces = [];

    /// <summary>Binds the shared enemy pool needed to validate Norfair formation membership.</summary>
    internal PipeBugsBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Pixels a Brinstar pipe bug rises above its stored spawn Y before flying.</summary>
    private const int BrinstarPipeBugEmergenceHeight = 16;
    /// <summary>Strict horizontal range that wakes a hidden Brinstar pipe bug.</summary>
    private const int BrinstarPipeBugTriggerWidth = 64;
    /// <summary>Vertical band above a Brinstar bug in which Samus can trigger emergence.</summary>
    private const int BrinstarPipeBugTriggerTop = 96;
    /// <summary>Wait before an offscreen Brinstar bug may emerge again.</summary>
    private const int BrinstarPipeBugRespawnFrames = 48;

    /// <summary>Leader plus four consecutive followers driven by Norfair's behaviour.</summary>
    private const int NorfairFormationSize = 5;

    /// <summary>Forward horizontal range that wakes a yellow pipe bug.</summary>
    private const int YellowPipeBugTriggerDistance = 192;
    /// <summary>Strict vertical distance allowed by the yellow bug's Samus trigger.</summary>
    private const int YellowPipeBugTriggerHeight = 48;
    /// <summary>Frames between a yellow bug becoming visible and starting its straight flight.</summary>
    private const int YellowPipeBugEmergenceDelayFrames = 24;
    /// <summary>Distance past Samus at which a yellow bug begins its one-time arc.</summary>
    private const int YellowPipeBugArcTriggerDistance = 48;
    /// <summary>The place along the quadratic speed table, row 40, the yellow bug's vertical arc starts from.</summary>
    private static readonly EnemyQuadraticSpeedIndex YellowPipeBugArcCounterStart = EnemyQuadraticSpeedIndex.FromRows(40);

    /// <summary>Initializes the Brinstar Pipe Bug.</summary>
    internal static void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        state.EmergenceTop = state.SpawnPosition + new Offset16<RoomPixels>(0, -BrinstarPipeBugEmergenceHeight);
        state.Function = PipeBugEnemyFunction.BrinstarWaitUntilOnScreen;
        state.RespawnDelay = BrinstarPipeBugRespawnFrames;
        state.RequestedAnimation = PipeBugAnimationSelector.RisingFacingLeft;
        state.InstalledAnimation = PipeBugAnimationSelector.RisingFacingLeft;
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugPrograms.BrinstarProgram(
            strong: slot.SpawnSettings<BrinstarPipeBugSpawnSettings>().Kind == BrinstarPipeBugKind.Strong,
            PipeBugAnimationSelector.RisingFacingLeft)), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Initializes the Norfair Pipe Bug.</summary>
    internal void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        // Only the formation's first bug has a trigger width; it leads the four its room lists after it.
        if (slot.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth != 0)
            _formations.Place(slot, [slot, .. _slotTable.ListedRunAfter(slot, NorfairFormationSize - 1)]);
        _ = _norfairPlaces.Add(slot);
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        SignedSixteenSixteen flightSpeed = slot.SpawnSettings<NorfairPipeBugSpawnSettings>().Speed;
        state.FlightSpeeds = new EnemyLinearSpeedRow(flightSpeed, SignedSixteenSixteen.Zero - flightSpeed);
        state.FormationCounter = 0;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugProgram.GametRisingLeft), AnimationPlace.StartOfAnimation);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
    }

    /// <summary>Initializes the yellow Brinstar Pipe Bug.</summary>
    internal static void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(
            slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugProgram.GeegaFlyingLeft
                : PipeBugProgram.GeegaFlyingRight), AnimationPlace.StartOfAnimation);

        // The spawn settings name the speed the bug flies at.
        SignedSixteenSixteen speed = slot.SpawnSettings<YellowPipeBugSpawnSettings>().Speed;
        state.PositiveVelocity = speed;
        state.NegativeVelocity = SignedSixteenSixteen.Zero - speed;
        state.Function = PipeBugEnemyFunction.YellowWaitForSamus;
        state.ArcCompleted = false;
    }

    /// <summary>Dispatches the three main behaviours and every function they select.</summary>
    private void RunPipeBugMain(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Function)
        {
            case PipeBugEnemyFunction.BrinstarWaitUntilOnScreen:
                // The game's on-screen check returns zero when the
                // origin is visible. The wait branches on that zero result, so this initial
                // state arms the proximity check on-screen, not after a despawn boundary.
                if (!EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, camera))
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;
            case PipeBugEnemyFunction.BrinstarWaitForSamus:
                RequirePipeBugSamus(samus);
                RunBrinstarPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugEnemyFunction.BrinstarEmerge:
                RequirePipeBugSamus(samus);
                RunBrinstarPipeBugEmergence(slot, state, samus);
                return;
            case PipeBugEnemyFunction.BrinstarFlyHorizontally:
                RunBrinstarPipeBugFlight(slot, state, camera.X);
                return;
            case PipeBugEnemyFunction.BrinstarRespawnDelay:
                if (--state.RespawnDelay == 0)
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;

            case PipeBugEnemyFunction.NorfairWaitForFormation:
                RunNorfairPipeBugFormationWait(slot, state);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairWaitForSamus:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugSamusWait(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairRise:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugRise(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLeaderStagger:
                state.CountFormationTick();
                state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairUpperNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairUpperFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLowerNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLowerFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairWaitForStagger:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugStaggerWait(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairFlyLeft:
                AddPipeBugLinearVelocity(slot, axis: Axis.Horizontal, state.FlightSpeeds.Negative);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairFlyRight:
                AddPipeBugLinearVelocity(slot, axis: Axis.Horizontal, state.FlightSpeeds.Positive);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;

            case PipeBugEnemyFunction.YellowWaitForSamus:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugEnemyFunction.YellowEmergenceDelay:
                RunYellowPipeBugDelay(slot, state);
                return;
            case PipeBugEnemyFunction.YellowFlyLeft:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, direction: HorizontalDirection.Left);
                return;
            case PipeBugEnemyFunction.YellowFlyRight:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, direction: HorizontalDirection.Right);
                return;
            case PipeBugEnemyFunction.YellowArcLeft:
                RunYellowPipeBugArc(slot, state, camera, direction: HorizontalDirection.Left);
                return;
            case PipeBugEnemyFunction.YellowArcRight:
                RunYellowPipeBugArc(slot, state, camera, direction: HorizontalDirection.Right);
                return;
            default:
                throw new InvalidDataException(
                    $"Pipe Bug function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Wakes a hidden Brinstar bug only when Samus enters its narrow horizontal and upper vertical band.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        int deltaY = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole);
        if (deltaY >= 0 || CoordinateAxis.AsSigned(deltaY + BrinstarPipeBugTriggerTop) < 0)
            return;

        state.BrinstarFliesLeft = (samus.Position.Whole - slot.Position.Whole).X < 0;
        if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) >=
            BrinstarPipeBugTriggerWidth)
        {
            return;
        }

        state.Function = PipeBugEnemyFunction.BrinstarEmerge;
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        slot.Timer = WrappingCountdown.Start(0);
        state.RequestedAnimation = PipeBugPrograms.Rising(
            state.BrinstarFliesLeft ? HorizontalDirection.Left : HorizontalDirection.Right);
        SelectBrinstarPipeBugAnimation(slot, state);
    }

    /// <summary>Applies the fractional Y decrement until the bug rises above Samus and switches to horizontal flight.</summary>
    private static void RunBrinstarPipeBugEmergence(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        // The rise decrements the subposition as a separate value, then subtracts either one
        // or two pixels according to the old value. This is deliberately not normal 16.16
        // subtraction; preserving the oddity prevents a one-pixel/frame timing drift.
        FixedCoordinate y = slot.Position.Y;
        bool oldSubpositionWasNonzero = y.Fraction != 0;
        slot.Position = slot.Position.WithY(new(
            CoordinateAxis.Wrap(y.Whole + (oldSubpositionWasNonzero ? -1 : -2)),
            CoordinateAxis.Wrap(y.Fraction - 1)));
        if ((state.EmergenceTop - slot.Position.Whole).Y < 0 ||
            slot.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        state.RequestedAnimation = PipeBugPrograms.ShootingWithSameFacing(state.RequestedAnimation);
        SelectBrinstarPipeBugAnimation(slot, state);
        state.Function = PipeBugEnemyFunction.BrinstarFlyHorizontally;
    }

    /// <summary>Moves two pixels per frame, then restores the stored spawn position and starts an invisible respawn wait offscreen.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        int cameraX)
    {
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (state.BrinstarFliesLeft ? -2 : 2));
        // The despawn tests only the horizontal screen extent.
        if (!EnemyProcessingWindow.EnemyIsHorizontallyOffScreen(slot, cameraX))
            return;

        slot.Position = slot.Position.WithWhole(state.SpawnPosition).WithFractionX(0);
        // This apparently strange assignment is literal game behaviour: the game
        // stores the integer spawn Y as both whole and fraction rather than clearing the fraction.
        slot.Position = slot.Position.WithFractionY(state.SpawnPosition.Y);
        state.RequestedAnimation = PipeBugAnimationSelector.RisingFacingLeft;
        SelectBrinstarPipeBugAnimation(slot, state);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        state.RespawnDelay = BrinstarPipeBugRespawnFrames;
        state.Function = PipeBugEnemyFunction.BrinstarRespawnDelay;
    }

    /// <summary>Installs a strong or ordinary Brinstar program only when the selected animation changes.</summary>
    internal static void SelectBrinstarPipeBugAnimation(
        RoomEnemySlot slot,
        PipeBugEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugPrograms.BrinstarProgram(
            strong: slot.SpawnSettings<BrinstarPipeBugSpawnSettings>().Kind == BrinstarPipeBugKind.Strong,
            state.RequestedAnimation)), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Arms the Norfair leader only after all five consecutive formation members remain dormant.</summary>
    private void RunNorfairPipeBugFormationWait(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        // Only the first record has a trigger width. It is the formation controller and may
        // arm itself only while all four following physical records are still at the
        // dormant function.
        if (slot.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth == 0)
            return;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(slot);
        if (formation.All(member => KilledGametHoldsFormation.Waits(
                member.SpeciesState<PipeBugEnemyState>(),
                static state => state.Function == PipeBugEnemyFunction.NorfairWaitForFormation)))
        {
            state.Function = PipeBugEnemyFunction.NorfairWaitForSamus;
        }
    }

    /// <summary>Launches the five-bug rise when Samus enters the leader's trigger, retaining follower frame timers.</summary>
    internal void RunNorfairPipeBugSamusWait(
        RoomEnemySlot leader,
        PipeBugEnemyState leaderState,
        SamusState samus)
    {
        int triggerWidth = leader.SpawnSettings<NorfairPipeBugSpawnSettings>().TriggerWidth;
        if (!EnemyMath.IsWithinStrictModularDistance(leader.Position.X.Whole, samus.Position.X.Whole, triggerWidth) ||
            CoordinateAxis.Difference(leader.Position.Y.Whole, samus.Position.Y.Whole) < 0)
        {
            return;
        }

        leaderState.CountFormationTick();
        // The formation tick resets only the current (leader) slot. The following
        // formation stores change every animation, but leave follower
        // timers intact, including zero timers in cleared respawn placeholders.
        leader.FrameHold = WrappingCountdown.Start(1);
        leader.Timer = WrappingCountdown.Start(0);
        bool samusIsRight = CoordinateAxis.Difference(samus.Position.X.Whole, leader.Position.X.Whole) >= 0;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(leader);
        // A member killed since the formation armed has nothing to launch: it respawns waiting, as a new bug.
        foreach (RoomEnemySlot member in formation)
        {
            if (member.SpeciesState<PipeBugEnemyState>() is not { } memberState)
                continue;
            memberState.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(samusIsRight
                ? PipeBugProgram.GametRisingRight
                : PipeBugProgram.GametRisingLeft), AnimationPlace.StartOfAnimation);
            memberState.Function = PipeBugEnemyFunction.NorfairRise;
        }

        for (int index = 0; index < formation.Length; index++)
        {
            if (formation[index].SpeciesState<PipeBugEnemyState>() is not { } memberState)
                continue;
            memberState.StaggerTarget = PipeBugFormation.NorfairStaggerTarget(index);
            memberState.NorfairPostRiseFunction = PipeBugFormation.NorfairPostRiseFunction(index);
        }
    }

    /// <summary>Raises one Norfair bug until it passes Samus's Y and selects its authored post-rise function.</summary>
    private static void RunNorfairPipeBugRise(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        AddPipeBugLinearVelocity(slot, axis: Axis.Vertical, OnePixelPerFrame.Negative);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, samus.Position.Y.Whole) >= 0)
            return;

        state.Function = state.NorfairPostRiseFunction;
        state.RiseEnd = slot.Position.Whole;
        InstallPipeBugProgram(slot, state,
            CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0
                ? PipeBugProgram.GametRisingRight
                : PipeBugProgram.GametRisingLeft);
    }

    /// <summary>Offsets a rising Norfair member by its signed stagger target before its timed flight wait.</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        int signedOffset)
    {
        state.CountFormationTick();
        // Negative offsets move upward and positive offsets downward, one pixel per frame. The
        // game clamps exactly at the emergence Y +/- 16/32.
        AddPipeBugLinearVelocity(
            slot,
            axis: Axis.Vertical,
            signedOffset < 0 ? OnePixelPerFrame.Negative : OnePixelPerFrame.Positive);
        Position16<RoomPixels> target = state.RiseEnd + new Offset16<RoomPixels>(0, signedOffset);
        int distancePastTarget = (slot.Position.Whole - target).Y;
        bool reached = signedOffset < 0
            ? distancePastTarget < 0
            : distancePastTarget >= 0;
        if (!reached)
            return;
        slot.Position = slot.Position.WithY(new(target.Y, 0));
        state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
    }

    /// <summary>Waits for a member's stagger counter and then chooses left or right flight from Samus's live X.</summary>
    private static void RunNorfairPipeBugStaggerWait(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        state.CountFormationTick();
        if (!state.HasReachedStaggerTarget)
            return;

        state.FormationCounter = 0;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        if (CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) >= 0)
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyRight;
            state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugProgram.GametFlyingRight), AnimationPlace.StartOfAnimation);
        }
        else
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyLeft;
            state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugProgram.GametFlyingLeft), AnimationPlace.StartOfAnimation);
        }
    }

    /// <summary>
    /// The off-screen reset tests the bare enemy center against the screen, not the sprite-padded visibility check.
    /// </summary>
    private static void ResetNorfairPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return;
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
    }

    /// <summary>
    /// The formation the leader found when it was placed, rejecting an incomplete or unrelated formation: every member's
    /// place was set up as a Norfair Pipe Bug, and holds one or, killed, none until it respawns
    /// (<see cref="KilledGametHoldsFormation"/>).
    /// </summary>
    private RoomEnemySlot[] RequireNorfairPipeBugFormation(RoomEnemySlot leader)
    {
        RoomEnemySlot[] formation = _formations.Of(leader);
        foreach (RoomEnemySlot member in formation)
        {
            PipeBugEnemyState? state = member.SpeciesState<PipeBugEnemyState>();
            bool killed = state is null && member.DefinitionId is EnemyDefinitionId.None or EnemyDefinitionId.Respawn;
            if (!_norfairPlaces.Contains(member) ||
                (!killed && state?.DefinitionAtInitialization != EnemyDefinitionId.Gamet))
            {
                throw new InvalidDataException(
                    $"Norfair Pipe Bug formation slot {member.SlotIndex} holds {member.DefinitionId}, " +
                    $"not a {EnemyDefinitionId.Gamet} or a killed one's place.");
            }
        }
        return formation;
    }

    /// <summary>Wakes a yellow bug when Samus is ahead on its authored side and within the narrow vertical band.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        int deltaX = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole);
        bool inFront = slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
            ? deltaX < 0 && CoordinateAxis.AsSigned(deltaX + YellowPipeBugTriggerDistance) >= 0
            : deltaX >= 0 && CoordinateAxis.Difference(deltaX, YellowPipeBugTriggerDistance) < 0;
        if (!inFront ||
            !EnemyMath.IsWithinStrictModularDistance(slot.Position.Y.Whole, samus.Position.Y.Whole, YellowPipeBugTriggerHeight))
        {
            return;
        }

        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        state.EmergenceDelay = YellowPipeBugEmergenceDelayFrames;
        state.Function = PipeBugEnemyFunction.YellowEmergenceDelay;
    }

    /// <summary>Counts down the visible emergence delay before installing directional straight-flight art.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        if (--state.EmergenceDelay != 0)
            return;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        if (slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left)
        {
            state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugProgram.GeegaFlyingLeft), AnimationPlace.StartOfAnimation);
            state.Function = PipeBugEnemyFunction.YellowFlyLeft;
        }
        else
        {
            state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(PipeBugProgram.GeegaFlyingRight), AnimationPlace.StartOfAnimation);
            state.Function = PipeBugEnemyFunction.YellowFlyRight;
        }
    }

    /// <summary>Advances yellow flight and begins its single quadratic arc after passing Samus by the threshold.</summary>
    private static void RunYellowPipeBugStraightFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        AddYellowPipeBugHorizontalVelocity(slot, state, direction == HorizontalDirection.Left);
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(slot.Position.X.Whole - samus.Position.X.Whole, YellowPipeBugArcTriggerDistance) < 0
            : CoordinateAxis.Difference(samus.Position.X.Whole - slot.Position.X.Whole, YellowPipeBugArcTriggerDistance) < 0;
        if (!passedSamus)
            return;

        state.Function = direction == HorizontalDirection.Left
            ? PipeBugEnemyFunction.YellowArcLeft
            : PipeBugEnemyFunction.YellowArcRight;
        state.ArcCounter = YellowPipeBugArcCounterStart;
        state.ArcRising = true;
        InstallPipeBugProgram(slot, state,
            direction == HorizontalDirection.Left
                ? PipeBugProgram.GeegaArcingLeft
                : PipeBugProgram.GeegaArcingRight);
    }

    /// <summary>Applies the two quadratic vertical phases until the bug returns to its spawn height.</summary>
    private static void RunYellowPipeBugArc(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera,
        HorizontalDirection direction)
    {
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(slot, state, direction == HorizontalDirection.Left);
        if (state.ArcRising)
        {
            // The game moves the place back and ends the rise when it would pass the first row, holding it there; the
            // place never passes the first row, so the end is tested before the step.
            if (state.ArcCounter == EnemyQuadraticSpeedIndex.Start)
            {
                state.ArcRising = false;
            }
            else
            {
                state.ArcCounter -= EnemyQuadraticSpeedIndex.OneRow;
                AddPipeBugDisplacement(slot, axis: Axis.Vertical,
                    state.ArcCounter.Row.Positive);
            }
            return;
        }

        state.ArcCounter += EnemyQuadraticSpeedIndex.OneRow;
        AddPipeBugDisplacement(slot, axis: Axis.Vertical,
            state.ArcCounter.Row.Negative);
        bool returnedToSpawnY = direction == HorizontalDirection.Left
            ? CoordinateAxis.Difference(slot.Position.Y.Whole, state.SpawnPosition.Y) < 0
            : CoordinateAxis.Difference(state.SpawnPosition.Y, slot.Position.Y.Whole) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcRising = true;
        state.Function = direction == HorizontalDirection.Left
            ? PipeBugEnemyFunction.YellowFlyLeft
            : PipeBugEnemyFunction.YellowFlyRight;
        InstallPipeBugProgram(slot, state,
            direction == HorizontalDirection.Left
                ? PipeBugProgram.GeegaFlyingLeft
                : PipeBugProgram.GeegaFlyingRight);
    }

    /// <summary>Hides an offscreen yellow bug, restores its spawn point, and makes its arc available again.</summary>
    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Every Geega flight function resets through the bare center's on-screen test.
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return false;
        slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
        state.Function = PipeBugEnemyFunction.YellowWaitForSamus;
        state.ArcCompleted = false;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(
            slot.SpawnSettings<YellowPipeBugSpawnSettings>().Facing == HorizontalDirection.Left
                ? PipeBugProgram.GeegaFlyingLeft
                : PipeBugProgram.GeegaFlyingRight), AnimationPlace.StartOfAnimation);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        return true;
    }

    /// <summary>Applies the separately authored positive or negative 16.16 flight speed.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        bool movingLeft)
    {
        slot.Position = slot.Position.AdvanceX(movingLeft ? state.NegativeVelocity : state.PositiveVelocity);
    }

    /// <summary>The linear-speed row a Norfair bug rises and staggers at.</summary>
    private static EnemyLinearSpeedRow OnePixelPerFrame => EnemyLinearSpeeds.Row(EnemyLinearSpeedDefinitions.OnePixelPerFrameRow);

    /// <summary>Adds a linear-speed velocity to one axis, carrying the fraction into the whole.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemySlot slot,
        Axis axis,
        SignedSixteenSixteen velocity)
    {
        slot.Position = axis == Axis.Horizontal ? slot.Position.AdvanceX(velocity) : slot.Position.AdvanceY(velocity);
    }

    /// <summary>Adds a wrapped signed 16.16 displacement to one position axis.</summary>
    private static void AddPipeBugDisplacement(
        RoomEnemySlot slot,
        Axis axis,
        SignedSixteenSixteen displacement)
    {
        if (axis == Axis.Horizontal)
        {
            slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(displacement));
        }
        else
        {
            slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(displacement));
        }
    }

    /// <summary>Creates species state at the enemy's physical slot before its first AI pass.</summary>
    private static PipeBugEnemyState CreatePipeBugState(RoomEnemySlot slot)
    {
        var state = new PipeBugEnemyState(slot);
        slot.SetSpeciesState(state);
        return state;
    }

    /// <summary>Restarts an animation at timer one and clears its animation frame timer.</summary>
    internal static void InstallPipeBugProgram(RoomEnemySlot slot, PipeBugEnemyState state, PipeBugProgram program)
    {
        state.Playback = new AnimationPlayback<PipeBugProgram>(PipeBugAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Rejects a behavior callback that needs Samus but received no live player actor.</summary>
    private static void RequirePipeBugSamus([NotNull] SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Pipe Bug behavior requires the active Samus actor.");
    }

    /// <summary>Advances PipeBugs's typed animation.</summary>
    internal static void AdvancePipeBugsTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, PipeBugAnimations.Segments,
            static (PipeBugEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A Pipe Bug animation raised {cue}, though its lists raise no events."));

    /// <summary>Runs PipeBugs's main AI.</summary>
    internal void RunPipeBugsMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPipeBugMain(
            slot,
            slot.RequireSpeciesState<PipeBugEnemyState>(),
            frame.Samus,
            frame.Camera);
    }
}
