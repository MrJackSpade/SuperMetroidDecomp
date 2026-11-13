using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The Zoa species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class ZoaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zoa];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ZoaEntity(slot, common);

    /// <summary>Advances Zoa's typed animation (#641) in place of the shared enemy animation.</summary>
    internal static void AdvanceZoaAnimation(RoomEnemySlot slot)
    {
        if (!slot.AnimationClock.Expire())
            return;
        ZoaEnemyState state = slot.RequireSpeciesState<ZoaEnemyState>();
        AnimationPlayback<ZoaSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, ZoaAnimations.Segments, ref playback,
            cue =>
            {
                ApplyZoaCue(state, cue);
                return null;
            });
        state.Playback = playback;
    }

    /// <summary>Carries out one cue of Zoa's animation: each selects the horizontal shot speed.</summary>
    private static void ApplyZoaCue(ZoaEnemyState state, ZoaCue cue)
    {
        state.ShotSpeedStage = cue.Kind switch
        {
            ZoaCueKind.SetSpeedStage1 => 1,
            ZoaCueKind.SetSpeedStage2 => 2,
            ZoaCueKind.SetSpeedStage3 => 3,
            _ => throw new InvalidOperationException($"Undefined {nameof(ZoaCueKind)} {cue.Kind}."),
        };
    }

    /// <summary>Samus must be within 128 horizontal pixels before a hidden Zoa rises.</summary>
    private const int ZoaActivationColumnDistance = 128;
    /// <summary>The sixteenths of a pixel in an eighth of a pixel.</summary>
    private const int SixteenthsPerEighth = 2;
    /// <summary>NTSC upward travel of half a pixel per gameplay frame.</summary>
    private static readonly SignedSixteenSixteen ZoaRisingSpeed = SignedSixteenSixteen.FromParts(0, 32768);

    /// <summary>Initializes the Zoa, saving the population coordinate and hiding the idle actor.</summary>
    internal static void InitializeZoa(RoomEnemySlot slot)
    {
        var state = new ZoaEnemyState
        {
            Function = ZoaEnemyFunction.WaitForSamus,
            RequestedAnimation = ZoaAnimationSelector.ShootingFacingLeft,
            InstalledAnimation = ZoaAnimationSelector.ShootingFacingLeft,
            ShotSpeedStage = 0,
            SpawnPosition = slot.Position.Whole,
        };
        slot.SetSpeciesState(state);

        // The initializer starts the left-facing shooting animation directly and records it as started, so asking for it
        // again at rest starts nothing.
        state.Playback = new(ZoaAnimations.StartingWith(ZoaAnimationChoices.Animation(ZoaAnimationSelector.ShootingFacingLeft)), AnimationPlace.StartOfAnimation);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
    }

    /// <summary>Runs a Zoa's per-frame behaviour, dispatching proximity wait, rise, or horizontal shot from the room state.</summary>
    private static void RunZoaMain(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (samus is null)
            throw new InvalidOperationException("Zoa AI requires the active Samus actor.");

        switch (state.Function)
        {
            case ZoaEnemyFunction.WaitForSamus:
                RunZoaWait(slot, state, samus);
                return;
            case ZoaEnemyFunction.Rising:
                RunZoaRising(slot, state, samus);
                return;
            case ZoaEnemyFunction.Shooting:
                RunZoaShooting(slot, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Zoa function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Wakes when Samus enters the 128-pixel column and selects a rising pose from the wrapped horizontal direction.</summary>
    internal static void RunZoaWait(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        int signedDistance = CoordinateAxis.Wrap(samus.Position.X.Whole - slot.Position.X.Whole);
        int absoluteDistance = CoordinateAxis.AsSigned(signedDistance) < 0
            ? CoordinateAxis.Wrap(-signedDistance)
            : signedDistance;
        if (absoluteDistance >= ZoaActivationColumnDistance)
            return;

        // The Zoa rises facing Samus; the sign test uses the same wrapped Samus-minus-enemy distance as the common helper.
        state.RequestedAnimation = ZoaAnimationChoices.Rising(facingRight: CoordinateAxis.AsSigned(signedDistance) >= 0);
        SetZoaAnimation(slot, state);
        state.Function = ZoaEnemyFunction.Rising;
    }

    /// <summary>Reveals Zoa and rises until it reaches Samus's height, then starts its shooting animation.</summary>
    internal static void RunZoaRising(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        if (CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole) < 0)
        {
            // NTSC subtracts exactly 0.5 px/frame. The PAL regional constant is 0.625;
            // this project targets the Japan/USA retail release and reads NTSC data.
            slot.Position = slot.Position.AdvanceY(-ZoaRisingSpeed);
            return;
        }

        state.RequestedAnimation = ZoaAnimationChoices.ShootingWithSameFacing(state.RequestedAnimation);
        SetZoaAnimation(slot, state);
        state.Function = ZoaEnemyFunction.Shooting;
    }

    /// <summary>Travels in the selected facing direction and resets whole-pixel position when its center exits the camera square.</summary>
    internal static void RunZoaShooting(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        Position16<RoomPixels> camera)
    {
        SignedSixteenSixteen speed = SignedSixteenSixteen.FromSixteenthsOfPixel(
            ZoaSpeedDefinitions.StageSpeedEighths[state.ShotSpeedStage] * SixteenthsPerEighth);

        // Shooting facing left travels left; any other animation travels right. The shooting animation's cues change the
        // speed at its 64-, 8- and 48-update marks.
        SignedSixteenSixteen xDisplacement = state.RequestedAnimation == ZoaAnimationSelector.ShootingFacingLeft
            ? -speed
            : speed;
        slot.Position = slot.Position.AdvanceX(xDisplacement);
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
        {
            SetZoaAnimation(slot, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        // The return stores only the whole pixels: the subpixel fractions left by the
        // rise and the shot carry into the next flight.
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.RequestedAnimation = ZoaAnimationSelector.ShootingFacingLeft;
        SetZoaAnimation(slot, state);
        state.Function = ZoaEnemyFunction.WaitForSamus;
    }

    /// <summary>Starts the selected animation, restarting its timers, only when it differs from the previous one.</summary>
    internal static void SetZoaAnimation(RoomEnemySlot slot, ZoaEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;
        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = EnemyAnimationPlayback.Start(slot, ZoaAnimations.StartingWith(ZoaAnimationChoices.Animation(state.RequestedAnimation)));
    }

    /// <summary>Runs Zoa's main AI.</summary>
    internal static void RunZoaMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunZoaMain(slot, slot.RequireSpeciesState<ZoaEnemyState>(), frame.Samus, frame.Camera);
    }
}
