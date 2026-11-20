using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms;

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
        switch (transfer)
        {
            case EndingSheetTransfer sheet:
                ExecuteSheet(ending, sheet, artwork);
                break;
            case EndingExplosionMapTransfer map:
                ending.ObjectFragment(map.Fragment).Load(MapBytes(Objects(artwork).Fragment(map.Fragment)).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
                break;
            case EndingPostShotPartTransfer part:
                ExecutePostShotPart(vram, part.Part, artwork);
                break;
            case EndingRewardCharactersTransfer reward:
                ReadOnlySpan<byte> rewardCharacters = reward.Reward == EndingReward.Suitless
                    ? Objects(artwork).SuitlessSamus.Characters.Span
                    : Objects(artwork).WaitingSamus.Characters.Span;
                ending.RewardCharacters.Load(rewardCharacters[..EndingArtworkExtentDefinitions.FloorBytes]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transfer), transfer, "Undefined ending transfer.");
        }
    }

    /// <summary>Writes one fixed sheet transfer into <paramref name="ending"/>.</summary>
    private static void ExecuteSheet(EndingVram ending, EndingTransferKind kind, EndingTransferArtwork artwork)
    {
        switch (kind)
        {
            case EndingTransfer.EscapeClouds:
                ending.PostCreditsObjects.Load(artwork.Objects.Clouds.Characters.Span[..EndingArtworkExtentDefinitions.FloorBytes]);
                break;
            case EndingTransfer.ExplosionObjects:
                ending.ExplosionObjects.Load(artwork.Objects.Explosion.Characters.Span[..EndingTransferExtentDefinitions.ExplosionObjectBytes]);
                break;
            case EndingTransfer.ExplosionMap(EndingExplosionPicture fragment):
                ending.ObjectFragment(fragment).Load(MapBytes(artwork.Objects.Fragment(fragment)).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
                break;
            case EndingTransfer.EndingFont:
                ending.EndingFont.Load(artwork.Font.Characters.Span[..EndingTransferExtentDefinitions.ObjectFragmentLimit]);
                break;
            case EndingTransfer.CreditsCharacterArt:
                ExecuteCreditsCharacterArt(ending, artwork);
                break;
            case EndingTransfer.PostShotPart(int chunk):
                ExecutePostShotPart(vram, chunk, artwork);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(transfer), transfer, "Undefined ending transfer.");
        }
    }

    /// <summary>The waiting and shooting characters with the post-credits map fragments; a reward page loads its reward's after.</summary>
    private static void ExecuteCreditsCharacterArt(EndingVram ending, EndingTransferArtwork artwork)
    {
        ImportEndingObjectArtwork objects = artwork.Objects;
        ReadOnlySpan<byte> waiting = objects.WaitingSamus.Characters.Span;
        ending.WaitingCharacters.Load(waiting[..EndingTransferExtentDefinitions.WaitingCharacterBytes]);
        ending.PostCreditsObjects.Load(objects.ShootingScreen.Characters.Span[..EndingTransferExtentDefinitions.ShootingCharacterBytes]);
        ending.WaitingTilemap.Load(MapBytes(objects.WaitingTilemap).AsSpan(0, EndingTransferExtentDefinitions.WaitingTilemapBytes));
        ending.PostCreditsFragmentA.Load(objects.PostCreditsFragmentA.Characters.Span[..EndingTransferExtentDefinitions.PostCreditsFragmentABytes]);
        ending.PostCreditsFragmentB.Load(MapBytes(objects.PostCreditsFragmentB).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
    }

    /// <summary>One post-shot subtitle, logo-tile or logo-map part at its native word.</summary>
    private static void ExecutePostShotPart(SnesVram vram, EndingPostShotUploadDefinition transfer, EndingTransferArtwork artwork)
    {
        int length = transfer.Length;
        ReadOnlySpan<byte> bytes = transfer.Source switch
        {
            EndingPostShotUploadSource.Subtitle => artwork.Font.Characters.Span.Slice(EndingPostShotUploadDefinitions.SubtitleFontOffset, length),
            EndingPostShotUploadSource.LogoTiles => artwork.Objects.PostShotLogoTiles.Characters.Span.Slice(
                transfer.Offset, length),
            EndingPostShotUploadSource.LogoMap => MapBytes(artwork.Objects.PostShotLogoMap).AsSpan(0, length),
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {(int)transfer.Source}."),
        };
        vram.LoadBytes(EndingVramDestinations.PostShotWordOf(transfer) * sizeof(ushort), bytes);
    }

    /// <summary>An installed map's tiles as the little-endian tilemap words a transfer writes.</summary>
    internal static byte[] MapBytes(RoomBackgroundTilemapAtlas map) => BackgroundTileWords.ToBytes(map.Tiles.Span);
}
