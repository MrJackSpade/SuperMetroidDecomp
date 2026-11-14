using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class AtomicBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new AtomicEntity(slot, common);


    /// <summary>Initializes the Atomic.</summary>
    internal static void InitializeAtomic(RoomEnemySlot slot)
    {
        var state = new AtomicEnemyState();
        slot.SetSpeciesState(state);

        slot.AnimationClock.Restart();

        AtomicSpawnSettings settings = slot.SpawnSettings<AtomicSpawnSettings>();
        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);

        state.Speed = settings.Speed;
    }

    /// <summary>
    /// Moves the Atomic one step toward Samus on each axis. It ignores terrain and chooses both directions again every
    /// update.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemySlot slot,
        AtomicEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Atomic movement requires the active Samus actor.");

        // Being level with Samus counts as her being below or to the right, so it moves down or right. The comparison is
        // by whether the coordinate difference across the room's wrap is negative, so an Atomic and Samus on either side
        // of the wrap still chase the short way.
        SignedSixteenSixteen vertical = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole) < 0
            ? -state.Speed
            : state.Speed;
        slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(vertical));

        SignedSixteenSixteen horizontal = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
            ? -state.Speed
            : state.Speed;
        slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(horizontal));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Atomic animation raised {cue}."));

    /// <summary>Runs Atomic's main AI.</summary>
    internal static void RunAtomicMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunAtomicMain(slot, slot.RequireSpeciesState<AtomicEnemyState>(), frame.Samus);
    }
}
