using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds every installed sprite sheet's map cell and HUD minimap pictures to the rule that a picture is named for the
/// domain thing it shows (#1513): a map cell by its kind, its walls and its exploration, a minimap cell by the same and its
/// shade, never by the character the cartridge's tilemap word drew it from, that word's palette row or its flip. A
/// numbered segment in such a frame's name is that cartridge identity: the room maps and the pause map named each cell
/// <c>Map.Cell.{character}.{row}.Back</c>, and the HUD each minimap cell <c>Hud.Minimap.{character}.{flip}.{shade}</c>.
/// </summary>
internal static class SheetPictureIdentitiesVerification
{
    /// <summary>The frame name prefixes of the pictures of map cells and minimap cells.</summary>
    private static readonly string[] CellPrefixes = ["Map.Cell.", "Hud.Minimap."];

    /// <summary>Fails, naming every map or minimap cell frame of every sprite sheet whose name holds a number.</summary>
    internal static void VerifyCellPicturesNamedByDomain()
    {
        var findings = new List<string>();
        int cells = 0;
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            foreach (string name in sheet.frames.Keys.Concat(sheet.blanks).Order(StringComparer.Ordinal))
            {
                if (!CellPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                    continue;
                cells++;
                if (name.Split('.').Any(segment => segment.Length > 0 && segment.All(char.IsAsciiDigit)))
                    findings.Add($"{id} frame {name} is named by a cartridge tile identity, not by the cell it shows");
            }
        }
        AssertTrue(cells > 0, "the installed sprite sheets hold map and minimap cell pictures");
        AssertTrue(findings.Count == 0,
            $"{findings.Count} map and minimap cell pictures are named by cartridge tile identities:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Cell pictures: each of {cells} map and minimap cell pictures is named by the cell it shows.");
    }

    /// <summary>Runs the cell pictures' identity rule.</summary>
    [VerificationCommand("--sheet-picture-identities")]
    internal static int SheetPictureIdentitiesCommand()
    {
        Suite(nameof(VerifyCellPicturesNamedByDomain), VerifyCellPicturesNamedByDomain);
        return 0;
    }
}
