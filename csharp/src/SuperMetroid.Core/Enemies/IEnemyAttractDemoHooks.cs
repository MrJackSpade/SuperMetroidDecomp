namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy setup the attract demo's room setups perform.</summary>
internal interface IEnemyAttractDemoHooks
{
    /// <summary>
    /// Starts the Kraid body's phase wait for <paramref name="updates"/> once his body has set up, as the Kraid demo's room
    /// setup does; the demo composes it before the room's enemies load (#2554).
    /// </summary>
    void StartKraidPhaseWaitAsHeSetsUp(int updates);
}
