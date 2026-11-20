using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's eight parts: his body (<see cref="KraidBodyEntity"/>), his arm or foot (<see cref="KraidLimbEntity"/>),
/// a lint (<see cref="KraidLintEntity"/>) or a fingernail (<see cref="KraidNailEntity"/>). Each joins its own reference
/// before it sets up, so his parts reach one another (#1877), and answers his hitboxes.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class KraidEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyFrameHitboxes
{
    /// <summary>The Kraid species owner.</summary>
    protected KraidBehavior Species => species;

    /// <summary>The encounter the part is part of, which it joins as it is built (#2218, #2554).</summary>
    internal abstract KraidEnemyState Encounter { get; }

    /// <summary>The part's own state in the encounter, which his other parts find in its place.</summary>
    internal abstract KraidPartState PartState { get; }

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <summary>None: no part but the arm has a touch reaction its hitboxes name.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the arm's, foot's and body's definitions name the ignoring shot reaction.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>None: only his arm and foot have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
