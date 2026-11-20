using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's eight parts: his body (<see cref="KraidBodyEntity"/>), his arm or foot (<see cref="KraidLimbEntity"/>),
/// a lint (<see cref="KraidLintEntity"/>) or a fingernail (<see cref="KraidNailEntity"/>). Each joins its own reference
/// before it sets up, so his parts reach one another (#1877), and answers his hitboxes.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the part, which it joins as it is set up.</param>
/// <param name="setUp">The part's setup once it has joined Kraid, which returns the encounter it joined.</param>
internal abstract class KraidEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    KraidPartReference reference, Func<KraidEnemyState> setUp)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyFrameHitboxes
{
    /// <summary>The Kraid species owner.</summary>
    protected KraidBehavior Species => species;

    /// <summary>The encounter the part is part of, which it holds from its setup (#2218, #2554).</summary>
    internal KraidEnemyState Encounter { get; } = Join(reference, occupant, setUp);

    /// <summary>Joins the part's reference, then sets the part up.</summary>
    /// <returns>The encounter the part joined.</returns>
    private static KraidEnemyState Join(KraidPartReference reference, RoomEnemyOccupant occupant, Func<KraidEnemyState> setUp)
    {
        Ensure.NotNull(reference).Join(occupant);
        return Ensure.NotNull(setUp)();
    }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <summary>None: only his arm and foot have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
