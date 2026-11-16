using SuperMetroid.Core.Animation;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EscapeAnimalsPartDefinitions.Parts[place.DefinitionId] switch
    {
        EscapeAnimalsPart.EtecoonEscape => new EscapeEtecoonEntity(this, place, common),
        EscapeAnimalsPart.DachoraEscape => new EscapeDachoraEntity(this, place, common),
        _ => throw new InvalidOperationException($"Escape animals do not own {place.DefinitionId}."),
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
    private const int EscapeDachoraPixelsPerCue = 6;
    /// <summary>Lava Y threshold tested by both families' acid-height branch cues.</summary>
    private const int EscapeAnimalLavaBranchY = 206;
    /// <summary>The one whole pixel an exiting Etecoon probes downward each update, which settles it on the room's floor.</summary>
    private static readonly SignedSixteenSixteen EscapeEtecoonGravity = SignedSixteenSixteen.FromWholePixels(1);
    /// <summary>Signed 1/65536-pixel horizontal speed, 3.5 pixels, used when Etecoon exits without a wall collision.</summary>
    private static readonly SignedSixteenSixteen EscapeEtecoonUncollidedExitSpeed = SignedSixteenSixteen.FromParts(3, 32768);

    /// <summary>Initializes the escape Etecoon.</summary>
    internal void InitializeEscapeEtecoon(RoomEnemyPlace place)
    {
        EscapeEtecoonInitialization initialization =
            EscapeEtecoons.Initialization(place.SpawnSettings<EscapeEtecoonSpawnSettings>().Role);
        var state = new EscapeEtecoonEnemyState(initialization.Action);
        place.SetSpeciesState(state);

        // The critters-escaped event means the animals already escaped on an earlier visit. Code
        // deletes the actor before changing any animation, position, colours, or AI fields.
        if (HasCrittersEscaped())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // An escaping animal runs its animation, is left out of the ordinary touch pass against Samus and
        // stands solid to her.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus | EnemyContact.SolidToSamus);
        place.AnimationClock.Restart();
        // The initializer selects the common sprite colours, which the import colours the escape Etecoons' sheets in (#1512).

        place.Position = place.Position.WithWhole(initialization.Position);
        state.Playback = new(EscapeEtecoonAnimations.StartingWith(initialization.Animation), AnimationPlace.StartOfAnimation);
        state.Velocity = initialization.Velocity;
    }

    /// <summary>Initializes the escape Dachora.</summary>
    internal void InitializeEscapeDachora(RoomEnemyPlace place)
    {
        place.SetSpeciesState(new EscapeDachoraEnemyState());
        if (HasCrittersEscaped())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        // Dachora's population already supplies both properties, but the initializer sets the animating property again.
        // Expressing the idempotent write documents the initializer and keeps a
        // synthetic/debug population faithful to retail initialization.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
        place.RequireSpeciesState<EscapeDachoraEnemyState>().Playback =
            new(EscapeDachoraAnimations.StartingWith(EscapeDachoraSegment.RunningAroundLowTide), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs an escaping Etecoon's action.</summary>
    private void RunEscapeEtecoonMain(
        RoomEnemyPlace place,
        EscapeEtecoonEnemyState state,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case EscapeEtecoonAction.ExpressingGratitude:
                return;

            case EscapeEtecoonAction.Escaping:
                AddEscapeAnimalX(place, EscapeEtecoonUncollidedExitSpeed);
                return;

            case EscapeEtecoonAction.AwaitingEscape:
                // The wait changes only the animation. It deliberately leaves the
                // existing frame hold intact, so the new animation begins on the same
                // update boundary the game observes.
                if (HasCrittersEscaped())
                    state.Playback = new(EscapeEtecoonAnimations.StartingWith(EscapeEtecoonSegment.ExpressGratitudeThenEscape), AnimationPlace.StartOfAnimation);
                return;

            case EscapeEtecoonAction.Walking:
                RunEscapeEtecoonWalking(place, state, level);
                return;

            default:
                throw new InvalidDataException($"Escape Etecoon has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Walks and falls, including the collision-only event branch.</summary>
    private void RunEscapeEtecoonWalking(
        RoomEnemyPlace place,
        EscapeEtecoonEnemyState state,
        RoomLevelData level)
    {
        // The stored speed is signed and in 1/256 pixels; the slope-ignoring horizontal mover receives signed
        // 1/65536-pixel speed, hence the exact scaling by 256. The event test is inside the collision
        // branch in the retail game and must not turn an animal around in open space.
        SignedSixteenSixteen horizontalDisplacement = state.Velocity.ToSixteenSixteenDelta();
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, place, horizontalDisplacement))
        {
            place.AnimationClock.StepOnNextUpdate();
            state.Velocity = state.Velocity.Negated();
            state.Playback = new(EscapeEtecoonAnimations.StartingWith(HasCrittersEscaped()
                ? EscapeEtecoonSegment.RunningForEscape
                : state.Velocity.IsNegative
                    ? EscapeEtecoonSegment.RunningLeftLowTide
                    : EscapeEtecoonSegment.RunningRightLowTide), AnimationPlace.StartOfAnimation);
        }

        // The vertical mover is called even after horizontal collision, and whether it hit terrain
        // is ignored. This one-pixel probe lets the common collision helper settle the actor on
        // the room's actual floor without introducing a bespoke ground model.
        _ = _terrain.MoveEnemyVertically(level, place, EscapeEtecoonGravity);
    }

    /// <summary>Runs an escaping Etecoon's or Dachora's typed animation where the shared enemy animation would.</summary>
    private void AdvanceEscapeAnimal(RoomEnemyPlace place, SamusState samus)
    {
        if (!place.AnimationClock.Expire())
            return;
        if (place.DefinitionId == EnemyDefinitionId.EtecoonEscape)
        {
            EscapeEtecoonEnemyState state = place.RequireSpeciesState<EscapeEtecoonEnemyState>();
            AnimationPlayback<EscapeEtecoonSegment> playback = state.Playback;
            EnemyAnimationPlayback.Play(place, EscapeEtecoonAnimations.Segments, ref playback,
                cue => ApplyEscapeEtecoonCue(place, state, samus, cue) is { } target
                    ? new AnimationContinuation<EscapeEtecoonSegment>(EscapeEtecoonAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                    : null);
            state.Playback = playback;
            return;
        }
        EscapeDachoraEnemyState dachora = place.RequireSpeciesState<EscapeDachoraEnemyState>();
        AnimationPlayback<EscapeDachoraSegment> dachoraPlayback = dachora.Playback;
        EnemyAnimationPlayback.Play(place, EscapeDachoraAnimations.Segments, ref dachoraPlayback,
            cue => ApplyEscapeDachoraCue(place, samus, cue) is { } target ? EscapeDachoraAnimations.BranchTargets[target] : null);
        dachora.Playback = dachoraPlayback;
    }

    /// <summary>Carries out one cue of an Etecoon's animation, returning the segment a branch continues at, or null to run on.</summary>
    private static EscapeEtecoonSegment? ApplyEscapeEtecoonCue(RoomEnemyPlace place, EscapeEtecoonEnemyState state, SamusState samus, EscapeEtecoonCue cue)
    {
        switch (cue)
        {
            case EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.Escapes }:
                state.Action = EscapeEtecoonAction.Escaping;
                return null;
            case EscapeEtecoonCue.Signal { Kind: EscapeEtecoonCueKind.ExpressesGratitude }:
                state.Action = EscapeEtecoonAction.ExpressingGratitude;
                return null;
            case EscapeEtecoonCue.GoToIfAcidHigh goToIfAcidHigh:
                return IsAcidLow(samus) ? null : goToIfAcidHigh.Target;
            case EscapeEtecoonCue.Move move:
                place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + move.Offset);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EscapeEtecoonCueKind)} {cue}.");
        }
    }

    /// <summary>Carries out one cue of the Dachora's animation, returning the target a branch continues at, or null to run on.</summary>
    private EscapeDachoraSegment? ApplyEscapeDachoraCue(RoomEnemyPlace place, SamusState samus, EscapeDachoraCue cue)
    {
        switch (cue.Kind)
        {
            case EscapeDachoraCueKind.GoToIfAcidHigh:
                return IsAcidLow(samus) ? null : cue.Target;
            case EscapeDachoraCueKind.GoToIfCrittersEscaped:
                return HasCrittersEscaped() ? cue.Target : null;
            case EscapeDachoraCueKind.MoveLeft:
                place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole - EscapeDachoraPixelsPerCue);
                return null;
            case EscapeDachoraCueKind.MoveRight:
                place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole + EscapeDachoraPixelsPerCue);
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
    private static bool IsAcidLow(SamusState samus) =>
        samus.LiquidPhysics.LavaAcidSurfaceY is not int surface || surface >= EscapeAnimalLavaBranchY;

    /// <summary>Moves the actor horizontally by <paramref name="displacement"/>, wrapping within the coordinate range.</summary>
    private static void AddEscapeAnimalX(RoomEnemyPlace place, SignedSixteenSixteen displacement) =>
        place.Position = place.Position.WithX(place.Position.X.MovedBy(displacement));

    /// <summary>Runs the escape Etecoon's main AI.</summary>
    internal void RunEscapeEtecoonMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunEscapeEtecoonMain(place, place.RequireSpeciesState<EscapeEtecoonEnemyState>(), frame.Level);

    /// <summary>Advances either escape animal's typed animation.</summary>
    internal void AdvanceEscapeAnimalTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context) =>
        AdvanceEscapeAnimal(place, context.Samus);

}
