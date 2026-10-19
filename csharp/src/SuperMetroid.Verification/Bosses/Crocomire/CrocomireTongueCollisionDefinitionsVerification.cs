using System.Reflection;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Crocomire tongue's nine visual frames and empty collision lists against bank A4.</summary>
internal static class CrocomireTongueCollisionDefinitionsVerification
{
    /// <summary>Checks that every typed tongue frame has installed art, with all cartridge reads denied.</summary>
    internal static void VerifyInstalledCrocomireTongueVisualSelectors(EnemyTileArtworkCatalog stock)
    {
        var denied = NewCrocomireTongueNoReadBus();
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.CrocomireTongue, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        { Bank = CrocomireTongueCollisionDefinitions.Bank });
        var seen = new HashSet<ushort>();
        foreach (CrocomireTongueFrame frame in Enum.GetValues<CrocomireTongueFrame>())
        {
            ushort selected = (ushort)frame;
            AssertTrue((stock.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    CrocomireTongueCollisionDefinitions.Bank, selected, out _),
                $"Crocomire tongue frame {frame} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(Enumerable.Range(0, CrocomireTongueCollisionDefinitions.FrameCount).Select(CrocomireTongueCollisionDefinitions.FramePointer)),
            "Crocomire tongue's nine operands select the nine extracted frames");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire tongue selectors never read ROM bytes");
        Console.WriteLine("Installed Crocomire tongue: nine native visual selectors and " +
            "editable frames, no ROM reads.");
    }

    /// <summary>Compares all nine instruction operands with the compiled frame-pointer order.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire tongue frame positions")]
    internal static void VerifyCrocomireTongueFramePositions(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(9, CrocomireTongueCollisionDefinitions.FrameCount, "tongue frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa40000 | operands[i]);
            AssertEqual(expected, CrocomireTongueCollisionDefinitions.FramePointer(i), "tongue native selected frame identity");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueCollisionDefinitions.FramePointer(-1), "negative tongue frame index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueCollisionDefinitions.FramePointer(9), "tongue frame index past end");
    }

    /// <summary>Checks each frame's single component and its empty native hitbox list.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire tongue component cases")]
    internal static void VerifyCrocomireTongueComponentCases(SuperMetroidAddressSpace rom)
    {
        ushort[] frames = [0xc65e, 0xc668, 0xc672, 0xc67c, 0xcace, 0xcad8, 0xcae2, 0xcaec, 0xcaf6];
        foreach (ushort frame in frames)
        {
            var component = CrocomireTongueCollisionDefinitions.ComponentAt(frame);
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | frame)),
                $"Crocomire tongue $A4:{frame:X4} native one-component header");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | frame + 8)), (ushort)component.HitboxPointer,
                "Crocomire tongue native hitbox-list pointer");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | (ushort)component.HitboxPointer)), "tongue native empty hitboxes");
            AssertEqual(0, CrocomireTongueCollisionDefinitions.HitboxCountAt(component.HitboxPointer), "tongue empty hitbox case");
        }
        for (int frame = 0; frame <= ushort.MaxValue; frame++)
            AssertEqual(Array.IndexOf(frames, (ushort)frame) >= 0,
                CrocomireTongueCollisionDefinitions.HasFrame((ushort)frame), "tongue exact frame domain");
        foreach (ushort frame in new ushort[] { 0, 0xc65d, 0xc65f, 0xc686, 0xcacd, 0xcacf, 0xcb00, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireTongueCollisionDefinitions.ComponentAt(frame), "tongue rejects non-frame identity");
        AssertThrows<InvalidDataException>(() => CrocomireTongueCollisionDefinitions.HitboxCountAt(ClosedNativeWords.Decode<CrocomireTongueHitboxList>(0x8000, "compiled Crocomire tongue hitbox list")), "tongue rejects unknown hitbox list");
    }
    /// <summary>Compares touch and shot collision outcomes for all tongue frames at wrapped and ordinary positions.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crocomire tongue collision definitions")]
    internal static void VerifyCrocomireTongueCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCrocomireTongueFramePositions), () => VerifyCrocomireTongueFramePositions(rom));
        Suite(nameof(VerifyCrocomireTongueComponentCases), () => VerifyCrocomireTongueComponentCases(rom));
        var denied = NewCrocomireTongueNoReadBus();
        var compiled = new RoomEnemySystem();
        compiled.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        var nativeSlot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        var boundDefinition = default(RoomEnemyDefinition) with
            { Bank = CrocomireTongueCollisionDefinitions.Bank };
        nativeSlot.Bind(nativeSlot.EnemyDefinitionPointer, boundDefinition);
        compiledSlot.Bind(compiledSlot.EnemyDefinitionPointer, boundDefinition);
        nativeSlot.Bind((EnemyDefinitionId)0xffff, nativeSlot.Definition);
        compiledSlot.Bind(EnemyDefinitionId.CrocomireTongue, compiledSlot.Definition);
        AssertEqual(9, CrocomireTongueCollisionDefinitions.FrameCount,
            "all selected Crocomire tongue frames have compiled collision");

        var seenLists = new HashSet<ushort>();
        int probes = 0;
        for (int frameIndex = 0; frameIndex < CrocomireTongueCollisionDefinitions.FrameCount; frameIndex++)
        {
            ushort frame = CrocomireTongueCollisionDefinitions.FramePointer(frameIndex);
            CrocomireTongueCollisionComponent component =
                CrocomireTongueCollisionDefinitions.ComponentAt(frame);
            seenLists.Add((ushort)component.HitboxPointer);
            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort x, ushort y) in new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006),
                         (0xfffc, 0xfffa),
                     })
            for (int shot = 0; shot <= 1; shot++)
            {
                compiledSlot.Position = compiledSlot.Position.WithWholeX(x);
                nativeSlot.Position = nativeSlot.Position.WithWholeX(x);
                compiledSlot.Position = compiledSlot.Position.WithWholeY(y);
                nativeSlot.Position = nativeSlot.Position.WithWholeY(y);
                object?[] nativeArguments =
                    [nativeSlot, new Position16<RoomPixels>(x, y), Extent16<RoomPixels>.Zero, shot != 0, (ushort)0];
                object?[] compiledArguments =
                    [compiledSlot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>((ushort)0, (ushort)0), shot != 0, (ushort)0];
                bool nativeHit = ReferenceExtendedCollision(rom, nativeArguments);
                bool compiledHit = ExtendedHitboxWalker.Find(compiled, compiledArguments);
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire tongue $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual((ushort)(nativeArguments[^1] ?? throw new InvalidOperationException("Verification requires nativeArguments[^1].")),
                    (ushort)(compiledArguments[^1] ?? throw new InvalidOperationException("Verification requires compiledArguments[^1].")),
                    "Crocomire tongue native/compiled callback parity");
                AssertTrue(!compiledHit,
                    "empty native Crocomire tongue hitbox list cannot hit Samus or shots");
                probes++;
            }
        }
        AssertTrue(seenLists.SetEquals(new ushort[] { 0xcbb3, 0xcc3b }),
            "Crocomire tongue selected frames refer to exactly two native hitbox lists");
        foreach (ushort list in seenLists)
        {
            AssertEqual((ushort)0, rom.ReadCartridgeWord((0xa40000 | list)),
                $"Crocomire tongue $A4:{list:X4} native hitbox list is empty");
            AssertEqual(0, CrocomireTongueCollisionDefinitions.HitboxCountAt(ClosedNativeWords.Decode<CrocomireTongueHitboxList>(list, "compiled Crocomire tongue hitbox list")),
                "Crocomire tongue compiled hitbox list is empty");
        }
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.ComponentAt(0x8000),
            "unknown Crocomire tongue frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.HitboxCountAt(ClosedNativeWords.Decode<CrocomireTongueHitboxList>(0x8000, "compiled Crocomire tongue hitbox list")),
            "unknown Crocomire tongue hitbox list fails loudly");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire tongue collision never reads ROM bytes");
        Console.WriteLine($"Crocomire tongue collision: nine frames, two empty lists, " +
            $"{probes} native-equivalent touch/shot probes; installed path reads no ROM.");
    }

    /// <summary>Provides an unbacked bus that rejects every cartridge read and write during installed tongue execution.</summary>
    internal static CartridgeReadGuard NewCrocomireTongueNoReadBus() =>
        CartridgeReadGuard.Unbacked()
            .ForbidAll(address => $"Installed Crocomire tongue read ROM byte ${address:X6}.")
            .RejectWrites(address => $"Installed Crocomire tongue wrote ROM byte ${address:X6}.");

    /// <summary>Runs the Crocomire tongue frame and collision checks from the command dispatcher.</summary>
    [VerificationCommand("--verify-crocomire-tongue-collision")]
    internal static int VerifyCrocomireTongueCollisionCommand()
    {
        Suite(nameof(VerifyCrocomireTongueCollisionDefinitions), () => VerifyCrocomireTongueCollisionDefinitions());
        return 0;
    }
}
