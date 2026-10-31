namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where and how the bank-$B4 tables an enemy header's words select are laid out.</summary>
internal static class EnemyHeaderTableDefinitions
{
    /// <summary>The bank that holds the item-drop and vulnerability tables.</summary>
    internal const int TableBank = 0xb40000;

    /// <summary><c>EnemyDropChances</c> at $B4:F1F4, the first six-byte item-drop table.</summary>
    internal const ushort FirstDropChances = 0xf1f4;

    /// <summary>The bytes of one item-drop table: small energy, big energy, missile, nothing, Super Missile, Power Bomb.</summary>
    internal const int DropChancesSize = 6;

    /// <summary>The number of authored item-drop tables, including the unused ones.</summary>
    internal const int DropChancesCount = 118;

    /// <summary><c>EnemyVulnerabilities_Default</c> at $B4:EC1C, the first 22-byte vulnerability table.</summary>
    internal const ushort FirstVulnerability = 0xec1c;

    /// <summary>
    /// The bytes of one vulnerability table: twelve beam combinations (Wave lowest, then Ice, Spazer and Plasma), then
    /// missile, Super Missile, bomb, Power Bomb, Speed Booster, shinespark, Screw Attack, charged beam and pseudo Screw
    /// Attack, and a final byte no routine reads.
    /// </summary>
    internal const int VulnerabilitySize = 22;

    /// <summary>The number of authored vulnerability tables, including three unused ones.</summary>
    internal const int VulnerabilityCount = 68;

    /// <summary>A vulnerability byte that freezes without damage.</summary>
    internal const int FreezeEntry = 0xff;

    /// <summary>A vulnerability byte's bit that keeps a lethal Ice hit from freezing the enemy instead.</summary>
    internal const int PreventsLethalFreezeBit = 0x80;

    /// <summary>A vulnerability byte's multiplier bits; every authored multiplier fits the low nibble the charged beam reads.</summary>
    internal const int MultiplierBits = 0x0f;
}
