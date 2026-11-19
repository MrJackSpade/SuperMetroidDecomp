namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy setup the attract demo's room setups perform.</summary>
internal interface IEnemyAttractDemoHooks
{
    /// <summary>Starts the Kraid body's phase wait for <paramref name="updates"/>, as the Kraid demo's room setup does.</summary>
    void StartKraidPhaseWait(int updates);
}
