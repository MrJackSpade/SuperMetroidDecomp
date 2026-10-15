using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>The scripted attract-demo input the runtime substitutes for the player, while a demo plays.</summary>
internal sealed class AttractDemoSession : IAttractDemoState
{
    /// <summary>The demo script replacing player input, or null outside a demo.</summary>
    internal AttractDemoInput? Input { get; set; }

    /// <summary>Restores the player's controller after the demo's scripted input for this frame.</summary>
    internal IDisposable? InputRestore { get; set; }

    /// <summary>Whether an attract demo is playing.</summary>
    public bool IsAttractDemo => Input is not null;
}
