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
/// <param name="reference">Kraid's reference to the part, which it joins as it initializes.</param>
internal abstract class KraidEntity(KraidBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyFrameHitboxes
{
    /// <summary>The Kraid species owner.</summary>
    protected KraidBehavior Species => species;

    /// <inheritdoc/>
    public void Initialize()
    {
        reference.Join(Occupant);
        SetUp();
    }

    /// <summary>Sets the part up, once it has joined Kraid.</summary>
    protected abstract void SetUp();

    /// <summary>The encounter the part is part of, which its state holds from its setup (#2218).</summary>
    /// <exception cref="InvalidOperationException">The part has not set up as one of his parts.</exception>
    protected KraidEnemyState Encounter => Place.RequireSpeciesState<KraidPartState>().Kraid;

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <summary>None: no part but the arm has a touch reaction its hitboxes name.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the arm's, foot's and body's definitions name the ignoring shot reaction.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>None: only his arm and foot have hitboxes of their own, which their entities find (#2301).</summary>
    public virtual bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return null;
    }
}
