using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedCollisionReferenceVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Crocomire;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Crocomire tongue's nine visual frames and empty collision lists against bank A4.</summary>
internal static class CrocomireTongueCollisionDefinitionsVerification
{
    /// <summary>Checks that every typed tongue frame has installed art, with all cartridge reads denied.</summary>
    internal static void VerifyInstalledCrocomireTongueVisualSelectors(ImportEnemyArtwork stock)
    {
        var denied = NewCrocomireTongueNoReadBus();
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = stock.Bosses };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.CrocomireTongue, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        var seen = new HashSet<ushort>();
        foreach (CrocomireTongueFrame frame in Enum.GetValues<CrocomireTongueFrame>())
        {
            ushort selected = CrocomireNativeWords.TongueFrames.Word(frame);
            AssertTrue((stock.Enemies.ExtendedFrames ?? throw new InvalidOperationException("Verification requires stock.ExtendedFrames.")).TryGetDisplay(
                    CrocomireTongueCollisionDefinitionsConstants.Bank, selected, out _),
                $"Crocomire tongue frame {frame} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(Enumerable.Range(0, CrocomireTongueCollisionDefinitionsConstants.FrameCount).Select(CrocomireTongueCollisionDefinitionsConstants.FramePointer)),
            "Crocomire tongue's nine operands select the nine extracted frames");
        AssertEqual(0, denied.ForbiddenReadAttempts,
            "installed Crocomire tongue selectors never read ROM bytes");
        Console.WriteLine("Installed Crocomire tongue: nine native visual selectors and " +
            "editable frames, no ROM reads.");
    }

    /// <summary>Compares all nine instruction operands with the compiled frame-pointer order.</summary>
    internal static void VerifyCrocomireTongueFramePositions(CartridgeImportAddressSpace rom)
    {
        ushort[] operands = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(9, CrocomireTongueCollisionDefinitionsConstants.FrameCount, "tongue frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa40000 | operands[i]);
            AssertEqual(expected, CrocomireTongueCollisionDefinitionsConstants.FramePointer(i), "tongue native selected frame identity");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireTongueCollisionDefinitionsConstants.FramePointer(-1), "negative tongue frame index");
        AssertThrows<ArgumentOutOfRangeException>(() => CrocomireTongueCollisionDefinitionsConstants.FramePointer(9), "tongue frame index past end");
    }

    /// <summary>
    /// Checks that each native tongue frame has one component whose hitbox list is empty, which is why the port's tongue
    /// has no hitboxes at all (#1870), and that the tongue's frames are exactly the native ones.
    /// </summary>
    internal static void VerifyCrocomireTongueComponentCases(CartridgeImportAddressSpace rom)
    {
        ushort[] frames = [0xc65e, 0xc668, 0xc672, 0xc67c, 0xcace, 0xcad8, 0xcae2, 0xcaec, 0xcaf6];
        foreach (ushort frame in frames)
        {
            AssertEqual((ushort)1, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | frame)),
                $"Crocomire tongue $A4:{frame:X4} native one-component header");
            ushort list = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | frame + 8));
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa40000 | list)), "tongue native empty hitboxes");
        }
        for (int frame = 0; frame <= ushort.MaxValue; frame++)
            AssertEqual(Array.IndexOf(frames, (ushort)frame) >= 0,
                CrocomireTongueCollisionDefinitionsAccess.HasFrame((ushort)frame), "tongue exact frame domain");
    }
    /// <summary>Compares touch and shot collision outcomes for all tongue frames at wrapped and ordinary positions.</summary>
    internal static void VerifyCrocomireTongueCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCrocomireTongueFramePositions), () => VerifyCrocomireTongueFramePositions(rom));
        Suite(nameof(VerifyCrocomireTongueComponentCases), () => VerifyCrocomireTongueComponentCases(rom));
        var denied = NewCrocomireTongueNoReadBus();
        var compiled = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        var nativeSlot = new RoomEnemySlot(0, EnemySlotFixtures.Registry);
        RoomEnemySlot compiledSlot = compiled.SlotTable.Slots[0];
        var boundDefinition = default(RoomEnemyDefinition);
        nativeSlot.Bind(nativeSlot.DefinitionId, boundDefinition);
        compiledSlot.Bind(compiledSlot.DefinitionId, boundDefinition);
        nativeSlot.Bind((EnemyDefinitionId)0xffff, nativeSlot.Definition);
        compiledSlot.Bind(EnemyDefinitionId.CrocomireTongue, compiledSlot.Definition);
        AssertEqual(9, CrocomireTongueCollisionDefinitionsConstants.FrameCount,
            "all selected Crocomire tongue frames have compiled collision");

        var seenLists = new HashSet<ushort>();
        int probes = 0;
        for (int frameIndex = 0; frameIndex < CrocomireTongueCollisionDefinitionsConstants.FrameCount; frameIndex++)
        {
            ushort frame = CrocomireTongueCollisionDefinitionsConstants.FramePointer(frameIndex);
            seenLists.Add(rom.ReadCartridgeWord(0xa40000 | frame + 8));
            nativeSlot.Frame = compiledSlot.Frame = RoomEnemySlotFrameAccess.FrameAt(CrocomireTongueCollisionDefinitionsConstants.Bank, frame);
            foreach ((ushort x, ushort y) in new VerifyCrocomireTongueCollisionDefinitionsCase[]
                     {
                         new VerifyCrocomireTongueCollisionDefinitionsCase(0x0100, 0x0100), new VerifyCrocomireTongueCollisionDefinitionsCase(0x0004, 0x0006),
                         new VerifyCrocomireTongueCollisionDefinitionsCase(0xfffc, 0xfffa),
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
                    [compiledSlot, new Position16<RoomPixels>(x, y), new Extent16<RoomPixels>(0, 0), shot != 0, (ushort)0];
                bool nativeHit = ReferenceExtendedCollision(rom, CrocomireTongueCollisionDefinitionsConstants.Bank, nativeArguments);
                bool compiledHit = ExtendedHitboxWalker.Find(compiledArguments);
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire tongue $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual(ExtendedHitboxWalker.NativeReaction(CrocomireTongueCollisionDefinitionsConstants.Bank, nativeArguments),
                    ExtendedHitboxWalker.Reaction(compiledArguments),
                    "Crocomire tongue native/compiled reaction parity");
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
        }
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
