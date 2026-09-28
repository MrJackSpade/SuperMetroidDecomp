using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compare every compiled physical record with the pinned cartridge, then
    /// exercise the actual enemy collision walker against a bus that forbids
    /// cartridge reads. The whole-room census separately checks the fight's
    /// first thirty live frames with pixel and state parity.
    /// </summary>
    private static void VerifyCrocomireBodyCollisionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new CrocomireTongueNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        var native = new RoomEnemySystem();
        var compiled = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(native, rom);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(compiled, denied);
        RoomEnemySlot nativeSlot = native.Slots[0];
        RoomEnemySlot compiledSlot = compiled.Slots[0];
        nativeSlot.Definition = compiledSlot.Definition =
            default(RoomEnemyDefinition) with
            { Bank = CrocomireBodyCollisionDefinitions.Bank };
        // The native side deliberately takes the generic ROM walker. The
        // installed side selects Crocomire's compiled physical data.
        nativeSlot.EnemyDefinitionPointer = 0xffff;
        compiledSlot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireDefinition;
        nativeSlot.XPosition = compiledSlot.XPosition = 0x1000;
        nativeSlot.YPosition = compiledSlot.YPosition = 0x1000;

        ReadOnlySpan<ushort> frames = CrocomireBodyVisualDefinitions.Frames;
        AssertEqual(50, frames.Length,
            "all fifty selected Crocomire fight-body roots have collision");
        var seenLists = new HashSet<ushort>();
        int probes = 0;
        foreach (ushort frame in frames)
        {
            AssertTrue(CrocomireBodyCollisionDefinitions.HasFrame(frame),
                $"Crocomire body $A4:{frame:X4} has compiled collision");
            ReadOnlySpan<CrocomireBodyCollisionComponent> components =
                CrocomireBodyCollisionDefinitions.ComponentsAt(frame);
            AssertEqual((byte)ReadWord(0xa40000 | frame), (byte)components.Length,
                $"Crocomire body $A4:{frame:X4} component count");
            for (int index = 0; index < components.Length; index++)
            {
                int address = 0xa40000 | unchecked((ushort)(frame + 2 + index * 8));
                CrocomireBodyCollisionComponent component = components[index];
                AssertEqual(unchecked((short)ReadWord(address)), component.X,
                    $"Crocomire $A4:{frame:X4} component {index} X");
                AssertEqual(unchecked((short)ReadWord(address + 2)), component.Y,
                    $"Crocomire $A4:{frame:X4} component {index} Y");
                AssertEqual(ReadWord(address + 6), component.HitboxPointer,
                    $"Crocomire $A4:{frame:X4} component {index} hitbox list");
                seenLists.Add(component.HitboxPointer);
            }

            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach (short dx in new short[] { -112, -96, -64, -40, -16, 0, 16, 40, 64 })
            foreach (short dy in new short[] { -96, -64, -40, -16, 0, 16, 40, 64 })
            foreach (bool shot in new[] { false, true })
            {
                ushort x = unchecked((ushort)(0x1000 + dx));
                ushort y = unchecked((ushort)(0x1000 + dy));
                object?[] nativeArguments =
                    [nativeSlot, x, y, (ushort)8, (ushort)8, shot, (ushort)0];
                object?[] compiledArguments =
                    [compiledSlot, x, y, (ushort)8, (ushort)8, shot, (ushort)0];
                bool nativeHit = (bool)walker.Invoke(native, nativeArguments)!;
                bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual((ushort)nativeArguments[^1]!,
                    (ushort)compiledArguments[^1]!,
                    $"Crocomire $A4:{frame:X4} selected callback");
                probes++;
            }
        }

        AssertEqual(15, seenLists.Count,
            "fifty Crocomire body frames refer to fifteen hitbox lists");
        foreach (ushort list in seenLists)
        {
            ReadOnlySpan<CrocomireBodyCollisionHitbox> hitboxes =
                CrocomireBodyCollisionDefinitions.HitboxesAt(list);
            AssertEqual(ReadWord(0xa40000 | list), (ushort)hitboxes.Length,
                $"Crocomire $A4:{list:X4} hitbox count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                int address = 0xa40000 | unchecked((ushort)(list + 2 + index * 12));
                CrocomireBodyCollisionHitbox hitbox = hitboxes[index];
                AssertEqual(unchecked((short)ReadWord(address)), hitbox.Left,
                    $"Crocomire $A4:{list:X4} hitbox {index} left");
                AssertEqual(unchecked((short)ReadWord(address + 2)), hitbox.Top,
                    $"Crocomire $A4:{list:X4} hitbox {index} top");
                AssertEqual(unchecked((short)ReadWord(address + 4)), hitbox.Right,
                    $"Crocomire $A4:{list:X4} hitbox {index} right");
                AssertEqual(unchecked((short)ReadWord(address + 6)), hitbox.Bottom,
                    $"Crocomire $A4:{list:X4} hitbox {index} bottom");
                AssertEqual(ReadWord(address + 8), hitbox.TouchAi,
                    $"Crocomire $A4:{list:X4} hitbox {index} touch callback");
                AssertEqual(ReadWord(address + 10), hitbox.ShotAi,
                    $"Crocomire $A4:{list:X4} hitbox {index} shot callback");
            }
        }
        AssertThrows<InvalidDataException>(
            () => CrocomireBodyCollisionDefinitions.ComponentsAt(0x8000),
            "unknown Crocomire body frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CrocomireBodyCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Crocomire body hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire body collision never reads ROM bytes");
        Console.WriteLine($"Crocomire body collision: {frames.Length} frames, " +
            $"{seenLists.Count} hitbox lists, {probes} native-equivalent probes; " +
            "installed path reads no ROM.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }
}
