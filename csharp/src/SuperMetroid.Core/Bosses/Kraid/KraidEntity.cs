using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's eight parts: his body (<see cref="KraidBodyEntity"/>), his arm or foot (<see cref="KraidLimbEntity"/>),
/// a lint (<see cref="KraidLintEntity"/>) or a fingernail (<see cref="KraidNailEntity"/>). Each holds the encounter it joins
/// from its setup, where his body found every part's place (#1877, #2554), and answers his hitboxes.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="encounter">The encounter the part joined, which its setup returned as the part was built.</param>
internal abstract partial class KraidEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    KraidEnemyState encounter)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyFrameHitboxes
{
    /// <summary>The Kraid species owner.</summary>
    protected KraidBehavior Species => species;

    /// <summary>The encounter the part is part of, which it holds from its setup (#2218, #2554).</summary>
    internal KraidEnemyState Encounter { get; } = Ensure.NotNull(encounter);


    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <summary>None: only his arm and foot have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
