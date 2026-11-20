namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An enemy's item-drop probabilities, each out of the full weight budget (<see cref="EnemyDropSelectionDefinitions.FullWeightBudget"/>);
/// an authored table's chances sum to it, and no table's sum past it, which the table requires as it is built.
/// </summary>
public readonly record struct EnemyDropChances
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

    /// <summary>The chance of the drop <paramref name="kind"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> is not a drop.</exception>
    internal int Of(EnemyPickupKind kind) => kind switch
    {
        EnemyPickupKind.SmallEnergy => SmallEnergy,
        EnemyPickupKind.BigEnergy => BigEnergy,
        EnemyPickupKind.Missile => Missile,
        EnemyPickupKind.NoDrop => Nothing,
        EnemyPickupKind.SuperMissile => SuperMissile,
        EnemyPickupKind.PowerBomb => PowerBomb,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy pickup kind."),
    };
}
