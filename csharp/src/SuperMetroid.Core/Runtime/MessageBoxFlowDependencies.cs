using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>The collaborators a <see cref="MessageBoxFlow"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record MessageBoxFlowDependencies
{
    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }

    /// <summary>Bomb projectiles.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Hud.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Message box.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Suit pickup.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Vram.</summary>
    public required SnesVram Vram { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
