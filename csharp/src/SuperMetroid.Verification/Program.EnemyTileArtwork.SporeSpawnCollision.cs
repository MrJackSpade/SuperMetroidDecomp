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
             index < SporeSpawnInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = SporeSpawnInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            selected.Add(ReadWord((DraygonBg2FrameDefinitions.Bank << 16) | address));
        }

        var installed = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed,
            new FrontendCartridgeReadGuard(rom));
        var walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        RoomEnemySlot installedSlot = installed.Slots[0];
        installedSlot.Definition =
            default(RoomEnemyDefinition) with
            {
                Bank = DraygonBg2FrameDefinitions.Bank,
            };
        // The test-only cartridge walker supplies an independent reference.
        installedSlot.EnemyDefinitionPointer = RoomEnemySystem.SporeSpawnDefinition;
        installedSlot.XPosition = 0x100;
        installedSlot.YPosition = 0x100;

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
            installedSlot.SpritemapPointer =
                frame.Pointer;
            int root = (frame.Bank << 16) | frame.Pointer;
            SporeSpawnCollisionDefinitions.ComponentSequence components =
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
                object?[] installedArguments =
                    [installedSlot, targetX, targetY, (ushort)2, (ushort)2,
                        shot != 0, (ushort)0];
                bool nativeFound = NativeCallback(root, targetX, targetY, shot != 0, out ushort nativeCallback);
                bool installedFound = (bool)walker.Invoke(installed, installedArguments)!;
                AssertEqual(nativeFound, installedFound,
                    $"Spore Spawn frame {frame.Pointer:X4} native overlap at {x},{y}, shot={shot}");
                AssertEqual(nativeCallback,
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
        foreach (ushort invalid in new ushort[] { 0xee64, 0xee66, 0xee70, 0xee7a, 0xeef7, 0xef01, 0xef33, 0xef62 })
        {
            AssertTrue(!SporeSpawnCollisionDefinitions.IsFrame(invalid), "Spore component exact native root domain");
            AssertThrows<InvalidDataException>(() => SporeSpawnCollisionDefinitions.ComponentsAt(invalid),
                "Spore invalid or unused component root rejected");
        }
        var closedSpore = SporeSpawnCollisionDefinitions.ComponentsAt(0xee6f);
        var openSpore = SporeSpawnCollisionDefinitions.ComponentsAt(0xef3d);
        AssertThrows<IndexOutOfRangeException>(() => _ = closedSpore[1], "Spore closed frame has no inner point");
        AssertThrows<IndexOutOfRangeException>(() => _ = openSpore[-1], "Spore component rejects negative index");
        AssertThrows<IndexOutOfRangeException>(() => _ = openSpore[2], "Spore open frame has exactly two components");
        Console.WriteLine($"  Spore Spawn collision: {frameCount} roots, {seenLists.Count} lists and {callbackComparisons} live touch/shot comparisons match the cartridge with installed ROM reads denied.");

        // Independent cartridge oracle: bank A0:9ADF..9B78 orders touch rectangles;
        // 9C43..9D20 supplies the distinct shot boundaries and callback operand.
        // Only this fixture reads original frame/list bytes; the real walker remains guarded.
        bool NativeCallback(int root, ushort targetX, ushort targetY, bool shot, out ushort callback)
        {
            callback = 0;
            ushort targetLeft = unchecked((ushort)(targetX - 2));
            ushort targetRight = unchecked((ushort)(targetX + 2));
            ushort targetTop = unchecked((ushort)(targetY - 2));
            ushort targetBottom = unchecked((ushort)(targetY + 2));
            int count = rom.ReadByte(root);
            for (int component = 0; component < count; component++)
            {
                int record = root + 2 + 8 * component;
                ushort centerX = unchecked((ushort)(0x100 + ReadWord(record)));
                ushort centerY = unchecked((ushort)(0x100 + ReadWord(record + 2)));
                int list = 0xa50000 | ReadWord(record + 6);
                int rectangles = ReadWord(list);
                for (int rectangle = 0; rectangle < rectangles; rectangle++)
                {
                    int box = list + 2 + 12 * rectangle;
                    ushort left = unchecked((ushort)(centerX + ReadWord(box)));
                    ushort top = unchecked((ushort)(centerY + ReadWord(box + 2)));
                    ushort right = unchecked((ushort)(centerX + ReadWord(box + 4)));
                    ushort bottom = unchecked((ushort)(centerY + ReadWord(box + 6)));
                    bool overlaps = shot
                        ? unchecked((short)(targetRight - left)) >= 0 && unchecked((short)(targetLeft - right)) < 0 &&
                          unchecked((short)(targetBottom - top)) >= 0 && unchecked((short)(targetTop - bottom)) < 0
                        : unchecked((short)(left - targetRight)) < 0 && unchecked((short)(right - targetLeft)) >= 0 &&
                          unchecked((short)(top - targetBottom)) < 0 && unchecked((short)(bottom - targetTop)) >= 0;
                    if (!overlaps) continue;
                    callback = ReadWord(box + (shot ? 10 : 8));
                    return true;
                }
            }
            return false;
        }

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }
}
