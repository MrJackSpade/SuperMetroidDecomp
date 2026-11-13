using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>The escape animals species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class EscapeAnimalsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. EscapeAnimalsPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => EscapeAnimalsPartDefinitions.Parts[slot.DefinitionId] switch
    {
        EscapeAnimalsPart.EtecoonEscape => new EscapeEtecoonEntity(this, slot, common),
        EscapeAnimalsPart.DachoraEscape => new EscapeDachoraEntity(this, slot, common),
        _ => throw new InvalidOperationException($"Escape animals do not own {slot.DefinitionId}."),
    };

    /// <summary>Provides room events and live Samus state used by both escape-animal branches.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Runs Etecoon's slope-ignoring horizontal move and settling ground probe.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds escape-animal AI to room events and terrain collision for the current enemy system.</summary>
    internal EscapeAnimalsBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    /// <summary>Whole-pixel horizontal step used by each Dachora movement-animation cue.</summary>
    private const int EscapeDachoraPixelsPerCallback = 6;
    /// <summary>Lava Y threshold tested by both families' acid-height branch cues.</summary>
    private const int EscapeAnimalLavaBranchY = 206;
    /// <summary>One unit of 16.16 downward acceleration applied during Etecoon's exit.</summary>
    private static readonly SignedSixteenSixteen EscapeEtecoonGravity = SignedSixteenSixteen.FromWholePixels(1);
    /// <summary>Signed 16.16 horizontal speed, 3.5 pixels, used when Etecoon exits without a wall collision.</summary>
    private static readonly SignedSixteenSixteen EscapeEtecoonUncollidedExitSpeed = SignedSixteenSixteen.FromParts(3, 32768);

    /// <summary>Initializes the escape Etecoon.</summary>
    internal void InitializeEscapeEtecoon(RoomEnemySlot slot)
    {
        var state = new EscapeEtecoonEnemyState();
        slot.SetSpeciesState(state);

        // The critters-escaped event means the animals already escaped on an earlier visit. Code
        // deletes the actor before changing any animation, position, colours, or AI fields.
        if (HasCrittersEscaped())
        {
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // An escaping animal runs its animation, is left out of the ordinary touch pass against Samus and
        // stands solid to her.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus | EnemyContact.SolidToSamus);
        slot.AnimationClock.Restart();
        // The initializer selects the common sprite colours, which the import colours the escape Etecoons' sheets in (#1512).

        EscapeEtecoonInitialization initialization =
            EscapeEtecoons.Initialization(slot.SpawnSettings<EscapeEtecoonSpawnSettings>().Role);
        slot.Position = slot.Position.WithWhole(initialization.Position);
        state.FrameBehavior = initialization.FrameBehavior;
        state.Playback = new(EscapeEtecoonAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation);
        state.Velocity = initialization.Velocity;
    }

    /// <summary>Initializes the escape Dachora.</summary>
    internal void InitializeEscapeDachora(RoomEnemySlot slot)
    {
        slot.SetSpeciesState(new EscapeDachoraEnemyState());
        if (HasCrittersEscaped())
        {
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // Dachora's population already supplies both properties, but the initializer sets the animating property again.
        // Expressing the idempotent write documents the initializer and keeps a
        // synthetic/debug population faithful to retail initialization.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();
        slot.RequireSpeciesState<EscapeDachoraEnemyState>().Playback =
            new(EscapeDachoraAnimations.StartingWith(EscapeDachoraSegment.RunningAroundLowTide), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs an escaping Etecoon's per-frame behaviour through its complete function dispatch.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        switch (state.FrameBehavior)
        {
            case EscapeEtecoonFrameBehavior.Cleared:
                return;

            case EscapeEtecoonFrameBehavior.EscapeRight:
                AddEscapeAnimalX(slot, EscapeEtecoonUncollidedExitSpeed);
                return;

            case EscapeEtecoonFrameBehavior.WaitForEscapeEvent:
                // The wait changes only the animation. It deliberately leaves the
                // existing frame hold intact, so the new animation begins on the same
                // update boundary the game observes.
                if (HasCrittersEscaped())
                    state.Playback = new(EscapeEtecoonAnimations.StartingWith(EscapeEtecoonSegment.ExpressGratitudeThenEscape), AnimationPlace.StartOfAnimation);
                return;

            case EscapeEtecoonFrameBehavior.WalkAndFall:
                RunEscapeEtecoonWalking(slot, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Escape Etecoon frame behaviour {state.FrameBehavior?.ToString() ?? "none"} " +
                    "is not translated.");
        }
    }

    /// <summary>Walks and falls, including the collision-only event branch.</summary>
    private void RunEscapeEtecoonWalking(
        RoomEnemySlot slot,
        EscapeEtecoonEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Escape Etecoon walking requires room collision data.");

        // The stored speed is signed 8.8; the slope-ignoring horizontal mover receives signed
        // 16.16, hence the exact scaling by 256. The event test is inside the collision
        // branch in the retail game and must not turn an animal around in open space.
        SignedSixteenSixteen horizontalDisplacement = state.Velocity.X.ToSixteenSixteenDelta();
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            slot.AnimationClock.StepOnNextUpdate();
            state.Velocity = state.Velocity.ReversedX();
            state.Playback = new(EscapeEtecoonAnimations.StartingWith(HasCrittersEscaped()
                ? EscapeEtecoonSegment.RunningForEscape
                : state.Velocity.X.IsNegative
                    ? EscapeEtecoonSegment.RunningLeftLowTide
                    : EscapeEtecoonSegment.RunningRightLowTide), AnimationPlace.StartOfAnimation);
        }

        // The vertical mover is called even after horizontal collision, and whether it hit terrain
        // is ignored. This one-pixel probe lets the common collision helper settle the actor on
        // the room's actual floor without introducing a bespoke ground model.
        _ = _terrain.MoveEnemyVertically(level, slot, EscapeEtecoonGravity);
    }

    /// <summary>Runs an escaping Etecoon's or Dachora's typed animation where the shared enemy animation would.</summary>
    private void AdvanceEscapeAnimal(RoomEnemySlot slot, SamusState? samus)
    {
        if (!slot.AnimationClock.Expire())
            return;
        if (slot.DefinitionId == EnemyDefinitionId.EtecoonEscape)
        {
            EscapeEtecoonEnemyState state = slot.RequireSpeciesState<EscapeEtecoonEnemyState>();
            AnimationPlayback<EscapeEtecoonSegment> playback = state.Playback;
            EnemyAnimationPlayback.Play(slot, EscapeEtecoonAnimations.Segments, ref playback,
                cue => ApplyEscapeEtecoonCue(slot, state, samus, cue) is { } target
                    ? new AnimationContinuation<EscapeEtecoonSegment>(EscapeEtecoonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                    : null);
            state.Playback = playback;
            return;
        }
        EscapeDachoraEnemyState dachora = slot.RequireSpeciesState<EscapeDachoraEnemyState>();
        AnimationPlayback<EscapeDachoraSegment> dachoraPlayback = dachora.Playback;
        EnemyAnimationPlayback.Play(slot, EscapeDachoraAnimations.Segments, ref dachoraPlayback,
            cue => ApplyEscapeDachoraCue(slot, samus, cue) is { } target ? EscapeDachoraAnimations.BranchTargets[target] : null);
        dachora.Playback = dachoraPlayback;
    }

    /// <summary>Carries out one cue of an Etecoon's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static EscapeEtecoonSegment? ApplyEscapeEtecoonCue(RoomEnemySlot slot, EscapeEtecoonEnemyState state, SamusState? samus, EscapeEtecoonCue cue)
    {
        switch (cue)
        {
            case EscapeEtecoonCue.SetFrameBehavior setFrameBehavior:
                state.FrameBehavior = setFrameBehavior.FrameBehavior;
                return null;
            case EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.ClearFrameBehavior }:
                state.FrameBehavior = EscapeEtecoonFrameBehavior.Cleared;
                return null;
            case EscapeEtecoonCue.GoToIfAcidHigh goToIfAcidHigh:
                return IsAcidLow(samus) ? null : goToIfAcidHigh.Target;
            case EscapeEtecoonCue.Move move:
                slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + move.Offset);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeEtecoonCueKind)} {cue}.");
        }
    }

    /// <summary>Carries out one cue of the Dachora's animation, returning the target a branch continues at, or null to run on.</summary>
    private EscapeDachoraSegment? ApplyEscapeDachoraCue(RoomEnemySlot slot, SamusState? samus, EscapeDachoraCue cue)
    {
        switch (cue.Kind)
        {
            case EscapeDachoraCueKind.GoToIfAcidHigh:
                return IsAcidLow(samus) ? null : cue.Target;
            case EscapeDachoraCueKind.GoToIfCrittersEscaped:
                return HasCrittersEscaped() ? cue.Target : null;
            case EscapeDachoraCueKind.MoveLeft:
                slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole - EscapeDachoraPixelsPerCallback);
                return null;
            case EscapeDachoraCueKind.MoveRight:
                slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole + EscapeDachoraPixelsPerCallback);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeDachoraCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Samples the critters-escaped event for Dachora's animation branch and room revisit handling.</summary>
    private bool HasCrittersEscaped() => _room.Services.Events.Has(StoryEvent.CrittersEscaped);

    /// <summary>
    /// Whether the acid-height branches find the acid low: at or below <see cref="EscapeAnimalLavaBranchY"/>, or absent,
    /// as it is with no Samus or liquid context.
    /// </summary>
    private static bool IsAcidLow(SamusState? samus) =>
        samus?.LiquidPhysics.LavaAcidSurfaceY is not int surface || surface >= EscapeAnimalLavaBranchY;

    /// <summary>Moves the actor horizontally by <paramref name="displacement"/>, wrapping within the coordinate range.</summary>
    private static void AddEscapeAnimalX(RoomEnemySlot slot, SignedSixteenSixteen displacement) =>
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(displacement));

    /// <summary>Runs the escape Etecoon's main AI.</summary>
    internal void RunEscapeEtecoonMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunEscapeEtecoonMain(slot, slot.RequireSpeciesState<EscapeEtecoonEnemyState>(), frame.Level);

    /// <summary>Advances either escape animal's typed animation.</summary>
    internal void AdvanceEscapeAnimalTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context) =>
        AdvanceEscapeAnimal(slot, context.Samus);

}
