using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Runtime;

/// <summary>The collaborators a <see cref="DeathSequenceRunner"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record DeathSequenceRunnerDependencies
{
    /// <summary>The display state, which supplies the active turning floor's transform for Samus's death draws.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>The accepted frame boundary.</summary>
    public required AcceptedFrameBoundary FrameBoundary { get; init; }

    /// <summary>Sprite frame the runner begins, draws into and finalizes.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Samus's frame state, which owns Samus and records the death sequence's last step.</summary>
    public required SamusFrameState Player { get; init; }

    /// <summary>The active room, whose camera Samus's death draws use.</summary>
    public required ActiveRoomState Room { get; init; }
}
