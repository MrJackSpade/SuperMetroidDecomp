namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy's item-drop probabilities, each out of the full weight budget (<see cref="EnemyDropSelectionDefinitions.FullWeightBudget"/>);
/// an authored table's chances sum to it, and no table's sum past it, which the table requires as it is built.
/// </summary>
public readonly partial record struct EnemyDropChances
{
    /// <summary>Builds a table of chances that sum to no more than the full weight budget.</summary>
    /// <param name="smallEnergy">The chance of a small energy drop.</param>
    /// <param name="bigEnergy">The chance of a big energy drop.</param>
    /// <param name="missile">The chance of a missile drop.</param>
    /// <param name="nothing">The chance of no drop.</param>
    /// <param name="superMissile">The chance of a Super Missile drop.</param>
    /// <param name="powerBomb">The chance of a Power Bomb drop.</param>
    /// <exception cref="ArgumentException">The chances sum past the full weight budget.</exception>
    public EnemyDropChances(int smallEnergy, int bigEnergy, int missile, int nothing, int superMissile, int powerBomb)
    {
        SmallEnergy = smallEnergy;
        BigEnergy = bigEnergy;
        Missile = missile;
        Nothing = nothing;
        SuperMissile = superMissile;
        PowerBomb = powerBomb;
        if (Total > EnemyDropSelectionDefinitions.FullWeightBudget)
            throw new ArgumentException($"A drop table's chances sum to {Total}, past the {EnemyDropSelectionDefinitions.FullWeightBudget} every chance is out of.");
    }

    /// <summary>The chance of a small energy drop.</summary>
    public int SmallEnergy { get; }

    /// <summary>The chance of a big energy drop.</summary>
    public int BigEnergy { get; }

    /// <summary>The chance of a missile drop.</summary>
    public int Missile { get; }

    /// <summary>The chance of no drop.</summary>
    public int Nothing { get; }

    /// <summary>The chance of a Super Missile drop.</summary>
    public int SuperMissile { get; }

    /// <summary>The chance of a Power Bomb drop.</summary>
    public int PowerBomb { get; }

    /// <summary>The chances together, the full weight budget for every authored table and never past it.</summary>
    internal int Total => SmallEnergy + BigEnergy + Missile + Nothing + SuperMissile + PowerBomb;

    /// <summary>The chance of the roll's <paramref name="outcome"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="outcome"/> is not a drop outcome.</exception>
    internal int Of(EnemyDropRoll outcome) => outcome switch
    {
        EnemyDropRoll.NothingDrops => Nothing,
        EnemyDropRoll.PickupDrops { Kind: EnemyPickupKind.SmallEnergy } => SmallEnergy,
        EnemyDropRoll.PickupDrops { Kind: EnemyPickupKind.BigEnergy } => BigEnergy,
        EnemyDropRoll.PickupDrops { Kind: EnemyPickupKind.Missile } => Missile,
        EnemyDropRoll.PickupDrops { Kind: EnemyPickupKind.SuperMissile } => SuperMissile,
        EnemyDropRoll.PickupDrops { Kind: EnemyPickupKind.PowerBomb } => PowerBomb,
        _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, "Undefined enemy drop outcome."),
    };
}
