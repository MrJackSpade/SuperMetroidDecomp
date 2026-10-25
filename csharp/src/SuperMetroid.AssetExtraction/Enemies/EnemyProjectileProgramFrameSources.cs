using System.Collections.Frozen;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Where each installed enemy-projectile program frame is stored (<see cref="EnemyProjectileProgramFrameDefinitions"/>).</summary>
internal static class EnemyProjectileProgramFrameSources
{
    /// <summary>Each installed program frame, by the bank-$86 operand it is stored at.</summary>
    private static readonly FrozenDictionary<ushort, EnemyProjectileFrameId> Frames =
        EnemyProjectileProgramFrameDefinitions.Operands.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The bank-$86 operand installed program frame <paramref name="frame"/> is stored at.</summary>
    internal static ushort Operand(EnemyProjectileFrameId frame) =>
        EnemyProjectileProgramFrameDefinitions.Operands.TryGetValue(frame, out ushort operand)
            ? operand
            : throw new ArgumentOutOfRangeException(nameof(frame), frame, "No installed program frame shows this projectile frame.");

    /// <summary>Whether bank-$86 operand <paramref name="operand"/> stores an installed program frame.</summary>
    internal static bool Contains(ushort operand) => Frames.ContainsKey(operand);

    /// <summary>The installed program frame stored at bank-$86 operand <paramref name="operand"/>.</summary>
    internal static EnemyProjectileFrameId Frame(ushort operand) => Frames.TryGetValue(operand, out EnemyProjectileFrameId frame)
        ? frame
        : throw new ArgumentOutOfRangeException(nameof(operand), operand, $"No installed program frame at $86:{operand:X4}.");
}
