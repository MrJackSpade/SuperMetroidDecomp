using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRidleyCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = new RidleyCollisionNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        MethodInfo ceresWalker = typeof(RoomEnemySystem).GetMethod(
            "ExtendedSpritemapOverlapsRectangle", BindingFlags.Static | BindingFlags.NonPublic)!;
        var compiled = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(compiled, denied);
        var nativeSlot = new RoomEnemySlot(0);
        RoomEnemySlot compiledSlot = compiled.Slots[0];
        compiledSlot.Definition =
            default(RoomEnemyDefinition) with { Bank = RidleyCollisionDefinitions.Bank };
        // The reference slot is read by the independent test-side cartridge
        // walker. The installed definition selects the new compiled Ridley branch.
        nativeSlot.EnemyDefinitionPointer = 0xffff;
        compiledSlot.EnemyDefinitionPointer = RoomEnemySystem.NorfairRidleyDefinition;
        AssertEqual(11, RidleyCollisionDefinitions.FramePointers.Length,
            "all Ceres/Norfair Ridley body frames have fixed collision");
        AssertEqual(17, RidleyCollisionDefinitions.HitboxPointers.Count(),
            "all distinct Ridley body hitbox lists are compiled");

        var seenLists = new HashSet<ushort>();
        int componentsChecked = 0;
        int boxesChecked = 0;
        int probes = 0;
        foreach (ushort frame in RidleyCollisionDefinitions.FramePointers)
        {
            AssertTrue(EnemyExtendedFrameDefinitions.Frames.ToArray().Any(
                    selected => selected.Bank == RidleyCollisionDefinitions.Bank &&
                        selected.Pointer == frame),
                $"Ridley collision frame $A6:{frame:X4} is selected artwork identity");
            var components =
                RidleyCollisionDefinitions.ComponentsAt(frame);
            AssertEqual((int)rom.ReadByte(0xa60000 | frame), components.Length,
                $"Ridley frame $A6:{frame:X4} native component count");
            for (int componentIndex = 0; componentIndex < components.Length;
                 componentIndex++)
            {
                RidleyCollisionComponent component = components[componentIndex];
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((short)ReadWord(record)), component.X,
                    "Ridley native component X");
                AssertEqual(unchecked((short)ReadWord(record + 2)), component.Y,
                    "Ridley native component Y");
                AssertEqual(ReadWord(record + 6), component.HitboxPointer,
                    "Ridley native component hitbox list");
                seenLists.Add(component.HitboxPointer);
                componentsChecked++;
            }

            compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                compiledSlot.XPosition = originX;
                compiledSlot.YPosition = originY;
                foreach (RidleyCollisionComponent component in components)
                foreach (RidleyCollisionHitbox hitbox in
                         RidleyCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort componentX = unchecked((ushort)(originX + component.X));
                    ushort componentY = unchecked((ushort)(originY + component.Y));
                    ushort left = unchecked((ushort)(componentX + hitbox.Left));
                    ushort right = unchecked((ushort)(componentX + hitbox.Right));
                    ushort top = unchecked((ushort)(componentY + hitbox.Top));
                    ushort bottom = unchecked((ushort)(componentY + hitbox.Bottom));
                    ushort[] xs =
                    [
                        unchecked((ushort)(left - 1)), left,
                        unchecked((ushort)(left + 1)),
                        unchecked((ushort)(right - 1)), right,
                        unchecked((ushort)(right + 1)),
                    ];
                    ushort[] ys =
                    [
                        unchecked((ushort)(top - 1)), top,
                        unchecked((ushort)(top + 1)),
                        unchecked((ushort)(bottom - 1)), bottom,
                        unchecked((ushort)(bottom + 1)),
                    ];
                    for (int xi = 0; xi < xs.Length; xi++)
                    for (int yi = 0; yi < ys.Length; yi++)
                    for (int shot = 0; shot <= 1; shot++)
                    {
                        ushort radiusX = unchecked((ushort)((xi + yi) & 1));
                        ushort radiusY = unchecked((ushort)((xi + yi * 2) & 1));
                        object?[] compiledArguments =
                        [compiledSlot, xs[xi], ys[yi], radiusX, radiusY,
                            shot != 0, (ushort)0];
                        bool nativeHit = NativeCallback(frame, originX, originY, xs[xi], ys[yi], radiusX, radiusY, shot != 0, out ushort nativeCallback);
                        bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
                        AssertEqual(nativeHit, compiledHit,
                            $"Ridley $A6:{frame:X4} overlap {xi},{yi}, shot={shot}");
                        AssertEqual(nativeCallback,
                            (ushort)compiledArguments[^1]!,
                            $"Ridley $A6:{frame:X4} callback {xi},{yi}, shot={shot}");
                        if (shot == 1)
                        {
                            // The Ceres encounter's private projectile walker has its
                            // own edge test; compare that method to the same native
                            // component/list data without invoking an unrelated callback.
                            bool expectedCeres = NativeCeresOverlap(frame, originX,
                                originY, xs[xi], ys[yi], radiusX, radiusY);
                            object?[] ceresArguments =
                                [compiledSlot, xs[xi], ys[yi], radiusX, radiusY];
                            bool actualCeres = (bool)ceresWalker.Invoke(
                                null, ceresArguments)!;
                            AssertEqual(expectedCeres, actualCeres,
                                $"Ceres private Ridley projectile overlap $A6:{frame:X4}");
                        }
                        probes++;
                    }
                }
            }
        }
        AssertEqual(RidleyCollisionDefinitions.HitboxPointers.Count(), seenLists.Count,
            "every compiled Ridley hitbox list belongs to a selected body frame");
        foreach (ushort list in RidleyCollisionDefinitions.HitboxPointers)
        {
            ReadOnlySpan<RidleyCollisionHitbox> hitboxes =
                RidleyCollisionDefinitions.HitboxesAt(list);
            AssertEqual(ReadWord(0xa60000 | list), (ushort)hitboxes.Length,
                $"Ridley $A6:{list:X4} native hitbox count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                RidleyCollisionHitbox box = hitboxes[index];
                int record = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)ReadWord(record)), box.Left,
                    "Ridley native left bound");
                AssertEqual(unchecked((short)ReadWord(record + 2)), box.Top,
                    "Ridley native top bound");
                AssertEqual(unchecked((short)ReadWord(record + 4)), box.Right,
                    "Ridley native right bound");
                AssertEqual(unchecked((short)ReadWord(record + 6)), box.Bottom,
                    "Ridley native bottom bound");
                AssertEqual(ReadWord(record + 8), box.TouchAi,
                    "Ridley native touch callback");
                AssertEqual(ReadWord(record + 10), box.ShotAi,
                    "Ridley native shot callback");
                boxesChecked++;
            }
        }
        AssertThrows<InvalidDataException>(
            () => RidleyCollisionDefinitions.ComponentsAt(0x8000),
            "unknown Ridley body frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => RidleyCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Ridley hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "both installed Ridley collision walkers never read cartridge bytes");
        Console.WriteLine($"Ridley collision: 11 body frames, {componentsChecked} " +
            $"components, 17 lists, {boxesChecked} boxes and {probes} touch/shot " +
            "boundary probes match native callbacks without ROM reads.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

        // A0:9ADF..9B78 touch and 9C43..9D20 shot comparisons consume the original
        // component/list records in order; only this reference reads cartridge data.
        bool NativeCallback(ushort frame, ushort originX, ushort originY,
            ushort x, ushort y, ushort radiusX, ushort radiusY, bool shot, out ushort callback)
        {
            callback = 0;
            ushort targetLeft = unchecked((ushort)(x - radiusX));
            ushort targetRight = unchecked((ushort)(x + radiusX));
            ushort targetTop = unchecked((ushort)(y - radiusY));
            ushort targetBottom = unchecked((ushort)(y + radiusY));
            int count = rom.ReadByte(0xa60000 | frame);
            for (int component = 0; component < count; component++)
            {
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + component * 8));
                ushort componentX = unchecked((ushort)(originX + ReadWord(record)));
                ushort componentY = unchecked((ushort)(originY + ReadWord(record + 2)));
                ushort list = ReadWord(record + 6);
                int boxCount = ReadWord(0xa60000 | list);
                for (int index = 0; index < boxCount; index++)
                {
                    int box = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                    ushort left = unchecked((ushort)(componentX + ReadWord(box)));
                    ushort top = unchecked((ushort)(componentY + ReadWord(box + 2)));
                    ushort right = unchecked((ushort)(componentX + ReadWord(box + 4)));
                    ushort bottom = unchecked((ushort)(componentY + ReadWord(box + 6)));
                    bool overlaps = shot
                        ? unchecked((short)(targetRight - left)) >= 0 && unchecked((short)(targetLeft - right)) < 0
                            && unchecked((short)(targetBottom - top)) >= 0 && unchecked((short)(targetTop - bottom)) < 0
                        : unchecked((short)(left - targetRight)) < 0 && unchecked((short)(right - targetLeft)) >= 0
                            && unchecked((short)(top - targetBottom)) < 0 && unchecked((short)(bottom - targetTop)) >= 0;
                    if (!overlaps) continue;
                    callback = ReadWord(box + (shot ? 10 : 8));
                    return true;
                }
            }
            return false;
        }
        bool NativeCeresOverlap(ushort frame, ushort originX, ushort originY,
            ushort x, ushort y, ushort radiusX, ushort radiusY)
        {
            int subjectLeft = unchecked((ushort)(x - radiusX));
            int subjectRight = unchecked((ushort)(x + radiusX));
            int subjectTop = unchecked((ushort)(y - radiusY));
            int subjectBottom = unchecked((ushort)(y + radiusY));
            int count = rom.ReadByte(0xa60000 | frame);
            for (int component = 0; component < count; component++)
            {
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + component * 8));
                int componentX = unchecked((ushort)(originX + ReadWord(record)));
                int componentY = unchecked((ushort)(originY + ReadWord(record + 2)));
                ushort list = ReadWord(record + 6);
                int boxCount = ReadWord(0xa60000 | list);
                for (int index = 0; index < boxCount; index++)
                {
                    int box = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                    int left = unchecked((ushort)(componentX + ReadWord(box)));
                    int top = unchecked((ushort)(componentY + ReadWord(box + 2)));
                    int right = unchecked((ushort)(componentX + ReadWord(box + 4)));
                    int bottom = unchecked((ushort)(componentY + ReadWord(box + 6)));
                    if (unchecked((short)(left - subjectRight)) <= 0 &&
                        unchecked((short)(right - subjectLeft)) > 0 &&
                        unchecked((short)(top - subjectBottom)) <= 0 &&
                        unchecked((short)(bottom - subjectTop)) > 0)
                        return true;
                }
            }
            return false;
        }
    }

    private sealed class RidleyCollisionNoReadBus : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Ridley collision read cartridge byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Ridley collision wrote cartridge byte ${address:X6}.");
    }
}
