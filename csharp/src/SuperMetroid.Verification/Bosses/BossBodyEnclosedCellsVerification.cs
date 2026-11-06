using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses;

/// <summary>
/// Holds every installed boss body picture to the whole-picture rule (#1513): no picture has a hole inside its outline
/// where a whole character is missing. A body is pictured cell by cell, eight pixels square from its corner; a cell all of
/// whose pixels show nothing, and which no clear pixel joins to the picture's edge, is a gap the body's own drawing leaves
/// for another part to fill, as Phantoon's body leaves its eye socket for his eye. Single clear pixels and gaps between a
/// figure's limbs that reach the picture's edge are the artwork's own and pass.
/// </summary>
internal static class BossBodyEnclosedCellsVerification
{
    /// <summary>The pixels across and down one body cell.</summary>
    private const int CellPixels = 8;

    /// <summary>Fails, naming every boss body picture with a clear cell enclosed by its outline.</summary>
    internal static void VerifyBossBodyEnclosedCells()
    {
        string directory = RepositoryInstallation.Installation.BossBodySheetDirectory;
        var findings = new List<string>();
        int pictures = 0;
        foreach (BossBodyOwner owner in Enum.GetValues<BossBodyOwner>())
        {
            SpriteSheet sheet = InstalledFixtures.BossBodySheets.Of(owner).Sheet;
            SpriteImage image = sheet.Image;
            byte[] inks = SheetInkRules.InkPlane(directory, image);
            var clear = new bool[inks.Length];
            for (int pixel = 0; pixel < inks.Length; pixel++)
                clear[pixel] = image.IsColourZero(new SpriteImageInk(inks[pixel], frontTile: false));
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                pictures++;
                int enclosed = EnclosedClearCells(clear, image.Size.X, frame);
                if (enclosed > 0)
                    findings.Add($"{owner} body picture {name} has {enclosed} clear cells inside its outline");
            }
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} boss body pictures have holes inside their outlines:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Boss body pictures: {pictures} pictures have no clear cell inside their outlines.");
    }

    /// <summary>
    /// How many whole cells of <paramref name="frame"/> show nothing and are joined by no clear pixel to its rectangle's edge.
    /// </summary>
    private static int EnclosedClearCells(bool[] clear, int width, SpriteSheetFrame frame)
    {
        int across = frame.Size.X, down = frame.Size.Y;
        var outside = new bool[across * down];
        var pending = new Queue<int>();
        for (int y = 0; y < down; y++)
        {
            for (int x = 0; x < across; x++)
            {
                if ((x == 0 || y == 0 || x == across - 1 || y == down - 1) && IsClear(x, y))
                {
                    outside[y * across + x] = true;
                    pending.Enqueue(y * across + x);
                }
            }
        }
        while (pending.Count > 0)
        {
            int at = pending.Dequeue();
            int x = at % across, y = at / across;
            Visit(x - 1, y);
            Visit(x + 1, y);
            Visit(x, y - 1);
            Visit(x, y + 1);
        }
        int enclosed = 0;
        for (int top = 0; top + CellPixels <= down; top += CellPixels)
        {
            for (int left = 0; left + CellPixels <= across; left += CellPixels)
            {
                bool hole = true;
                for (int y = top; y < top + CellPixels && hole; y++)
                {
                    for (int x = left; x < left + CellPixels && hole; x++)
                        hole = IsClear(x, y) && !outside[y * across + x];
                }
                if (hole)
                    enclosed++;
            }
        }
        return enclosed;

        // Whether the frame's pixel at x, y shows nothing.
        bool IsClear(int x, int y) => clear[(frame.Corner.Y + y) * width + frame.Corner.X + x];

        // Marks the clear pixel at x, y as joined to the edge and follows it on.
        void Visit(int x, int y)
        {
            if (x < 0 || y < 0 || x >= across || y >= down || outside[y * across + x] || !IsClear(x, y))
                return;
            outside[y * across + x] = true;
            pending.Enqueue(y * across + x);
        }
    }

    /// <summary>Runs the boss body pictures' enclosed-cell rule.</summary>
    [VerificationCommand("--boss-body-enclosed-cells")]
    internal static int BossBodyEnclosedCellsCommand()
    {
        Suite(nameof(VerifyBossBodyEnclosedCells), VerifyBossBodyEnclosedCells);
        return 0;
    }
}
