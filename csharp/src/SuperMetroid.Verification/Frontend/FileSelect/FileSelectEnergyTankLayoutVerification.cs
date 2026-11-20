using System.Collections.Immutable;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Saves;
using static SuperMetroid.Verification.Compatibility.ReplacedExpressionAssertions;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;

namespace SuperMetroid.Verification.Frontend.FileSelect;

/// <summary>
/// Confirms that file select's energy tanks laid out in rows of seven by index (#2206) sit where the replaced drawing walk
/// put them, for every tank count a save can hold.
/// </summary>
internal static class FileSelectEnergyTankLayoutVerification
{
    /// <summary>The most tanks Samus can hold.</summary>
    private const int MostTanks = 14;

    /// <summary>Compares every tank's place, for each count from none to fourteen, with the replaced walk's.</summary>
    internal static void VerifyFileSelectEnergyTankLayout()
    {
        FileSelectPresentation presentation = RetailPresentationFixture().FileSelect;
        for (int tanks = 0; tanks <= MostTanks; tanks++)
        {
            int maxHealth = tanks * 100 + 99;
            var save = global::SuperMetroid.Core.PrivateState.New<global::SuperMetroid.Core.Frontend.FileSelect.ShownFileSelectSave>([typeof(int), typeof(int), typeof(int), typeof(int)], new object?[] { (int)(maxHealth), (int)(maxHealth), (int)(1), (int)(2) });
            var page = new ShownFileSelectPage(FileSelectPage.MainWithData,
                SaveSlots.All.ToImmutableDictionary(slot => slot, slot => slot == SaveSlot.A ? save : null), SaveSlot.A, SaveSlot.A);
            Position16<PlanePixels>[] shown = [.. page.Pictures(presentation)
                .Where(picture => picture.Frame == FileSelectParts.TankFrame(filled: true)
                    || picture.Frame == FileSelectParts.TankFrame(filled: false))
                .Select(picture => picture.Origin)];
            Position16<PlanePixels> energy = presentation.Slot(dataManagement: false, SaveSlot.A).EnergyAnchor;
            Position16<PlanePixels>[] walked = WalkedTanks(energy, tanks);
            AssertEqual(walked.Length, shown.Length, $"{tanks} tanks shown");
            for (int tank = 0; tank < tanks; tank++)
                AssertSameAt(walked[tank], shown[tank], $"tank place of {tanks}", tank);
        }
        Console.WriteLine("File-select energy tanks: every count from none to fourteen sits where the replaced walk put it.");
    }

    /// <summary>
    /// The replaced walk: seven tanks from four tanks right of and one below ENERGY, then each later row of eight one tank
    /// higher, stepping back seven tanks.
    /// </summary>
    private static Position16<PlanePixels>[] WalkedTanks(Position16<PlanePixels> energy, int tanks)
    {
        var placed = new Position16<PlanePixels>[tanks];
        int x = energy.X + 4 * 8;
        int y = energy.Y + 8;
        int rowRemaining = 7;
        for (int tank = 0; tank < tanks; tank++)
        {
            placed[tank] = new(x, y);
            x += 8;
            if (--rowRemaining == 0)
            {
                x -= 7 * 8;
                y -= 8;
                rowRemaining = 8;
            }
        }
        return placed;
    }
}
