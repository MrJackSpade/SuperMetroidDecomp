using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</summary>
public sealed record RoomEnemyFrame
{
    /// <summary>Layer-one camera position (viewport top-left) in whole room pixels.</summary>
    public required Position16<RoomPixels> Camera { get; init; }

    /// <summary>Whether time-freeze gating suppresses actor AI and instructions; this does not suppress every timer, palette hook, or frame publication.</summary>
    public required bool TimeIsFrozen { get; init; }

    /// <summary>Active player state required by player-dependent enemy paths.</summary>
    public SamusState? Samus { get; init; } = null;

    /// <summary>Raw SNES button-edge mask for this update, distinct from held buttons.</summary>
    public ushort NewlyPressedControllerInput { get; init; } = 0;

    /// <summary>Room terrain and collision data required by terrain-dependent actors.</summary>
    public RoomLevelData? Level { get; init; } = null;

    /// <summary>Raw held SNES controller-button mask for this update.</summary>
    public ushort ControllerInput { get; init; } = 0;

    /// <summary>Player shot system used by enemy collision and custom touch paths.</summary>
    public SamusProjectileSystem? SamusProjectiles { get; init; } = null;

    /// <summary>Current native eight-bit NMI clock; null uses the low byte of the standalone enemy-pass clock.</summary>
    public byte? NmiFrameCounter8 { get; init; } = null;

    /// <summary>Active Mode-7 transform sampled by rotating Ceres steam to derive draw offsets without changing its world collision coordinates.</summary>
    public SamusMode7Transform? Mode7Transform { get; init; } = null;

    /// <summary>Shared bomb and power-bomb state used by native collision order and actor effects.</summary>
    public SamusBombProjectileSystem? SharedProjectiles { get; init; } = null;

    /// <summary>Optional destination for corpse-row graphics transfers produced during this pass.</summary>
    public VramWriteQueue? VramWriteQueue { get; init; } = null;

    /// <summary>Whether to perform the configured projectile, bomb, and player-contact pass against each actor's pre-AI state.</summary>
    public bool ResolveSamusContactBeforeAi { get; init; } = false;

    /// <summary>Optional room PLMs participating in the scoped enemy terrain-collision probes.</summary>
    public RoomPlmSystem? CollisionPlms { get; init; } = null;

    /// <summary>Current independent sixteen-bit NMI clock; null uses the standalone enemy-pass clock.</summary>
    public ushort? NmiFrameCounter { get; init; } = null;

    /// <summary>True when <see cref="RoomEnemySystem.PrepareEnemyProcessingList"/> already ran at the native pre-Samus boundary; false prepares the lists here for standalone callers.</summary>
    public bool ProcessingListPrepared { get; init; } = false;

    /// <summary>The camera's frame-start Samus checkpoint under the live previous-position words that Draygon and Yapping Maw cap; null when no camera owns them.</summary>
    public FixedPosition<RoomPixels>? SamusPreviousPositionCheckpoint { get; init; } = null;
}
