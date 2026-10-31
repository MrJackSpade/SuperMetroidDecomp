using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The attract demo hooks over Kraid.</summary>
/// <param name="kraid">Kraid.</param>
internal sealed class EnemyAttractDemoHooks(KraidBehavior kraid) : IEnemyAttractDemoHooks
{
    /// <inheritdoc/>
    public void SetKraidFunctionTimer(int frames) =>
        (kraid.Access.Kraid ?? throw new InvalidDataException("The Kraid demo requires Kraid's encounter state.")).Parts[0].FunctionTimer = WrappingCountdown.Start(frames);
}
