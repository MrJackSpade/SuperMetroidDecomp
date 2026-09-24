using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWalkingPirateCollisionDefinitions()
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(38, WalkingPirateCollisionDefinitions.FrameCount,
            "compiled walking-Pirate extended-frame count");
        AssertEqual(46, WalkingPirateCollisionDefinitions.ListCount,
            "compiled walking-Pirate hitbox-list count");
        int componentCount = 0;
        var nativeRanges = new HashSet<int>();
        for (int index = 0; index < WalkingPirateCollisionDefinitions.FrameCount;
             index++)
        {
            WalkingPirateCollisionFrame frame =
                WalkingPirateCollisionDefinitions.Frame(index);
            int native = 0xb20000 | frame.Pointer;
            AssertEqual(rom.ReadByte(native), frame.Components.Length,
                $"walking-Pirate frame $B2:{frame.Pointer:X4} component count");
            Block(frame.Pointer, 2 + frame.Components.Length * 8);
            for (int componentIndex = 0;
                 componentIndex < frame.Components.Length; componentIndex++)
            {
                WalkingPirateCollisionComponent component =
                    frame.Components[componentIndex];
                ushort record = unchecked((ushort)(frame.Pointer + 2 +
                    componentIndex * 8));
                AssertEqual(unchecked((short)ReadWord(record)), component.X,
                    "walking-Pirate native collision-component X");
                AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(record + 2)))),
                    component.Y, "walking-Pirate native collision-component Y");
                AssertEqual(ReadWord(unchecked((ushort)(record + 6))),
                    component.HitboxPointer,
                    "walking-Pirate native component hitbox identity");
                componentCount++;
            }
        }
        AssertEqual(76, componentCount,
            "walking-Pirate compiled collision-component count");
        foreach (EnemyExtendedFrameDefinition frame in EnemyExtendedFrameDefinitions.Frames)
            AssertTrue(WalkingPirateCollisionDefinitions.ComponentsAt(frame.Pointer).Length > 0,
                $"walking-Pirate editable frame {frame.Name} has fixed collision");
        AssertTrue(WalkingPirateCollisionDefinitions.ComponentsAt(
                EnemyAiCodePointers.BankB2.EmptyExtendedSpritemap).Length > 0,
            "walking-Pirate initializer's common empty frame has fixed collision");
        int rectangleCount = 0;
        for (int index = 0; index < WalkingPirateCollisionDefinitions.ListCount;
             index++)
        {
            WalkingPirateCollisionList list =
                WalkingPirateCollisionDefinitions.List(index);
            AssertEqual(ReadWord(list.Pointer), list.Rectangles.Length,
                $"walking-Pirate hitbox $B2:{list.Pointer:X4} rectangle count");
            Block(list.Pointer, 2 + list.Rectangles.Length * 12);
            for (int rectangle = 0; rectangle < list.Rectangles.Length; rectangle++)
            {
                WalkingPirateCollisionHitbox expected = list.Rectangles[rectangle];
                ushort address = unchecked((ushort)(list.Pointer + 2 +
                    rectangle * 12));
                AssertEqual(unchecked((short)ReadWord(address)), expected.Left,
                    "walking-Pirate native hitbox left");
                AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(address + 2)))),
                    expected.Top, "walking-Pirate native hitbox top");
                AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(address + 4)))),
                    expected.Right, "walking-Pirate native hitbox right");
                AssertEqual(unchecked((short)ReadWord(unchecked((ushort)(address + 6)))),
                    expected.Bottom, "walking-Pirate native hitbox bottom");
                AssertEqual(ReadWord(unchecked((ushort)(address + 8))),
                    expected.TouchAi, "walking-Pirate native touch callback");
                AssertEqual(ReadWord(unchecked((ushort)(address + 10))),
                    expected.ShotAi, "walking-Pirate native shot callback");
                rectangleCount++;
            }
        }
        AssertEqual(48, rectangleCount,
            "walking-Pirate compiled collision-rectangle count");
        AssertThrows<InvalidDataException>(
            () => WalkingPirateCollisionDefinitions.ComponentsAt(0x8000),
            "unknown walking-Pirate frame rejects instead of reading ROM");
        AssertThrows<InvalidDataException>(
            () => WalkingPirateCollisionDefinitions.HitboxesAt(0x8000),
            "unknown walking-Pirate hitbox rejects instead of reading ROM");
        ushort warmedFrame = WalkingPirateCollisionDefinitions.Frame(0).Pointer;
        ushort warmedList = WalkingPirateCollisionDefinitions.List(0).Pointer;
        _ = WalkingPirateCollisionDefinitions.ComponentsAt(warmedFrame).Length;
        _ = WalkingPirateCollisionDefinitions.HitboxesAt(warmedList).Length;
        long beforeLookup = GC.GetAllocatedBytesForCurrentThread();
        int lookupChecksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            lookupChecksum += WalkingPirateCollisionDefinitions
                .ComponentsAt(warmedFrame).Length;
            lookupChecksum += WalkingPirateCollisionDefinitions
                .HitboxesAt(warmedList).Length;
        }
        AssertTrue(lookupChecksum > 0, "walking-Pirate lookup probe consumes data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - beforeLookup,
            "warmed walking-Pirate collision lookups allocate no frame storage");

        var guard = new WalkingPirateCollisionReadGuard(rom, nativeRanges);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        int probes = 0;
        foreach (WalkingPirateCollisionFrame frame in
                 Enumerable.Range(0, WalkingPirateCollisionDefinitions.FrameCount)
                     .Select(WalkingPirateCollisionDefinitions.Frame))
        {
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                var native = CreateSystem(rom, frame.Pointer, originX,
                    originY, walkingPirate: false);
                var compiled = CreateSystem(guard, frame.Pointer, originX,
                    originY, walkingPirate: true);
                foreach (WalkingPirateCollisionComponent component in
                         frame.Components)
                {
                    foreach (WalkingPirateCollisionHitbox hitbox in
                             WalkingPirateCollisionDefinitions.HitboxesAt(
                                 component.HitboxPointer))
                    {
                        ushort componentX = unchecked((ushort)(originX + component.X));
                        ushort componentY = unchecked((ushort)(originY + component.Y));
                        ushort left = unchecked((ushort)(componentX + hitbox.Left));
                        ushort right = unchecked((ushort)(componentX + hitbox.Right));
                        ushort top = unchecked((ushort)(componentY + hitbox.Top));
                        ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                        ushort[] xs = [unchecked((ushort)(left - 1)), left,
                            unchecked((ushort)(left + 1)),
                            unchecked((ushort)(right - 1)), right,
                            unchecked((ushort)(right + 1))];
                        ushort[] ys = [unchecked((ushort)(top - 1)), top,
                            unchecked((ushort)(top + 1)),
                            unchecked((ushort)(bottom - 1)), bottom,
                            unchecked((ushort)(bottom + 1))];
                        for (int xi = 0; xi < xs.Length; xi++)
                        for (int yi = 0; yi < ys.Length; yi++)
                        for (int mode = 0; mode < 2; mode++)
                        {
                            ushort radiusX = unchecked((ushort)((xi + yi) & 1));
                            ushort radiusY = unchecked((ushort)((xi + yi * 2) & 1));
                            (bool nativeHit, ushort nativeCallback) = Invoke(
                                native.Enemies, native.Slot, xs[xi], ys[yi],
                                radiusX, radiusY, mode != 0);
                            (bool compiledHit, ushort compiledCallback) = Invoke(
                                compiled.Enemies, compiled.Slot, xs[xi], ys[yi],
                                radiusX, radiusY, mode != 0);
                            AssertEqual(nativeHit, compiledHit,
                                $"walking-Pirate frame $B2:{frame.Pointer:X4} native overlap");
                            AssertEqual(nativeCallback, compiledCallback,
                                $"walking-Pirate frame $B2:{frame.Pointer:X4} native callback");
                            probes++;
                        }
                    }
                }
            }
        }
        AssertEqual(78 * 3 * 6 * 6 * 2, probes,
            "walking-Pirate exhaustive authored-rectangle boundary probes");
        AssertEqual(0, guard.BlockedReadAttempts,
            "compiled walking-Pirate collision does not read native component or hitbox bytes");
        Console.WriteLine($"Walking Pirate collision: 38 frames, 76 components, " +
            $"46 lists, 48 rectangles and {probes} native/compiled touch-shot " +
            "boundary probes pass with authored ROM bytes blocked.");

        ushort ReadWord(ushort address) => (ushort)(
            rom.ReadByte(0xb20000 | address) |
            rom.ReadByte(0xb20000 | unchecked((ushort)(address + 1))) << 8);

        void Block(ushort pointer, int length)
        {
            for (int offset = 0; offset < length; offset++)
                nativeRanges.Add(0xb20000 | unchecked((ushort)(pointer + offset)));
        }

        (RoomEnemySystem Enemies, RoomEnemySlot Slot) CreateSystem(
            ISnesAddressSpace bus, ushort pointer, ushort x, ushort y,
            bool walkingPirate)
        {
            var enemies = new RoomEnemySystem();
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = walkingPirate
                ? RoomEnemySystem.GreyWalkingSpacePirateDefinition
                : (ushort)0xffff;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.SpritemapPointer = pointer;
            slot.XPosition = x;
            slot.YPosition = y;
            return (enemies, slot);
        }

        (bool Hit, ushort Callback) Invoke(RoomEnemySystem enemies,
            RoomEnemySlot slot, ushort x, ushort y, ushort radiusX,
            ushort radiusY, bool shot)
        {
            object?[] arguments =
                [slot, x, y, radiusX, radiusY, shot, (ushort)0];
            bool hit = (bool)walker.Invoke(enemies, arguments)!;
            return (hit, (ushort)arguments[6]!);
        }
    }

    private sealed class WalkingPirateCollisionReadGuard(
        ISnesAddressSpace source, HashSet<int> blocked) : ISnesAddressSpace
    {
        internal int BlockedReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (blocked.Contains(address))
            {
                BlockedReadAttempts++;
                throw new InvalidOperationException(
                    $"Compiled walking-Pirate collision read native byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
