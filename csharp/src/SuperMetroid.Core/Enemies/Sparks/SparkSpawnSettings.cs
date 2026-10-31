namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>How a Spark starts and how long it waits (#1472).</summary>
/// <param name="InitialState">Its first state, 0 through 65535.</param>
/// <param name="BaseFunctionTime">Updates, 0 through 65535, it waits between functions.</param>
internal sealed record SparkSpawnSettings(int InitialState, int BaseFunctionTime) : EnemySpawnSettings
{
    /// <summary>Its first state, 0 through 65535.</summary>
    public int InitialState { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, InitialState);

    /// <summary>Updates, 0 through 65535, it waits between functions.</summary>
    public int BaseFunctionTime { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, BaseFunctionTime);
}
