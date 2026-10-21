namespace SuperMetroid.Core.Enemies;

/// <summary>What an enemy callback does when the engine calls it.</summary>
public enum EnemyCallbackBody
{
    /// <summary>The header names no callback.</summary>
    None,

    /// <summary>A routine with effects.</summary>
    Routine,

    /// <summary>A routine that returns at once.</summary>
    ImmediateReturn,
}
