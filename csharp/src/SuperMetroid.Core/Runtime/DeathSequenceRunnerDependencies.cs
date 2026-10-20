using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>The collaborators a <see cref="DeathSequenceRunner"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record DeathSequenceRunnerDependencies
{
    /// <summary>Artwork.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Cgram.</summary>
    public required SnesCgram Cgram { get; init; }

    /// <summary>Display.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Nmi handler.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Sprite frame the runner begins, draws into and finalizes.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Player.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>Room.</summary>
    public required ActiveRoomState Room { get; init; }
}
