using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>Compares every compiled Noob Tube PLM draw against native geometry, collision, and visual words.</summary>
internal static class LookupTubeDrawsVerification
{
    /// <summary>Checks the seven draw identities, run shapes, and native ordering.</summary>
    internal static void VerifyTubeGeometry(CartridgeImportAddressSpace rom) => VerifyTubeDrawField(rom, 0);
    /// <summary>Checks each tube cell's collision nibble against the cartridge draw list.</summary>
    internal static void VerifyTubeCollision(CartridgeImportAddressSpace rom) => VerifyTubeDrawField(rom, 1);
    /// <summary>Checks each tube cell's visual tile bits against the cartridge draw list.</summary>
    internal static void VerifyTubeVisuals(CartridgeImportAddressSpace rom) => VerifyTubeDrawField(rom, 2);

    /// <summary>Walks all seven native draw lists, selecting geometry, collision, or visual assertions by field.</summary>
    private static void VerifyTubeDrawField(CartridgeImportAddressSpace rom, int field)
    {
        ushort[] pointers = [0x98d1,0x98d7,0x98dd,0x98e3,0x9953,0x9991,0x99e5];
        string[] ids = ["intact","damaged","opened","cleared","broken-late","opened-rows","broken-full"];
        var exported = RoomObjectNoobTubeVisualFiles.All.ToArray();
        AssertEqual(7, exported.Length, "Tube export count");
        InstalledRoomObjectNoobTubeVisuals? pictures = field == 2 ? InstalledRoomObjectNoobTubeVisuals.Stock() : null;
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, NoobTubeObjectDraws.TryDescribe(NativeObjectDraws.At<NoobTubeDraw>(pointer), out var shape), "Tube shape domain");
                AssertEqual(owned, RoomObjectNoobTubeVisualFiles.TryGet(NativeObjectDraws.At<NoobTubeDraw>(pointer), out var dto), "Tube DTO domain");
                if (!owned)
                {
                    AssertEqual(default(NoobTubeObjectDraws.Draw), shape, "Tube missing shape");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Tube missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectNoobTubeVisualFiles.VisualId(NativeObjectDraws.At<NoobTubeDraw>(pointer)), "Tube missing ID");
                }
            }
            foreach (string id in new[] { "INTACT", "broken", "", "unknown" })
                AssertTrue(!RoomObjectNoobTubeVisualFiles.Family.Ids.Values.Contains(id, StringComparer.Ordinal), "Tube ordinal ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            NoobTubeObjectDraws.TryDescribe(NativeObjectDraws.At<NoobTubeDraw>(pointer), out var shape);
            RoomObjectNoobTubeVisualFiles.TryGet(NativeObjectDraws.At<NoobTubeDraw>(pointer), out var dto);
            RoomObjectDraw byId = RoomObjectNoobTubeVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            if (field == 0)
            {
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Tube original export order");
                AssertEqual(ids[index], RoomObjectNoobTubeVisualFiles.VisualId(NativeObjectDraws.At<NoobTubeDraw>(pointer)), "Tube published ID");
                AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Tube reverse ID");
                foreach (int invalid in new[] {int.MinValue,-1,shape.RunCount,int.MaxValue})
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockCount(invalid), "Tube run bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(invalid), "Tube start bounds");
                    AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(invalid, 0), "Tube word run bounds");
                }
            }
            int cursor = 0x840000 | pointer;
            int run = 0;
            while (true)
            {
                ushort header = ReadSamusEaterObjectWord(rom, cursor);
                int count = header & 0x7fff;
                ushort offset = ReadSamusEaterObjectWord(rom, cursor + 2 + count * 2);
                if (field == 0)
                {
                    AssertEqual((ushort)count, header, "Tube native horizontal run");
                    AssertEqual(count, shape.BlockCount(run), "Tube native calculated count");
                    // The cartridge ends each run with the next run's start, and the last with no offset.
                    Offset16<RoomBlocks> continuation = run + 1 < shape.RunCount ? shape.Start(run + 1) : Offset16<RoomBlocks>.Zero;
                    AssertEqual(LittleEndianBytes.Word(unchecked((byte)continuation.X), unchecked((byte)continuation.Y)), offset, "Tube native calculated continuation");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        var part = frame.Runs.Span[run];
                        AssertRunHeader(header, part, "Tube native DTO geometry");
                        AssertEqual(count, part.Count, "Tube native DTO count");
                        AssertEqual(offset, RoomObjectRunContinuationCodecConstants.Encode(frame, run), "Tube native DTO offset");
                    }
                    int selectedRun = run;
                    foreach (int invalid in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.CollisionAt(selectedRun, invalid), "Tube cell bounds");
                }
                else for (int cell = 0; cell < count; cell++)
                {
                    RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, cursor + 2 + cell * 2));
                    if (field == 1)
                    {
                        AssertEqual(expected.Collision, shape.CollisionAt(run, cell), "Tube original calculated collision");
                        foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                            AssertEqual(expected.Collision, frame.Runs.Span[run].Collisions.Span[cell], "Tube original DTO collision");
                    }
                    else
                        // Core holds no appearance: what the cell shows is the tube's installed picture.
                        AssertEqual(expected.Appearance, pictures!.Image(NativeObjectDraws.At<NoobTubeDraw>(pointer)).At(dto.Runs.Span[run].CellAt(cell)),
                            "Tube installed picture shows the cartridge's block");
                }
                cells += count;
                run++;
                cursor += 4 + count * 2;
                if (offset == 0) break;
                AssertTrue(run < 5, "Tube native run termination");
            }
            AssertEqual(run, shape.RunCount, "Tube native run count");
            AssertEqual(run, dto.Runs.Length, "Tube native DTO run count");
        }
        AssertEqual(149, cells, "Tube independent physical cell count");
    }
}
