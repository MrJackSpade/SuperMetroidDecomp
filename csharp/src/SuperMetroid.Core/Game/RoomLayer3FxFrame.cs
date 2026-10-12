using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>What one layer-3 FX step reads from the frame: camera, freeze and main-loop state, RNG, darkness and the power bomb (#633).</summary>
public sealed record RoomLayer3FxFrame
{
    /// <summary>Camera.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Time is frozen.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>Main game loop carry.</summary>
    public required bool MainGameLoopCarry { get; init; }

    /// <summary>Random number.</summary>
    public ushort RandomNumber { get; init; } = 0;

    /// <summary>Fireflea darkness level.</summary>
    public ushort FirefleaDarknessLevel { get; init; } = 0;

    /// <summary>Power bomb.</summary>
    public SamusPowerBombExplosionState? PowerBomb { get; init; } = null;

    /// <summary>Liquid motion already advanced.</summary>
    public bool LiquidMotionAlreadyAdvanced { get; init; } = false;
}
