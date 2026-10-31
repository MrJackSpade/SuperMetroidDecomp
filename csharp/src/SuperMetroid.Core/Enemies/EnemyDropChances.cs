namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy's item-drop probabilities, each out of 255; an authored table's six chances sum to 255.</summary>
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
        EnemyPickupKind.None => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No pickup is not a drop."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined enemy pickup kind."),
    };
}
