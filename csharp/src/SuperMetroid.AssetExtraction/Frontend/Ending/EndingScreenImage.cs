using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Executes the ending's transfers once over the installed artwork into the memory image its sprite pages are cut from
/// (#1358, #1467), each at the native words, lanes and order its transfer wrote.
/// </summary>
internal static class EndingScreenImage
{
    /// <summary>The image <paramref name="transfers"/> leave over <paramref name="artwork"/>, in order.</summary>
    internal static byte[] Compose(IReadOnlyList<EndingTransfer> transfers, EndingTransferArtwork artwork)
    {
        var vram = new SnesVram();
        foreach (EndingTransfer transfer in transfers)
            Execute(vram, transfer, artwork);
        return vram.CaptureImage();
    }

    /// <summary>Writes one transfer into <paramref name="vram"/>.</summary>
    private static void Execute(SnesVram vram, EndingTransfer transfer, EndingTransferArtwork artwork)
    {
        var ending = new EndingVram(vram);
        switch (transfer.Kind)
        {
            case EndingTransferKind.EscapeClouds:
                ending.PostCreditsObjects.Load(Objects(artwork).Clouds.Characters.Span[..EndingArtworkExtentDefinitions.FloorBytes]);
                break;
            case EndingTransferKind.ExplosionObjects:
                ending.ExplosionObjects.Load(Objects(artwork).Explosion.Characters.Span[..EndingTransferExtentDefinitions.ExplosionObjectBytes]);
                break;
            case EndingTransferKind.ExplosionMap:
                EndingObjectFragmentId fragment = transfer.Fragment ?? throw new InvalidOperationException("An explosion map transfer names its map.");
                ending.ObjectFragment(fragment).Load(MapBytes(Objects(artwork).Fragment(fragment)).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
                break;
            case EndingTransferKind.EndingFont:
                ending.EndingFont.Load(Font(artwork).Characters.Span[..EndingTransferExtentDefinitions.ObjectFragmentLimit]);
                break;
            case EndingTransferKind.CreditsCharacterArt:
                ExecuteCreditsCharacterArt(ending, artwork);
                break;
            case EndingTransferKind.PostShotPart:
                ExecutePostShotPart(vram, transfer.Chunk, artwork);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transfer), transfer.Kind, "Undefined ending transfer.");
        }
    }

    /// <summary>The waiting, shooting and reward characters with the post-credits map fragments.</summary>
    private static void ExecuteCreditsCharacterArt(EndingVram ending, EndingTransferArtwork artwork)
    {
        EndingObjectSourceArtwork objects = Objects(artwork);
        ReadOnlySpan<byte> waiting = objects.WaitingSamus.Characters.Span;
        ending.WaitingCharacters.Load(waiting[..EndingTransferExtentDefinitions.WaitingCharacterBytes]);
        ending.PostCreditsObjects.Load(objects.ShootingScreen.Characters.Span[..EndingTransferExtentDefinitions.ShootingCharacterBytes]);
        ending.WaitingTilemap.Load(MapBytes(objects.WaitingTilemap).AsSpan(0, EndingTransferExtentDefinitions.WaitingTilemapBytes));
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the latter reuses the waiting-scene sheet.
        ReadOnlySpan<byte> reward = artwork.Reward == EndingReward.Suitless ? objects.SuitlessSamus.Characters.Span : waiting;
        ending.RewardCharacters.Load(reward[..EndingArtworkExtentDefinitions.FloorBytes]);
        ending.PostCreditsFragmentA.Load(objects.PostCreditsFragmentA.Characters.Span[..EndingTransferExtentDefinitions.PostCreditsFragmentABytes]);
        ending.PostCreditsFragmentB.Load(MapBytes(objects.PostCreditsFragmentB).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
    }

    /// <summary>One post-shot subtitle, logo-tile or logo-map part at its native word.</summary>
    private static void ExecutePostShotPart(SnesVram vram, int index, EndingTransferArtwork artwork)
    {
        EndingPostShotUploadDefinition transfer = EndingPostShotUploads.Get(index);
        int length = transfer.Length;
        ReadOnlySpan<byte> bytes = transfer.Source switch
        {
            EndingPostShotUploadSource.Subtitle => Font(artwork).Characters.Span.Slice(EndingPostShotUploadDefinitions.SubtitleFontOffset, length),
            EndingPostShotUploadSource.LogoTiles => Objects(artwork).PostShotLogoTiles.Characters.Span.Slice(
                transfer.Offset, length),
            EndingPostShotUploadSource.LogoMap => MapBytes(Objects(artwork).PostShotLogoMap).AsSpan(0, length),
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
        };
        vram.LoadBytes(EndingVramDestinations.PostShotWordOf(transfer) * sizeof(ushort), bytes);
    }

    /// <summary>An installed map's tiles as the little-endian tilemap words a transfer writes.</summary>
    internal static byte[] MapBytes(RoomBackgroundTilemapAtlas map) => BackgroundTileWords.ToBytes(map.Tiles.Span);

    /// <summary>The bound OBJ artwork as the import pipeline read it, with its character atlases and maps.</summary>
    private static EndingObjectSourceArtwork Objects(EndingTransferArtwork artwork) =>
        EndingObjectSourceArtwork.Of(artwork.Objects ?? throw new InvalidOperationException("Ending objects require installed artwork."));

    /// <summary>The bound ending font.</summary>
    private static EndingFontCharacters Font(EndingTransferArtwork artwork) =>
        EndingFontCharacters.Of(artwork.Font ?? throw new InvalidOperationException("Ending font requires installed presentation assets."));
}
