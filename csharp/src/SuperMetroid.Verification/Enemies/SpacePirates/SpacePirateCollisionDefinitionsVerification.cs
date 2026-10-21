using SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.World.Artwork;

/// <summary>Retail oracle and guarded runtime comparison for bank-$B2 Space Pirate extended collision frames and hitbox lists.</summary>
internal static class SpacePirateCollisionDefinitionsVerification
{
    /// <summary>Bank $B2, which holds the Space Pirates' code and frames.</summary>
    private const byte SpacePirateFrameBank = 0xb2;

    /// <summary>Checks all compiled Pirate frame and list bytes against the cartridge, then compares collision callbacks and hits through the production walker.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Space Pirate collision definitions")]
    internal static void VerifySpacePirateCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var walkingFramePointers = EnemyExtendedFrameDefinitions.Frames
            .Where(frame => frame.Name.StartsWith("walking_pirate_",
                StringComparison.Ordinal))
            .Select(frame => frame.Pointer).ToHashSet();
        var ninjaFramePointers = new HashSet<ushort>();
        AssertEqual(132, SpacePirateCollisionDefinitions.FrameCount,
            "compiled Space Pirate extended-frame count");
        AssertEqual(147, SpacePirateCollisionDefinitions.ListCount,
            "compiled Space Pirate hitbox-list count");
        CollisionRecordRuns<SpacePirateCollisionHitbox> nativeLists = NativeHitboxLists();
        int componentCount = 0;
        var nativeRanges = new HashSet<int>();
        for (int index = 0; index < SpacePirateCollisionDefinitions.FrameCount;
             index++)
        {
            SpacePirateCollisionFrame frame =
                SpacePirateCollisionDefinitions.Frame(index);
            int native = 0xb20000 | frame.Pointer;
            AssertEqual(rom.ReadCartridgeByte(native), frame.Components.Length,
                $"Space Pirate frame $B2:{frame.Pointer:X4} component count");
            Block(frame.Pointer, 2 + frame.Components.Length * 8);
            for (int componentIndex = 0;
                 componentIndex < frame.Components.Length; componentIndex++)
            {
                SpacePirateCollisionComponent component =
                    frame.Components[componentIndex];
                ushort record = unchecked((ushort)(frame.Pointer + 2 +
                    componentIndex * 8));
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | record)), component.Offset.X,
                    "Space Pirate native collision-component X");
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(record + 2))))),
                    component.Offset.Y, "Space Pirate native collision-component Y");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(record + 6)))),
                    NativeListPointer(nativeLists, component.Hitboxes),
                    "Space Pirate native component hitbox identity");
                componentCount++;
            }
        }
        AssertEqual(229, componentCount,
            "Space Pirate compiled collision-component count");
        AssertTrue(EnemyExtendedFrameDefinitions.Frames.Any(
                frame => frame.Bank != EnemyExtendedFrameDefinitions.Bank),
            "shared extended artwork catalog also contains non-Pirate boss frames");
        foreach (EnemySpritemapDefinition frame in EnemyExtendedFrameDefinitions.Frames)
        {
            // The artwork manifest now contains Ridley, Draygon, and Spore Spawn
            // frames as well. Their physical hitboxes belong to their own banks,
            // not to this bank-$B2 Space Pirate collision catalog.
            if (frame.Bank != EnemyExtendedFrameDefinitions.Bank)
                continue;
            AssertTrue(SpacePirateCollisionDefinitions.ComponentsAt(frame.Pointer).Length > 0,
                $"installed Pirate frame {frame.Name} has fixed collision");
        }
        foreach (ushort pointer in WallSpacePirateAnimationsConstants.Spritemaps(Enum.GetValues<WallSpacePirateProgram>()))
            AssertTrue(SpacePirateCollisionDefinitions.ComponentsAt(pointer).Length > 0,
                $"wall-Pirate shown frame $B2:{pointer:X4} has fixed collision");
        foreach (ushort pointer in NinjaSpacePirateAnimationsConstants.Spritemaps(Enum.GetValues<NinjaSpacePirateProgram>()))
        {
            ninjaFramePointers.Add(pointer);
            AssertTrue(SpacePirateCollisionDefinitions.ComponentsAt(pointer).Length > 0,
                $"ninja-Pirate shown frame $B2:{pointer:X4} has fixed collision");
        }
        AssertEqual(76, ninjaFramePointers.Count,
            "distinct ninja-Pirate collision-frame identities");
        AssertTrue(SpacePirateCollisionDefinitions.ComponentsAt(
                EnemyB2ExtendedSpritemaps.EmptyExtendedSpritemap).Length > 0,
            "Space Pirate initializer's common empty frame has fixed collision");
        int rectangleCount = 0;
        for (int index = 0; index < SpacePirateCollisionDefinitions.ListCount;
             index++)
        {
            SpacePirateCollisionList list = new(nativeLists.PointerAt(index), nativeLists.RecordAt(index));
            AssertEqual(rom.ReadCartridgeWordFixedBank(0xb20000 | list.Pointer), list.Rectangles.Length,
                $"Space Pirate hitbox $B2:{list.Pointer:X4} rectangle count");
            Block(list.Pointer, 2 + list.Rectangles.Length * 12);
            for (int rectangle = 0; rectangle < list.Rectangles.Length; rectangle++)
            {
                SpacePirateCollisionHitbox expected = list.Rectangles[rectangle];
                ushort address = unchecked((ushort)(list.Pointer + 2 +
                    rectangle * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | address)), expected.Left,
                    "Space Pirate native hitbox left");
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(address + 2))))),
                    expected.Top, "Space Pirate native hitbox top");
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(address + 4))))),
                    expected.Right, "Space Pirate native hitbox right");
                AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(address + 6))))),
                    expected.Bottom, "Space Pirate native hitbox bottom");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(address + 8)))),
                    SpacePirateHitboxCallbackCodec.ToCallbackWord(expected.Touch), "Space Pirate native touch callback");
                AssertEqual(rom.ReadCartridgeWordFixedBank(0xb20000 | (unchecked((ushort)(address + 10)))),
                    SpacePirateHitboxCallbackCodec.ToCallbackWord(expected.Shot), "Space Pirate native shot callback");
                rectangleCount++;
            }
        }
        AssertEqual(155, rectangleCount,
            "Space Pirate compiled collision-rectangle count");
        AssertThrows<InvalidDataException>(
            () => SpacePirateCollisionDefinitions.ComponentsAt(0x8000),
            "unknown Space Pirate frame rejects instead of reading ROM");
        ushort warmedFrame = SpacePirateCollisionDefinitions.Frame(0).Pointer;
        _ = SpacePirateCollisionDefinitions.ComponentsAt(warmedFrame).Length;
        _ = MeasureLookupAllocation(warmedFrame);
        (long allocated, int lookupChecksum) = MeasureLookupAllocation(warmedFrame);
        AssertTrue(lookupChecksum > 0, "Space Pirate lookup probe consumes data");
        AssertEqual(0L, allocated,
            "warmed Space Pirate collision lookups allocate no frame storage");

        // Warm and measure the same isolated loop; assertions and the large
        // surrounding reference fixture are outside the allocation interval.
        [System.Runtime.CompilerServices.MethodImpl(
            System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static MeasureLookupAllocationResult MeasureLookupAllocation(ushort frame)
        {
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 0;
            for (int index = 0; index < 65536; index++)
            {
                ReadOnlySpan<SpacePirateCollisionComponent> components = SpacePirateCollisionDefinitions.ComponentsAt(frame);
                checksum += components.Length;
                checksum += components[0].Hitboxes.Count;
            }
            return new MeasureLookupAllocationResult(GC.GetAllocatedBytesForCurrentThread() - before, checksum);
        }
        var guard = NewSpacePirateCollisionReadGuard(rom, nativeRanges);
        int probes = 0;
        foreach (SpacePirateCollisionFrame frame in
                 Enumerable.Range(0, SpacePirateCollisionDefinitions.FrameCount)
                     .Select(SpacePirateCollisionDefinitions.Frame))
        {
            foreach ((ushort originX, ushort originY) in
                     new VerifySpacePirateCollisionDefinitionsCase[]
                     {
                         new VerifySpacePirateCollisionDefinitionsCase(0x0100, 0x0100), new VerifySpacePirateCollisionDefinitionsCase(0x0004, 0x0006), new VerifySpacePirateCollisionDefinitionsCase(0xfffc, 0xfffa),
                     })
            {
                var native = CreateSystem(rom, frame.Pointer, originX,
                    originY, compiledPirate: false);
                var compiled = CreateSystem(guard, frame.Pointer, originX,
                    originY, compiledPirate: true);
                foreach (SpacePirateCollisionComponent component in
                         frame.Components)
                {
                    foreach (SpacePirateCollisionHitbox hitbox in component.Hitboxes)
                    {
                        ushort componentX = unchecked((ushort)(originX + component.Offset.X));
                        ushort componentY = unchecked((ushort)(originY + component.Offset.Y));
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
                        foreach (bool mode in (bool[])[false, true])
                        {
                            ushort radiusX = unchecked((ushort)((xi + yi) & 1));
                            ushort radiusY = unchecked((ushort)((xi + yi * 2) & 1));
                            (bool nativeHit, ushort nativeCallback) = Invoke(
                                native.Enemies, native.Slot, xs[xi], ys[yi],
                                radiusX, radiusY, mode);
                            (bool compiledHit, ushort compiledCallback) = Invoke(
                                compiled.Enemies, compiled.Slot, xs[xi], ys[yi],
                                radiusX, radiusY, mode);
                            AssertEqual(nativeHit, compiledHit,
                                $"Space Pirate frame $B2:{frame.Pointer:X4} native overlap");
                            AssertEqual(nativeCallback, compiledCallback,
                                $"Space Pirate frame $B2:{frame.Pointer:X4} native callback");
                            probes++;
                        }
                    }
                }
            }
        }
        AssertEqual(237 * 3 * 6 * 6 * 2, probes,
            "Space Pirate exhaustive authored-rectangle boundary probes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "compiled Space Pirate collision does not read native component or hitbox bytes");
        Console.WriteLine($"Space Pirate collision: 132 frames, 229 components, " +
            $"147 lists, 155 rectangles and {probes} native/compiled touch-shot " +
            "boundary probes pass with authored ROM bytes blocked.");

        void Block(ushort pointer, int length)
        {
            for (int offset = 0; offset < length; offset++)
                nativeRanges.Add(0xb20000 | unchecked((ushort)(pointer + offset)));
        }

        DachoraInstructionProgramDefinitionsNewDachoraResult CreateSystem(
            ISnesAddressSpace bus, ushort pointer, ushort x, ushort y,
            bool compiledPirate)
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            bool wallFrame = pointer != EnemyB2ExtendedSpritemaps.EmptyExtendedSpritemap &&
                !ninjaFramePointers.Contains(pointer) &&
                !walkingFramePointers.Contains(pointer);
            slot.Bind(compiledPirate switch
            {
                false => (EnemyDefinitionId)0xffff,
                true when ninjaFramePointers.Contains(pointer) =>
                    EnemyDefinitionId.PirateGreyNinja,
                true when wallFrame => EnemyDefinitionId.PirateGreyWall,
                _ => EnemyDefinitionId.PirateGreyWalking,
            }, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Frame = RoomEnemySlotFrameAccess.FrameAt(SpacePirateFrameBank, pointer);
            slot.Position = slot.Position.WithWhole(new(x, y));
            return new DachoraInstructionProgramDefinitionsNewDachoraResult(enemies, slot);
        }

        HitboxResult Invoke(RoomEnemySystem enemies,
            RoomEnemySlot slot, ushort x, ushort y, ushort radiusX,
            ushort radiusY, bool shot)
        {
            object?[] arguments =
                [slot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(radiusX, radiusY), shot, SnesButton.None];
            bool hit = slot.DefinitionId == (EnemyDefinitionId)0xffff
                ? ReferenceExtendedCollision(rom, SpacePirateFrameBank, arguments)
                : ExtendedHitboxWalker.Find(enemies, arguments);
            return new HitboxResult(hit, (ushort)(arguments[4] ?? throw new InvalidOperationException("Verification requires arguments[4].")));
        }
    }

    /// <summary>Lays the catalog's hitbox lists over the oracle's native runs, so each list's bank-$B2 address derives from its run.</summary>
    private static CollisionRecordRuns<SpacePirateCollisionHitbox> NativeHitboxLists()
    {
        var runs = new List<CollisionRecordRun<SpacePirateCollisionHitbox>>();
        int first = 0;
        foreach ((ushort start, int count) in SpacePirateHitboxListNativeAddresses.Runs)
        {
            runs.Add(new(start, SpacePirateCollisionDefinitions.HitboxLists[first..(first + count)]));
            first += count;
        }
        AssertEqual(SpacePirateCollisionDefinitions.HitboxLists.Length, first,
            "native Space Pirate hitbox-list runs cover every catalog list");
        return CollisionRecordRuns<SpacePirateCollisionHitbox>.HitboxLists([.. runs]);
    }

    /// <summary>The native bank-$B2 address of the catalog hitbox list a collision component references.</summary>
    private static ushort NativeListPointer(
        CollisionRecordRuns<SpacePirateCollisionHitbox> nativeLists,
        IReadOnlyList<SpacePirateCollisionHitbox> hitboxes)
    {
        int index = Array.FindIndex(SpacePirateCollisionDefinitions.HitboxLists, list => ReferenceEquals(list, hitboxes));
        return index >= 0
            ? nativeLists.PointerAt(index)
            : throw new InvalidDataException("A Space Pirate collision component references a list outside the catalog.");
    }

    /// <summary>Rejects runtime reads of the exact native frame and list byte ranges already compiled into the Pirate catalog.</summary>
    private static CartridgeReadGuard NewSpacePirateCollisionReadGuard(ISnesAddressSpace source, HashSet<int> blocked) =>
        new CartridgeReadGuard(source).Forbid(blocked.Contains,
            address => $"Compiled Space Pirate collision read native byte ${address:X6}.");

    /// <summary>Runs the Space Pirate collision definition comparison from its verification command.</summary>
    [VerificationCommand("--verify-space-pirate-collision")]
    internal static int VerifySpacePirateCollisionCommand()
    {
        Suite(nameof(VerifySpacePirateCollisionDefinitions), () => VerifySpacePirateCollisionDefinitions());
        return 0;
    }
}
