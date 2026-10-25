using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Areas.Ceres;

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
                floor.LoadCharacters(EndingFloorSceneCharacters.Of(scene).Characters.Span);
                break;
            case EndingUploadKind.FlyawayChunk:
                ReplayFlyawayChunk(vram, upload.Key, artwork);
                break;
            case EndingUploadKind.EscapeClouds:
                ending.PostCreditsObjects.Load(Objects(artwork).Clouds.Characters.Span[..EndingArtworkExtentDefinitions.FloorBytes]);
                break;
            case EndingUploadKind.ExplosionObjects:
                ending.ExplosionObjects.Load(Objects(artwork).Explosion.Characters.Span[..EndingArtworkExtentDefinitions.ExplosionObjectBytes]);
                break;
            case EndingUploadKind.ObjectFragment:
                var fragment = (EndingObjectFragmentId)upload.Key;
                ending.ObjectFragment(fragment).Load(Objects(artwork).Fragment(fragment).Characters.Span[..EndingArtworkExtentDefinitions.ObjectFragmentBytes]);
                break;
            case EndingUploadKind.EndingFont:
                ending.EndingFont.Load(Font(artwork).Characters.Span[..EndingArtworkExtentDefinitions.ObjectFragmentLimit]);
                break;
            case EndingUploadKind.CreditsFont:
                ending.CreditsFont.Load(Font(artwork).Characters.Span[..EndingArtworkExtentDefinitions.FontCharacterBytes]);
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
                int source = EndingRewardGraphicsUploadDefinitions.ChunkOffset(upload.Key);
                vram.LoadBytes(EndingVramDestinations.RewardChunkWordOf(upload.Key) * sizeof(ushort),
                    EndingRewardIconCharacters.Of(Mode7(artwork).RewardIcon).Bytes.Span.Slice(source,
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
        int chunkBytes = EndingArtworkExtentDefinitions.FlyawayChunkBytes;
        int chunksPerLane = EndingArtworkExtentDefinitions.FloorBytes / chunkBytes;
        int offset = index % chunksPerLane * chunkBytes;
        var floor = new Mode7Vram(vram);
        if (index < chunksPerLane)
        {
            floor.LoadCharacters(CeresFlightCharacters.Of(flight).FloorCharacters.Span.Slice(offset, chunkBytes), offset);
            return;
        }
        // The map lane is the flight's map data padded with the blank tile to the full lane.
        var map = new byte[EndingArtworkExtentDefinitions.FloorBytes];
        flight.FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(map);
        map.AsSpan(EndingArtworkExtentDefinitions.FlyawayMapDataBytes).Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        floor.LoadMap(map.AsSpan(offset, chunkBytes), offset);
    }

    /// <summary>The waiting, shooting and reward characters with the post-credits map fragments.</summary>
    private static void ReplayCreditsCharacterArt(EndingVram ending, EndingScreenArtwork artwork)
    {
        EndingObjectArtworkCatalog objects = Objects(artwork);
        ReadOnlySpan<byte> waiting = objects.WaitingSamus.Characters.Span;
        ending.WaitingCharacters.Load(waiting[..EndingArtworkExtentDefinitions.WaitingCharacterBytes]);
        ending.PostCreditsObjects.Load(objects.ShootingScreen.Characters.Span[..EndingArtworkExtentDefinitions.ShootingCharacterBytes]);
        ending.WaitingTilemap.Load(objects.WaitingTilemap.Bytes.Span[..EndingArtworkExtentDefinitions.WaitingTilemapBytes]);
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the latter reuses the waiting-scene sheet.
        ReadOnlySpan<byte> reward = artwork.Reward == EndingReward.Suitless ? objects.SuitlessSamus.Characters.Span : waiting;
        ending.RewardCharacters.Load(reward[..EndingArtworkExtentDefinitions.FloorBytes]);
        ending.PostCreditsFragmentA.Load(objects.PostCreditsFragmentA.Characters.Span[..EndingArtworkExtentDefinitions.PostCreditsFragmentABytes]);
        ending.PostCreditsFragmentB.Load(objects.PostCreditsFragmentB.Characters.Span[..EndingArtworkExtentDefinitions.ObjectFragmentBytes]);
    }

    /// <summary>One post-shot subtitle, logo-tile or logo-map transfer at its native word.</summary>
    private static void ReplayPostShotTransfer(SnesVram vram, int index, EndingScreenArtwork artwork)
    {
        EndingPostShotUploadDefinition transfer = EndingPostShotUploadDefinitions.Get(index);
        int length = transfer.Length;
        ReadOnlySpan<byte> bytes = transfer.Source switch
        {
            EndingPostShotUploadSource.Subtitle => Font(artwork).Characters.Span.Slice(EndingPostShotDefinitions.SubtitleFontOffset, length),
            EndingPostShotUploadSource.LogoTiles => Objects(artwork).PostShotLogoTiles.Characters.Span.Slice(
                transfer.Offset, length),
            EndingPostShotUploadSource.LogoMap => Objects(artwork).PostShotLogoMap.Bytes.Span[..length],
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
        };
        vram.LoadBytes(EndingVramDestinations.PostShotWordOf(transfer) * sizeof(ushort), bytes);
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
    private static EndingFontCharacters Font(EndingScreenArtwork artwork) =>
        EndingFontCharacters.Of(artwork.Font ?? throw new InvalidOperationException("Ending font requires installed presentation assets."));
}
