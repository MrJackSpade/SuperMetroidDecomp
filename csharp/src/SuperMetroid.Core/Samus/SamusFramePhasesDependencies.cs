using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Samus;

/// <summary>The collaborators a <see cref="SamusFramePhases"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record SamusFramePhasesDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Bomb projectiles.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Enemy phase.</summary>
    public required EnemyFramePhase EnemyPhase { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Game time.</summary>
    public required GameTimeState GameTime { get; init; }

    /// <summary>Nmi.</summary>
    public required NmiClock Nmi { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Plms.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>Projectiles.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Room palette fx.</summary>
    public required RoomPaletteFxSystem RoomPaletteFx { get; init; }

    /// <summary>System.</summary>
    public required Bank80SystemState System { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
