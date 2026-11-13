using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>The Kzan species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class KzanBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. KzanPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new KzanEntity(this, slot, common);

    /// <summary>The room's enemy places, where each bottom finds its top when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The top each bottom found when it was placed, which its room lists right before it.</summary>
    private readonly EnemyPartLinks<RoomEnemySlot> _tops = new();

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Couples each Kzan bottom to the top its room lists before it.</summary>
    internal KzanBehavior(EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _audio = audio;
        _slotTable = slotTable;
    }

    /// <summary>NTSC pause at the bottom before the top begins to rise.</summary>
    private const int KzanNtscRiseWaitFrames = 64;
    /// <summary>The rise of each rising update: half a pixel upward.</summary>
    private static readonly SignedSixteenSixteen KzanRisingStep = SignedSixteenSixteen.FromSixteenthsOfPixel(-8);
    /// <summary>The speed each falling update adds: one sixteenth of a pixel per update.</summary>
    private static readonly SignedSixteenSixteen KzanFallAcceleration = SignedSixteenSixteen.FromSixteenthsOfPixel(1);
    /// <summary>The fastest the falling acceleration reaches: four pixels per update.</summary>
    private static readonly SignedSixteenSixteen KzanMaximumFallingSpeed = SignedSixteenSixteen.FromParts(4, 0);

    /// <summary>The enemy-library landing sound published when the falling top reaches its target.</summary>
    private static readonly SoundEffectId KzanLandingSound =
        EnemySoundEffectDefinitions.KzanLandingSoundEffect;

    /// <summary>Initializes the top's idle animation, fall speed, fall distance and wait timer from its spawn settings.</summary>
    internal static void InitializeKzanTop(RoomEnemySlot slot)
    {
        var state = new KzanEnemyState();
        slot.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(slot, KzanAnimations.StartingWith(KzanSegment.Idle));
        state.Action = KzanAction.WaitingToFall;

        // The spawn settings name the speed the fall starts at. Retail placements include four pixels per update,
        // the fastest the fall reaches.
        state.FallingSpeed = slot.SpawnSettings<KzanSpawnSettings>().FallSpeed;

        // The spawn settings name the fall distance in pixels and the top wait countdown.
        // A zero countdown deliberately wraps to -1 on its first decrement rather than
        // triggering immediately.
        var spawn = new Position16<RoomPixels>(0, slot.Position.Y.Whole);
        state.FallingTarget = spawn + new Offset16<RoomPixels>(0, slot.SpawnSettings<KzanSpawnSettings>().FallDistance);
        state.RisingTarget = spawn;
        state.FallWaitTimerResetValue = slot.SpawnSettings<KzanSpawnSettings>().FallWait;
        state.FallWaitTimer = WrappingCountdown.Start(state.FallWaitTimerResetValue);
    }

    /// <summary>Places the lower actor twelve pixels below the preceding top during setup.</summary>
    /// <exception cref="InvalidDataException">The enemy before the bottom is not a Kzan top.</exception>
    internal void InitializeKzanBottom(RoomEnemySlot slot)
    {
        // The bottom follows whatever enemy precedes it. Every retail Kzan population (the Wasteland and the
        // Wrecked Ship's main shaft) places each bottom directly after its top, so that enemy is its top; any
        // other placement is refused rather than followed.
        if (_slotTable.ListedBeforeOrNone(slot, 1) is not { DefinitionId: EnemyDefinitionId.KzanTop } top)
        {
            throw new InvalidDataException(
                $"Kzan bottom in slot {slot.SlotIndex} does not directly follow a Kzan top, as every retail Kzan population places it.");
        }
        _tops.Place(slot, top);
        FollowKzanTop(slot);
    }

    /// <summary>Keeps the lower actor twelve pixels below the top on every enemy update.</summary>
    internal void RunKzanBottomMain(RoomEnemySlot slot) => FollowKzanTop(slot);

    /// <summary>
    /// Follows the top the bottom found when it was placed, the enemy its room lists directly before it, whatever that
    /// place holds now.
    /// </summary>
    private void FollowKzanTop(RoomEnemySlot bottom)
    {
        RoomEnemySlot top = _tops.Of(bottom);
        bottom.Position = bottom.Position.WithWhole(new(top.Position.X.Whole, CoordinateAxis.Wrap(top.Position.Y.Whole + 12)));
    }

    /// <summary>Dispatches the top actor through fall wait, accelerated descent, rise wait, and half-pixel ascent.</summary>
    private void RunKzanTopMain(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        switch (state.Action)
        {
            case KzanAction.WaitingToFall:
                RunKzanWaitingToFall(slot, state);
                return;

            case KzanAction.Falling:
                RunKzanFalling(slot, state, samus);
                return;

            case KzanAction.WaitingToRise:
                RunKzanWaitingToRise(state);
                return;

            case KzanAction.Rising:
                RunKzanRising(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Kzan has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Decrements the wrapping top wait and resets the fall speed when descent begins.</summary>
    private static void RunKzanWaitingToFall(RoomEnemySlot slot, KzanEnemyState state)
    {
        state.FallWaitTimer = state.FallWaitTimer.Decrement().Remaining;
        if (!state.FallWaitTimer.IsZero)
            return;

        state.FallWaitTimer = WrappingCountdown.Start(state.FallWaitTimerResetValue);
        state.FallingSpeed = slot.SpawnSettings<KzanSpawnSettings>().FallSpeed;
        state.Action = KzanAction.Falling;
    }

    /// <summary>Applies the downward speed, clamps to the target using signed wrapped comparison, carries Samus, and accelerates the fall.</summary>
    private void RunKzanFalling(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousPosition = slot.Position.Whole;
        slot.Position = slot.Position.AdvanceY(state.FallingSpeed);

        // The comparison checks only whether the wrapped difference is negative, which the spatial difference
        // keeps: coordinates near the room's edge retain the same game behavior.
        if ((slot.Position.Whole - state.FallingTarget).Y >= 0)
        {
            state.RiseWaitTimer = KzanNtscRiseWaitFrames;
            state.Action = KzanAction.WaitingToRise;
            slot.Position = slot.Position.WithWholeY(state.FallingTarget.Y);
            _audio.QueueEnemySound(KzanLandingSound, priority: SoundRequestPriority.Normal);
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);

        // The game caps the speed by whether its difference from the cap is negative; both stay far
        // inside the signed range, so that is the ordinary minimum.
        SignedSixteenSixteen accelerated = state.FallingSpeed + KzanFallAcceleration;
        state.FallingSpeed = (accelerated - KzanMaximumFallingSpeed).IsNegative ? accelerated : KzanMaximumFallingSpeed;
    }

    /// <summary>Counts down the 64-frame NTSC landing hold before switching to ascent.</summary>
    private static void RunKzanWaitingToRise(KzanEnemyState state)
    {
        if (--state.RiseWaitTimer == 0)
            state.Action = KzanAction.Rising;
    }

    /// <summary>Subtracts one-half pixel per update, stops at the spawn height, and carries Samus by the whole-pixel step.</summary>
    private static void RunKzanRising(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        state.PreviousPosition = slot.Position.Whole;

        // The NTSC build rises by exactly half a pixel per actor frame, the fraction's borrow reaching the whole pixels.
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(KzanRisingStep));
        if ((slot.Position.Whole - state.RisingTarget).Y < 0)
        {
            state.Action = KzanAction.WaitingToFall;
            slot.Position = slot.Position.WithWholeY(state.RisingTarget.Y);
        }

        CarrySamusWithKzanIfTouchingFromBelow(slot, state, samus);
    }

    /// <summary>
    /// Kzan's private copy of the check whether an enemy touches Samus from below. Its only
    /// difference from the common helper is the five-pixel shifted Y sample. Horizontal
    /// overlap is strict; vertical tangency is accepted after the shifted center test.
    /// </summary>
    private static bool IsKzanTouchingSamusFromBelow(
        RoomEnemySlot slot,
        SamusState samus)
    {
        int xDistance = CoordinateAxis.Distance(samus.Position.X.Whole, slot.Position.X.Whole);
        if (xDistance >= samus.Kinematics.Radius.X + slot.Radius.X)
            return false;

        int shiftedYDelta = CoordinateAxis.AsSigned(
            CoordinateAxis.Wrap(samus.Position.Y.Whole + 5 - slot.Position.Y.Whole));
        if (shiftedYDelta >= 0)
            return false;

        int yDistance = Math.Abs(shiftedYDelta);
        if (yDistance < samus.Kinematics.Radius.Y)
            return true;

        return yDistance - samus.Kinematics.Radius.Y <= slot.Radius.Y;
    }

    /// <summary>
    /// Adds only the accepted whole-pixel Kzan delta to Samus's extra Y displacement.
    /// Code deliberately leaves extra Y subdisplacement untouched.
    /// </summary>
    private static void CarrySamusWithKzanIfTouchingFromBelow(
        RoomEnemySlot slot,
        KzanEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !IsKzanTouchingSamusFromBelow(slot, samus))
            return;

        samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, (slot.Position.Whole - state.PreviousPosition).Y);
    }

    /// <summary>Advances Kzan's typed animation.</summary>
    internal static void AdvanceKzanTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, KzanAnimations.Segments,
            static (KzanEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Kzan animation raised {cue}."));

    /// <summary>Runs KzanTop's main AI.</summary>
    internal void RunKzanTopMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunKzanTopMain(slot, slot.RequireSpeciesState<KzanEnemyState>(), frame.Samus);
    }
}
