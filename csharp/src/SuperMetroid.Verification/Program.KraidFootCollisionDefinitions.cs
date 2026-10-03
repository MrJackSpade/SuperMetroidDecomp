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
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
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
        VerifyKraidFootFirstX(rom);
        VerifyKraidFootFirstY(rom);
        VerifyKraidFootSecondX(rom);
        VerifyKraidFootSecondY(rom);
        VerifyKraidFootSharedHitbox(rom);

        for (int index = 0;
             index < KraidFootInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address = KraidFootInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort selectedFrame = ReadWord(address);
            AssertTrue(KraidFootCollisionDefinitions.TryGetComponents(selectedFrame, out _),
                $"Kraid foot instruction $A7:{address:X4} selects compiled physical frame");
        }

        KraidFootHitboxSequence hitboxes =
            KraidFootCollisionDefinitions.HitboxesAt(KraidFootCollisionDefinitions.HitboxList);
        KraidFootCollisionHitbox box = hitboxes[0];

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
            foreach (KraidFootCollisionComponent component in components)
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
