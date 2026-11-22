using System.Collections.Immutable;
using System.Text;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>Reads the retail room-FX headers and renders the generated Core catalog that holds them.</summary>
internal static partial class RoomFxRecordCatalogSource
{
    /// <summary>Each catalog selection's member name in <see cref="RoomFxDefinitions"/>, which names it in the generated catalog.</summary>
    private static readonly Dictionary<RoomFxSelection, string> SelectionNames = SelectionMembers()
        .ToDictionary(member => member.Value, member => member.Name);

    /// <summary>The area door catalogs, each naming the doors of its area's rooms; a door none names is the shared catalog's.</summary>
    private static readonly Dictionary<string, string[]> DoorClasses = DoorMembers()
        .GroupBy(member => member.Name, StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.Select(member => member.Owner).ToArray(), StringComparer.Ordinal);

    /// <summary>The selections <see cref="RoomFxDefinitions"/> names.</summary>
    [NamedMembers(typeof(RoomFxDefinitions))]
    private static partial (RoomFxSelection Value, string Owner, string Name)[] SelectionMembers();

    /// <summary>The doors each area's door catalog names.</summary>
    [NamedMembers(typeof(BrinstarDoorDefinitions), typeof(CeresDoorDefinitions), typeof(CrateriaDoorDefinitions),
        typeof(MaridiaDoorDefinitions), typeof(NorfairDoorDefinitions), typeof(TourianDoorDefinitions), typeof(WreckedShipDoorDefinitions))]
    private static partial (DoorDefinition Value, string Owner, string Name)[] DoorMembers();

    /// <summary>The blends <see cref="RoomEffectBlendDefinitions"/> names.</summary>
    [NamedMembers(typeof(RoomEffectBlendDefinitions))]
    private static partial (RoomEffectBlend Value, string Owner, string Name)[] BlendMembers();

    /// <summary>The checked-in generated catalog the renderer's output must equal.</summary>
    internal static readonly string GeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Fx", "RoomFxDefinitions.cs"));

    /// <summary>
    /// Every captured record by its bank-$83 pointer, the entry door of each door record, and each header's record pointers
    /// in native order (#1546).
    /// </summary>
    internal sealed record RetailRoomFxCapture(
        SortedDictionary<ushort, RoomEffectDefinition> Records,
        SortedDictionary<ushort, DoorDefinition> Doors,
        SortedDictionary<ushort, ushort[]> Lists);

    /// <summary>The names of the records Mother Brain's fight applies by itself, after her room's default record, by pointer.</summary>
    private static readonly SortedDictionary<ushort, string> MotherBrainRecordNames = new()
    {
        [MotherBrainFxRecordPointers.Initial] = "MotherBrainAcidRises",
        [MotherBrainFxRecordPointers.FakeDeath] = "MotherBrainAcidDrains",
    };

    /// <summary>
    /// Walks every compiled header's records, plus the Mother Brain records her FX entries index after its default, each
    /// decoded in the area of the rooms that load its header (<see cref="RoomFxSelectionAreas"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">A record shared by headers reads differently in one of them, as one shared
    /// by headers of areas whose lists select different effects does.</exception>
    internal static RetailRoomFxCapture CaptureRetailRoomFx(ISnesAddressSpace bus)
    {
        var records = new SortedDictionary<ushort, RoomEffectDefinition>();
        var doors = new SortedDictionary<ushort, DoorDefinition>();
        var lists = new SortedDictionary<ushort, ushort[]>();
        foreach (ushort header in RoomFxSources.Pointers)
        {
            AreaId area = RoomFxSelectionAreas.Of(RoomFxSources.Fx(header));
            var list = new List<ushort>();
            ushort pointer = header;
            for (int guard = 0; ; guard++)
            {
                if (guard >= 256)
                    throw new InvalidDataException($"Room FX header $83:{header:X4} does not terminate.");
                if (pointer < 0x8000)
                    throw new InvalidDataException($"Room FX header $83:{header:X4} crossed below the LoROM window.");
                ushort door = DoorWord(bus, pointer);
                if (door == RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                {
                    // The terminator is the end of the header's list, not a record.
                    break;
                }
                Capture(bus, records, pointer, area);
                list.Add(pointer);
                if (door == 0)
                {
                    // Mother Brain selects these records by FX index after room load; the ordinary
                    // door walk stops at the default record that precedes them.
                    for (ushort next = unchecked((ushort)(pointer + RoomFxToolingRomData.Record.ByteCount));
                         MotherBrainFxRecordPointers.DirectRecords.Contains(next);
                         next = unchecked((ushort)(next + RoomFxToolingRomData.Record.ByteCount)))
                    {
                        Capture(bus, records, next, area);
                        list.Add(next);
                    }
                    break;
                }
                doors.Add(pointer, DoorHeaderSources.Door(door));
                pointer = unchecked((ushort)(pointer + RoomFxToolingRomData.Record.ByteCount));
            }
            lists.Add(header, list.ToArray());
        }
        return new RetailRoomFxCapture(records, doors, lists);
    }

    /// <summary>The door word of the record at <paramref name="pointer"/>: a door's pointer, zero for a default, or the terminator.</summary>
    private static ushort DoorWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(bus, RoomFxRomDataBanksTooling.RoomDefinitions | pointer);

    /// <summary>Reads the record at <paramref name="pointer"/>, in a room of <paramref name="area"/>, into <paramref name="records"/>.</summary>
    private static void Capture(ISnesAddressSpace bus, SortedDictionary<ushort, RoomEffectDefinition> records, ushort pointer,
        AreaId area)
    {
        int source = RoomFxRomDataBanksTooling.RoomDefinitions | pointer;
        RoomEffectDefinition record = RoomFxRecordCodec.Decode(area,
            RomDataReader.ReadFixedBank(bus, source, RoomFxToolingRomData.Record.ByteCount),
            $"room-FX record $83:{pointer:X4}");
        if (records.TryGetValue(pointer, out RoomEffectDefinition? existing))
        {
            // The effect lists are arrays, so the record's own equality compares them by reference; compare them in order here.
            if (!(existing with { ColourEffects = record.ColourEffects, BlockAnimations = record.BlockAnimations }).Equals(record)
                || !existing.ColourEffects.SequenceEqual(record.ColourEffects) || !existing.BlockAnimations.SequenceEqual(record.BlockAnimations))
                throw new InvalidDataException($"Shared room-FX record $83:{pointer:X4} reads differently from another header.");
        }
        else
            records.Add(pointer, record);
    }

    /// <summary>Renders an entry door as the generated catalog spells it.</summary>
    private static string RenderDoor(DoorDefinition door) => $"{DeclaringDoorClass(door.Name)}.{door.Name}";

    /// <summary>The door-definition class that names <paramref name="name"/>: its area's, or the shared one for doors no area names.</summary>
    private static string DeclaringDoorClass(string name)
    {
        string[] areas = DoorClasses.GetValueOrDefault(name) ?? [];
        return areas.Length switch
        {
            0 => nameof(DoorDefinitions),
            1 => areas[0],
            _ => throw new InvalidOperationException($"Door {name} is named by {areas.Length} area classes."),
        };
    }

    /// <summary>
    /// Renders a room effect's blend as the generated catalog spells it: the <see cref="RoomEffectBlendDefinitions"/>
    /// member it is, or null where the effect shows none (#1545).
    /// </summary>
    private static string EffectBlend(RoomEffectBlend? blend)
    {
        if (blend is null)
            return "null";
        (RoomEffectBlend Value, string Owner, string Name)[] members = [.. BlendMembers().Where(member => ReferenceEquals(member.Value, blend))];
        return members.Length == 1 ? $"{nameof(RoomEffectBlendDefinitions)}.{members[0].Name}"
            : throw new InvalidDataException($"Blend {blend} is not one {nameof(RoomEffectBlendDefinitions)} member.");
    }

    /// <summary>Renders one member of a closed identity as the generated catalog spells it.</summary>
    private static string Member<TIdentity>(TIdentity value)
        where TIdentity : struct, Enum =>
        Enum.IsDefined(value) ? $"{typeof(TIdentity).Name}.{value}"
            : throw new InvalidDataException($"{value} is not a {typeof(TIdentity).Name} member.");

    /// <summary>
    /// Renders the liquid a record fills its room with as the generated catalog spells it (#1695): its kind's constructor
    /// with each fact named, or null for an effect with no liquid.
    /// </summary>
    /// <exception cref="InvalidDataException">The liquid is of no kind the catalog spells.</exception>
    private static string Liquid(RoomLiquid? liquid) => liquid switch
    {
        null => "null",
        RoomWater water => $"new {nameof(RoomWater)}({Member(water.Tide)}, {nameof(RoomWater.SurfaceDrift)}: {Bool(water.SurfaceDrift)}, " +
            $"{nameof(RoomWater.BackgroundWave)}: {Bool(water.BackgroundWave)}, " +
            $"{nameof(RoomWater.IgnoresSurfaceUntilEnabled)}: {Bool(water.IgnoresSurfaceUntilEnabled)})",
        RoomLavaOrAcid lavaOrAcid => $"new {nameof(RoomLavaOrAcid)}({Member(lavaOrAcid.Tide)}, {Member(lavaOrAcid.Haze)})",
        _ => throw new InvalidDataException($"{liquid.GetType().Name} is no room liquid the catalog spells."),
    };

    /// <summary>Renders a fact as the generated catalog spells it: its C# literal.</summary>
    private static string Bool(bool value) => value ? "true" : "false";

    /// <summary>Renders an effect list as the generated catalog spells it: its members in order, in brackets.</summary>
    private static string Effects<TIdentity>(ImmutableArray<TIdentity> effects)
        where TIdentity : struct, Enum =>
        $"[{string.Join(", ", effects.Select(Member))}]";

    /// <summary>Renders a surface height as the generated catalog spells it: its pixel row in decimal, or null where the record names none.</summary>
    private static string SurfaceY(int? y) =>
        y is int named ? named.ToString(System.Globalization.CultureInfo.InvariantCulture) : "null";

    /// <summary>
    /// Renders a liquid level's speed as the generated catalog spells it (#2555): none, whole pixels an update, or a
    /// fraction of a pixel an update in lowest terms.
    /// </summary>
    private static string LevelSpeed(SignedEightEight speed)
    {
        int numerator = speed.InTwoHundredFiftySixthsOfPixel;
        if (numerator == 0)
            return $"{nameof(SignedEightEight)}.{nameof(SignedEightEight.Zero)}";
        int denominator = 256;
        while (denominator > 1 && numerator % 2 == 0)
        {
            numerator /= 2;
            denominator /= 2;
        }
        string pixels = numerator.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return denominator == 1
            ? $"{nameof(SignedEightEight)}.{nameof(SignedEightEight.FromPixels)}({pixels})"
            : $"{nameof(SignedEightEight)}.{nameof(SignedEightEight.FromPixelFraction)}({pixels}, " +
                $"{denominator.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
    }

    /// <summary>Renders one record as the generated catalog spells it.</summary>
    private static string RenderRecord(RoomEffectDefinition record) =>
        $"new({SurfaceY(record.LiquidStartLevel)}, {SurfaceY(record.LiquidTargetLevel)}, " +
        $"{LevelSpeed(record.LiquidLevelSpeed)}, " +
        $"{record.LiquidMoveDelay.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {Member(record.Type)}, " +
        $"{Member(record.VisorAnimation)}, {EffectBlend(record.EffectBlend)}, " +
        $"{Liquid(record.Liquid)}, {Effects(record.ColourEffects)}, " +
        $"{Effects(record.BlockAnimations)}, {Member(record.Tint)})";

    /// <summary>
    /// The summary the generated catalog gives a selection (#2491): what its rooms show entered through a door no door
    /// effect names, and how many entry doors select an effect of their own. A selection with no default effect loads none,
    /// so the overlay and the HUD keep their own colours; one whose default shows nothing still lays its tint.
    /// </summary>
    private static string SelectionSummary(RoomEffectDefinition? defaultEffect, int doorEffects)
    {
        string shown = defaultEffect is null
            ? "no room effect, the overlay and the HUD keeping their own colours"
            : EffectSummary(defaultEffect);
        string doors = doorEffects switch
        {
            0 => string.Empty,
            1 => " unless entered through the one door that selects an effect of its own",
            _ => $" unless entered through one of the {doorEffects.ToString(System.Globalization.CultureInfo.InvariantCulture)} doors that each select an effect of their own",
        };
        return $"A selection whose rooms show {shown}{doors}.";
    }

    /// <summary>What a room effect shows (#2491): its overlay and liquid surface, the colour effects and block animations it starts, its tint and Samus's visor.</summary>
    private static string EffectSummary(RoomEffectDefinition effect)
    {
        var parts = new List<string> { OverlaySummary(effect.Type) + SurfaceSummary(effect) };
        if (effect.ColourEffects.Length > 0)
            parts.Add("starting " + CrefList(effect.ColourEffects));
        if (effect.BlockAnimations.Length > 0)
            parts.Add((effect.ColourEffects.Length > 0 ? "and animating " : "animating ") + CrefList(effect.BlockAnimations));
        parts.Add(effect.Tint == RoomEffectTint.None ? "in the untinted look" : $"in the {Cref(effect.Tint)} tint");
        if (effect.VisorAnimation == RoomVisorAnimation.Animated)
            parts.Add("Samus's visor cycling its room shades");
        return string.Join(", ", parts);
    }

    /// <summary>What a room effect of <paramref name="type"/> shows on the overlay.</summary>
    /// <exception cref="InvalidDataException">The type is no room-effect type the catalog describes.</exception>
    private static string OverlaySummary(RoomFxType type) => type switch
    {
        RoomFxType.None => "no effect of their own",
        RoomFxType.Lava => "lava",
        RoomFxType.Acid => "acid",
        RoomFxType.Water => "water",
        RoomFxType.Spores => "spores",
        RoomFxType.Rain => "rain",
        RoomFxType.Fog => "fog",
        RoomFxType.ScrollingSky => "the sky outside",
        RoomFxType.Fireflea => "the Fireflea rooms' darkness",
        RoomFxType.TourianEntranceStatue => "the statue room's surface",
        RoomFxType.CeresRidley => "Ceres Ridley's turning floor",
        RoomFxType.CeresElevator => "the Ceres elevator shaft",
        RoomFxType.CeresHaze => "the haze of Ceres's destruction",
        _ => throw new InvalidDataException($"{type} is no room-effect type the catalog describes."),
    };

    /// <summary>Where a room effect's liquid surface starts and the line it rises or falls to; nothing for an effect with no surface.</summary>
    private static string SurfaceSummary(RoomEffectDefinition effect)
    {
        if (effect.LiquidStartLevel is not int start)
            return string.Empty;
        string surface = $", the liquid level starting at line {start.ToString(System.Globalization.CultureInfo.InvariantCulture)}";
        if (effect.LiquidTargetLevel is not int target)
            return surface;
        // Levels are lines from the room's top, so a target above the start is a rise.
        return surface + (target < start ? " and rising to line " : target > start ? " and falling to line " : " and holding at line ") +
            target.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>A reference to one member of a closed identity, as the generated documentation links it.</summary>
    private static string Cref<TIdentity>(TIdentity value)
        where TIdentity : struct, Enum => $"<see cref=\"{Member(value)}\"/>";

    /// <summary>References to <paramref name="values"/>, in order, joined as English joins a list.</summary>
    private static string CrefList<TIdentity>(ImmutableArray<TIdentity> values)
        where TIdentity : struct, Enum
    {
        string[] references = [.. values.Select(Cref)];
        return references.Length == 1 ? references[0]
            : string.Join(", ", references[..^1]) + " and " + references[^1];
    }

    /// <summary>
    /// Renders the named headers for the generated Core catalog, each as its door records and its default record (#1546),
    /// then the records Mother Brain's fight applies by itself, which follow her room's default record.
    /// </summary>
    /// <exception cref="InvalidDataException">A header lists a record after its default that Mother Brain's fight does not apply.</exception>
    internal static string RenderRoomFxDefinitions(RetailRoomFxCapture capture)
    {
        Ensure.NotNull(capture);
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");
        source.AppendLine("using SuperMetroid.Core.Fx.Artwork;");
        source.AppendLine("using SuperMetroid.Core.Fx.ColourEffects;");
        source.AppendLine("using SuperMetroid.Core.Rooms.Doors;");
        source.AppendLine();
        source.AppendLine("namespace SuperMetroid.Core.Fx;");
        source.AppendLine();
        source.AppendLine("/// <summary>");
        source.AppendLine("/// The room states' effect selections, each holding its door effects in the game's order and its default effect, and the effects");
        source.AppendLine("/// Mother Brain's fight applies by itself; regenerate with --generate-room-effects.");
        source.AppendLine("/// </summary>");
        source.AppendLine("public static partial class RoomFxDefinitions");
        source.AppendLine("{");
        bool first = true;
        // A selection the cartridge stores under two headers (the quicksand shafts') is one member, written where its first
        // header stands; the later header must hold the same records.
        var written = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach ((ushort header, ushort[] pointers) in capture.Lists)
        {
            string name = SelectionNames[RoomFxSources.Fx(header)];
            string rendered = string.Join(" ", pointers.Select(pointer => capture.Doors.TryGetValue(pointer, out DoorDefinition? door)
                ? $"{RenderDoor(door)} {RenderRecord(capture.Records[pointer])}" : RenderRecord(capture.Records[pointer])));
            if (written.TryGetValue(name, out string? earlier))
            {
                if (!string.Equals(earlier, rendered, StringComparison.Ordinal))
                    throw new InvalidDataException($"Room FX headers of {name} hold different records.");
                continue;
            }
            written.Add(name, rendered);
            if (!first)
                source.AppendLine();
            first = false;
            ushort[] doorRecords = [.. pointers.TakeWhile(capture.Doors.ContainsKey)];
            ushort[] rest = [.. pointers.Skip(doorRecords.Length)];
            if ((rest.Length > 0 && MotherBrainRecordNames.ContainsKey(rest[0])) ||
                rest.Skip(1).Any(pointer => !MotherBrainRecordNames.ContainsKey(pointer)))
                throw new InvalidDataException($"Room FX header {name} lists a record after its default that Mother Brain's fight does not apply.");
            source.AppendLine($"    /// <summary>{SelectionSummary(rest.Length == 0 ? null : capture.Records[rest[0]], doorRecords.Length)}</summary>");
            if (doorRecords.Length == 0)
                source.AppendLine($"    public static RoomFxSelection {name} {{ get; }} = new([],");
            else
            {
                source.AppendLine($"    public static RoomFxSelection {name} {{ get; }} = new(");
                source.AppendLine("    [");
                foreach (ushort pointer in doorRecords)
                    source.AppendLine($"        new({RenderDoor(capture.Doors[pointer])}, {RenderRecord(capture.Records[pointer])}),");
                source.AppendLine("    ],");
            }
            source.AppendLine($"        {(rest.Length == 0 ? "null" : RenderRecord(capture.Records[rest[0]]))});");
        }
        foreach ((ushort pointer, string name) in MotherBrainRecordNames)
        {
            source.AppendLine();
            source.AppendLine($"    /// <summary>An effect Mother Brain's fight applies to her room after its default effect: {EffectSummary(capture.Records[pointer])}.</summary>");
            source.AppendLine($"    public static RoomEffectDefinition {name} {{ get; }} = {RenderRecord(capture.Records[pointer])};");
        }
        source.AppendLine("}");
        return source.ToString();
    }
}
