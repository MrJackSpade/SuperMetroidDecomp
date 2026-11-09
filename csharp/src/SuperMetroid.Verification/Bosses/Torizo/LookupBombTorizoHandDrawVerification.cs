using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.TerrainBlockFieldAssertions;
namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>Checks Bomb Torizo hand PLM geometry, collision, and artwork against native draw lists.</summary>
internal static class LookupBombTorizoHandDrawVerification
{
    /// <summary>Checks both hand layouts' run geometry and pointer/name domain.</summary>
    internal static void VerifyBombTorizoHandDrawGeometry(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 0);
    /// <summary>Checks the collision kind of every hand draw block against the decoded cartridge block.</summary>
    internal static void VerifyBombTorizoHandDrawCollision(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 1);
    /// <summary>Checks the appearance of every hand draw block against the decoded cartridge block.</summary>
    internal static void VerifyBombTorizoHandDrawVisuals(SuperMetroidAddressSpace rom) => VerifyBombTorizoHandDrawField(rom, 2);

    /// <summary>Compares the selected draw field across decoded native blocks, descriptors, and exported artwork records.</summary>
    private static void VerifyBombTorizoHandDrawField(SuperMetroidAddressSpace rom, int field)
    {
        ushort[] pointers = [0x9877,0x989d];
        string[] ids = ["intact","cleared"];
        var exported = RoomObjectBombTorizoHandVisualFiles.All.ToArray();
        AssertEqual(2, exported.Length, "Torizo hand export count");
        if (field == 0)
        {
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                ushort pointer = (ushort)raw;
                bool owned = pointers.Contains(pointer);
                AssertEqual(owned, BombTorizoHandDraws.TryDescribe(NativeObjectDraws.At<BombTorizoHandDraw>(pointer), out var shape), "Torizo hand descriptor domain");
                AssertEqual(owned, RoomObjectBombTorizoHandVisualFiles.TryGet(NativeObjectDraws.At<BombTorizoHandDraw>(pointer), out var dto), "Torizo hand DTO domain");
                if (!owned)
                {
                    AssertEqual(default(BombTorizoHandDraws.Draw), shape, "Torizo hand missing descriptor");
                    AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), dto, "Torizo hand missing DTO");
                    AssertThrows<InvalidDataException>(() => RoomObjectBombTorizoHandVisualFiles.VisualId(NativeObjectDraws.At<BombTorizoHandDraw>(pointer)), "Torizo hand missing ID");
                }
            }
            foreach (string bad in new[] {"Intact","cleared ","","unknown"})
                AssertTrue(!RoomObjectBombTorizoHandVisualFiles.Family.Ids.Values.Contains(bad, StringComparer.Ordinal), "Torizo hand ordinal ID domain");
        }
        int cells = 0;
        for (int index = 0; index < pointers.Length; index++)
        {
            ushort pointer = pointers[index];
            BombTorizoHandDraws.TryDescribe(NativeObjectDraws.At<BombTorizoHandDraw>(pointer), out var shape);
            RoomObjectBombTorizoHandVisualFiles.TryGet(NativeObjectDraws.At<BombTorizoHandDraw>(pointer), out var dto);
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(exported[index].Draw), "Torizo hand original export order");
            AssertEqual(ids[index], RoomObjectBombTorizoHandVisualFiles.VisualId(NativeObjectDraws.At<BombTorizoHandDraw>(pointer)), "Torizo hand published identity");
            RoomObjectDraw byId = RoomObjectBombTorizoHandVisualFiles.Family.Ids.Single(pair => pair.Value == ids[index]).Key;
            AssertEqual(pointer, RoomObjectDrawAddressCodec.Of(byId), "Torizo hand reverse pointer");
            int cursor = pointer, run = 0;
            sbyte x = 0, y = 0;
            while (true)
            {
                ushort count = ReadSamusEaterObjectWord(rom, 0x840000 | cursor);
                cursor += 2;
                AssertTrue(count is > 0 and < 0x8000, "Torizo hand original horizontal run");
                if (field == 0)
                {
                    AssertEqual(count, shape.BlockCount(run), "Torizo hand native run width");
                    AssertEqual(x, shape.Start(run).X, "Torizo hand original origin-relative X");
                    AssertEqual(y, shape.Start(run).Y, "Torizo hand original origin-relative Y");
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        AssertRunHeader(count, frame.Runs.Span[run], "Torizo hand DTO direction/count");
                        AssertEqual(count, frame.Runs.Span[run].Blocks.Length, "Torizo hand DTO width");
                    }
                    foreach (int bad in new[] {int.MinValue,-1,count,int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(run,bad), "Torizo hand cell bounds");
                }
                for (int word = 0; word < count; word++, cursor += 2)
                {
                    if (field != 0)
                    {
                        RoomTerrainBlock expected = RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | cursor));
                        AssertTerrainField(field, expected, shape.BlockAt(run,word), "Torizo hand original calculated field");
                        AssertTerrainField(field, expected, dto.Runs.Span[run].Blocks.Span[word], "Torizo hand original DTO field");
                        AssertTerrainField(field, expected, exported[index].Runs.Span[run].Blocks.Span[word], "Torizo hand original export field");
                    }
                    cells++;
                }
                x = unchecked((sbyte)BombTorizoHandDrawFieldRomByte(rom, 0x840000 | cursor++));
                y = unchecked((sbyte)BombTorizoHandDrawFieldRomByte(rom, 0x840000 | cursor++));
                if (field == 0)
                {
                    foreach (var frame in new[] {dto,exported[index],exported.Single(entry => entry.Draw == byId)})
                    {
                        AssertEqual(x, RoomObjectRunContinuationCodecConstants.X(frame, run), "Torizo hand DTO next X");
                        AssertEqual(y, RoomObjectRunContinuationCodecConstants.Y(frame, run), "Torizo hand DTO next Y");
                    }
                }
                run++;
                if (x == 0 && y == 0) break;
            }
            AssertEqual(index == 0 ? 0x9897 : 0x98d1, cursor, "Torizo hand native record end");
            AssertEqual(run, shape.RunCount, "Torizo hand native run count");
            AssertEqual(run, dto.Runs.Length, "Torizo hand DTO run count");
            AssertEqual(run, exported[index].Runs.Length, "Torizo hand export run count");
            if (field == 0)
            foreach (int bad in new[] {int.MinValue,-1,run,int.MaxValue})
            {
                AssertThrows<ArgumentOutOfRangeException>(() => shape.BlockAt(bad,0), "Torizo hand run bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => shape.Start(bad), "Torizo hand start bounds");
            }
        }
        AssertEqual(24, cells, "Torizo hand complete native cell domain");
    }

    /// <summary>Reads a native bank-$84 hand-draw offset byte for run transitions.</summary>
    private static byte BombTorizoHandDrawFieldRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
