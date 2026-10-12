using System.Text.Json;
using SuperMetroid.EnsureAnalyzer;

namespace SuperMetroid.CartridgeEncodingAudit;

/// <summary>
/// Writes the #641 inventory: every audited file grouped into independently migratable subsystems,
/// with its occurrences counted by rule and by proposed audit class.
/// </summary>
internal static class EncodingInventory
{
    // First match wins. Boss and sequence names precede the generic families that also contain them.
    private static readonly (string Fragment, string Subsystem)[] NameRules =
    [
        ("FakeKraid", Enemies), ("MiniKraid", Enemies),
        ("MotherBrain", "Boss: Mother Brain"), ("Shitroid", "Boss: Mother Brain"), ("Zebetite", "Boss: Mother Brain"),
        ("Kraid", "Boss: Kraid"), ("Phantoon", "Boss: Phantoon"), ("Draygon", "Boss: Draygon"),
        ("CeresBaby", Sequences), ("Ridley", "Boss: Ridley"), ("Crocomire", "Boss: Crocomire"),
        ("Botwoon", "Boss: Botwoon"), ("SporeSpawn", "Boss: Spore Spawn"), ("Torizo", "Boss: Torizo"),
        ("Ceres", Sequences), ("Escape", Sequences), ("Gunship", Sequences), ("Intro", Sequences),
        ("Ending", Sequences), ("Credits", Sequences), ("ZebesExplosion", Sequences), ("Cinematic", Sequences),
        ("EnemyProjectile", EnemyProjectiles),
        ("Samus", Samus), ("Grapple", Samus), ("Xray", Samus), ("Beam", Samus), ("Missile", Samus),
        ("PowerBomb", Samus), ("Shinespark", Samus), ("ChargeFlare", Samus),
        ("Projectile", EnemyProjectiles),
        ("Plm", Plms), ("Door", Plms), ("Collectible", Plms), ("Station", Plms),
        ("PaletteFx", RoomEffects), ("Hdma", RoomEffects), ("Fx", RoomEffects), ("Earthquake", RoomEffects),
    ];

    private const string Enemies = "Enemies: regular AI and instruction programs";
    private const string EnemyProjectiles = "Enemy projectiles";
    private const string Samus = "Samus: movement, poses and projectiles";
    private const string Plms = "PLMs and doors";
    private const string RoomEffects = "Room FX, palette FX and HDMA";
    private const string Sequences = "Cinematics and scripted sequences";

    /// <summary>Writes the inventory JSON to <paramref name="path"/>.</summary>
    public static void Write(EncodingMeasurement measurement, string path)
    {
        var subsystems = measurement.Files
            .Where(file => file.Occurrences.Count > 0 || file.StorageWidthMembers > 0)
            .GroupBy(file => Subsystem(file.Path))
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => new
            {
                name = group.Key,
                totals = Totals(group.SelectMany(file => file.Occurrences).ToList(), group.Sum(file => file.StorageWidthMembers)),
                addressInterpretingTypes = group.SelectMany(file => file.AddressInterpretingTypes)
                    .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
                files = group.Select(file => new
                {
                    path = file.Path,
                    project = file.Project,
                    totals = Totals(file.Occurrences, file.StorageWidthMembers),
                }).ToList(),
            })
            .ToList();
        var all = measurement.Files.SelectMany(file => file.Occurrences).ToList();
        var document = new
        {
            issue = 641,
            regenerate = "dotnet run --project csharp/src/SuperMetroid.CartridgeEncodingAudit -c Release -- " +
                "--solution csharp/SuperMetroid.Full.slnx --write",
            rules = new Dictionary<string, string>
            {
                [nameof(CartridgeEncodingRule.RawHexLiteral)] = "SME6340: hexadecimal literal in functional code (ratcheted)",
                [nameof(CartridgeEncodingRule.PointerDispatch)] =
                    "SME6341: primitive compared with, offset from or switched over an address-valued code constant (ratcheted)",
                [nameof(CartridgeEncodingRule.OpaqueWordArray)] = "SME6342: integral array literal holding hexadecimal words (ratcheted)",
                [nameof(CartridgeEncodingRule.CatalogHexLiteral)] = "hexadecimal literal owned by a named definition catalog (inventoried)",
                [nameof(CartridgeEncodingRule.JustifiedHexLiteral)] =
                    "functional hexadecimal literal with a reviewed same-line allowance (inventoried)",
                [nameof(CartridgeEncodingRule.RawWidthOperation)] =
                    "SME6345: shift by a byte/word/bank width or byte/word/bank mask in functional code (ratcheted, #223)",
                [nameof(CartridgeEncodingRule.OwnedWidthOperation)] =
                    "width operation inside a catalog or [SnesEncodingOwner] type that documents it (inventoried)",
                [nameof(CartridgeEncodingRule.JustifiedWidthOperation)] =
                    "functional width operation with a reviewed same-line allowance (inventoried)",
            },
            classes = new Dictionary<string, string>
            {
                [nameof(CartridgeEncodingClass.NumericQuantity)] = "1. real numeric quantity: use decimal and ordinary or unit types",
                [nameof(CartridgeEncodingClass.DomainIdentity)] = "2. mutually exclusive identity: an enum once the domain is proven",
                [nameof(CartridgeEncodingClass.ComposableBits)] = "3. composable bits: [Flags] only where the cartridge composes them",
                [nameof(CartridgeEncodingClass.StructuredInstruction)] = "4. structured instruction or definition: a typed record, timeline or state transition",
                [nameof(CartridgeEncodingClass.CartridgeAddress)] = "5. cartridge address or encoded boundary value: a typed catalog or decoder",
                [nameof(CartridgeEncodingClass.Unknown)] = "6. unknown semantics: kept at the boundary under an explicit unresolved name",
            },
            classification = "Each occurrence's class is proposed by the scanner from its syntax (array element, address range, " +
                "enum or [Flags] member, switch arm, bitwise operand, pointer-named context, equality or arithmetic); " +
                "the subsystem migration confirms or corrects it.",
            totals = Totals(all, measurement.Files.Sum(file => file.StorageWidthMembers)),
            subsystems,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true }) + "\n");
    }

    /// <summary>The subsystem a repository-relative source path migrates with.</summary>
    public static string Subsystem(string path)
    {
        string[] segments = path.Split('/');
        string project = segments.Length > 2 ? segments[2] : path;
        string name = Path.GetFileNameWithoutExtension(path);
        switch (project)
        {
            case "SuperMetroid.AssetExtraction":
                return "Asset extraction (encoded boundary)";
            case "SuperMetroid.Diagnostics":
                return "Diagnostics";
            case "SuperMetroid.Core":
                break;
            default:
                return "Hosts and GPU renderer";
        }
        string directory = segments.Length > 3 ? segments[3] : string.Empty;
        if (directory is "Game" or "Assets" or "Rooms" or "Rendering")
        {
            foreach (var (fragment, subsystem) in NameRules)
            {
                if (name.Contains(fragment, StringComparison.Ordinal))
                    return subsystem;
            }
        }
        return directory switch
        {
            "Game" => name.Contains("Enemy", StringComparison.Ordinal) || name.Contains("Instruction", StringComparison.Ordinal)
                ? Enemies : "Gameplay core",
            "Assets" => "Asset catalogs and decoders",
            "Rooms" => "Rooms: setup, scrolling and level data",
            "Audio" => "Audio",
            "Frontend" => "Menus, map, HUD and messages",
            "Hardware" or "Rendering" or "Spatial" => "Rendering and hardware",
            "Runtime" or "Input" => "Runtime and input",
            _ => "Core infrastructure",
        };
    }

    private static Dictionary<string, int> Totals(IReadOnlyCollection<CartridgeEncodingOccurrence> occurrences, int storageWidthMembers)
    {
        var totals = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var group in occurrences.GroupBy(occurrence => occurrence.Rule).OrderBy(group => group.Key))
            totals[group.Key.ToString()] = group.Count();
        foreach (var group in occurrences.GroupBy(occurrence => occurrence.Class).OrderBy(group => group.Key))
            totals["class:" + group.Key] = group.Count();
        if (storageWidthMembers > 0)
            totals["storageWidthMembers"] = storageWidthMembers;
        return totals;
    }
}
