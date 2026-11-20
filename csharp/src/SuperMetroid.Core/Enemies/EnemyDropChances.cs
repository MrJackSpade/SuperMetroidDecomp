namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy's item-drop probabilities, each out of the full weight budget (<see cref="EnemyDropSelectionDefinitions.FullWeightBudget"/>); an authored table's chances sum to it.</summary>
/// <param name="SmallEnergy">The chance of a small energy drop.</param>
/// <param name="BigEnergy">The chance of a big energy drop.</param>
/// <param name="Missile">The chance of a missile drop.</param>
/// <param name="Nothing">The chance of no drop.</param>
/// <param name="SuperMissile">The chance of a Super Missile drop.</param>
/// <param name="PowerBomb">The chance of a Power Bomb drop.</param>
public readonly record struct EnemyDropChances(
    int SmallEnergy,
    int BigEnergy,
    int Missile,
    int Nothing,
    int SuperMissile,
    int PowerBomb)
{
    /// <summary>The chances together, the full weight budget for every authored table.</summary>
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
