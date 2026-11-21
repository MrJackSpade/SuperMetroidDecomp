namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>What the Ki-Hunter's body's actions are (<see cref="KiHunterBodyState"/>).</summary>
internal static class KiHunterActionDefinitions
{
    /// <summary>The actions of a body that still has its wings: its patrol, swoop, swipe and the recovery from a swoop.</summary>
    internal static readonly KiHunterAction[] Flying =
        [KiHunterAction.FlyingPatrol, KiHunterAction.Swooping, KiHunterAction.Swiping, KiHunterAction.RecoveringFromSwoop];
}
