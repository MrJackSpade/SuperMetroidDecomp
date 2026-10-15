namespace SuperMetroid.Core.Gameplay;

/// <summary>Whether the frontend's attract demo, rather than the player, is driving gameplay.</summary>
internal interface IAttractDemoState
{
    /// <summary>Whether an attract demo is playing.</summary>
    bool IsAttractDemo { get; }
}
