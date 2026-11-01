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
/// 100% movie shows Upper Crateria's block 08E with graphics slot 0 empty, and the sheet had no picture for it.
/// </summary>
internal static class CollectibleSlotLoadPictureVerification
{
    /// <summary>The block the 100% movie shows over an empty graphics slot.</summary>
    private const int ReportedBlock = 0x8E;

    /// <summary>The graphics slot that block belongs to.</summary>
    private const int ReportedSlot = 0;

    /// <summary>Every Upper Crateria tileset's installed sheet holds block 08E's picture for graphics slot 0 showing the room's load.</summary>
    internal static void VerifyCollectibleSlotLoadPicture()
    {
        InstalledRoomArtwork artwork = RepositoryInstallation.Installation.LoadRoomBlockSheets();
        ImmutableArray<AppearanceInput> slot = [new CollectibleSlotAppearance(ReportedSlot)];
        string name = RoomBlockPictureNames.Of(ReportedBlock, RoomBlockArtwork.Tileset, slot, [RoomAppearanceValues.Loaded]);
        RoomBlockSheet[] sheets = [.. artwork.Sheets.Where(pair => pair.Key.GraphicsSet == RoomGraphicsSet.UpperCrateria).Select(pair => pair.Value)];
        AssertTrue(sheets.Length > 0, "the installation has an Upper Crateria tileset sheet");
        foreach (RoomBlockSheet sheet in sheets)
        {
            SpriteSheetFrame picture = sheet.Picture(name);
            AssertEqual(RoomBlockSheet.BlockPixels, (int)picture.Size.X, $"{sheet.Sheet.Image.Name} picture {name} is one block wide");
        }
        Console.WriteLine($"Collectible slot load picture: {sheets.Length} Upper Crateria sheet(s) hold {name}.");
    }

    [VerificationCommand("--collectible-slot-load-picture")]
    internal static int CollectibleSlotLoadPictureCommand()
    {
        Suite(nameof(VerifyCollectibleSlotLoadPicture), VerifyCollectibleSlotLoadPicture);
        return 0;
    }
}
