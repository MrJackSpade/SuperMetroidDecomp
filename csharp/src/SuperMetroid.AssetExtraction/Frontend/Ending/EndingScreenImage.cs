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
internal static partial class EndingScreenImage
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
    internal static void Execute(SnesVram vram, EndingTransfer transfer, EndingTransferArtwork artwork)
    {
        var ending = new EndingVram(vram);
        switch (transfer)
        {
            case EndingSheetTransfer sheet:
                ExecuteSheet(ending, sheet.Kind, artwork);
                break;
            case EndingExplosionMapTransfer map:
                ending.ObjectFragment(map.Fragment).Load(MapBytes(artwork.Objects.Fragment(map.Fragment)).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
                break;
            case EndingPostShotPartTransfer part:
                ExecutePostShotPart(vram, part.Part, artwork);
                break;
            case EndingRewardCharactersTransfer reward:
                ReadOnlySpan<byte> rewardCharacters = EndingRewardDefinitions.Rewards[reward.Reward].Dress switch
                {
                    EndingRewardDress.Suitless => artwork.Objects.SuitlessSamus.Characters.Span,
                    EndingRewardDress.Suited => artwork.Objects.WaitingSamus.Characters.Span,
                    _ => throw new InvalidDataException($"Undefined {nameof(EndingRewardDress)} for {reward.Reward}."),
                };
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
            case EndingTransferKind.EscapeClouds:
                ending.PostCreditsObjects.Load(artwork.Objects.Clouds.Characters.Span[..EndingArtworkExtentDefinitions.FloorBytes]);
                break;
            case EndingTransferKind.ExplosionObjects:
                ending.ExplosionObjects.Load(artwork.Objects.Explosion.Characters.Span[..EndingTransferExtentDefinitions.ExplosionObjectBytes]);
                break;
            case EndingTransferKind.EndingFont:
                ending.EndingFont.Load(artwork.Font.Characters.Span[..EndingTransferExtentDefinitions.EndingFontUploadBytes]);
                break;
            case EndingTransferKind.CreditsCharacterArt:
                ExecuteCreditsCharacterArt(ending, artwork);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined ending sheet transfer.");
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
        ending.TransformationCharacters.Load(objects.TransformationCharacters.Characters.Span[..EndingTransferExtentDefinitions.TransformationCharacterBytes]);
        ending.TransformationMap.Load(MapBytes(objects.TransformationMap).AsSpan(0, EndingTransferExtentDefinitions.ObjectFragmentBytes));
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
            _ => throw new InvalidOperationException($"Undefined {nameof(EndingPostShotUploadSource)} {transfer.Source}."),
        };
        vram.LoadBytes(EndingVramDestinations.PostShotWordOf(transfer) * sizeof(ushort), bytes);
    }

    /// <summary>An installed map's tiles as the little-endian tilemap words a transfer writes.</summary>
    internal static byte[] MapBytes(RoomBackgroundTilemapAtlas map) => BackgroundTileWords.ToBytes(map.Tiles.Span);
}
