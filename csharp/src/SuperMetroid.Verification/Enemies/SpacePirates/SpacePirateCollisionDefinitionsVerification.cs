using SuperMetroid.Verification.Bosses;
using SuperMetroid.Verification.Enemies.Animals;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Retail oracle and guarded runtime comparison for bank-$B2 Space Pirate extended collision frames and hitbox lists.</summary>
internal static class SpacePirateCollisionDefinitionsVerification
{
    /// <summary>Bank $B2, which holds the Space Pirates' code and frames.</summary>
    private const byte SpacePirateFrameBank = 0xb2;

    /// <summary>
    /// Checks every compiled Pirate frame's hitboxes against the cartridge's frame, whose components place shared hitbox
    /// lists, then compares collision callbacks and hits through the production walker.
    /// </summary>
    internal static void VerifySpacePirateCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var walkingFramePointers = EnemyExtendedFrameDefinitions.Frames
            .Where(frame => frame.Name.StartsWith("walking_pirate_",
                StringComparison.Ordinal))
            .Select(frame => frame.Pointer).ToHashSet();
        var ninjaFramePointers = new HashSet<ushort>();
        AssertEqual(129, SpacePirateCollisionDefinitions.FrameCount,
            "compiled Space Pirate extended-frame count");
        int componentCount = 0;
        int hitboxCount = 0;
        var nativeLists = new HashSet<ushort>();
        int rectangleCount = 0;
        var nativeRanges = new HashSet<int>();
        for (int index = 0; index < SpacePirateCollisionDefinitions.FrameCount;
             index++)
        {
            SpacePirateCollisionFrame frame =
                SpacePirateCollisionDefinitions.Frame(index);
            // The cartridge's frame places hitbox lists it shares between frames: each component is an offset from the
            // Pirate's origin and a list, so a frame's hitboxes are its components' lists, each moved by its offset, in order.
            int components = rom.ReadCartridgeByte(0xb20000 | frame.Pointer);
            Block(frame.Pointer, 2 + components * 8);
            int hitbox = 0;
            for (int componentIndex = 0; componentIndex < components; componentIndex++)
            {
                ushort record = unchecked((ushort)(frame.Pointer + 2 + componentIndex * 8));
                short offsetX = unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | record));
                short offsetY = unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(record + 2))));
                ushort list = rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(record + 6)));
                int rectangles = rom.ReadCartridgeWordFixedBank(0xb20000 | list);
                Block(list, 2 + rectangles * 12);
                if (nativeLists.Add(list))
                    rectangleCount += rectangles;
                for (int rectangle = 0; rectangle < rectangles; rectangle++, hitbox++)
                {
                    ushort address = unchecked((ushort)(list + 2 + rectangle * 12));
                    AssertTrue(hitbox < frame.Hitboxes.Length,
                        $"Space Pirate frame $B2:{frame.Pointer:X4} has its native hitbox {hitbox}");
                    EnemyCollisionHitbox expected = frame.Hitboxes[hitbox];
                    AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | address)) + offsetX, expected.Left,
                        $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} left");
                    AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(address + 2)))) + offsetY,
                        expected.Top, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} top");
                    AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(address + 4)))) + offsetX,
                        expected.Right, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} right");
                    AssertEqual(unchecked((short)rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(address + 6)))) + offsetY,
                        expected.Bottom, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} bottom");
                    AssertEqual(EnemyReactionAccess.Of(0xb2, rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(address + 8))), shot: false),
                        (EnemyReaction?)expected.Touch, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} touch callback");
                    AssertEqual(EnemyReactionAccess.Of(0xb2, rom.ReadCartridgeWordFixedBank(0xb20000 | unchecked((ushort)(address + 10))), shot: true),
                        (EnemyReaction?)expected.Shot, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox {hitbox} shot callback");
                }
                componentCount++;
            }
            AssertEqual(hitbox, frame.Hitboxes.Length, $"Space Pirate frame $B2:{frame.Pointer:X4} hitbox count");
            hitboxCount += hitbox;
        }
        AssertEqual(223, componentCount,
            "Space Pirate native collision-component count");
        AssertEqual(147, nativeLists.Count, "Space Pirate native hitbox-list count");
        AssertEqual(155, rectangleCount, "Space Pirate native collision-rectangle count");
        AssertEqual(231, hitboxCount, "Space Pirate compiled hitbox count");
        AssertTrue(EnemyExtendedFrameDefinitions.Frames.Any(
                frame => frame.Bank != EnemyExtendedFrameDefinitions.Bank),
            "shared extended artwork catalog also contains non-Pirate boss frames");
        foreach (EnemyFrameName frame in EnemyExtendedFrameDefinitions.Frames)
        {
            // The artwork manifest now contains Ridley, Draygon, and Spore Spawn
            // frames as well. Their physical hitboxes belong to their own banks,
            // not to this bank-$B2 Space Pirate collision catalog.
            if (frame.Bank != EnemyExtendedFrameDefinitions.Bank)
                continue;
            AssertTrue(SpacePirateCollisionDefinitions.HitboxesAt(frame.Pointer).Length > 0,
                $"installed Pirate frame {frame.Name} has fixed collision");
        }
        foreach (ushort pointer in WallSpacePirateAnimationsConstants.Spritemaps(Enum.GetValues<WallSpacePirateSegment>()))
            AssertTrue(SpacePirateCollisionDefinitions.HitboxesAt(pointer).Length > 0,
                $"wall-Pirate shown frame $B2:{pointer:X4} has fixed collision");
        foreach (ushort pointer in NinjaSpacePirateAnimationsConstants.Spritemaps(Enum.GetValues<NinjaSpacePirateSegment>()))
        {
            ninjaFramePointers.Add(pointer);
            AssertTrue(SpacePirateCollisionDefinitions.HitboxesAt(pointer).Length > 0,
                $"ninja-Pirate shown frame $B2:{pointer:X4} has fixed collision");
        }
        AssertEqual(75, ninjaFramePointers.Count,
            "distinct ninja-Pirate collision-frame identities");
        AssertTrue(SpacePirateCollisionDefinitions.HitboxesAt(
                EnemyB2ExtendedSpritemaps.EmptyExtendedSpritemap).Length > 0,
            "Space Pirate initializer's common empty frame has fixed collision");
        AssertThrows<InvalidDataException>(
            () => SpacePirateCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Space Pirate frame rejects instead of reading ROM");
        ushort warmedFrame = SpacePirateCollisionDefinitions.Frame(0).Pointer;
        _ = SpacePirateCollisionDefinitions.HitboxesAt(warmedFrame).Length;
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
                ReadOnlySpan<EnemyCollisionHitbox> hitboxes = SpacePirateCollisionDefinitions.HitboxesAt(frame);
                checksum += hitboxes.Length;
                checksum += hitboxes[0].Right - hitboxes[0].Left + 1;
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
                var native = CreateSystem(frame.Pointer, originX, originY);
                var compiled = CreateSystem(frame.Pointer, originX, originY);
                foreach (EnemyCollisionHitbox hitbox in frame.Hitboxes)
                {
                    ushort left = unchecked((ushort)(originX + hitbox.Left));
                    ushort right = unchecked((ushort)(originX + hitbox.Right));
                    ushort top = unchecked((ushort)(originY + hitbox.Top));
                    ushort bottom = unchecked((ushort)(originY + hitbox.Bottom));
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
                        (bool nativeHit, EnemyReaction? nativeReaction) = Invoke(
                            native.Place, xs[xi], ys[yi],
                            radiusX, radiusY, mode, nativePath: true);
                        (bool compiledHit, EnemyReaction? compiledReaction) = Invoke(
                            compiled.Place, xs[xi], ys[yi],
                            radiusX, radiusY, mode, nativePath: false);
                        AssertEqual(nativeHit, compiledHit,
                            $"Space Pirate frame $B2:{frame.Pointer:X4} native overlap");
                        AssertEqual(nativeReaction, compiledReaction,
                            $"Space Pirate frame $B2:{frame.Pointer:X4} native reaction");
                        probes++;
                    }
                }
            }
        }
        AssertEqual(231 * 3 * 6 * 6 * 2, probes,
            "Space Pirate exhaustive authored-rectangle boundary probes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "compiled Space Pirate collision does not read native component or hitbox bytes");
        Console.WriteLine($"Space Pirate collision: 129 frames, 223 components, " +
            $"147 lists, 155 rectangles and {probes} native/compiled touch-shot " +
            "boundary probes pass with authored ROM bytes blocked.");

        void Block(ushort pointer, int length)
        {
            for (int offset = 0; offset < length; offset++)
                nativeRanges.Add(0xb20000 | unchecked((ushort)(pointer + offset)));
        }

        // Both places name the Pirate whose frame they show, whose header lies in the Pirates' bank: the reference walk
        // reads the frame's record there, the compiled one its hitboxes.
        DachoraInstructionProgramDefinitionsNewDachoraResult CreateSystem(ushort pointer, ushort x, ushort y)
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            bool wallFrame = pointer != EnemyB2ExtendedSpritemaps.EmptyExtendedSpritemap &&
                !ninjaFramePointers.Contains(pointer) &&
                !walkingFramePointers.Contains(pointer);
            place.Bind(ninjaFramePointers.Contains(pointer)
                ? EnemyDefinitionId.PirateSilverNinja
                : wallFrame ? EnemyDefinitionId.PirateGreyWall : EnemyDefinitionId.PirateGreyWalking, place.Definition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            // Space Pirates collide by their frames' hitboxes, so the empty frame is the point-hitbox record.
            place.CollidesByFrameHitboxes = true;
            place.Frame = RoomEnemyPlaceFrameAccess.FrameAt(SpacePirateFrameBank, pointer);
            place.Position = place.Position.WithWhole(new(x, y));
            return new DachoraInstructionProgramDefinitionsNewDachoraResult(enemies, place);
        }

        HitboxResult Invoke(RoomEnemyPlace place, ushort x, ushort y, ushort radiusX,
            ushort radiusY, bool shot, bool nativePath)
        {
            object?[] arguments =
                [place, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(radiusX, radiusY), shot, default(GameplayAction)];
            bool native = nativePath;
            bool hit = native
                ? ReferenceExtendedCollision(rom, SpacePirateFrameBank, arguments)
                : ExtendedHitboxWalker.Find(arguments);
            return new HitboxResult(hit, native
                ? ExtendedHitboxWalker.NativeReaction(SpacePirateFrameBank, arguments)
                : ExtendedHitboxWalker.Reaction(arguments));
        }
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
