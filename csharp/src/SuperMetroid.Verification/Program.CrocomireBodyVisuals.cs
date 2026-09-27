using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledCrocomireBodyVisuals(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.ExtendedFrames is not null && stock.CrocomireBg2Frames is not null,
            "installed Crocomire body has both OAM and BG2 presentations");
        HashSet<ushort> selectedFrames = [];
        for (int index = 0;
             index < CrocomireInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = CrocomireInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort native = ReadWord(0xa40000 | operand);
            AssertTrue(CompiledEnemyVisualSelectors.TryGet(
                    CrocomireBodyVisualDefinitions.Bank, operand, out ushort compiled),
                $"Crocomire selector $A4:{operand:X4} is compiled");
            AssertEqual(native, compiled,
                $"Crocomire selector $A4:{operand:X4} matches the pinned cartridge");
            if (compiled < CrocomireBodyVisualDefinitions.FirstSkeletonFrame)
                selectedFrames.Add(compiled);
        }
        AssertTrue(selectedFrames.SetEquals(CrocomireBodyVisualDefinitions.Frames.ToArray()),
            "all fifty fight-body roots come from native presentation operands");
        AssertEqual(CrocomireBodyVisualDefinitions.MixedBg2FrameCount,
            CrocomireBg2FrameDefinitions.Frames.Length,
            "forty-two Crocomire body roots have a BG2 half");

        foreach (ushort pointer in CrocomireBodyVisualDefinitions.Frames)
        {
            AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                    CrocomireBodyVisualDefinitions.Bank, pointer, out _),
                $"Crocomire body OAM frame $A4:{pointer:X4} is installed");
            var guard = new ExtendedVisualReadGuard(rom);
            guard.BlockFrame(CrocomireBodyVisualDefinitions.Bank, pointer);
            foreach (bool newFrame in new[] { false, true })
            {
                (OamBuffer nativeOam, byte[] nativeVram) = Draw(null, rom, pointer,
                    newFrame);
                (OamBuffer installedOam, byte[] installedVram) = Draw(stock, guard,
                    pointer, newFrame);
                AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                           nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                           nativeOam.NextByteOffset == installedOam.NextByteOffset,
                    $"Crocomire body $A4:{pointer:X4} OAM matches native; new frame={newFrame}");
                AssertTrue(nativeVram.SequenceEqual(installedVram),
                    $"Crocomire body $A4:{pointer:X4} BG2 writes match native; new frame={newFrame}");
            }
        }

        foreach (EnemyBg2FrameDefinition frame in CrocomireBg2FrameDefinitions.Frames)
        {
            AssertTrue(stock.CrocomireBg2Frames!.TryGet(frame.Pointer,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"Crocomire BG2 half $A4:{frame.Pointer:X4} is installed");
            var native = new List<(ushort DestinationWord, ushort[] Tiles)>();
            int componentCount = ReadWord(0xa40000 | frame.Pointer);
            for (int component = 0; component < componentCount; component++)
            {
                int record = 0xa40000 | unchecked((ushort)(
                    frame.Pointer + 2 + component * 8));
                ushort stream = ReadWord(record + 4);
                if (ReadWord(0xa40000 | stream) != EnemyBg2FrameLayout.StreamMarker)
                    continue;
                ushort cursor = unchecked((ushort)(stream + 2));
                bool terminated = false;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream;
                     command++)
                {
                    ushort destination = ReadWord(0xa40000 | cursor);
                    if (destination == 0xffff)
                    {
                        terminated = true;
                        break;
                    }
                    ushort count = ReadWord(0xa40000 |
                        unchecked((ushort)(cursor + 2)));
                    var tiles = new ushort[count];
                    for (int tile = 0; tile < tiles.Length; tile++)
                        tiles[tile] = ReadWord(0xa40000 |
                            unchecked((ushort)(cursor + 4 + tile * 2)));
                    native.Add((unchecked((ushort)((destination -
                        EnemyBg2FrameLayout.WorkingRamBase) >> 1)), tiles));
                    cursor = unchecked((ushort)(cursor + 4 + count * 2));
                }
                AssertTrue(terminated,
                    $"Crocomire native BG2 stream $A4:{stream:X4} terminates");
            }
            AssertEqual(native.Count, installed.Length,
                $"Crocomire BG2 frame $A4:{frame.Pointer:X4} retains every command");
            for (int index = 0; index < native.Count; index++)
            {
                AssertEqual(native[index].DestinationWord,
                    installed.Span[index].DestinationWord,
                    $"Crocomire BG2 frame $A4:{frame.Pointer:X4} write {index} destination");
                AssertTrue(native[index].Tiles.AsSpan().SequenceEqual(
                        installed.Span[index].Tiles.Span),
                    $"Crocomire BG2 frame $A4:{frame.Pointer:X4} write {index} tiles");
            }
        }

        string overrideDirectory = Path.Combine(stockDirectory, "crocomire-body-override");
        Directory.CreateDirectory(overrideDirectory);
        string source = Path.Combine(stockDirectory, CrocomireBg2FrameDefinitions.FileName);
        var authored = JsonSerializer.Deserialize<EnemyBg2FrameDocument>(
            File.ReadAllBytes(source),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        EnemyBg2FrameDefinition editedFrame = CrocomireBg2FrameDefinitions.Frames[0];
        EnemyBg2WriteDocument lastWrite = authored.Frames[editedFrame.Name][^1];
        lastWrite.Tiles[0] ^= 1;
        string overridePath = Path.Combine(overrideDirectory,
            CrocomireBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(authored,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var editedGuard = new ExtendedVisualReadGuard(rom);
        editedGuard.BlockFrame(CrocomireBodyVisualDefinitions.Bank, editedFrame.Pointer);
        (OamBuffer stockOam, byte[] stockVram) = Draw(stock, editedGuard,
            editedFrame.Pointer, true);
        (OamBuffer editedOam, byte[] editedVram) = Draw(edited, editedGuard,
            editedFrame.Pointer, true);
        AssertTrue(stockOam.LowTable.SequenceEqual(editedOam.LowTable) &&
                   stockOam.HighTable.SequenceEqual(editedOam.HighTable),
            "Crocomire BG2 edit does not move its OAM components");
        AssertTrue(!stockVram.SequenceEqual(editedVram),
            "Crocomire BG2 edit changes the live VRAM result");
        AssertTrue(Draw(edited, editedGuard, editedFrame.Pointer, false).Vram
                .SequenceEqual(Draw(stock, editedGuard, editedFrame.Pointer, false).Vram),
            "Crocomire BG2 edit still obeys the native new-frame gate");
        Console.WriteLine(
            "  Crocomire body: 236 selectors, 50 mixed/pure OAM roots and 42 BG2 roots match native OAM and VRAM with visual ROM reads denied; BG2 edits obey the new-frame gate.");

        ushort ReadWord(int address) => unchecked((ushort)(rom.ReadByte(address) |
            rom.ReadByte((address & 0xff0000) | unchecked((ushort)(address + 1))) << 8));

        static (OamBuffer Oam, byte[] Vram) Draw(EnemyTileArtworkCatalog? art,
            ISnesAddressSpace bus, ushort pointer, bool newFrame)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = art };
            var vram = new SnesVram();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
            var queues = (List<ushort>[])typeof(RoomEnemySystem)
                .GetField("_drawQueues", flags)!.GetValue(enemies)!;
            queues[0].Add(0);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireDefinition;
            slot.Definition = default(RoomEnemyDefinition) with
                { Bank = CrocomireBodyVisualDefinitions.Bank };
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.UsesExtendedSpritemap);
            if (newFrame)
                slot.ExtraProperties = slot.ExtraProperties.With(
                    EnemyExtraProperties.NewInstructionFrame);
            slot.SpritemapPointer = pointer;
            slot.XPosition = 0x0080;
            slot.YPosition = 0x0080;
            var oam = new OamBuffer();
            enemies.DrawLayers(oam, 0, 0, 0, 0);
            return (oam, vram.Bytes.ToArray());
        }
    }
}
