using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidFootCollisionDefinitions()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new KraidFootCollisionNoReadBus();
        var native = new RoomEnemySystem();
        var installed = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(native, rom);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed, denied);

        RoomEnemySlot nativeFoot = native.Slots[0];
        RoomEnemySlot installedFoot = installed.Slots[0];
        nativeFoot.Definition = installedFoot.Definition =
            default(RoomEnemyDefinition) with { Bank = 0xa7 };
        nativeFoot.EnemyDefinitionPointer = 0xffff; // Force the cartridge reference walker.
        installedFoot.EnemyDefinitionPointer = RoomEnemySystem.KraidFootDefinition;
        AssertEqual(35, KraidFootCollisionDefinitions.FrameCount,
            "Kraid foot walking extended-frame count");
        ushort[] frames =
        [
            KraidFootCollisionDefinitions.InitialFrame,
            .. Enumerable.Range(0, KraidFootCollisionDefinitions.FrameCount)
                .Select(KraidFootCollisionDefinitions.FramePointer),
        ];
        foreach (ushort frame in frames)
        {
            AssertTrue(KraidFootCollisionDefinitions.TryGetComponents(
                    frame, out var components),
                $"Kraid foot frame $A7:{frame:X4} has compiled physical components");
            AssertEqual(2, components.Length,
                $"Kraid foot frame $A7:{frame:X4} native component count");
            AssertEqual(2, rom.ReadByte(0xa70000 | frame),
                $"Kraid foot frame $A7:{frame:X4} native header");
            for (int index = 0; index < components.Length; index++)
            {
                KraidFootCollisionComponent component = components.Span[index];
                ushort record = unchecked((ushort)(frame + 2 + index * 8));
                AssertEqual(unchecked((short)ReadWord(record)), component.X,
                    $"Kraid foot frame $A7:{frame:X4} component {index} X");
                AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(record + 2)))),
                    component.Y,
                    $"Kraid foot frame $A7:{frame:X4} component {index} Y");
                AssertEqual(ReadWord(unchecked((ushort)(record + 6))),
                    component.HitboxPointer,
                    $"Kraid foot frame $A7:{frame:X4} component {index} hitbox list");
            }
        }

        for (int index = 0;
             index < KraidFootInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KraidFootInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort selectedFrame = ReadWord(address);
            AssertTrue(KraidFootCollisionDefinitions.TryGetComponents(selectedFrame, out _),
                $"Kraid foot instruction $A7:{address:X4} selects compiled physical frame");
        }

        ReadOnlySpan<KraidFootCollisionHitbox> hitboxes =
            KraidFootCollisionDefinitions.HitboxesAt(KraidFootCollisionDefinitions.HitboxList);
        AssertEqual(1, hitboxes.Length, "Kraid foot shared physical hitbox count");
        AssertEqual(1, ReadWord(KraidFootCollisionDefinitions.HitboxList),
            "Kraid foot native shared hitbox count");
        KraidFootCollisionHitbox box = hitboxes[0];
        ushort hitboxRecord = unchecked((ushort)(KraidFootCollisionDefinitions.HitboxList + 2));
        AssertEqual(unchecked((short)ReadWord(hitboxRecord)), box.Left, "Kraid foot hitbox left");
        AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(hitboxRecord + 2)))),
            box.Top, "Kraid foot hitbox top");
        AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(hitboxRecord + 4)))),
            box.Right, "Kraid foot hitbox right");
        AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(hitboxRecord + 6)))),
            box.Bottom, "Kraid foot hitbox bottom");
        AssertEqual(ReadWord(unchecked((ushort)(hitboxRecord + 8))),
            box.TouchAi, "Kraid foot touch callback");
        AssertEqual(ReadWord(unchecked((ushort)(hitboxRecord + 10))),
            box.ShotAi, "Kraid foot shot callback");

        int probes = 0;
        foreach (ushort frame in frames)
        foreach ((ushort originX, ushort originY) in
                 new (ushort, ushort)[]
                 {
                     (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                 })
        {
            nativeFoot.SpritemapPointer = installedFoot.SpritemapPointer = frame;
            nativeFoot.XPosition = installedFoot.XPosition = originX;
            nativeFoot.YPosition = installedFoot.YPosition = originY;
            KraidFootCollisionDefinitions.TryGetComponents(frame, out var components);
            foreach (KraidFootCollisionComponent component in components.Span)
            {
                ushort componentX = unchecked((ushort)(originX + component.X));
                ushort componentY = unchecked((ushort)(originY + component.Y));
                foreach (ushort x in BoundaryPoints(componentX, box.Left, box.Right))
                foreach (ushort y in BoundaryPoints(componentY, box.Top, box.Bottom))
                for (int shot = 0; shot <= 1; shot++)
                {
                    object?[] nativeArguments =
                        [nativeFoot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                    object?[] installedArguments =
                        [installedFoot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                    bool nativeHit = (bool)walker.Invoke(native, nativeArguments)!;
                    bool installedHit = (bool)walker.Invoke(installed, installedArguments)!;
                    AssertEqual(nativeHit, installedHit,
                        $"Kraid foot $A7:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                    AssertEqual((ushort)nativeArguments[^1]!,
                        (ushort)installedArguments[^1]!,
                        $"Kraid foot $A7:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                    probes++;
                }
            }
        }

        AssertTrue(!KraidFootCollisionDefinitions.TryGetComponents(0x8000, out _),
            "uncompiled Kraid foot frame is rejected");
        AssertThrows<InvalidDataException>(
            () => KraidFootCollisionDefinitions.HitboxesAt(0x8000).ToArray(),
            "uncompiled Kraid foot hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "all installed Kraid foot collision probes read no cartridge data");
        Console.WriteLine($"Kraid foot collision: {frames.Length} frames, " +
            $"one hitbox list and {probes} native-equivalent touch/shot probes; " +
            "installed path forbids ROM reads.");

        ushort ReadWord(ushort address) => unchecked((ushort)(
            rom.ReadByte(0xa70000 | address) |
            rom.ReadByte(0xa70000 | unchecked((ushort)(address + 1))) << 8));

        static ushort[] BoundaryPoints(ushort origin, short low, short high) =>
        [
            unchecked((ushort)(origin + low - 1)),
            unchecked((ushort)(origin + low)),
            unchecked((ushort)(origin + low + 1)),
            unchecked((ushort)(origin + high - 1)),
            unchecked((ushort)(origin + high)),
            unchecked((ushort)(origin + high + 1)),
        ];
    }

    private sealed class KraidFootCollisionNoReadBus : ISnesAddressSpace,
        IImportCartridgeSource
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Kraid-foot collision read ROM byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException(
                $"Installed Kraid-foot collision wrote byte ${address:X6}.");
    }
}
