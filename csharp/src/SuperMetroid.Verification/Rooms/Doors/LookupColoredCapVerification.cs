using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks colored door-cap draw geometry and tile collision/appearance across all colors and directions.</summary>
internal static class LookupColoredCapVerification
{
    /// <summary>Compares all 48 cap-frame pointers and their one-run geometry with the cartridge draw lists.</summary>
    internal static void VerifyColoredCapGeometry(CartridgeImportAddressSpace rom) => VerifyColoredCapField(rom, 0);
    /// <summary>Checks the collision nibble of each exported cap tile against its native level word.</summary>
    internal static void VerifyColoredCapCollision(CartridgeImportAddressSpace rom) => VerifyColoredCapField(rom, 1);
    /// <summary>Checks the visual tile bits of each exported cap frame against its native level word.</summary>
    internal static void VerifyColoredCapVisuals(CartridgeImportAddressSpace rom) => VerifyColoredCapField(rom, 2);

    /// <summary>Checks the selected geometry, collision, or visual field across yellow, green, and red caps in all four directions.</summary>
    private static void VerifyColoredCapField(CartridgeImportAddressSpace rom, int field)
    {
        LookupColoredCapFamily[] families = [
            new LookupColoredCapFamily(0xa767,"yellow-left"),new LookupColoredCapFamily(0xa797,"yellow-right"),new LookupColoredCapFamily(0xa7c7,"yellow-up"),new LookupColoredCapFamily(0xa7f7,"yellow-down"),
            new LookupColoredCapFamily(0xa827,"green-left"),new LookupColoredCapFamily(0xa857,"green-right"),new LookupColoredCapFamily(0xa887,"green-up"),new LookupColoredCapFamily(0xa8b7,"green-down"),
            new LookupColoredCapFamily(0xa8e7,"red-left"),new LookupColoredCapFamily(0xa917,"red-right"),new LookupColoredCapFamily(0xa947,"red-up"),new LookupColoredCapFamily(0xa977,"red-down")];
        ushort[] pointers = families.SelectMany(family => Enumerable.Range(0,4).Select(frame => (ushort)(family.First+12*frame))).ToArray();
        string[] ids = families.SelectMany(family => Enumerable.Range(0,4).Select(frame => $"{family.Name}-frame-{frame}")).ToArray();
        var exported = RoomObjectColoredDoorVisualFiles.All.ToArray();
        AssertEqual(48, exported.Length, "Colored cap export count");
        InstalledRoomObjectColoredDoorVisuals? pictures = field == 2 ? InstalledRoomObjectColoredDoorVisuals.Stock() : null;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, ColoredDoorCaps.TryDescribe(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer), out var shape), "Colored cap descriptor domain");
                AssertEqual(owned, RoomObjectColoredDoorVisualFiles.TryGet(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer), out var dto), "Colored cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(ColoredDoorCaps.Draw), shape, "Colored cap missing descriptor");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Colored cap missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectColoredDoorVisualFiles.VisualId(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer)), "Colored cap missing ID");
                }
            }
            foreach (string id in new[] { "YELLOW-LEFT-FRAME-0", "green-left-frame-4", "", "unknown" })
                AssertTrue(!RoomObjectColoredDoorVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Colored cap ordinal ID domain");
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            ColoredDoorCaps.TryDescribe(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer), out var shape);
            RoomObjectColoredDoorVisualFiles.TryGet(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer), out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Colored cap original order");
                AssertEqual(ids[index], RoomObjectColoredDoorVisualFiles.VisualId(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer)), "Colored cap published ID");
                RoomObjectDraw byId = RoomObjectColoredDoorVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Colored cap ID identity");
                AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), dto.Runs.Span[0].Direction, 4, "Colored cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 10)), "Colored cap native final offset");
                foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                {
                    AssertEqual(1, frame.Runs.Length, "Colored cap one run");
                    AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), frame.Runs.Span[0], "Colored cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].Count, "Colored cap DTO width");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.X(frame, 0), "Colored cap DTO final X");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.Y(frame, 0), "Colored cap DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid), "Colored cap calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 2 + row * 2)));
                if (field == 1)
                {
                    AssertEqual(expected.Collision, shape.CollisionAt(row), "Colored cap original calculated collision");
                    AssertEqual(expected.Collision, dto.Runs.Span[0].Collisions.Span[row], "Colored cap original DTO collision");
                    AssertEqual(expected.Collision, exported[index].Runs.Span[0].Collisions.Span[row], "Colored cap original export collision");
                }
                else
                    // Core holds no appearance: what the cell shows is the cap's installed picture.
                    AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<ColoredDoorCaps.Draw>(pointer)).At(dto.Runs.Span[0].CellAt(row)),
                        "Colored cap installed picture shows the cartridge's block");
            }
        }
    }
}
