using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The collaborators a <see cref="PlmFramePhase"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record PlmFramePhaseDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }



    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }


    /// <summary>Enemy room effects.</summary>
    public required EnemyRoomEffects EnemyRoomEffects { get; init; }

    /// <summary>Freeze.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>Message box.</summary>
    public required GameplayMessageBoxState MessageBox { get; init; }

    /// <summary>Message boxes.</summary>
    public required MessageBoxFlow MessageBoxes { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }


    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Suit pickup.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }


    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

}
