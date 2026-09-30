using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyCatalogIdentityStateCompatibility()
    {
        var room = CreateRoomIdentityFixture();
        var samus = CreateSamusIdentityFixture();
        object[] catalogs =
        [
            room.Characters, room.Palettes, room.Metatiles, room.Backgrounds, room.Skies, room.Layouts,
            samus, samus.Spritemaps, samus.Atmosphere, samus.DeathPalettes, samus.DeathTiles, samus.ArmCannon,
            RoomPlmBlueDoorVisualCatalog.Stock(),
            RoomPlmBombTorizoHandVisualCatalog.Stock(),
            RoomPlmChozoStatueVisualCatalog.Stock(),
            RoomPlmColoredDoorVisualCatalog.Stock(),
            RoomPlmBotwoonWallVisualCatalog.Stock(),
            RoomPlmCollectibleVisualCatalog.Stock(),
            RoomPlmEyeDoorVisualCatalog.Stock(),
            RoomPlmElevatorPlatformVisualCatalog.Stock(),
            RoomPlmDynamicCollectibleArtCatalog.Stock(),
            RoomPlmEscapeGateVisualCatalog.Stock(),
            RoomPlmDraygonCannonVisualCatalog.Stock(),
            RoomPlmDownwardGateVisualCatalog.Stock(),
            RoomPlmCrocomireVisualCatalog.Stock(),
            RoomPlmLinkedRestoreVisualCatalog.Stock(),
            RoomPlmGreyDoorVisualCatalog.Stock(),
            RoomPlmGrappleBlockVisualCatalog.Stock(),
            RoomPlmKraidVisualCatalog.Stock(),
            RoomPlmNoobTubeVisualCatalog.Stock(),
            RoomPlmMotherBrainGlassVisualCatalog.Stock(),
            RoomPlmMaridiaElevatubeVisualCatalog.Stock(),
            RoomPlmMotherBrainFakeDeathVisualCatalog.Stock(),
            RoomPlmSamusEaterVisualCatalog.Stock(),
            RoomPlmShotBlockVisualCatalog.Stock(),
            RoomPlmStationVisualCatalog.Stock(),
            RoomPlmSpeedBoosterVisualCatalog.Stock(),
            RoomPlmSporeSpawnCeilingVisualCatalog.Stock(),
            RoomPlmTourianAccessVisualCatalog.Stock(),
        ];
        AssertEqual(39, catalogs.Length, "all statically identified cached-hash catalog layouts");
        foreach (object original in catalogs)
        {
            Type type = original.GetType();
            string baseline = Identity(original);
            AssertTrue(DebuggerPresentationIdentityFieldDefinitions.Contains(type), type.Name + " has an explicit legacy schema");
            AssertTrue(type.GetField(DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName,
                BindingFlags.NonPublic | BindingFlags.Instance) is null, type.Name + " no longer persists a derived hash");
            foreach (bool cached in new[] { false, true })
            {
                using var fixture = CatalogStateFixture(original, includeIdentity: cached);
                object restored = DebuggerObjectGraphSerializer.Deserialize<object>(fixture);
                AssertEqual(type, restored.GetType(), type.Name + " restores its exact type");
                AssertEqual(baseline, Identity(restored), type.Name + " restores selected content from " +
                    (cached ? "the cached-hash layout without trusting its stale digest" : "the pre-fingerprint layout"));
                AssertEqual(fixture.Length, fixture.Position, type.Name + " consumes the full historical payload");
            }
            using var current = new MemoryStream();
            DebuggerObjectGraphSerializer.Serialize(current, original);
            current.Position = 0;
            AssertEqual(baseline, Identity(DebuggerObjectGraphSerializer.Deserialize<object>(current)),
                type.Name + " current schema round-trips without a derived field");
        }
        object sample = catalogs[0];
        foreach (var bad in new (string Name, object Value, string Field, Type? Declaring, int Copies, bool Omit)[]
        {
            ("invalid digest", "not-sha256", DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName, null, 1, false),
            ("wrong payload type", 42, DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName, null, 1, false),
            ("unknown extra field", new string('A', 64), "unknown-field", null, 1, false),
            ("wrong declaring type", new string('A', 64), DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName, typeof(RoomPlmBlueDoorVisualCatalog), 1, false),
            ("duplicate digest", new string('A', 64), DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName, null, 2, false),
            ("missing real field", new string('A', 64), DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName, null, 1, true),
        })
        {
            using var fixture = CatalogStateFixture(sample, true, bad.Value, bad.Field, bad.Declaring, bad.Copies, bad.Omit);
            AssertThrows<InvalidDataException>(() => DebuggerObjectGraphSerializer.Deserialize<object>(fixture),
                "catalog migration rejects " + bad.Name);
        }
        using var unrelated = CatalogStateFixture(new DebuggerFieldOrderProbe(), true);
        AssertThrows<InvalidDataException>(() => DebuggerObjectGraphSerializer.Deserialize<object>(unrelated),
            "the retired hash name is not a generic unknown-field bypass");
        Console.WriteLine("  Catalog state compatibility: all 39 pre-hash, cached-hash and current layouts; " +
            "selected-content equality and seven malformed/unknown-layout rejections pass without a ROM.");

        static string Identity(object catalog) => (string)catalog.GetType().GetProperty("ContentIdentity")!.GetValue(catalog)!;
    }

    /// <summary>
    /// Emits the historical root field layout while reusing the real encoder for child values.
    /// This reproduces catalog schema drift without a ROM, a historical build, or a playthrough.
    /// </summary>
    private static MemoryStream CatalogStateFixture(object catalog, bool includeIdentity,
        object? identityValue = null, string identityFieldName = DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName,
        Type? identityDeclaringType = null, int identityCopies = 1, bool omitLastField = false)
    {
        Type type = catalog.GetType();
        var fields = ((FieldInfo[])typeof(DebuggerObjectGraphSerializer)
            .GetMethod("GetSerializableFields", BindingFlags.Static | BindingFlags.NonPublic)!
            .Invoke(null, [type])!).Where(field => field.Name != DebuggerPresentationIdentityFieldDefinitions.RetiredFieldName).ToArray();
        if (omitLastField) fields = fields[..^1];
        var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(DebuggerGraphWireDefinitions.NewObjectMarker);
            writer.Write(1);
            writer.Write(DebuggerStateTypeIdentity.GetSerializedName(type));
            writer.Write(DebuggerGraphWireDefinitions.FieldsPayloadKind);
            writer.Write(fields.Length + (includeIdentity ? identityCopies : 0));
            Type encoderType = typeof(DebuggerObjectGraphSerializer).GetNestedType("GraphWriter", BindingFlags.NonPublic)!;
            object encoder = Activator.CreateInstance(encoderType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [writer], null)!;
            encoderType.GetField("nextReferenceId", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(encoder, 2);
            var references = (Dictionary<object, int>)encoderType.GetField("references", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(encoder)!;
            references.Add(catalog, 1);
            MethodInfo write = encoderType.GetMethod("Write", BindingFlags.Instance | BindingFlags.Public)!;
            if (includeIdentity)
            {
                for (int index = 0; index < identityCopies; index++)
                {
                    writer.Write(DebuggerStateTypeIdentity.GetSerializedName(identityDeclaringType ?? type));
                    writer.Write(identityFieldName);
                    write.Invoke(encoder, [identityValue ?? new string('A', DebuggerPresentationIdentityFieldDefinitions.DigestHexLength)]);
                }
            }
            foreach (FieldInfo field in fields.Reverse())
            {
                writer.Write(DebuggerStateTypeIdentity.GetSerializedName(field.DeclaringType!));
                writer.Write(field.Name);
                write.Invoke(encoder, [field.GetValue(catalog)]);
            }
        }
        stream.Position = 0;
        return stream;
    }
}
