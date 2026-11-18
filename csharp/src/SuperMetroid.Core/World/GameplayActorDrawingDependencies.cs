using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="GameplayActorDrawing"/> is constructed with, built once by the owner that creates it (#621).</summary>
internal sealed record GameplayActorDrawingDependencies
{
    /// <summary>The installed artwork Samus, her shots and her suits' colours are drawn from.</summary>
    public required RuntimeArtwork Artwork { get; init; }

    /// <summary>Whether the attract demo is playing, which changes how Samus is shown.</summary>
    public required IAttractDemoState AttractSession { get; init; }

    /// <summary>The player's gameplay actions as the frame boundary latched them.</summary>
    public required GameplayActionInput PlayerActions { get; init; }

    /// <summary>What the gameplay display shows, whose modes can hide Samus's ordinary drawing.</summary>
    public required GameplayDisplayState Display { get; init; }

    /// <summary>Whether gameplay time is frozen, which holds the enemy projectiles' drawing.</summary>
    public required GameplayTimeFreeze Freeze { get; init; }

    /// <summary>The game's frame counters, which time enemies, animations and blinking.</summary>
    public required FrameClock Clock { get; init; }

    /// <summary>Sprite frame receiving actor sprites and whole-frame draws.</summary>
    public required SpriteFrameBuffer Sprites { get; init; }

    /// <summary>Samus: her pose, colours, grapple beam and movement effects.</summary>
    public required SamusFrameState Player { get; init; }
}
