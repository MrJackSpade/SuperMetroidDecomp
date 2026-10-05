using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledSporeSpawnCollision(
        SuperMetroidAddressSpace rom)
    {
        var selected = new HashSet<ushort>();
        for (int index = 0;
             index < SporeSpawnInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = SporeSpawnInstructionProgramDefinitions
                .PresentationWordAddress(index);
            selected.Add(ReadWord((DraygonBg2FrameDefinitions.Bank << 16) | address));
        }

        var native = new RoomEnemySystem();
        var installed = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed,
            new FrontendCartridgeReadGuard(rom));
        var walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        RoomEnemySlot nativeSlot = native.Slots[0];
        RoomEnemySlot installedSlot = installed.Slots[0];
        nativeSlot.Definition = installedSlot.Definition =
            default(RoomEnemyDefinition) with
            {
                Bank = DraygonBg2FrameDefinitions.Bank,
            };
        // The test-only cartridge walker supplies an independent reference.
        nativeSlot.EnemyDefinitionPointer = 0;
        installedSlot.EnemyDefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;
        nativeSlot.XPosition = installedSlot.XPosition = 0x100;
        nativeSlot.YPosition = installedSlot.YPosition = 0x100;

        var seenLists = new HashSet<ushort>();
        int frameCount = 0;
        int callbackComparisons = 0;
        foreach (EnemyExtendedFrameDefinition frame in
                 EnemyExtendedFrameDefinitions.Frames)
        {
            if (frame.Bank != DraygonBg2FrameDefinitions.Bank ||
                !SporeSpawnCollisionDefinitions.IsFrame(frame.Pointer))
                continue;
            AssertTrue(selected.Contains(frame.Pointer),
                $"Spore Spawn program selects native frame {frame.Pointer:X4}");
            nativeSlot.SpritemapPointer = installedSlot.SpritemapPointer =
                frame.Pointer;
            int root = (frame.Bank << 16) | frame.Pointer;
            ReadOnlySpan<SporeSpawnCollisionComponent> components =
                SporeSpawnCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual((int)rom.ReadByte(root), components.Length,
                $"Spore Spawn frame {frame.Pointer:X4} native component count");
            for (int component = 0; component < components.Length; component++)
            {
                int record = root + 2 + component * 8;
                SporeSpawnCollisionComponent compiled = components[component];
                AssertEqual(ReadWord(record), unchecked((ushort)compiled.X),
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} X");
                AssertEqual(ReadWord(record + 2), unchecked((ushort)compiled.Y),
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} Y");
                AssertEqual(ReadWord(record + 6), compiled.HitboxPointer,
                    $"Spore Spawn frame {frame.Pointer:X4} component {component} list");
                seenLists.Add(compiled.HitboxPointer);
            }
            for (int y = -80; y <= 80; y += 16)
            for (int x = -64; x <= 64; x += 16)
            for (int shot = 0; shot <= 1; shot++)
            {
                ushort targetX = unchecked((ushort)(0x100 + x));
                ushort targetY = unchecked((ushort)(0x100 + y));
                object?[] nativeArguments =
                    [nativeSlot, targetX, targetY, (ushort)2, (ushort)2,
                        shot != 0, (ushort)0];
                object?[] installedArguments =
                    [installedSlot, targetX, targetY, (ushort)2, (ushort)2,
                        shot != 0, (ushort)0];
                bool nativeFound = ReferenceExtendedCollision(
                    (SuperMetroid.AssetExtraction.CartridgeImportAddressSpace)rom, nativeArguments);
                bool installedFound = (bool)walker.Invoke(installed, installedArguments)!;
                AssertEqual(nativeFound, installedFound,
                    $"Spore Spawn frame {frame.Pointer:X4} native overlap at {x},{y}, shot={shot}");
                AssertEqual((ushort)nativeArguments[^1]!,
                    (ushort)installedArguments[^1]!,
                    $"Spore Spawn frame {frame.Pointer:X4} callback at {x},{y}, shot={shot}");
                callbackComparisons++;
            }
            frameCount++;
        }
        AssertEqual(SporeSpawnCollisionDefinitions.FrameCount, frameCount,
            "all compiled Spore Spawn frames are installed visual identities");
        AssertEqual(12, frameCount, "all twelve Spore Spawn OAM frame roots");
        AssertEqual(SporeSpawnCollisionDefinitions.ListCount, seenLists.Count,
            "all compiled Spore Spawn hitbox lists are referenced");
        foreach (ushort pointer in seenLists)
        {
            int list = (DraygonBg2FrameDefinitions.Bank << 16) | pointer;
            ReadOnlySpan<SporeSpawnCollisionHitbox> hitboxes =
                SporeSpawnCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((int)ReadWord(list), hitboxes.Length,
                $"Spore Spawn list {pointer:X4} native hitbox count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                int record = list + 2 + index * 12;
                SporeSpawnCollisionHitbox box = hitboxes[index];
                AssertEqual(ReadWord(record), unchecked((ushort)box.Left),
                    $"Spore Spawn list {pointer:X4} box {index} left");
                AssertEqual(ReadWord(record + 2), unchecked((ushort)box.Top),
                    $"Spore Spawn list {pointer:X4} box {index} top");
                AssertEqual(ReadWord(record + 4), unchecked((ushort)box.Right),
                    $"Spore Spawn list {pointer:X4} box {index} right");
                AssertEqual(ReadWord(record + 6), unchecked((ushort)box.Bottom),
                    $"Spore Spawn list {pointer:X4} box {index} bottom");
                AssertEqual(ReadWord(record + 8), box.TouchAi,
                    $"Spore Spawn list {pointer:X4} box {index} touch callback");
                AssertEqual(ReadWord(record + 10), box.ShotAi,
                    $"Spore Spawn list {pointer:X4} box {index} shot callback");
            }
        }
        AssertThrows<InvalidDataException>(
            () => SporeSpawnCollisionDefinitions.ComponentsAt(0xa2df),
            "Draygon frame cannot enter Spore Spawn collision data");
        Console.WriteLine($"  Spore Spawn collision: {frameCount} roots, {seenLists.Count} lists and {callbackComparisons} live touch/shot comparisons match the cartridge with installed ROM reads denied.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }
}
