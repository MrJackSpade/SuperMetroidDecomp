using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>What a flickering n00b-tube crack keeps: the column it shows at between hidden updates.</summary>
internal sealed class NoobTubeCrackState : EnemyProjectileState
{
    /// <summary>The column the crack last showed at; zero until its first flicker records it.</summary>
    internal int ShownX
    {
        get;
        set => field = Ensure.BetweenInclusive(0, ushort.MaxValue, value);
    }
}
