using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Replays the ending's recorded transfers over the bound artwork into the memory image the backends draw (#1358), each
/// at the native words, lanes and order its transfer wrote.
/// </summary>
[PpuLayoutBoundary("codec: the ending's transfers at their native VRAM words")]
internal static class EndingScreenImage
{
    /// <summary>The image <paramref name="uploads"/> leave over <paramref name="artwork"/>, in order.</summary>
    internal static byte[] Compose(IReadOnlyList<EndingUpload> uploads, EndingScreenArtwork artwork)
    {
        var vram = new SnesVram();
        foreach (EndingUpload upload in uploads)
            Replay(vram, upload, artwork);
        return vram.CaptureImage();
    }

    /// <summary>Writes one transfer into <paramref name="vram"/>.</summary>
    private static void Replay(SnesVram vram, EndingUpload upload, EndingScreenArtwork artwork)
    {
        var ending = new EndingVram(vram);
        switch (upload.Kind)
        {
            case EndingUploadKind.FloorSceneTiles:
                EndingFloorSceneArtwork scene = Mode7(artwork)[(EndingFloorSceneId)upload.Key];
                var floor = new Mode7Vram(vram);
                floor.LoadMap(scene.Map.Span);
                floor.LoadMap(scene.Map.Span, cell: EndingFloorArtworkFormat.MapByteCount);
                floor.LoadCharacters(scene.Characters.Span);
                break;
            case EndingUploadKind.FlyawayChunk:
                ReplayFlyawayChunk(vram, upload.Key, artwork);
                break;
            case EndingUploadKind.EscapeClouds:
                ending.PostCreditsObjects.Load(Objects(artwork).Clouds.Transfer.Span[..EndingCreditsRomData.Rendering.FloorBytes]);
                break;
            case EndingUploadKind.ExplosionObjects:
                ending.ExplosionObjects.Load(Objects(artwork).Explosion.Transfer.Span[..EndingCreditsRomData.Rendering.ExplosionObjectBytes]);
                break;
            case EndingUploadKind.ObjectFragment:
                var fragment = (EndingObjectFragmentId)upload.Key;
                ending.ObjectFragment(fragment).Load(Objects(artwork).Fragment(fragment).Transfer.Span[..EndingCreditsRomData.Rendering.ObjectFragmentBytes]);
                break;
            case EndingUploadKind.EndingFont:
                ending.EndingFont.Load(Font(artwork).Transfer.Span[..EndingCreditsRomData.Rendering.ObjectFragmentLimit]);
                break;
            case EndingUploadKind.CreditsFont:
                ending.CreditsFont.Load(Font(artwork).Transfer.Span[..EndingCreditsRomData.Rendering.FontCharacterBytes]);
                break;
            case EndingUploadKind.CreditsCharacterArt:
                ReplayCreditsCharacterArt(ending, artwork);
                break;
            case EndingUploadKind.CreditsTilemap:
                ending.CreditsTilemap.Write(Words(upload));
                break;
            case EndingUploadKind.TextTilemap:
                vram.ExecuteWordTransfer(Words(upload), EndingVram.TextTilemapWord((EndingTextTilemap)upload.Key), wordIncrement: 1);
                break;
            case EndingUploadKind.PostShotLowerTilemap:
                ending.PostShotLowerTilemap.Write(Words(upload));
                break;
            case EndingUploadKind.RewardChunk:
                // The queued transfer writes both VRAM ports with increment-after-high; the bytes are already interleaved.
                int source = EndingRewardGraphicsUploadDefinitions.SourceWord(upload.Key);
                vram.LoadBytes(EndingRewardGraphicsUploadDefinitions.DestinationWord(upload.Key) * sizeof(ushort),
                    Mode7(artwork).RewardIcon.Transfer.Span.Slice(source - EndingRewardGraphicsUploadDefinitions.SourceBase,
                        EndingRewardGraphicsUploadDefinitions.ChunkBytes));
                break;
            case EndingUploadKind.PostShotTransfer:
                ReplayPostShotTransfer(vram, upload.Key, artwork);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(upload), upload.Kind, "Undefined ending upload.");
        }
    }

    /// <summary>One of the sixteen flyaway chunks: the first half replace Mode 7 characters, the rest the padded map.</summary>
    private static void ReplayFlyawayChunk(SnesVram vram, int index, EndingScreenArtwork artwork)
    {
        SuperMetroid.Core.Frontend.CeresCinematics.Artwork.CeresFlightArtworkCatalog flight = artwork.Flight ?? throw new InvalidOperationException(
            "Ending flyaway requires installed flight artwork.");
        int chunkBytes = EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        int chunksPerLane = EndingCreditsRomData.Rendering.FloorBytes / chunkBytes;
        int offset = index % chunksPerLane * chunkBytes;
        var floor = new Mode7Vram(vram);
        if (index < chunksPerLane)
        {
            floor.LoadCharacters(flight.FloorCharacters.Span.Slice(offset, chunkBytes), offset);
            return;
        }
        // The map lane is the flight's map data padded with the blank tile to the full lane.
        var map = new byte[EndingCreditsRomData.Rendering.FloorBytes];
        flight.FloorMaps.Span[..EndingCreditsRomData.Rendering.FlyawayMapDataBytes].CopyTo(map);
        map.AsSpan(EndingCreditsRomData.Rendering.FlyawayMapDataBytes).Fill(EndingCreditsRomData.Rendering.FlyawayBlankTile);
        floor.LoadMap(map.AsSpan(offset, chunkBytes), offset);
    }

    /// <summary>The waiting, shooting and reward characters with the post-credits map fragments.</summary>
    private static void ReplayCreditsCharacterArt(EndingVram ending, EndingScreenArtwork artwork)
    {
        EndingObjectArtworkCatalog objects = Objects(artwork);
        ReadOnlySpan<byte> waiting = objects.WaitingSamus.Transfer.Span;
        ending.WaitingCharacters.Load(waiting[..EndingCreditsRomData.Rendering.WaitingCharacterBytes]);
        ending.PostCreditsObjects.Load(objects.ShootingScreen.Transfer.Span[..EndingCreditsRomData.Rendering.ShootingCharacterBytes]);
        ending.WaitingTilemap.Load(objects.WaitingTilemap.Transfer.Span[..EndingCreditsRomData.Rendering.WaitingTilemapBytes]);
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the latter reuses the waiting-scene sheet.
        ReadOnlySpan<byte> reward = artwork.Reward == EndingReward.Suitless ? objects.SuitlessSamus.Transfer.Span : waiting;
        ending.RewardCharacters.Load(reward[..EndingCreditsRomData.Rendering.FloorBytes]);
        ending.PostCreditsFragmentA.Load(objects.PostCreditsFragmentA.Transfer.Span[..EndingCreditsRomData.Rendering.PostCreditsFragmentABytes]);
        ending.PostCreditsFragmentB.Load(objects.PostCreditsFragmentB.Transfer.Span[..EndingCreditsRomData.Rendering.ObjectFragmentBytes]);
    }

    /// <summary>One post-shot subtitle, logo-tile or logo-map transfer at its native word.</summary>
    private static void ReplayPostShotTransfer(SnesVram vram, int index, EndingScreenArtwork artwork)
    {
        EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
        int length = transfer.Length;
        ReadOnlySpan<byte> bytes = transfer.Source switch
        {
            EndingPostShotUploadSource.Subtitle => Font(artwork).Transfer.Span.Slice(EndingPostShotDefinitions.SubtitleFontOffset, length),
            EndingPostShotUploadSource.LogoTiles => Objects(artwork).PostShotLogoTiles.Transfer.Span.Slice(
                transfer.SourceAddress - EndingPostShotDefinitions.LogoTileSource, length),
            EndingPostShotUploadSource.LogoMap => Objects(artwork).PostShotLogoMap.Transfer.Span[..length],
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
        };
        vram.LoadBytes(transfer.DestinationWord * sizeof(ushort), bytes);
    }

    /// <summary>The tilemap words a tilemap transfer recorded.</summary>
    private static ReadOnlySpan<ushort> Words(EndingUpload upload) =>
        upload.Words ?? throw new InvalidOperationException($"The {upload.Kind} transfer records no tilemap words.");

    /// <summary>The bound Mode 7 artwork.</summary>
    private static EndingFloorArtworkCatalog Mode7(EndingScreenArtwork artwork) =>
        artwork.Floor ?? throw new InvalidOperationException("Ending Mode-7 scenes require installed artwork.");

    /// <summary>The bound OBJ artwork.</summary>
    private static EndingObjectArtworkCatalog Objects(EndingScreenArtwork artwork) =>
        artwork.Objects ?? throw new InvalidOperationException("Ending objects require installed artwork.");

    /// <summary>The bound ending font.</summary>
    private static EndingFontAtlas Font(EndingScreenArtwork artwork) =>
        artwork.Font ?? throw new InvalidOperationException("Ending font requires installed presentation assets.");
}
