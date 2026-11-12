using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms the block under a collectible's first frame is pictured over the room's load where no collectible lies (#1486):
/// the 100% movie shows Upper Crateria's block 08E with no collectible on it, and the sheet had no picture for it. A
/// collectible shows its own picture over the block (#1538), so the picture checked is the one the room names from the
/// inputs the sheet records for the block, each showing the room's load.
/// </summary>
internal static class CollectibleSlotLoadPictureVerification
{
    /// <summary>The block the 100% movie shows with no collectible on it.</summary>
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
            // The sheet holds the picture as its part behind and its part in front (#1634); a part that shows nothing is not
            // installed (#1636), and a name the sheet does not hold throws.
            _ = sheet.Picture(name);
        }
        Console.WriteLine($"Collectible block load picture: {sheets.Length} Upper Crateria sheet(s) hold block {ReportedBlock:X3}'s picture over the room's load.");
    }

    [VerificationCommand("--collectible-slot-load-picture")]
    internal static int CollectibleSlotLoadPictureCommand()
    {
        Suite(nameof(VerifyCollectibleSlotLoadPicture), VerifyCollectibleSlotLoadPicture);
        return 0;
    }
}
