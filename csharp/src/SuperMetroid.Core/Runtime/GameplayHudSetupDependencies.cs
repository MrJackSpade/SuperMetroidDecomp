using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>The collaborators a <see cref="GameplayHudSetup"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record GameplayHudSetupDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Enemies.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>Hud.</summary>
    public required HudState Hud { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Vram.</summary>
    public required SnesVram Vram { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
