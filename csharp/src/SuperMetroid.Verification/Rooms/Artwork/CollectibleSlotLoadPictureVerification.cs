using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms a collectible slot's block is pictured over the room's load while its slot holds no collectible (#1486): the
/// 100% movie shows Upper Crateria's block 08E with graphics slot 0 empty, and the sheet had no picture for it. The block
/// depends on its slot only in a tileset whose rooms place a collectible (#1513), so the picture checked is the one the
/// room names from the inputs the sheet records for the block, each showing the room's load.
/// </summary>
internal static class CollectibleSlotLoadPictureVerification
{
    /// <summary>The block the 100% movie shows over an empty graphics slot.</summary>
    private const int ReportedBlock = 0x8E;

    /// <summary>Every Upper Crateria tileset's installed sheet holds block 08E's picture showing the room's load.</summary>
    internal static void VerifyCollectibleSlotLoadPicture()
    {
        InstalledRoomArtwork artwork = RepositoryInstallation.Installation.LoadRoomBlockSheets();
        RoomBlockSheet[] sheets = [.. artwork.Sheets.Where(pair => pair.Key.GraphicsSet == RoomGraphicsSet.UpperCrateria).Select(pair => pair.Value)];
        AssertTrue(sheets.Length > 0, "the installation has an Upper Crateria tileset sheet");
        foreach (RoomBlockSheet sheet in sheets)
        {
            ImmutableArray<AppearanceInput> inputs = sheet.Inputs.Of(ReportedBlock, RoomBlockArtwork.Tileset);
            string[] loaded = [.. inputs.Select(_ => RoomAppearanceValues.Loaded)];
            string name = RoomBlockPictureNames.Of(ReportedBlock, RoomBlockArtwork.Tileset, inputs, loaded);
            SpriteSheetFrame picture = sheet.Picture(name);
            AssertEqual(RoomBlockSheet.BlockPixels, (int)picture.Size.X, $"{sheet.Sheet.Image.Name} picture {name} is one block wide");
        }
        Console.WriteLine($"Collectible slot load picture: {sheets.Length} Upper Crateria sheet(s) hold block {ReportedBlock:X3}'s picture over the room's load.");
    }

    [VerificationCommand("--collectible-slot-load-picture")]
    internal static int CollectibleSlotLoadPictureCommand()
    {
        Suite(nameof(VerifyCollectibleSlotLoadPicture), VerifyCollectibleSlotLoadPicture);
        return 0;
    }
}
