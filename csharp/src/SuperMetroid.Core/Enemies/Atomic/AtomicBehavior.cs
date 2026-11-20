using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AtomicBehavior : IEnemySpecies, IEnemySettingsFactory<AtomicSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, AtomicSpawnSettings settings) => new AtomicEntity(occupant, place, common, settings);


    /// <summary>Initializes the Atomic.</summary>
    internal static AtomicEnemyState InitializeAtomic(RoomEnemyPlace place, AtomicSpawnSettings settings)
    {
        var state = new AtomicEnemyState(settings);

        place.AnimationClock.Restart();

        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>
    /// Moves the Atomic one step toward Samus on each axis at its spawn settings' speed. It ignores terrain and chooses both
    /// directions again every update.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemyPlace place,
        AtomicEnemyState state,
        SamusState samus)
    {
        // The Atomic moves towards Samus on each axis; being level with her counts as her being below or to the right, so
        // it moves down or right.
        SignedSixteenSixteen speed = state.Settings.Speed;
        SignedSixteenSixteen vertical = speed.Toward(samus.Position.Y.Whole < place.Position.Y.Whole ? VerticalDirection.Up : VerticalDirection.Down);
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(vertical));

        SignedSixteenSixteen horizontal = speed.Toward(samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right);
        place.Position = place.Position.WithX(place.Position.X.MovedBy(horizontal));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemyPlace place, AtomicEnemyState atomicEnemyState, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, atomicEnemyState, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Atomic animation raised {cue}."));

    /// <summary>Runs Atomic's main AI.</summary>
    internal static void RunAtomicMainAi(RoomEnemyPlace place, AtomicEnemyState state, IEnemyAiFrame frame)
    {
        RunAtomicMain(place, state, frame.Samus);
    }
}
