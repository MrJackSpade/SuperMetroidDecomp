using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares installed Kraid-foot collision results with native hitboxes and ensures runtime probes avoid ROM reads.</summary>
    private static void VerifyKraidFootCollisionDefinitions()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new KraidFootCollisionNoReadBus();
        var installed = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(installed, denied);

        RoomEnemySlot installedFoot = installed.Slots[0];
        installedFoot.Definition =
            default(RoomEnemyDefinition) with { Bank = 0xa7 };
        installedFoot.EnemyDefinitionPointer = RoomEnemySystem.KraidFootDefinition;
        AssertEqual(35, KraidFootCollisionDefinitions.FrameCount,
            "Kraid foot walking extended-frame count");
        ushort[] frames =
        [
            KraidFootCollisionDefinitions.InitialFrame,
            .. Enumerable.Range(0, KraidFootCollisionDefinitions.FrameCount)
                .Select(KraidFootCollisionDefinitions.FramePointer),
        ];
        Suite(nameof(VerifyKraidFootFirstX), () => VerifyKraidFootFirstX(rom));
        Suite(nameof(VerifyKraidFootFirstY), () => VerifyKraidFootFirstY(rom));
        Suite(nameof(VerifyKraidFootSecondX), () => VerifyKraidFootSecondX(rom));
        Suite(nameof(VerifyKraidFootSecondY), () => VerifyKraidFootSecondY(rom));
        Suite(nameof(VerifyKraidFootSharedHitbox), () => VerifyKraidFootSharedHitbox(rom));

        for (int index = 0;
             index < KraidFootInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort address = KraidFootInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
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
            installedFoot.SpritemapPointer = frame;
            installedFoot.XPosition = originX;
            installedFoot.YPosition = originY;
            KraidFootCollisionDefinitions.TryGetComponents(frame, out var components);
            foreach (KraidFootCollisionComponent component in components)
            {
                ushort componentX = unchecked((ushort)(originX + component.X));
                ushort componentY = unchecked((ushort)(originY + component.Y));
                foreach (ushort x in BoundaryPoints(componentX, box.Left, box.Right))
                foreach (ushort y in BoundaryPoints(componentY, box.Top, box.Bottom))
                for (int shot = 0; shot <= 1; shot++)
                {
                    object?[] installedArguments =
                        [installedFoot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                    bool nativeHit = NativeHit(frame, originX, originY, x, y, shot != 0, out ushort nativeCallback);
                    bool installedHit = (bool)walker.Invoke(installed, installedArguments)!;
                    AssertEqual(nativeHit, installedHit,
                        $"Kraid foot $A7:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                    AssertEqual(nativeCallback,
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

        bool NativeHit(ushort frame, ushort originX, ushort originY,
            ushort x, ushort y, bool shot, out ushort callback)
        {
            // Import-only decoder: components are eight bytes; hitboxes are twelve.
            // Native $A0:9A5A and $9B7F differ at their rectangle boundaries.
            callback = 0;
            for (int part = 0; part < ReadWord(frame); part++)
            {
                ushort component = unchecked((ushort)(frame + 2 + part * 8));
                ushort cx = unchecked((ushort)(originX + ReadWord(component)));
                ushort cy = unchecked((ushort)(originY + ReadWord((ushort)(component + 2))));
                ushort list = ReadWord((ushort)(component + 6));
                for (int boxIndex = 0; boxIndex < ReadWord(list); boxIndex++)
                {
                    ushort rectangle = unchecked((ushort)(list + 2 + boxIndex * 12));
                    ushort left = unchecked((ushort)(cx + ReadWord(rectangle)));
                    ushort top = unchecked((ushort)(cy + ReadWord((ushort)(rectangle + 2))));
                    ushort right = unchecked((ushort)(cx + ReadWord((ushort)(rectangle + 4))));
                    ushort bottom = unchecked((ushort)(cy + ReadWord((ushort)(rectangle + 6))));
                    bool hit = shot
                        ? unchecked((short)(x - left)) >= 0 && unchecked((short)(x - right)) < 0 &&
                          unchecked((short)(y - top)) >= 0 && unchecked((short)(y - bottom)) < 0
                        : unchecked((short)(left - x)) < 0 && unchecked((short)(right - x)) >= 0 &&
                          unchecked((short)(top - y)) < 0 && unchecked((short)(bottom - y)) >= 0;
                    if (!hit) continue;
                    callback = ReadWord((ushort)(rectangle + (shot ? 10 : 8)));
                    return true;
                }
            }
            return false;
        }
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

    /// <summary>Address-space sentinel that records and rejects every cartridge read or write.</summary>
    private sealed class KraidFootCollisionNoReadBus : ISnesAddressSpace,
        IImportCartridgeSource
    {
        /// <summary>Number of forbidden cartridge-byte read attempts observed by this sentinel.</summary>
        internal int ReadAttempts { get; private set; }

        /// <summary>Records a cartridge read attempt and rejects it to prove the installed path is ROM-free.</summary>
        /// <param name="address">The cartridge address the caller attempted to read.</param>
        /// <returns>No value is returned because every read attempt is rejected.</returns>
        /// <exception cref="InvalidOperationException">Always thrown after the attempt counter is incremented.</exception>
        public byte ReadCartridgeByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Kraid-foot collision read ROM byte ${address:X6}.");
        }

        /// <summary>Rejects writes because this sentinel is only used to assert that collision probing has no bus effects.</summary>
        /// <param name="address">The address the caller attempted to modify.</param>
        /// <param name="value">The byte the caller attempted to write.</param>
        /// <exception cref="InvalidOperationException">Always thrown because this sentinel does not permit writes.</exception>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException(
                $"Installed Kraid-foot collision wrote byte ${address:X6}.");
    }
}
