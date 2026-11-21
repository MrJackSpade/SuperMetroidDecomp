namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How much of a weapon's damage a hit deals an enemy (#2555): none, half, full or double. A hit deals half the weapon's
/// damage, rounded down, once for each half of the factor, so an odd weapon damage loses its odd point before the factor
/// scales it.
/// </summary>
public readonly record struct WeaponDamageFactor
{
    /// <summary>The halves of a weapon's damage a full-damage hit deals.</summary>
    private const int HalvesPerWholeDamage = 2;

    /// <summary>Keeps the factor as the halves of the weapon's damage a hit deals, zero or more.</summary>
    internal WeaponDamageFactor(int halves) => _halves = Ensure.AtLeastZero(halves);

    /// <summary>The halves of the weapon's damage a hit deals, zero or more.</summary>
    private readonly int _halves;

    /// <summary>The weapon does no damage.</summary>
    public static WeaponDamageFactor None => default;

    /// <summary>The weapon does half its damage.</summary>
    public static WeaponDamageFactor Half { get; } = new(HalvesPerWholeDamage / 2);

    /// <summary>The weapon does its full damage.</summary>
    public static WeaponDamageFactor Full { get; } = new(HalvesPerWholeDamage);

    /// <summary>The weapon does double its damage.</summary>
    public static WeaponDamageFactor Double { get; } = new(2 * HalvesPerWholeDamage);

    /// <summary>Whether the weapon does no damage at all.</summary>
    public bool IsNone => _halves == 0;

    /// <summary>
    /// The damage a hit by a weapon of <paramref name="weaponDamage"/>, zero or more, deals: half the weapon's damage,
    /// rounded down, once for each half of the factor.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The weapon damage is negative.</exception>
    public int DamageFrom(int weaponDamage) => Ensure.AtLeastZero(weaponDamage) / HalvesPerWholeDamage * _halves;
}
