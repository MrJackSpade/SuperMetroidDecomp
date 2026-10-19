using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.MaridiaSnails;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Oum's extracted display selectors and compiled collision records against ROM and the installed runtime.</summary>
internal static class MaridiaLargeSnailCollisionDefinitionsVerification
{
    /// <summary>Confirms every pose Oum's programs show is one of the extracted frames and has installed editable art.</summary>
    internal static void VerifyInstalledOumVisualSelectors(EnemyTileArtworkCatalog stock)
    {
        var seen = MaridiaLargeSnailAnimationsConstants.Spritemaps(Enum.GetValues<MaridiaLargeSnailProgram>()).ToHashSet();
        foreach (ushort pose in seen)
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    MaridiaLargeSnailCollisionDefinitions.Bank, pose, out _),
                $"Oum pose ${pose:X4} has installed art");
        AssertTrue(seen.SetEquals(MaridiaLargeSnailCollisionDefinitions.FramePointers.ToArray()),
            "Oum's programs show exactly the 30 extracted physical frames");
        Console.WriteLine("Installed Oum: the 30 poses its programs show are the editable extracted frames.");
    }

    /// <summary>Compares Oum frame pointers and hitboxes with ROM, then checks touch and shot overlap at rectangle boundaries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Maridia large snail collision definitions")]
    internal static void VerifyMaridiaLargeSnailCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var denied = NewOumNoReadBus();
        var compiled = new RoomEnemySystem(new EnemySession());
        compiled.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        var nativeSlot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        var boundDefinition = default(RoomEnemyDefinition) with
            { Bank = MaridiaLargeSnailCollisionDefinitions.Bank };
        nativeSlot.Bind(nativeSlot.EnemyDefinitionPointer, boundDefinition);
        compiledSlot.Bind(compiledSlot.EnemyDefinitionPointer, boundDefinition);
        nativeSlot.Bind((EnemyDefinitionId)0xffff, nativeSlot.Definition);
        compiledSlot.Bind(EnemyDefinitionId.Oum, compiledSlot.Definition);
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.FramePointers.Length,
            "all Oum extended frames have compiled collision");
        AssertEqual(30, MaridiaLargeSnailCollisionDefinitions.HitboxPointers.Count(),
            "all Oum hitbox lists have compiled collision");

        var seenLists = new HashSet<ushort>();
        int boxesChecked = 0;
        int probes = 0;
        foreach (ushort frame in MaridiaLargeSnailCollisionDefinitions.FramePointers)
        {
            AssertEqual((ushort)1, rom.ReadCartridgeWord((0xa20000 | frame)),
                $"Oum $A2:{frame:X4} native one-component header");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 2)),
                "Oum native component X is zero");
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa20000 | frame + 4)),
                "Oum native component Y is zero");
            ushort list = MaridiaLargeSnailCollisionDefinitions.HitboxListAt(frame);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | frame + 8)), list,
                "Oum native frame hitbox-list pointer");
            seenLists.Add(list);
            ReadOnlySpan<MaridiaLargeSnailCollisionHitbox> boxes =
                MaridiaLargeSnailCollisionDefinitions.HitboxesAt(list);
            AssertEqual(rom.ReadCartridgeWord((0xa20000 | list)), (ushort)boxes.Length,
                $"Oum $A2:{list:X4} native hitbox count");
            for (int index = 0; index < boxes.Length; index++)
            {
                MaridiaLargeSnailCollisionHitbox box = boxes[index];
                int record = 0xa20000 | unchecked((ushort)(list + 2 + index * 12));
                AssertEqual(unchecked((short)rom.ReadCartridgeWord(record)), box.Left, "Oum left");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 2))), box.Top, "Oum top");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 4))), box.Right, "Oum right");
                AssertEqual(unchecked((short)rom.ReadCartridgeWord((record + 6))), box.Bottom, "Oum bottom");
                AssertEqual(rom.ReadCartridgeWord((record + 8)), EnemyInteractionCallbackAccess.Word(box.TouchAi), "Oum touch callback");
                AssertEqual(rom.ReadCartridgeWord((record + 10)), EnemyInteractionCallbackAccess.Word(box.ShotAi), "Oum shot callback");
                boxesChecked++;
            }

            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort originX, ushort originY) in
                     new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006), (0xfffc, 0xfffa),
                     })
            {
                compiledSlot.Position = compiledSlot.Position.WithWholeX(originX);
                nativeSlot.Position = nativeSlot.Position.WithWholeX(originX);
                compiledSlot.Position = compiledSlot.Position.WithWholeY(originY);
                nativeSlot.Position = nativeSlot.Position.WithWholeY(originY);
                foreach (MaridiaLargeSnailCollisionHitbox box in boxes)
                {
                    ushort[] xs = BoundaryPoints(originX, box.Left, box.Right);
                    ushort[] ys = BoundaryPoints(originY, box.Top, box.Bottom);
                    foreach (ushort x in xs)
                    foreach (ushort y in ys)
                    for (int shot = 0; shot <= 1; shot++)
                    {
                        object?[] nativeArguments =
                            [nativeSlot, new Position16<RoomPixels>(x, y), Extent16<RoomPixels>.Zero, shot != 0, (ushort)0];
                        object?[] compiledArguments =
                            [compiledSlot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>((ushort)0, (ushort)0), shot != 0, (ushort)0];
                        bool nativeHit = ReferenceExtendedCollision(rom, nativeArguments);
                        bool compiledHit = ExtendedHitboxWalker.Find(compiled, compiledArguments);
                        AssertEqual(nativeHit, compiledHit,
                            $"Oum $A2:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                        AssertEqual((ushort)(nativeArguments[^1] ?? throw new InvalidOperationException("Verification requires nativeArguments[^1].")),
                            (ushort)(compiledArguments[^1] ?? throw new InvalidOperationException("Verification requires compiledArguments[^1].")),
                            $"Oum $A2:{frame:X4} callback {x:X4},{y:X4}, shot={shot}");
                        probes++;
                    }
                }
            }
        }
        AssertTrue(seenLists.SetEquals(MaridiaLargeSnailCollisionDefinitions.HitboxPointers),
            "every compiled Oum list belongs to a selected frame");
        AssertEqual(66, boxesChecked, "all Oum physical rectangles were verified");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxListAt(0x8000),
            "unknown Oum frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => MaridiaLargeSnailCollisionDefinitions.HitboxesAt(0x8000),
            "unknown Oum hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Oum touch and shot walkers never read ROM bytes");
        Console.WriteLine($"Oum collision: 30 frames, 66 rectangles, {probes} " +
            "native-equivalent touch/shot boundary probes with no ROM reads.");

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

    /// <summary>Provides an unbacked bus that rejects any Oum visual or collision ROM access.</summary>
    private static CartridgeReadGuard NewOumNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Oum frame or collision read ROM byte ${address:X6}.")
            .RejectWrites(address => $"Installed Oum frame or collision wrote ROM byte ${address:X6}.");

    /// <summary>Runs the installed Oum collision verification through the command dispatcher.</summary>
    [VerificationCommand("--verify-oum-collision")]
    internal static int VerifyOumCollisionCommand()
    {
        Suite(nameof(VerifyMaridiaLargeSnailCollisionDefinitions), () => VerifyMaridiaLargeSnailCollisionDefinitions());
        return 0;
    }
}
