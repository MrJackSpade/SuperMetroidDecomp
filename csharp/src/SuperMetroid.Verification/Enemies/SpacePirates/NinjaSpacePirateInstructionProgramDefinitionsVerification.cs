using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;

/// <summary>Checks compiled ninja Space Pirate programs, frame selectors, and attack callbacks.</summary>
internal static class NinjaSpacePirateInstructionProgramDefinitionsVerification
{
    /// <summary>Loads the retail cartridge for ninja Pirate instruction comparison.</summary>
    internal static void VerifyNinjaSpacePirateInstructionProgramDefinitions()
    {
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
    }

    /// <summary>Checks all twenty guarded programs, claw/dive effects, native frame selectors, and allocation-free lookups.</summary>
    internal static void VerifyNinjaSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        for (int index = 0;
             index < NinjaSpacePirateInstructionProgramDefinitions.MechanicsWordCount;
             index++)
        {
            InstructionMechanicsWord definition =
                NinjaSpacePirateInstructionProgramDefinitions.MechanicsWord(index);
            AssertEqual(definition.Value,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb20000 | definition.Address)),
                $"ninja Pirate mechanics word $B2:{definition.Address:X4}");
        }

        // The initializer requires the installed shared gold-Pirate palette.
        // Import its exact native colors into this bounded instruction fixture.
        var colors = new PaletteRgb5[EnemyPaletteSheet.ColorCount];
        for (int color = 0; color < colors.Length; color++)
        {
            ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord((NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource + color * 2));
            colors[color] = new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        }
        // This instruction-only fixture never renders character graphics, but the
        // artwork catalog requires a matching tile sheet for its palette key.
        byte[] pixels = SnesGraphics.DecodePlanarTiles(new byte[RoomCharacterAtlasFormat.BytesPerTile], 4, 1,
            out Extent16<ImagePixels> size);
        using var tilePng = new MemoryStream();
        IndexedPng.Write(tilePng, size, pixels, SnesGraphics.DiagnosticPalette(16));
        tilePng.Position = 0;
        var artwork = EnemyTileArtworkCatalog.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>
            {
                [EnemyDefinitionId.PirateGoldWall] = RoomCharacterAtlas.Load(tilePng, RoomCharacterAtlasFormat.BytesPerTile),
            }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.PirateGoldWall] = EnemyPaletteSheet.Load(new MemoryStream(
                    EnemyPaletteSheet.Write(new EnemyPaletteSheetDocument { Version = 1, Colors = colors }))),
            } });
        var guard = NewNinjaSpacePirateInstructionReadGuard(rom);
        ushort[] programs =
        [
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackLeft,
            NinjaSpacePirateInstructionProgramDefinitions.SpinJumpLeft,
            NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftJump,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftDive,
            NinjaSpacePirateInstructionProgramDefinitions.WalkToLeftPost,
            NinjaSpacePirateInstructionProgramDefinitions.InitialFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.LandFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.KickFacingLeft,
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackRight,
            NinjaSpacePirateInstructionProgramDefinitions.SpinJumpRight,
            NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.FlinchFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightJump,
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightDive,
            NinjaSpacePirateInstructionProgramDefinitions.WalkToRightPost,
            NinjaSpacePirateInstructionProgramDefinitions.InitialFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.LandFacingRight,
            NinjaSpacePirateInstructionProgramDefinitions.KickFacingRight,
        ];

        foreach (ushort program in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 140);
        }

        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackLeft,
            expectedFirstXOffset: -32));
        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateInstructionProgramDefinitions.ClawAttackRight,
            expectedFirstXOffset: 32));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateInstructionProgramDefinitions.DivekickLeftDive,
            NinjaSpacePirateFunction.DivekickLeftDive));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateInstructionProgramDefinitions.DivekickRightDive,
            NinjaSpacePirateFunction.DivekickRightDive));

        for (int index = 0;
             index < NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort address =
                NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);
            var (enemies, slot, samus) = CreateSystem();
            slot.CurrentInstruction = unchecked((ushort)(address - 2));
            slot.InstructionTimer = NativeCountdown.Start(1);
            EnemyInstructionRunner.Process(enemies, [slot, samus, null, Position16<RoomPixels>.Zero, (ushort)0]);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xb20000 | address)),
                slot.SpritemapPointer,
                $"production execution selects ninja Pirate frame $B2:{address:X4}");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "production execution avoids compiled ninja Pirate mechanics and visual bytes");
        AssertThrows<InvalidDataException>(
            () => NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xf162),
            "ninja Pirate spritemap pointer is rejected as mechanics");
        AssertThrows<InvalidDataException>(
            () => NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(0xf536),
            "adjacent ninja Pirate callback code is rejected as mechanics");

        _ = ProbeNinjaSpacePirateInstructionMechanicsAllocation();
        long before = GC.GetAllocatedBytesForCurrentThread();
        int checksum = ProbeNinjaSpacePirateInstructionMechanicsAllocation();
        AssertTrue(checksum != 0, "ninja Pirate allocation probe consumes live data");
        AssertEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before,
            "warmed ninja Pirate mechanics lookups allocate no per-frame storage");

        Console.WriteLine(
            "Ninja Space Pirate instruction mechanics: 308 compiled words, all twenty " +
            "production programs, claw/palette/sound/function callbacks, and 140 " +
            "compiled frame selectors pass with source bytes forbidden.");

        // Checks that a left or right claw attack spawns its first projectile at the correct horizontal offset.
        void VerifyClawAttack(ushort program, int expectedFirstXOffset)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 40);
            NinjaSpacePirateEnemyState state = (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0]."));
            AssertEqual(1, (state ?? throw new InvalidOperationException("Verification requires state.")).SpawnedClawCount,
                $"ninja Pirate claw program $B2:{program:X4} first spawn count");
            RoomEnemyProjectileSlot claw = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(
                unchecked((ushort)(slot.Position.X.Whole + expectedFirstXOffset)),
                claw.Position.X.Whole,
                $"ninja Pirate claw program $B2:{program:X4} first spawn X");
        }

        // Checks the dive's function switch, shared gold palette, and attack sound.
        void VerifyDive(ushort program, NinjaSpacePirateFunction function)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            slot.CurrentInstruction = program;
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 3);
            AssertEqual(function, (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0].")).Function,
                $"ninja Pirate dive program $B2:{program:X4} function");
            AssertEqual((ushort)0x0e00, slot.PaletteIndex,
                $"ninja Pirate dive program $B2:{program:X4} palette");
            AssertEqual((ushort)0x0066, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
                $"ninja Pirate dive program $B2:{program:X4} sound");
        }

        // Initializes a ninja Pirate actor with installed shared palette and guarded instruction bytes.
        (RoomEnemySystem Enemies, RoomEnemySlot Slot, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureBus(guard);
            enemies.Presentation._cgram = new SnesCgram();
            enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 1);
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.PirateGreyNinja;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.Radius = new(8, 16);
            slot.Parameter2 = 0x0080;
            enemies.NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            return (enemies, slot, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)) });
        }

        // Advances production enemy instructions for the requested number of updates.
        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [slot, samus, null, Position16<RoomPixels>.Zero, (ushort)0];
            for (int frame = 0; frame < frames; frame++) EnemyInstructionRunner.Process(enemies, arguments);
        }
    }

    /// <summary>Repeatedly consumes a compiled control word for steady-state allocation measurement.</summary>
    private static int ProbeNinjaSpacePirateInstructionMechanicsAllocation()
    {
        int checksum = 0;
        for (int index = 0; index < 65536; index++)
        {
            checksum += NinjaSpacePirateInstructionProgramDefinitions.ReadMechanicsWord(
                NinjaSpacePirateInstructionProgramDefinitions.ActiveFacingLeft);
        }
        return checksum;
    }

    /// <summary>Forbids reads of compiled control bytes and native frame-selector words during production execution.</summary>
    private static CartridgeReadGuard NewNinjaSpacePirateInstructionReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(NinjaSpacePirateInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte,
                address => $"Production read compiled ninja Pirate mechanics ${address:X6}.")
            .ForbidWords(0xb20000, NinjaSpacePirateInstructionProgramDefinitions.PresentationWordCount,
                NinjaSpacePirateInstructionProgramDefinitions.PresentationWordAddress,
                address => $"Production read compiled ninja Pirate frame selector " +
                    $"${address:X6}.");

    /// <summary>Runs the retail-cartridge ninja Pirate instruction verification.</summary>
    [VerificationCommand("--ninja-space-pirate-instruction-mechanics")]
    internal static int NinjaSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
