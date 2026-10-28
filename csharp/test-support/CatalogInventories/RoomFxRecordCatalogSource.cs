using System.Reflection;
using System.Text;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.TestSupport.CatalogInventories;

/// <summary>Reads the retail room-FX headers and renders the generated Core catalog that holds them.</summary>
internal static class RoomFxRecordCatalogSource
{
    /// <summary>The checked-in generated catalog the renderer's output must equal.</summary>
    internal static readonly string GeneratedPath = Path.GetFullPath(Path.Combine(
        "csharp", "src", "SuperMetroid.Core", "Fx", "RoomFxDefinitions.cs"));

    /// <summary>Every captured record by its bank-$83 pointer, and each header's record pointers in native order.</summary>
    internal sealed record RetailRoomFxCapture(
        SortedDictionary<ushort, RoomFxRecordDefinition> Records,
        SortedDictionary<ushort, ushort[]> Lists);

    /// <summary>Walks every compiled header's records, plus the Mother Brain records her FX entries index after its default.</summary>
    internal static RetailRoomFxCapture CaptureRetailRoomFx(ISnesAddressSpace bus)
    {
        var records = new SortedDictionary<ushort, RoomFxRecordDefinition>();
        var lists = new SortedDictionary<ushort, ushort[]>();
        foreach (ushort header in RoomFxSources.Pointers)
        {
            var list = new List<ushort>();
            ushort pointer = header;
            for (int guard = 0; ; guard++)
            {
                if (guard >= 256)
                    throw new InvalidDataException($"Room FX header $83:{header:X4} does not terminate.");
                if (pointer < 0x8000)
                    throw new InvalidDataException($"Room FX header $83:{header:X4} crossed below the LoROM window.");
                ushort door = Capture(bus, records, pointer);
                list.Add(pointer);
                if (door == RoomFxRomDataRecordTooling.TerminatorDoorPointer)
                    break;
                if (door == 0)
                {
                    // Mother Brain selects these records by FX index after room load; the ordinary
                    // door walk stops at the default record that precedes them.
                    for (ushort next = unchecked((ushort)(pointer + RoomFxToolingRomData.Record.ByteCount));
                         MotherBrainFxRecordPointers.DirectRecords.Contains(next);
                         next = unchecked((ushort)(next + RoomFxToolingRomData.Record.ByteCount)))
                    {
                        Capture(bus, records, next);
                        list.Add(next);
                    }
                    break;
                }
                pointer = unchecked((ushort)(pointer + RoomFxToolingRomData.Record.ByteCount));
            }
            lists.Add(header, list.ToArray());
        }
        return new RetailRoomFxCapture(records, lists);
    }

    /// <summary>Reads the record at <paramref name="pointer"/> into <paramref name="records"/> and returns its door word.</summary>
    private static ushort Capture(ISnesAddressSpace bus, SortedDictionary<ushort, RoomFxRecordDefinition> records, ushort pointer)
    {
        int source = RoomFxRomDataBanksTooling.RoomDefinitions | pointer;
        ushort door = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source);
        RoomFxRecordDefinition record = door == RoomFxRomDataRecordTooling.TerminatorDoorPointer
            ? RoomFxRecordCodec.Terminator
            : RoomFxRecordCodec.Decode(door == 0 ? null : DoorHeaderSources.Door(door),
                RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), source, RoomFxToolingRomData.Record.ByteCount),
                $"room-FX record $83:{pointer:X4}");
        if (records.TryGetValue(pointer, out RoomFxRecordDefinition? existing))
        {
            if (!existing.Equals(record))
                throw new InvalidDataException($"Shared room-FX record $83:{pointer:X4} reads differently from another header.");
        }
        else
            records.Add(pointer, record);
        return door;
    }

    /// <summary>Renders a record's door and terminator flag as the generated catalog spells them.</summary>
    private static string RenderDoor(RoomFxRecordDefinition record) =>
        record.Terminates ? "null, true" : record.Door is { } door ? $"{DeclaringDoorClass(door.Name)}.{door.Name}, false" : "null, false";

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

    /// <summary>Renders one member of a closed identity as the generated catalog spells it.</summary>
    private static string Member<TIdentity>(TIdentity value)
        where TIdentity : struct, Enum =>
        Enum.IsDefined(value) ? $"{typeof(TIdentity).Name}.{value}"
            : throw new InvalidDataException($"{value} is not a {typeof(TIdentity).Name} member.");

    /// <summary>
    /// Renders a flag combination as the generated catalog spells it: its members in ascending value order joined
    /// with <c>|</c>, or the type's zero member when none is set.
    /// </summary>
    private static string Flags<TFlags>(TFlags value)
        where TFlags : struct, Enum
    {
        ulong bits = Convert.ToUInt64(value, System.Globalization.CultureInfo.InvariantCulture);
        TFlags[] members = [.. Enum.GetValues<TFlags>().Where(member => Convert.ToUInt64(member, System.Globalization.CultureInfo.InvariantCulture) != 0)];
        List<string> set = [];
        ulong covered = 0;
        foreach (TFlags member in members)
        {
            ulong flag = Convert.ToUInt64(member, System.Globalization.CultureInfo.InvariantCulture);
            if ((bits & flag) != flag)
                continue;
            set.Add($"{typeof(TFlags).Name}.{member}");
            covered |= flag;
        }
        if (covered != bits)
            throw new InvalidDataException($"{typeof(TFlags).Name} 0x{bits:X2} sets a bit no member names.");
        return set.Count == 0 ? $"{typeof(TFlags).Name}.{Enum.GetValues<TFlags>().Single(member => Convert.ToUInt64(member, System.Globalization.CultureInfo.InvariantCulture) == 0)}"
            : string.Join(" | ", set);
    }

    /// <summary>Renders a surface height as the generated catalog spells it: its pixel row as the record's word, or null where the record names none.</summary>
    private static string SurfaceY(int? y) =>
        y is int named ? $"0x{named:X4}" : "null";

    /// <summary>Renders the named headers for the generated Core catalog.</summary>
    internal static string RenderRoomFxDefinitions(RetailRoomFxCapture capture)
    {
        Ensure.NotNull(capture);
        var source = new StringBuilder();
        source.AppendLine("// <auto-generated/>");
        source.AppendLine("#nullable enable");
        source.AppendLine("using SuperMetroid.Core.Fx.Artwork;");
        source.AppendLine("using SuperMetroid.Core.Rooms.Doors;");
        source.AppendLine();
        source.AppendLine("namespace SuperMetroid.Core.Fx;");
        source.AppendLine();
        source.AppendLine("/// <summary>The retail room FX headers, each holding its records in native order; regenerate with --generate-room-fx-records.</summary>");
        source.AppendLine("public static class RoomFxDefinitions");
        source.AppendLine("{");
        bool first = true;
        foreach ((ushort header, ushort[] pointers) in capture.Lists)
        {
            if (!first)
                source.AppendLine();
            first = false;
            string name = RoomFxSources.Fx(header).Name;
            source.AppendLine($"    /// <summary>The {name} room FX.</summary>");
            source.AppendLine($"    public static RoomFxHeader {name} {{ get; }} = new(nameof({name}),");
            source.AppendLine("    [");
            foreach (ushort pointer in pointers)
            {
                RoomFxRecordDefinition record = capture.Records[pointer];
                source.AppendLine($"        new({RenderDoor(record)}, " +
                    $"{SurfaceY(record.BaseYPosition)}, {SurfaceY(record.TargetYPosition)}, " +
                    $"SignedEightEight.FromSignedRaw({record.YVelocity.SignedRawValue.ToString(System.Globalization.CultureInfo.InvariantCulture)}), " +
                    $"0x{record.Timer:X2}, {Member(record.Type)}, " +
                    $"{Member(record.DefaultLayerBlend)}, {Member(record.Layer3LayerBlend)}, " +
                    $"{Flags(record.LiquidOptions)}, {Flags(record.PaletteFxSlots)}, " +
                    $"{Flags(record.AnimatedTileSlots)}, {Member(record.PaletteBlend)}),");
            }
            source.AppendLine("    ]);");
        }
        source.AppendLine("}");
        return source.ToString();
    }
}
