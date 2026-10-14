using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Runtime;

/// <summary>The collaborators a <see cref="DeathSequenceRunner"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record DeathSequenceRunnerDependencies
{
    /// <summary>Actor drawing.</summary>
    public required GameplayActorDrawing ActorDrawing { get; init; }

    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Nmi handler.</summary>
    public required NmiHandler NmiHandler { get; init; }

    /// <summary>Oam.</summary>
    public required OamBuffer Oam { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>Vram writes.</summary>
    public required VramWriteQueue VramWrites { get; init; }

    /// <summary>Address space.</summary>
    public required ISnesAddressSpace AddressSpace { get; init; }
}
