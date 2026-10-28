using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's BG2 body layer: his working map composed from the installed artwork and the tilemap of the frame he shows.</summary>
[RoomLifetime]
internal sealed class KraidBodyLayer
{
    /// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
    private readonly KraidEncounterAccess _access;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidBodyLayer(KraidEncounterAccess access, BossArtworkBinding bossArtwork)
    {
        _bossArtwork = bossArtwork;
        _access = access;
    }

    /// <summary>
    /// The body tilemap Kraid shows while he owns BG2, at his BG2 scroll (#1352); null when he does not. Resolved from
    /// his installed artwork once per distinct body frame.
    /// </summary>
    internal BossBg2Layer? CaptureBodyLayer()
    {
        if (_access.Kraid is not { OwnsBg2Tilemap: true } state)
            return null;
        if (bodyTilemap is not { } cached || cached.Frame != state.DisplayedBody)
        {
            KraidBackgroundArtwork art = RequireBackgroundArtwork();
            bodyTilemap = cached = new KraidBodyTilemapCache(state.DisplayedBody, KraidBodyTilemap.Resolve(state.DisplayedBody, WorkingMap(), art));
        }
        return new BossBg2Layer(cached.Tilemap, state.Bg2Scroll);
    }

    /// <summary>The last body frame resolved, and its tilemap.</summary>
    [NonSerialized]
    private KraidBodyTilemapCache? bodyTilemap;

    /// <summary>Kraid's working map composed from his installed artwork, and that artwork.</summary>
    [NonSerialized]
    private KraidWorkingMap? workingMap;

    /// <summary>The body the installed artwork composes; a rebound installation recomposes it.</summary>
    internal BackgroundTile[] WorkingMap()
    {
        KraidBackgroundArtwork art = RequireBackgroundArtwork();
        if (workingMap is { } composed && ReferenceEquals(composed.Art, art))
            return composed.Tiles;
        BackgroundTile[] tiles = KraidBodyTilemap.ComposeWorkingMap(art);
        workingMap = new KraidWorkingMap(art, tiles);
        bodyTilemap = null;
        return tiles;
    }

    /// <summary>Kraid's installed BG2 artwork.</summary>
    private KraidBackgroundArtwork RequireBackgroundArtwork() =>
        _bossArtwork.Artwork?.KraidBackground ?? throw new InvalidOperationException("Kraid BG2 requires installed enemy artwork.");
}
