using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Checks the exported downward gate draw records against their bank $84 originals.</summary>
internal static class LookupDownwardGateDrawsVerification
{
    /// <summary>Checks gate run directions, widths, offsets, identifiers, and record boundaries.</summary>
    internal static void VerifyDownwardGateDrawGeometry(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 0));
    /// <summary>Checks the collision-type bits of every gate draw word against cartridge data.</summary>
    internal static void VerifyDownwardGateDrawCollision(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 1));
    /// <summary>Checks the tile index bits of every gate draw word against cartridge data.</summary>
    internal static void VerifyDownwardGateDrawVisuals(CartridgeImportAddressSpace rom) =>
        Suite(nameof(VerifyDownwardGateDrawField), () => VerifyDownwardGateDrawField(rom, 2));

    /// <summary>Compares a selected field across descriptors and exports while walking all native gate records.</summary>
    private static void VerifyDownwardGateDrawField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0xa517,0xa525,0xa533,0xa541,0xa54f,0xa55d,
            0xa5d7,0xa5e3,0xa5eb,0xa5f7,0xa5ff,0xa60b,0xa613,0xa61f];
        string[] ids = ["column-frame-0","column-frame-1","column-frame-2","column-frame-3",
            "column-frame-4","column-frame-5","blue-left-trigger","blue-right-trigger",
            "green-left-trigger","green-right-trigger","red-left-trigger","red-right-trigger",
            "yellow-left-trigger","yellow-right-trigger"];
        var exported = RoomObjectDownwardGateVisualFiles.All.ToArray();
        AssertEqual(14, exported.Length, "Gate draw export count");
        InstalledRoomObjectDownwardGateVisuals? pictures = field == 2 ? InstalledRoomObjectDownwardGateVisuals.Stock() : null;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, DownwardGateObjectDraws.TryDescribe(NativeObjectDraws.At<DownwardGateDraw>(pointer), out var shape), "Gate descriptor ownership");
                AssertEqual(owned, RoomObjectDownwardGateVisualFiles.TryGet(NativeObjectDraws.At<DownwardGateDraw>(pointer), out var dto), "Gate DTO ownership");
                if (!owned)
                {
                    AssertEqual(default(DownwardGateObjectDraws.Draw), shape, "Gate missing descriptor");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Gate missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectDownwardGateVisualFiles.VisualId(NativeObjectDraws.At<DownwardGateDraw>(pointer)), "Gate missing ID");
                }
            }
            foreach (string invalid in new[] { "column-frame-6", "column-frame-00", "Blue-left-trigger", "", "unknown" })
                AssertTrue(!RoomObjectDownwardGateVisualFiles.Family.Ids.Values.Contains(invalid, StringComparer.Ordinal), "Gate ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            DownwardGateObjectDraws.TryDescribe(NativeObjectDraws.At<DownwardGateDraw>(pointer), out var shape);
            RoomObjectDownwardGateVisualFiles.TryGet(NativeObjectDraws.At<DownwardGateDraw>(pointer), out var dto);
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Gate export order");
            AssertEqual(ids[index], RoomObjectDownwardGateVisualFiles.VisualId(NativeObjectDraws.At<DownwardGateDraw>(pointer)), "Gate published ID mapping");
            RoomObjectDraw byId = RoomObjectDownwardGateVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Gate reverse ID mapping");
            int cursor = pointer, run = 0;
            while (true)
            {
                ushort count = ReadSamusEaterObjectWord(rom, 0x840000 | cursor);
                cursor += 2;
                int length = count & 0x7fff;
                if (field == 0)
                {
                    AssertRunHeader(count, dto.Runs.Span[run].Direction, shape.BlockCount(run), "Gate native count/direction");
                    AssertEqual(length, shape.BlockCount(run), "Gate native run width");
                    AssertEqual((count & 0x8000) != 0, shape.Column, "Gate runtime direction");
                    AssertRunHeader(count, dto.Runs.Span[run], "Gate DTO count/direction");
                    AssertRunHeader(count, exported[index].Runs.Span[run], "Gate export count/direction");
                    AssertEqual(length, dto.Runs.Span[run].Count, "Gate DTO width");
                    AssertEqual(length, exported[index].Runs.Span[run].Count, "Gate export width");
                    foreach (int invalid in new[] { int.MinValue, -1, length, int.MaxValue })
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(run, invalid), "Gate cell bounds");
                }
                for (int word = 0; word < length; word++, cursor += 2)
                {
                    if (field != 0)
                    {
                        RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | cursor));
                        if (field == 1)
                        {
                            AssertEqual(expected.Collision, shape.CollisionAt(run, word), "Gate original calculated collision");
                            AssertEqual(expected.Collision, dto.Runs.Span[run].Collisions.Span[word], "Gate original DTO collision");
                            AssertEqual(expected.Collision, exported[index].Runs.Span[run].Collisions.Span[word], "Gate original export collision");
                        }
                        else
                            // Core holds no appearance: what the cell shows is the gate's installed picture.
                            AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<DownwardGateDraw>(pointer)).At(dto.Runs.Span[run].CellAt(word)),
                                "Gate installed picture shows the cartridge's block");
                    }
                    cells++;
                }
                sbyte x = unchecked((sbyte)DownwardGateDrawFieldRomByte(rom, 0x840000 | cursor++));
                sbyte y = unchecked((sbyte)DownwardGateDrawFieldRomByte(rom, 0x840000 | cursor++));
                if (field == 0)
                {
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    AssertEqual(x, run + 1 < shape.RunCount ? shape.Start(run + 1).X : (short)0, "Gate original continuation X");
                    AssertEqual((sbyte)0, y, "Gate runtime continuation Y");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(dto, run), "Gate DTO X");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(dto, run), "Gate DTO Y");
                    AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(exported[index], run), "Gate export X");
                    AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(exported[index], run), "Gate export Y");
                }
                run++;
                if (x == 0 && y == 0) break;
            }
            if (field == 0)
            {
                AssertEqual(run, shape.RunCount, "Gate original run count");
                AssertEqual(run, dto.Runs.Length, "Gate DTO run count");
                AssertEqual(run, exported[index].Runs.Length, "Gate export run count");
                int end = index == 5 ? 0xa56b : index == 13 ? 0xa627 : pointers[index + 1];
                AssertEqual(end, cursor, "Gate original record end");
                foreach (int invalid in new[] { int.MinValue, -1, run, int.MaxValue })
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Gate run bounds");

                    AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Gate start bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Gate run-width bounds");
                }
            }
        }
        AssertEqual(46, cells, "Gate complete original cell domain");
    }

    /// <summary>Reads a native draw-list byte for the retail ROM comparison only.</summary>
    private static byte DownwardGateDrawFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
