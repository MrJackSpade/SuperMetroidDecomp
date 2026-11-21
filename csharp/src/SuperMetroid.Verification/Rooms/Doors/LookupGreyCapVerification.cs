using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks twenty grey-door draw frames' geometry, collision, and visual words against native data.</summary>
internal static class LookupGreyCapVerification
{
    /// <summary>Checks pointer ownership, native run geometry, and visual identity mapping.</summary>
    internal static void VerifyGreyCapGeometry(CartridgeImportAddressSpace rom) => VerifyGreyCapField(rom, 0);
    /// <summary>Checks each frame's compiled collision nibble against native draw words.</summary>
    internal static void VerifyGreyCapCollision(CartridgeImportAddressSpace rom) => VerifyGreyCapField(rom, 1);
    /// <summary>Checks each frame's compiled metatile and flip bits against native draw words.</summary>
    internal static void VerifyGreyCapVisuals(CartridgeImportAddressSpace rom) => VerifyGreyCapField(rom, 2);

    /// <summary>Compares the selected frame field across all stock pointers and rejects unsupported identities.</summary>
    private static void VerifyGreyCapField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa677,0xa683,0xa68f,0xa69b,0xa6a7,0xa6b3,0xa6bf,0xa6cb,0xa6d7,0xa6e3,0xa6ef,0xa6fb,0xa707,0xa713,0xa71f,0xa72b,0xa737,0xa743,0xa74f,0xa75b];
        string[] ids = ["clear-left","clear-right","clear-up","clear-down","grey-left-closed","grey-left-ajar","grey-left-half-open","grey-left-nearly-open","grey-right-closed","grey-right-ajar","grey-right-half-open","grey-right-nearly-open","grey-up-closed","grey-up-ajar","grey-up-half-open","grey-up-nearly-open","grey-down-closed","grey-down-ajar","grey-down-half-open","grey-down-nearly-open"];
        var exported = RoomObjectGreyDoorVisualFiles.All.ToArray();
        AssertEqual(20, exported.Length, "Grey cap export count");
        InstalledRoomObjectPictures? pictures = field == 2 ? RoomObjectPicturesStock.GreyDoor : null;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer), out var shape), "Grey cap descriptor domain");
                AssertEqual(owned, RoomObjectGreyDoorVisualFiles.TryGet(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer), out var dto), "Grey cap DTO domain");
                if (!owned)
                {
                    AssertEqual(default(GreyDoorCaps.Draw), shape, "Grey cap missing descriptor");
                    AssertEqual(default(RoomObjectDrawLists.DrawList), dto, "Grey cap missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectGreyDoorVisualFiles.VisualId(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer)), "Grey cap missing ID");
                }
            }
            foreach (string id in new[] { "CLEAR-LEFT", "grey-left-frame-0", "", "unknown" })
                AssertTrue(!RoomObjectGreyDoorVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Grey cap ordinal ID domain");
        }
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            RoomObjectDrawIdentity.TryFrame<GreyDoorCaps.Draw>(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer), out var shape);
            RoomObjectGreyDoorVisualFiles.TryGet(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer), out var dto);
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Grey cap original order");
                AssertEqual(ids[index], RoomObjectGreyDoorVisualFiles.VisualId(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer)), "Grey cap published ID");
                RoomObjectDraw byId = RoomObjectGreyDoorVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Grey cap ID identity");
                AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), dto.Runs.Span[0].Direction, 4, "Grey cap native geometry");
                AssertEqual((ushort)0, ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 10)), "Grey cap native final offset");
                foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                {
                    AssertEqual(1, frame.Runs.Length, "Grey cap one run");
                    AssertRunHeader(ReadSamusEaterObjectWord(rom, 0x840000 | pointer), frame.Runs.Span[0], "Grey cap DTO geometry");
                    AssertEqual(4, frame.Runs.Span[0].Count, "Grey cap DTO width");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.X(frame, 0), "Grey cap DTO final X");
                    AssertEqual((sbyte)0, RoomObjectRunContinuationCodecConstants.Y(frame, 0), "Grey cap DTO final Y");
                }
                // The cap's block shape is its four blocks in one line (#1862).
                AssertEqual(4, shape.Blocks.Cells.Count, "Grey cap block shape covers its four blocks");
            }
            else for (int row = 0; row < 4; row++)
            {
                RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (pointer + 2 + row * 2)));
                if (field == 1)
                {
                    AssertEqual(expected.Collision, shape.Blocks.CollisionAt(dto.Runs.Span[0].CellAt(row)), "Grey cap original calculated collision");
                    AssertEqual(expected.Collision, dto.Runs.Span[0].Collisions.Span[row], "Grey cap original DTO collision");
                    AssertEqual(expected.Collision, exported[index].Runs.Span[0].Collisions.Span[row], "Grey cap original export collision");
                }
                else
                    // Core holds no appearance: what the cell shows is the cap's installed picture.
                    AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<GreyDoorCaps.Draw>(pointer)).At(dto.Runs.Span[0].CellAt(row)),
                        "Grey cap installed picture shows the cartridge's block");
            }
        }
    }
}
