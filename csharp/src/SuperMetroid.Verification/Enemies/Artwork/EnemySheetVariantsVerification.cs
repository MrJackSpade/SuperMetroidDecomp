using System.Security.Cryptography;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds the installed enemy sheets to the rule that an enemy's variants are separate sheets only where the player can tell
/// them apart (#1643): no two installed sheets of one enemy hold the same frames, pictures and looks; the Chozo statue, whose
/// only difference the player sees is its site's statue colours, installs one sheet per site, and the elevator, which
/// shows no difference, installs one sheet; and every placement in the room data of an enemy the installation pictures
/// resolves, through its population's artwork, to an installed variant and sheet.
/// </summary>
internal static class EnemySheetVariantsVerification
{
    /// <summary>Checks the installed enemy sheets' variants against the differences the player can see.</summary>
    internal static void VerifyEnemySheetVariants()
    {
        InstalledEnemySheets installed = InstalledFixtures.EnemySheets;
        Dictionary<EnemyDefinitionId, string[]> sheetsOf = installed.EnemyVariants
            .GroupBy(entry => entry.Key)
            .ToDictionary(group => group.Key,
                group => group.Select(entry => entry.Value.Sheet.Image.Name).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray());
        var failures = new List<string>();
        int sheets = DistinctSheets(sheetsOf, failures);
        int placements = ResolvedPlacements(installed, sheetsOf, failures);
        string[] chozo = sheetsOf.GetValueOrDefault(EnemyDefinitionId.Chozo, []);
        ChozoStatueSite[] sites = [.. SheetsBySite(installed, failures).Keys.Order()];
        AssertTrue(sites.Length > 0, "a room places the Chozo statue");
        if (chozo.Length != sites.Length)
            failures.Add($"Chozo installs {chozo.Length} sheets ({string.Join(", ", chozo)}), not one for each of its {sites.Length} statue sites ({string.Join(", ", sites)})");
        string[] elevator = sheetsOf.GetValueOrDefault(EnemyDefinitionId.Elevator, []);
        if (elevator.Length != 1)
            failures.Add($"Elevator installs {elevator.Length} sheets ({string.Join(", ", elevator)}), not one");
        AssertTrue(failures.Count == 0,
            $"{failures.Count} installed enemy sheet variants differ in nothing the player sees, or resolve wrongly:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Enemy sheet variants: {sheets} installed enemy sheets each differ from their enemy's others; {placements} placements resolve to installed sheets; "
            + $"Chozo installs one sheet per statue site ({sites.Length}), the elevator one.");
    }

    /// <summary>
    /// Adds to <paramref name="failures"/> each pair of installed sheets of one enemy whose installed manifests, which hold
    /// the sheet's frames, the pictures each shows and the looks it is coloured in, are the same.
    /// </summary>
    /// <returns>The installed enemy sheets checked.</returns>
    private static int DistinctSheets(Dictionary<EnemyDefinitionId, string[]> sheetsOf, List<string> failures)
    {
        string directory = RepositoryInstallation.Installation.EnemySheetDirectory;
        int sheets = 0;
        foreach ((EnemyDefinitionId enemy, string[] names) in sheetsOf.OrderBy(entry => entry.Key))
        {
            // An enemy with one sheet has no other its sheet could repeat.
            if (names.Length == 1)
            {
                sheets++;
                continue;
            }
            var spelledBy = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string name in names)
            {
                sheets++;
                string spelling = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(directory, SpriteSheetStore.FrameFile(name)))));
                if (spelledBy.TryGetValue(spelling, out string? same))
                    failures.Add($"{enemy}: sheets {same} and {name} hold the same frames, pictures and looks");
                else
                    spelledBy.Add(spelling, name);
            }
        }
        return sheets;
    }

    /// <summary>
    /// Adds to <paramref name="failures"/> each placement in the room data of an enemy the installation pictures whose
    /// population's artwork names no variant of it, or a variant or sheet that is not installed.
    /// </summary>
    /// <returns>The placements resolved.</returns>
    private static int ResolvedPlacements(InstalledEnemySheets installed, Dictionary<EnemyDefinitionId, string[]> sheetsOf, List<string> failures)
    {
        int placements = 0;
        foreach (RoomEnemyPopulationDefinition population in RoomEnemyPopulationDefinitions.Definitions)
        {
            foreach (RoomEnemyPlacement record in population.Placements.ToArray())
            {
                if (!sheetsOf.TryGetValue(record.DefinitionId, out string[]? sheets))
                    continue;
                placements++;
                if (!installed.For(population).enemies.TryGetValue(record.DefinitionId, out EnemyArtworkVariant<SpriteSheet>? variant))
                    failures.Add($"population {population.Name} places {record.DefinitionId}, but its artwork draws no variant of it");
                else if (!installed.variants.TryGetValue(variant.Name, out EnemyArtworkVariant<SpriteSheet>? held) || !ReferenceEquals(held, variant))
                    failures.Add($"population {population.Name} draws {record.DefinitionId} in variant {variant.Name}, which is not installed");
                else if (!sheets.Contains(variant.Sheet.Image.Name, StringComparer.Ordinal))
                    failures.Add($"population {population.Name} draws {record.DefinitionId} from sheet {variant.Sheet.Image.Name}, which is none of its installed sheets");
            }
        }
        AssertTrue(placements > 0, "the room data places an enemy the installation pictures");
        return placements;
    }

    /// <summary>
    /// The installed sheet every placement of the Chozo statue at each site draws from, adding to <paramref name="failures"/>
    /// each site whose placements draw from two sheets, and each sheet two sites draw from.
    /// </summary>
    private static Dictionary<ChozoStatueSite, string> SheetsBySite(InstalledEnemySheets installed, List<string> failures)
    {
        var bySite = new Dictionary<ChozoStatueSite, string>();
        foreach (RoomEnemyPopulationDefinition population in RoomEnemyPopulationDefinitions.Definitions)
        {
            foreach (RoomEnemyPlacement record in population.Placements.ToArray().Where(record => record.DefinitionId == EnemyDefinitionId.Chozo))
            {
                ChozoStatueSite site = record.SpawnSettingsOf<ChozoStatueSpawnSettings>().Site;
                string sheet = installed.For(population).Enemy(EnemyDefinitionId.Chozo).Sheet.Image.Name;
                if (!bySite.TryGetValue(site, out string? drawn))
                    bySite.Add(site, sheet);
                else if (drawn != sheet)
                    failures.Add($"Chozo statues at site {site} draw from two sheets, {drawn} and {sheet} (population {population.Name})");
            }
        }
        foreach (IGrouping<string, ChozoStatueSite> shared in bySite.GroupBy(entry => entry.Value, entry => entry.Key, StringComparer.Ordinal).Where(group => group.Count() > 1))
            failures.Add($"Chozo statues at sites {string.Join(", ", shared)} draw from one sheet, {shared.Key}, though their statue colours differ");
        return bySite;
    }

    /// <summary>Runs the installed enemy sheets' variant check.</summary>
    [VerificationCommand("--enemy-sheet-variants")]
    internal static int EnemySheetVariantsCommand()
    {
        Suite(nameof(VerifyEnemySheetVariants), VerifyEnemySheetVariants);
        return 0;
    }
}
