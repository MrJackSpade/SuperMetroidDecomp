using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The collaborators a <see cref="AcceptedFrameBoundary"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record AcceptedFrameBoundaryDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>The HUD whose rows the BG3 plane shows.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Background scroll.</summary>
    public required BackgroundScrollState BackgroundScroll { get; init; }


    /// <summary>Controller1.</summary>
    public required ControllerInputState Controller1 { get; init; }

    /// <summary>Physical controller-1 latch.</summary>
    public required ControllerInputState PhysicalController1 { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Sprite frame the NMI makes visible.</summary>
    public required SpriteFrameBuffer DisplayedSprites { get; init; }

    /// <summary>The enemy words and installed enemy artwork that outlive each room.</summary>
    public required EnemySession EnemySession { get; init; }

    /// <summary>Gameplay windows.</summary>
    public required GameplayWindowRegisters GameplayWindows { get; init; }

    /// <summary>Nmi.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Sprite frame the main loop finalized.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Options.</summary>
    public required GameplayOptions Options { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Active room, door, and scrolling sky.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Suit pickup.</summary>
    public required SamusSuitPickupState SuitPickup { get; init; }

    /// <summary>The session COLDATA shadows.</summary>
    public required RoomFxSession RoomFx { get; init; }


    /// <summary>Vram.</summary>
    public required SnesVram Vram { get; init; }

    /// <summary>Xray.</summary>
    public required XrayReveal Xray { get; init; }

    /// <summary>Executes the queued video transfers, whose address-sourced entries read live memory.</summary>
    public required Action DrainQueuedVramWrites { get; init; }
}
