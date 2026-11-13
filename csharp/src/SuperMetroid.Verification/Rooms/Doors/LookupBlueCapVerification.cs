using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks blue-door cap draw geometry, collision, visuals, and four alias pointers.</summary>
internal static class LookupBlueCapVerification
{
    /// <summary>Checks pointer ownership, visual alias mapping, and native run geometry.</summary>
    internal static void VerifyBlueCapGeometry(CartridgeImportAddressSpace rom) => VerifyBlueCapField(rom, 0);
    /// <summary>Checks each frame's compiled collision nibble against native draw words.</summary>
    internal static void VerifyBlueCapCollision(CartridgeImportAddressSpace rom) => VerifyBlueCapField(rom, 1);
    /// <summary>Checks each frame's metatile and flip bits against native draw words.</summary>
    internal static void VerifyBlueCapVisuals(CartridgeImportAddressSpace rom) => VerifyBlueCapField(rom, 2);

    /// <summary>Compares a selected field across sixteen editable frames and four visual aliases.</summary>
    private static void VerifyBlueCapField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa9b3,0xa9bf,0xa9cb,0xa9d7,0xa9ef,0xa9fb,0xaa07,0xaa13,0xaa2b,0xaa37,0xaa43,0xaa4f,0xaa67,0xaa73,0xaa7f,0xaa8b,0xa9a7,0xa9e3,0xaa1f,0xaa5b];
        string[] ids = ["left-frame-0","left-frame-1","left-frame-2","left-frame-3","right-frame-0","right-frame-1","right-frame-2","right-frame-3","up-frame-0","up-frame-1","up-frame-2","up-frame-3","down-frame-0","down-frame-1","down-frame-2","down-frame-3","left-frame-0","right-frame-0","up-frame-0","down-frame-0"];
        var exported = BlueDoorObjectDrawDefinitionsTooling.All.ToArray();
        AssertEqual(20, exported.Length, "Blue cap export count");
        InstalledRoomObjectBlueDoorVisuals? pictures = field == 2 ? InstalledRoomObjectBlueDoorVisuals.Stock() : null;
        AssertTrue(RoomObjectBlueDoorVisualFiles.Editable.Select(frame => RoomObjectDrawAddressCodec.Of(frame.Draw)).SequenceEqual(pointers.Take(16)), "Blue cap editable identity domain and order");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                ushort alias = pointer switch { 0xa9a7 => 0xa9b3, 0xa9e3 => 0xa9ef, 0xaa1f => 0xaa2b, 0xaa5b => 0xaa67, _ => pointer };
                AssertEqual(NativeObjectDraws.At<BlueDoorCaps.Draw>(alias), BlueDoorCaps.VisualSource(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer)), "Blue cap alias and passthrough domain");
                AssertEqual(owned, BlueDoorCaps.TryDescribe(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer), out var shape), "Blue cap descriptor domain");
                AssertEqual(owned, RoomObjectBlueDoorVisualFiles.TryGet(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer), out var dto), "Blue cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(BlueDoorCaps.Draw), shape, "Blue cap missing descriptor");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Blue cap missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectBlueDoorVisualFiles.VisualId(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer)), "Blue cap missing ID");
                }
            }
            foreach (string id in new[] { "LEFT-FRAME-0", "left-frame-4", "", "unknown" })
                AssertTrue(!RoomObjectBlueDoorVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Blue cap ordinal ID domain");
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            BlueDoorCaps.TryDescribe(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer), out var shape);
            RoomObjectBlueDoorVisualFiles.TryGet(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer), out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Blue cap original order");
                AssertEqual(ids[index], RoomObjectBlueDoorVisualFiles.VisualId(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer)), "Blue cap published ID");
                RoomObjectDraw byId = RoomObjectBlueDoorVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
                AssertEqual(index < 16 ? pointer : pointers[(index - 16) * 4], RoomObjectDrawAddressCodec.Of(byId), "Blue cap ID identity");
                AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), dto.Runs.Span[0].Direction, 4, "Blue cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 10)), "Blue cap native final offset");
                foreach (var frame in new[] {dto,exported[index]})
                {
                    AssertEqual(1, frame.Runs.Length, "Blue cap one run");
                    AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), frame.Runs.Span[0], "Blue cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].Count, "Blue cap DTO width");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.X(frame, 0), "Blue cap DTO final X");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.Y(frame, 0), "Blue cap DTO final Y");
                }
                foreach (int invalid in new[] {int.MinValue,-1,4,int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid), "Blue cap calculated row bounds");
            }
            else for (int row = 0; row < 4; row++)
            {
                RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 2 + row * 2)));
                if (field == 1)
                {
                    AssertEqual(expected.Collision, shape.CollisionAt(row), "Blue cap original calculated collision");
                    AssertEqual(expected.Collision, dto.Runs.Span[0].Collisions.Span[row], "Blue cap original DTO collision");
                    AssertEqual(expected.Collision, exported[index].Runs.Span[0].Collisions.Span[row], "Blue cap original export collision");
                }
                else
                    // Core holds no appearance: what the cell shows is the cap's installed picture, an alias showing frame zero's.
                    AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<BlueDoorCaps.Draw>(pointer)).At(dto.Runs.Span[0].CellAt(row)),
                        "Blue cap installed picture shows the cartridge's block");
            }
        }
    }
}
