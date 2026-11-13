using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Checks the damage Samus's weapons deal enemies (#1632): ordinary shots, bombs, Power Bomb explosions and contact
/// attacks on enemies, Kraid, Mother Brain, Phantoon and the Metroids each wrote out half the weapon's damage times the
/// enemy's multiplier, and now take <see cref="EnemyWeaponVulnerability.DamageFrom(int)"/>. Every weapon damage a word
/// holds, with every multiplier an authored response can hold, gives the replaced arithmetic's damage.
/// </summary>
internal static class EnemyHitDamageVerification
{
    /// <summary>The largest multiplier an authored response holds: seven bits.</summary>
    private const int LargestAuthoredMultiplier = 127;

    /// <summary>Compares every weapon damage and multiplier, through both a response and an ordinary shot's reading, with the replaced arithmetic.</summary>
    internal static void VerifyEnemyHitDamage()
    {
        long compared = 0;
        int mismatches = 0;
        string? firstMismatch = null;
        for (int multiplier = 0; multiplier <= LargestAuthoredMultiplier; multiplier++)
        {
            var response = new EnemyWeaponVulnerability(multiplier, EnemyIceResponse.FreezesOnLethalHit);
            var shot = new EnemyCollision.NormalShotVulnerability(multiplier, FreezeImmediately: false, PreventsLethalFreeze: false);
            for (int weaponDamage = 0; weaponDamage <= ushort.MaxValue; weaponDamage++)
            {
                int expected = (weaponDamage >> 1) * multiplier;
                int fromResponse = response.DamageFrom(weaponDamage);
                int fromShot = shot.DamageFrom(weaponDamage);
                compared++;
                if (fromResponse != expected || fromShot != expected)
                {
                    mismatches++;
                    firstMismatch ??= $"weapon damage {weaponDamage} x{multiplier}: expected {expected}, response {fromResponse}, shot {fromShot}";
                }
            }
        }

        AssertEqual(0, mismatches, $"enemy hit damage against the replaced halving and multiply; first: {firstMismatch}");
        AssertEqual(0, EnemyWeaponVulnerability.NoDamage.DamageFrom(ushort.MaxValue), "no damage takes nothing from any weapon");
        AssertEqual(200, EnemyWeaponVulnerability.FullDamage.DamageFrom(201), "full damage drops an odd point");
        AssertEqual(100, EnemyWeaponVulnerability.HalfDamage.DamageFrom(201), "half damage halves the weapon's even part");
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyWeaponVulnerability.FullDamage.DamageFrom(-1), "weapon damage is never negative");
        Console.WriteLine($"Enemy hit damage: {compared} weapon damages and multipliers give the replaced halving and multiply's damage.");
    }
}
