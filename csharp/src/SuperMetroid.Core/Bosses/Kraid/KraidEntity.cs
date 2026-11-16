using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// One of Kraid's eight parts: his body (<see cref="KraidBodyEntity"/>), his arm or foot (<see cref="KraidLimbEntity"/>),
/// a lint (<see cref="KraidLintEntity"/>) or a fingernail (<see cref="KraidNailEntity"/>). Each joins its own reference
/// before it sets up, so his parts reach one another (#1877), and answers his engine-owned hitboxes.
/// </summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="place">The place the part occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="reference">Kraid's reference to the part, which it joins as it initializes.</param>
internal abstract class KraidEntity(KraidBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, KraidPartReference reference)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyFrameHitboxes
{
    /// <summary>The Kraid species owner.</summary>
    protected KraidBehavior Species => species;

    /// <inheritdoc/>
    public void Initialize()
    {
        reference.Join(Place);
        SetUp();
    }

    /// <summary>Sets the part up, once it has joined Kraid.</summary>
    protected abstract void SetUp();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <summary>None: no part but the arm has a touch reaction its hitboxes name.</summary>
    public virtual EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the arm's, foot's and body's definitions name the ignoring shot reaction.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return KraidCombat.FindKraidFootHitboxReaction(Place, probe, out reaction) ??
            KraidCombat.FindKraidArmHitboxReaction(Place, probe, out reaction);
    }
}
