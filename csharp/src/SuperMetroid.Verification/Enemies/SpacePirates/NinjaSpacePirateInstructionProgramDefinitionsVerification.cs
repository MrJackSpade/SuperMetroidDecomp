using SuperMetroid.Core.Fx;
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

    /// <summary>Checks the typed programs against the cartridge and exercises all twenty programs and the claw and dive effects.</summary>
    internal static void VerifyNinjaSpacePirateInstructionProgramDefinitions(
        SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Suite(nameof(NinjaSpacePirateAnimationsVerification.VerifyNinjaSpacePirateAnimations), () => NinjaSpacePirateAnimationsVerification.VerifyNinjaSpacePirateAnimations(rom));

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
        NinjaSpacePirateProgram[] programs =
        [
            NinjaSpacePirateProgram.ClawAttackLeft,
            NinjaSpacePirateProgram.SpinJumpLeft,
            NinjaSpacePirateProgram.ActiveFacingLeft,
            NinjaSpacePirateProgram.FlinchFacingLeft,
            NinjaSpacePirateProgram.DivekickLeftJump,
            NinjaSpacePirateProgram.DivekickLeftDive,
            NinjaSpacePirateProgram.WalkToLeftPost,
            NinjaSpacePirateProgram.InitialFacingLeft,
            NinjaSpacePirateProgram.LandFacingLeft,
            NinjaSpacePirateProgram.KickFacingLeft,
            NinjaSpacePirateProgram.ClawAttackRight,
            NinjaSpacePirateProgram.SpinJumpRight,
            NinjaSpacePirateProgram.ActiveFacingRight,
            NinjaSpacePirateProgram.FlinchFacingRight,
            NinjaSpacePirateProgram.DivekickRightJump,
            NinjaSpacePirateProgram.DivekickRightDive,
            NinjaSpacePirateProgram.WalkToRightPost,
            NinjaSpacePirateProgram.InitialFacingRight,
            NinjaSpacePirateProgram.LandFacingRight,
            NinjaSpacePirateProgram.KickFacingRight,
        ];

        foreach (NinjaSpacePirateProgram program in programs)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Program =
                AnimationProgramCursor<NinjaSpacePirateProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 140);
        }

        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateProgram.ClawAttackLeft,
            expectedFirstXOffset: -32));
        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateProgram.ClawAttackRight,
            expectedFirstXOffset: 32));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateProgram.DivekickLeftDive,
            NinjaSpacePirateFunction.DivekickLeftDive));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateProgram.DivekickRightDive,
            NinjaSpacePirateFunction.DivekickRightDive));

        Console.WriteLine("Ninja space pirate: programs and their cues pass through real behavior.");

        // Checks that a left or right claw attack spawns its first projectile at the correct horizontal offset.
        void VerifyClawAttack(NinjaSpacePirateProgram program, int expectedFirstXOffset)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Program =
                AnimationProgramCursor<NinjaSpacePirateProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 40);
            NinjaSpacePirateEnemyState state = (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0]."));
            AssertEqual(1, (state ?? throw new InvalidOperationException("Verification requires state.")).SpawnedClawCount,
                $"ninja Pirate claw program {program} first spawn count");
            RoomEnemyProjectileSlot claw = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(
                unchecked((ushort)(slot.Position.X.Whole + expectedFirstXOffset)),
                claw.Position.X.Whole,
                $"ninja Pirate claw program {program} first spawn X");
        }

        // Checks the dive's function switch, shared gold palette, and attack sound.
        void VerifyDive(NinjaSpacePirateProgram program, NinjaSpacePirateFunction function)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Program =
                AnimationProgramCursor<NinjaSpacePirateProgram>.Start(program);
            slot.InstructionTimer = NativeCountdown.Start(1);
            RunFrames(enemies, slot, samus, 3);
            AssertEqual(function, (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0].")).Function,
                $"ninja Pirate dive program {program} function");
            AssertEqual((ushort)0x0e00, slot.PaletteIndex,
                $"ninja Pirate dive program {program} palette");
            AssertEqual((ushort)0x0066, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
                $"ninja Pirate dive program {program} sound");
        }

        // Initializes a ninja Pirate actor with the installed shared palette.
        (RoomEnemySystem Enemies, RoomEnemySlot Slot, SamusState Samus) CreateSystem()
        {
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            enemies.BindFixtureBus(rom);
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

    /// <summary>Runs the retail-cartridge ninja Pirate instruction verification.</summary>
    [VerificationCommand("--ninja-space-pirate-instruction-mechanics")]
    internal static int NinjaSpacePirateInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyNinjaSpacePirateInstructionProgramDefinitions), () => VerifyNinjaSpacePirateInstructionProgramDefinitions());
        return 0;
    }
}
