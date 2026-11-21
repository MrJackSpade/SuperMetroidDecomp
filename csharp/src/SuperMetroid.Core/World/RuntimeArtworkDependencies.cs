using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Hud;

namespace SuperMetroid.Core.World;

/// <summary>The collaborators a <see cref="RuntimeArtwork"/> is constructed with, built once by the owner that creates it (#621).</summary>
[ArchitectureReview("wide-dependencies", "Its members beside its collaborators are the immutable installed artwork catalogs the runtime binds once at construction (#2049), data rather than collaborators; the runtime-layer GameplayArtwork that groups them sits above World, so World cannot take it without a module cycle.")]
internal sealed partial record RuntimeArtworkDependencies
{
    /// <summary>The installed screen content the runtime is built over (#2049, #2203).</summary>
    public required SuperMetroid.Core.Gameplay.IGameplayPresentationContent ScreenContent { get; init; }

    /// <summary>The installed pre-coloured sprite sheets the runtime is built over.</summary>
    public required SuperMetroid.Core.Assets.InstalledSpriteSheets SpriteSheets { get; init; }

    /// <summary>The installed Samus body artwork the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Artwork.InstalledSamusBodyArtwork SamusBodyArt { get; init; }

    /// <summary>The installed frames Samus's body can show in each pose, with the picture frame each draws, the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Drawing.SamusShownFrames SamusShownFrames { get; init; }

    /// <summary>The installed grapple flare placements the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements GrappleFlarePlacement { get; init; }

    /// <summary>The installed grapple swing pictures the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames GrappleSwingFrames { get; init; }

    /// <summary>The installed projectile frame choices the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Artwork.InstalledProjectileFrameBindings ProjectileFrameBindings { get; init; }

    /// <summary>The installed charge-flare placements the runtime is built over.</summary>
    public required SuperMetroid.Core.Samus.Artwork.BeamFlarePlacements ChargeFlarePlacement { get; init; }

    /// <summary>The installed block pictures the runtime is built over.</summary>
    public required SuperMetroid.Core.Rooms.Artwork.InstalledRoomArtwork RoomBlockSheets { get; init; }

    /// <summary>The installed room layouts the runtime is built over.</summary>
    public required SuperMetroid.Core.Rooms.RoomLevelArtworks RoomVisualLayouts { get; init; }

    /// <summary>The installed room terrain the runtime is built over.</summary>
    public required SuperMetroid.Core.Rooms.LevelData.InstalledRoomLevelStreams RoomLevelStreams { get; init; }

    /// <summary>The session's installed boss artwork.</summary>
    public required BossArtworkBinding BossArtwork { get; init; }

    /// <summary>The loaded room, whose active escape typewriters follow a rebound escape-warning text.</summary>
    public required ActiveRoomState Room { get; init; }

    /// <summary>The HUD, which takes its installed pictures.</summary>
    public required HudState Hud { get; init; }
}
