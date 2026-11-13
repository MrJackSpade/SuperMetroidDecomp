using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>Reads the retail room-FX headers and renders the generated Core catalog that holds them.</summary>
internal static class RoomFxRecordCatalogSource
{
    /// <summary>The checked-in generated catalog the renderer's output must equal.</summary>
    internal static readonly string GeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Fx", "RoomFxDefinitions.cs"));

    /// <summary>
    /// Every captured record by its bank-$83 pointer, the entry door of each door record, and each header's record pointers
    /// in native order (#1546).
    /// </summary>
    internal sealed record RetailRoomFxCapture(
        SortedDictionary<ushort, RoomFxRecordDefinition> Records,
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
        var records = new SortedDictionary<ushort, RoomFxRecordDefinition>();
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
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), RoomFxRomDataBanksTooling.RoomDefinitions | pointer);

    /// <summary>Reads the record at <paramref name="pointer"/>, in a room of <paramref name="area"/>, into <paramref name="records"/>.</summary>
    private static void Capture(ISnesAddressSpace bus, SortedDictionary<ushort, RoomFxRecordDefinition> records, ushort pointer,
        AreaId area)
    {
        int source = RoomFxRomDataBanksTooling.RoomDefinitions | pointer;
        RoomFxRecordDefinition record = RoomFxRecordCodec.Decode(area,
            RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source, RoomFxToolingRomData.Record.ByteCount),
            $"room-FX record $83:{pointer:X4}");
        if (records.TryGetValue(pointer, out RoomFxRecordDefinition? existing))
        {
            // The effect lists are arrays, so the record's own equality compares them by reference; compare them in order here.
            if (!(existing with { PaletteEffects = record.PaletteEffects, AnimatedTiles = record.AnimatedTiles }).Equals(record)
                || !existing.PaletteEffects.SequenceEqual(record.PaletteEffects) || !existing.AnimatedTiles.SequenceEqual(record.AnimatedTiles))
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
        Type[] areas = [.. typeof(DoorDefinitions).Assembly.GetTypes().Where(type =>
            type.Namespace == typeof(DoorDefinitions).Namespace && type != typeof(DoorDefinitions) &&
            type.Name.EndsWith(nameof(DoorDefinitions), StringComparison.Ordinal) &&
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Static) is not null)];
        return areas.Length switch
        {
            0 => nameof(DoorDefinitions),
            1 => areas[0].Name,
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
        PropertyInfo[] members = [.. typeof(RoomEffectBlendDefinitions).GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(property => ReferenceEquals(property.GetValue(null), blend))];
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

    /// <summary>Renders one record as the generated catalog spells it.</summary>
    private static string RenderRecord(RoomFxRecordDefinition record) =>
        $"new({SurfaceY(record.BaseYPosition)}, {SurfaceY(record.TargetYPosition)}, " +
        $"SignedEightEight.FromTwoHundredFiftySixthsOfPixel({record.YVelocity.InTwoHundredFiftySixthsOfPixel.ToString(System.Globalization.CultureInfo.InvariantCulture)}), " +
        $"{record.Timer.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {Member(record.Type)}, " +
        $"{Member(record.VisorAnimation)}, {EffectBlend(record.EffectBlend)}, " +
        $"{Liquid(record.Liquid)}, {Effects(record.PaletteEffects)}, " +
        $"{Effects(record.AnimatedTiles)}, {Member(record.PaletteBlend)})";

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
        source.AppendLine("using SuperMetroid.Core.Fx.PaletteFx;");
        source.AppendLine("using SuperMetroid.Core.Rooms.Doors;");
        source.AppendLine();
        source.AppendLine("namespace SuperMetroid.Core.Fx;");
        source.AppendLine();
        source.AppendLine("/// <summary>");
        source.AppendLine("/// The retail room states' effect selections, each holding its door records in the game's order and its default record, and the records");
        source.AppendLine("/// Mother Brain's fight applies by itself; regenerate with --generate-room-fx-records.");
        source.AppendLine("/// </summary>");
        source.AppendLine("public static class RoomFxDefinitions");
        source.AppendLine("{");
        bool first = true;
        foreach ((ushort header, ushort[] pointers) in capture.Lists)
        {
            if (!first)
                source.AppendLine();
            first = false;
            string name = RoomFxSources.Fx(header).Name;
            ushort[] doorRecords = [.. pointers.TakeWhile(capture.Doors.ContainsKey)];
            ushort[] rest = [.. pointers.Skip(doorRecords.Length)];
            if ((rest.Length > 0 && MotherBrainRecordNames.ContainsKey(rest[0])) ||
                rest.Skip(1).Any(pointer => !MotherBrainRecordNames.ContainsKey(pointer)))
                throw new InvalidDataException($"Room FX header {name} lists a record after its default that Mother Brain's fight does not apply.");
            source.AppendLine($"    /// <summary>The {name} room FX.</summary>");
            if (doorRecords.Length == 0)
                source.AppendLine($"    public static RoomFxSelection {name} {{ get; }} = new(nameof({name}), [],");
            else
            {
                source.AppendLine($"    public static RoomFxSelection {name} {{ get; }} = new(nameof({name}),");
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
            source.AppendLine($"    /// <summary>The {name} room FX record, which Mother Brain's fight applies to her room after its default record.</summary>");
            source.AppendLine($"    public static RoomFxRecordDefinition {name} {{ get; }} = {RenderRecord(capture.Records[pointer])};");
        }
        source.AppendLine("}");
        return source.ToString();
    }
}
