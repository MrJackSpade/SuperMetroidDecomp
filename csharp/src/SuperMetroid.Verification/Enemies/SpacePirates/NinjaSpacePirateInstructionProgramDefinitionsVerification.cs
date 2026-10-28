using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
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
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;

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
                [EnemyDefinitionId.PirateGoldWall] = RoomCharacterAtlasPng.Read(tilePng, RoomCharacterAtlasFormat.BytesPerTile),
            }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.PirateGoldWall] = EnemyPaletteSheetFile.Read(new MemoryStream(
                    EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument { Version = 1, Colors = colors }))),
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
            slot.InstructionTimer = WrappingCountdown.Start(1);
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
        Suite(nameof(VerifyDiveLanding), () => VerifyDiveLanding(movingRight: false));
        Suite(nameof(VerifyDiveLanding), () => VerifyDiveLanding(movingRight: true));
        Suite(nameof(VerifyDivekickRiseSpeed), () => VerifyDivekickRiseSpeed(movingRight: false));
        Suite(nameof(VerifyDivekickRiseSpeed), () => VerifyDivekickRiseSpeed(movingRight: true));
        Suite(nameof(VerifyDiveSpeedWrap), () => VerifyDiveSpeedWrap(movingRight: false));
        Suite(nameof(VerifyDiveSpeedWrap), () => VerifyDiveSpeedWrap(movingRight: true));

        Console.WriteLine("Ninja space pirate: programs and their cues pass through real behavior.");

        // Checks that the divekick landing ($B2:FA0E/$FAE2) restores the spawn row's whole Y but keeps the
        // dive's Y fraction. The contributed lowc movie lands one with a fraction of $0780 (update 111,288).
        void VerifyDiveLanding(bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState _) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            const ushort diveFraction = 1920;
            slot.Position = slot.Position.WithFractionY(diveFraction);
            // One speed step left: the dive ends after this movement, wherever it is.
            state.Speed = new SignedEightEight(64);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32),
                new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);
            enemies.NinjaSpacePirateBehavior.StepNinjaPirateDive(slot, state, level, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left);
            AssertEqual(movingRight ? NinjaSpacePirateFunction.DivekickRightWalkToPost : NinjaSpacePirateFunction.DivekickLeftWalkToPost,
                state.Function, $"ninja Pirate dive {(movingRight ? "right" : "left")} lands");
            AssertEqual(state.SpawnPosition.Y, slot.Position.Y.Whole, "the landing restores the spawn row");
            // This last descent adds the speed's low byte to the fraction ($B2:F9D5) before the landing.
            AssertEqual((ushort)(diveFraction + 64), slot.Position.Y.Fraction, "the landing keeps the dive's Y fraction");
        }

        // Drives the divekick rise through production main AI. Each update raises the Pirate by the speed's whole
        // pixels and lowers the 8.8 speed by 64; the rise ends on the update the sixteen-bit speed word turns
        // negative, which switches to the dive and restores the initial speed. The expectation is the native word
        // arithmetic, computed here independently of the production speed type.
        void VerifyDivekickRiseSpeed(bool movingRight)
        {
            const ushort initialDiveSpeed = 1536;
            const int speedStep = 64;
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            NinjaSpacePirateFunction rising = movingRight ? NinjaSpacePirateFunction.DivekickRightJump : NinjaSpacePirateFunction.DivekickLeftJump;
            NinjaSpacePirateFunction diving = movingRight ? NinjaSpacePirateFunction.DivekickRightDive : NinjaSpacePirateFunction.DivekickLeftDive;
            state.Function = rising;
            state.Speed = new SignedEightEight(initialDiveSpeed);
            var frame = new ClockedEnemyAiFrame(
                new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
                new FrameCounters(0));
            ushort nativeSpeed = initialDiveSpeed;
            int risingUpdates = 0;
            for (int update = 1; update <= 40; update++)
            {
                ushort expectedY = unchecked((ushort)(slot.Position.Y.Whole - nativeSpeed / 256));
                nativeSpeed = unchecked((ushort)(nativeSpeed - speedStep));
                bool riseEnds = unchecked((short)nativeSpeed) < 0;
                enemies.NinjaSpacePirateBehavior.RunNinjaSpacePirateMainAi(slot, frame);
                AssertEqual(expectedY, slot.Position.Y.Whole, $"ninja divekick rise update {update} climbs by the speed's whole pixels");
                if (!riseEnds)
                {
                    AssertEqual(new SignedEightEight(nativeSpeed), state.Speed, $"ninja divekick rise update {update} lowers the speed by 64");
                    AssertEqual(rising, state.Function, $"ninja divekick rise update {update} keeps rising while the speed is not negative");
                    risingUpdates++;
                    continue;
                }

                AssertEqual(ushort.MaxValue + 1 - speedStep, (int)nativeSpeed, "the native speed word wraps below zero on the last rising update");
                AssertEqual(diving, state.Function, $"ninja divekick rise ends on update {update}, when the speed turns negative");
                AssertEqual(new SignedEightEight(initialDiveSpeed), state.Speed, "the dive starts again from the initial speed");
                break;
            }
            AssertEqual(initialDiveSpeed / speedStep, risingUpdates, "the rise lasts until the speed passes zero");
        }

        // Drives the production dive with a zero speed: the update lowers it by 64, so the speed word wraps to its
        // negative raw value and that negative speed lands the dive, exactly as the native word does.
        void VerifyDiveSpeedWrap(bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState _) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            state.Speed = new SignedEightEight(0);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32),
                new RoomLevelPlanes(new ushort[1024], new byte[1024], new ushort[1024]), new byte[8]);
            ushort nativeSpeed = unchecked((ushort)(0 - 64));
            enemies.NinjaSpacePirateBehavior.StepNinjaPirateDive(slot, state, level, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left);
            AssertEqual(nativeSpeed, state.Speed.RawValue, "the dive's speed word wraps from zero to its negative raw value");
            AssertEqual((short)-64, state.Speed.SignedRawValue, "the wrapped dive speed reads as minus 64");
            AssertEqual(movingRight ? NinjaSpacePirateFunction.DivekickRightWalkToPost : NinjaSpacePirateFunction.DivekickLeftWalkToPost,
                state.Function, "a negative dive speed lands the dive");
        }

        // Checks that a left or right claw attack spawns its first projectile at the correct horizontal offset.
        void VerifyClawAttack(NinjaSpacePirateProgram program, int expectedFirstXOffset)
        {
            (RoomEnemySystem enemies, RoomEnemySlot slot, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Program =
                AnimationProgramCursor<NinjaSpacePirateProgram>.Start(program);
            slot.InstructionTimer = WrappingCountdown.Start(1);
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
            slot.InstructionTimer = WrappingCountdown.Start(1);
            RunFrames(enemies, slot, samus, 3);
            AssertEqual(function, (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0].")).Function,
                $"ninja Pirate dive program {program} function");
            AssertEqual(0x0e00, slot.PaletteRow,
                $"ninja Pirate dive program {program} palette");
            AssertEqual(NinjaSpacePirateSoundDefinitions.Attack, (enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.WalkingSpacePirateBehavior.LastSpacePirateSoundEffect.")),
                $"ninja Pirate dive program {program} sound");
        }

        // Initializes a ninja Pirate actor with the installed shared palette.
        NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null)) { TileArtwork = artwork };
            enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
            enemies.VerificationServices().NextRandom = () => 1;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.PirateGreyNinja, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.Radius = new(8, 16);
            slot.SetSpawnParameter2(0x0080);
            enemies.NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            return new NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, slot, new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)) });
        }

        // Advances production enemy instructions for the requested number of updates.
        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemySlot slot,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [slot, samus, null, Position16<RoomPixels>.Zero, ControllerButton.None];
            for (int frame = 0; frame < frames; frame++) EnemyInstructionRunner.Process(arguments);
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
