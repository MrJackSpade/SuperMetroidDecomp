using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>What the PLM handler reads from the rest of the frame: scroll zones, enemy deaths and quota, newly pressed input, collected items and the power-bomb status (#633).</summary>
public readonly record struct PlmFrameInputs
{
    /// <summary>Scrolls.</summary>
    public required RoomScrollGrid? Scrolls { get; init; }

    /// <summary>Enemy deaths.</summary>
    public required ushort EnemyDeaths { get; init; }

    /// <summary>Enemy death quota.</summary>
    public required byte EnemyDeathQuota { get; init; }

    /// <summary>Controller new input.</summary>
    public required ushort ControllerNewInput { get; init; }

    /// <summary>Collected items.</summary>
    public ushort CollectedItems { get; init; }

    /// <summary>Power bomb explosion status.</summary>
    public ushort PowerBombExplosionStatus { get; init; }
}
