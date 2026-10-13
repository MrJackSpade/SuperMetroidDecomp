using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Retail oracle checks for Ceres and Norfair Ridley body hitboxes and both production collision walkers.</summary>
internal static class RidleyCollisionDefinitionsVerification
{
    /// <summary>Compares eleven frame component lists and seventeen hitbox lists with bank $A6, then checks touch, shot, and Ceres projectile edge overlaps without runtime ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley collision definitions")]
    internal static void VerifyRidleyCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewRidleyCollisionNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var compiled = new RoomEnemySystem();
        compiled.BindFixtureBus(denied);
        var nativeSlot = new RoomEnemySlot(0);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        compiledSlot.Definition =
            default(RoomEnemyDefinition) with { Bank = RidleyCollisionDefinitions.Bank };
        // The reference slot is read by the independent test-side cartridge
        // walker. The installed definition selects the new compiled Ridley branch.
        nativeSlot.EnemyDefinitionPointer = (EnemyDefinitionId)0xffff;
        compiledSlot.EnemyDefinitionPointer = EnemyDefinitionId.Ridley;
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
            AssertEqual((int)rom.ReadCartridgeByte(0xa60000 | frame), components.Length,
                $"Ridley frame $A6:{frame:X4} native component count");
            for (int componentIndex = 0; componentIndex < components.Length;
                 componentIndex++)
            {
                RidleyCollisionComponent component = components[componentIndex];
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + componentIndex * 8));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), component.Offset.X,
                    "Ridley native component X");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), component.Offset.Y,
                    "Ridley native component Y");
                AssertEqual(rom.ReadCartridgeWord((record + 6)), component.HitboxPointer,
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
                compiledSlot.Position = compiledSlot.Position.WithWhole(new(originX, originY));
                foreach (RidleyCollisionComponent component in components)
                foreach (RidleyCollisionHitbox hitbox in
                         RidleyCollisionDefinitions.HitboxesAt(component.HitboxPointer))
                {
                    ushort componentX = unchecked((ushort)(originX + component.Offset.X));
                    ushort componentY = unchecked((ushort)(originY + component.Offset.Y));
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
                        [compiledSlot, new Position16<RoomPixels>(xs[xi], ys[yi]), new Extent16<RoomPixels>(radiusX, radiusY), shot != 0, (ushort)0];
                        bool nativeHit = NativeCallback(frame, new Position16<RoomPixels>(originX, originY), new Position16<RoomPixels>(xs[xi], ys[yi]),
                            new Extent16<RoomPixels>(radiusX, radiusY), shot != 0, out ushort nativeCallback);
                        bool compiledHit = ExtendedHitboxWalker.Find(compiled, compiledArguments);
                        AssertEqual(nativeHit, compiledHit,
                            $"Ridley $A6:{frame:X4} overlap {xi},{yi}, shot={shot}");
                        AssertEqual(nativeCallback,
                            (ushort)(compiledArguments[^1] ?? throw new InvalidOperationException("Verification requires compiledArguments[^1].")),
                            $"Ridley $A6:{frame:X4} callback {xi},{yi}, shot={shot}");
                        if (shot == 1)
                        {
                            // The Ceres encounter's private projectile walker has its
                            // own edge test; compare that method to the same native
                            // component/list data without invoking an unrelated callback.
                            bool expectedCeres = NativeCeresOverlap(frame, new Position16<RoomPixels>(originX, originY),
                                new Position16<RoomPixels>(xs[xi], ys[yi]), new Extent16<RoomPixels>(radiusX, radiusY));
                            bool actualCeres = CeresRidleyBehavior.ExtendedSpritemapOverlapsRectangle(
                                compiledSlot, new Position16<RoomPixels>(xs[xi], ys[yi]), new Extent16<RoomPixels>(radiusX, radiusY));
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
            AssertEqual(rom.ReadCartridgeWord((0xa60000 | list)), (ushort)hitboxes.Length,
                $"Ridley $A6:{list:X4} native hitbox count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                RidleyCollisionHitbox box = hitboxes[index];
                int record = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), box.Left,
                    "Ridley native left bound");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), box.Top,
                    "Ridley native top bound");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 4))), box.Right,
                    "Ridley native right bound");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 6))), box.Bottom,
                    "Ridley native bottom bound");
                AssertEqual(rom.ReadCartridgeWord((record + 8)), box.TouchAi,
                    "Ridley native touch callback");
                AssertEqual(rom.ReadCartridgeWord((record + 10)), box.ShotAi,
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
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "both installed Ridley collision walkers never read cartridge bytes");
        Console.WriteLine($"Ridley collision: 11 body frames, {componentsChecked} " +
            $"components, 17 lists, {boxesChecked} boxes and {probes} touch/shot " +
            "boundary probes match native callbacks without ROM reads.");

        // A0:9ADF..9B78 touch and 9C43..9D20 shot comparisons consume the original
        // component/list records in order; only this reference reads cartridge data.
        // Walk native components and hitboxes in order with distinct touch and shot edge comparisons.
        bool NativeCallback(ushort frame, Position16<RoomPixels> origin, Position16<RoomPixels> target,
            Extent16<RoomPixels> radius, bool shot, out ushort callback)
        {
            callback = 0;
            ushort targetLeft = unchecked((ushort)(target.X - radius.X));
            ushort targetRight = unchecked((ushort)(target.X + radius.X));
            ushort targetTop = unchecked((ushort)(target.Y - radius.Y));
            ushort targetBottom = unchecked((ushort)(target.Y + radius.Y));
            int count = rom.ReadCartridgeByte(0xa60000 | frame);
            for (int component = 0; component < count; component++)
            {
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + component * 8));
                ushort componentX = unchecked((ushort)(origin.X + rom.ReadCartridgeWord(record)));
                ushort componentY = unchecked((ushort)(origin.Y + rom.ReadCartridgeWord((record + 2))));
                ushort list = rom.ReadCartridgeWord((record + 6));
                int boxCount = rom.ReadCartridgeWord((0xa60000 | list));
                for (int index = 0; index < boxCount; index++)
                {
                    int box = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                    ushort left = unchecked((ushort)(componentX + rom.ReadCartridgeWord(box)));
                    ushort top = unchecked((ushort)(componentY + rom.ReadCartridgeWord((box + 2))));
                    ushort right = unchecked((ushort)(componentX + rom.ReadCartridgeWord((box + 4))));
                    ushort bottom = unchecked((ushort)(componentY + rom.ReadCartridgeWord((box + 6))));
                    bool overlaps = shot
                        ? unchecked((short)(targetRight - left)) >= 0 && unchecked((short)(targetLeft - right)) < 0
                            && unchecked((short)(targetBottom - top)) >= 0 && unchecked((short)(targetTop - bottom)) < 0
                        : unchecked((short)(left - targetRight)) < 0 && unchecked((short)(right - targetLeft)) >= 0
                            && unchecked((short)(top - targetBottom)) < 0 && unchecked((short)(bottom - targetTop)) >= 0;
                    if (!overlaps) continue;
                    callback = rom.ReadCartridgeWord((box + (shot ? 10 : 8)));
                    return true;
                }
            }
            return false;
        }
        // Recreate the Ceres private projectile overlap with retail hitbox bounds and edge rules.
        bool NativeCeresOverlap(ushort frame, Position16<RoomPixels> origin, Position16<RoomPixels> target,
            Extent16<RoomPixels> radius)
        {
            int subjectLeft = unchecked((ushort)(target.X - radius.X));
            int subjectRight = unchecked((ushort)(target.X + radius.X));
            int subjectTop = unchecked((ushort)(target.Y - radius.Y));
            int subjectBottom = unchecked((ushort)(target.Y + radius.Y));
            int count = rom.ReadCartridgeByte(0xa60000 | frame);
            for (int component = 0; component < count; component++)
            {
                int record = 0xa60000 | unchecked((ushort)(frame + 2 + component * 8));
                int componentX = unchecked((ushort)(origin.X + rom.ReadCartridgeWord(record)));
                int componentY = unchecked((ushort)(origin.Y + rom.ReadCartridgeWord((record + 2))));
                ushort list = rom.ReadCartridgeWord((record + 6));
                int boxCount = rom.ReadCartridgeWord((0xa60000 | list));
                for (int index = 0; index < boxCount; index++)
                {
                    int box = 0xa60000 | unchecked((ushort)(list + 2 + index * 12));
                    int left = unchecked((ushort)(componentX + rom.ReadCartridgeWord(box)));
                    int top = unchecked((ushort)(componentY + rom.ReadCartridgeWord((box + 2))));
                    int right = unchecked((ushort)(componentX + rom.ReadCartridgeWord((box + 4))));
                    int bottom = unchecked((ushort)(componentY + rom.ReadCartridgeWord((box + 6))));
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

    /// <summary>Supplies an unbacked bus that rejects all cartridge reads and writes during compiled Ridley collision dispatch.</summary>
    private static CartridgeReadGuard NewRidleyCollisionNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Ridley collision read cartridge byte ${address:X6}.")
            .RejectWrites(address => $"Installed Ridley collision wrote cartridge byte ${address:X6}.");

    /// <summary>Runs the focused Ridley collision comparison from its command-line switch.</summary>
    [VerificationCommand("--verify-ridley-collision")]
    internal static int VerifyRidleyCollisionCommand()
    {
        Suite(nameof(VerifyRidleyCollisionDefinitions), () => VerifyRidleyCollisionDefinitions());
        return 0;
    }
}
