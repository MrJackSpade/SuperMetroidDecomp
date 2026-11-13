namespace SuperMetroid.Core.Enemies;

/// <summary>The enemy setup the attract demo's room setups perform.</summary>
internal interface IEnemyAttractDemoHooks
{
    /// <summary>Sets the Kraid body's phase timer, as the Kraid demo's room setup does.</summary>
    void SetKraidPhaseTimer(int frames);
}
