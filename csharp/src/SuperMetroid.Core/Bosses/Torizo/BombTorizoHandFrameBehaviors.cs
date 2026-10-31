using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Bomb Torizo's hand's wake.</summary>
internal static class BombTorizoHandFrameBehaviors
{
    /// <summary><c>$84:D33B</c>: the sleeping hand wakes once Samus has bombs.</summary>
    internal static PlmFrameBehavior WakeIfSamusHasBombs { get; } = new(
        "Bomb Torizo hand wake",
        static (slot, context) => context.Plms.Bosses().BombTorizoHand.RunBombTorizoHandFrameBehavior(slot));
}
