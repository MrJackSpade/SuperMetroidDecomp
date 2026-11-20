using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.SpacePirates;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

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
        CartridgeImportAddressSpace rom)
    {
        Suite(nameof(NinjaSpacePirateAnimationsVerification.VerifyNinjaSpacePirateAnimations), () => NinjaSpacePirateAnimationsVerification.VerifyNinjaSpacePirateAnimations(rom));

        // The initializer requires the installed shared gold-Pirate palette.
        // Import its exact native colors into this bounded instruction fixture.
        var colors = new PaletteRgb5[EnemyPaletteSheetFile.ColorCount];
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
        var artwork = EnemyTileSourceArtwork.FromArtworkForVerification(new EnemyTileArtworkParts { Sheets = new Dictionary<EnemyDefinitionId, RoomCharacterAtlas>
            {
                [EnemyDefinitionId.PirateGoldWall] = RoomCharacterAtlasPng.Read(tilePng, RoomCharacterAtlasFormat.BytesPerTile),
            }, Palettes = new Dictionary<EnemyDefinitionId, EnemyPaletteSheet>
            {
                [EnemyDefinitionId.PirateGoldWall] = EnemyPaletteSheetFile.Read(new MemoryStream(
                    EnemyPaletteSheetFile.Write(new EnemyPaletteSheetDocument { Version = 1, Colors = colors }))),
            } });
        NinjaSpacePirateSegment[] programs =
        [
            NinjaSpacePirateSegment.ClawAttackLeft,
            NinjaSpacePirateSegment.SpinJumpLeft,
            NinjaSpacePirateSegment.ActiveFacingLeft,
            NinjaSpacePirateSegment.FlinchFacingLeft,
            NinjaSpacePirateSegment.DivekickLeftJump,
            NinjaSpacePirateSegment.DivekickLeftDive,
            NinjaSpacePirateSegment.WalkToLeftPost,
            NinjaSpacePirateSegment.AwaitingSamusFacingLeft,
            NinjaSpacePirateSegment.LandFacingLeft,
            NinjaSpacePirateSegment.KickFacingLeft,
            NinjaSpacePirateSegment.ClawAttackRight,
            NinjaSpacePirateSegment.SpinJumpRight,
            NinjaSpacePirateSegment.ActiveFacingRight,
            NinjaSpacePirateSegment.FlinchFacingRight,
            NinjaSpacePirateSegment.DivekickRightJump,
            NinjaSpacePirateSegment.DivekickRightDive,
            NinjaSpacePirateSegment.WalkToRightPost,
            NinjaSpacePirateSegment.AwaitingSamusFacingRight,
            NinjaSpacePirateSegment.LandFacingRight,
            NinjaSpacePirateSegment.KickFacingRight,
        ];

        foreach (NinjaSpacePirateSegment program in programs)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Playback =
                new(NinjaSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, 140);
        }

        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateSegment.ClawAttackLeft,
            expectedFirstXOffset: -32));
        Suite(nameof(VerifyClawAttack), () => VerifyClawAttack(
            NinjaSpacePirateSegment.ClawAttackRight,
            expectedFirstXOffset: 32));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateSegment.DivekickLeftDive,
            HorizontalDirection.Left));
        Suite(nameof(VerifyDive), () => VerifyDive(
            NinjaSpacePirateSegment.DivekickRightDive,
            HorizontalDirection.Right));
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
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState _) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            const ushort diveFraction = 1920;
            place.Position = place.Position.WithFractionY(diveFraction);
            // One speed step left: the dive ends after this movement, wherever it is.
            state.DiveSpeed = new SignedEightEight(64);
            RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(32, 32));
            enemies.NinjaSpacePirateBehavior.StepNinjaPirateDive(place, state, level, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left);
            AssertEqual(NinjaSpacePirateAction.DivekickWalkToPost, state.Action, $"ninja Pirate dive {(movingRight ? "right" : "left")} lands");
            AssertEqual(movingRight ? HorizontalDirection.Right : HorizontalDirection.Left, state.Heading,
                $"ninja Pirate dive {(movingRight ? "right" : "left")} walks back its way (#2165)");
            AssertEqual(state.SpawnPosition.Y, place.Position.Y.Whole, "the landing restores the spawn row");
            // This last descent adds the speed's low byte to the fraction ($B2:F9D5) before the landing.
            AssertEqual((ushort)(diveFraction + 64), place.Position.Y.Fraction, "the landing keeps the dive's Y fraction");
        }

        // Drives the divekick rise through production main AI. Each update raises the Pirate by the speed's whole
        // pixels and lowers the 8.8 speed by 64; the rise ends on the update the sixteen-bit speed word turns
        // negative, which switches to the dive and restores the initial speed. The expectation is the native word
        // arithmetic, computed here independently of the production speed type.
        void VerifyDivekickRiseSpeed(bool movingRight)
        {
            const ushort initialDiveSpeed = 1536;
            const int speedStep = 64;
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState samus) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            NinjaSpacePirateAction rising = NinjaSpacePirateAction.DivekickJump;
            NinjaSpacePirateAction diving = NinjaSpacePirateAction.DivekickDive;
            HorizontalDirection heading = movingRight ? HorizontalDirection.Right : HorizontalDirection.Left;
            state.Action = rising;
            state.Heading = heading;
            state.DiveSpeed = new SignedEightEight(initialDiveSpeed);
            var frame = new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
                new FrameCounters(SuperMetroid.Core.Compatibility.ShortFrameCount.Zero));
            ushort nativeSpeed = initialDiveSpeed;
            int risingUpdates = 0;
            for (int update = 1; update <= 40; update++)
            {
                ushort expectedY = unchecked((ushort)(place.Position.Y.Whole - nativeSpeed / 256));
                nativeSpeed = unchecked((ushort)(nativeSpeed - speedStep));
                bool riseEnds = unchecked((short)nativeSpeed) < 0;
                enemies.NinjaSpacePirateBehavior.RunNinjaSpacePirateMainAi(place, frame);
                AssertEqual(expectedY, place.Position.Y.Whole, $"ninja divekick rise update {update} climbs by the speed's whole pixels");
                if (!riseEnds)
                {
                    AssertEqual(new SignedEightEight(nativeSpeed), state.DiveSpeed, $"ninja divekick rise update {update} lowers the speed by 64");
                    AssertEqual(rising, state.Action, $"ninja divekick rise update {update} keeps rising while the speed is not negative");
                    risingUpdates++;
                    continue;
                }

                AssertEqual(ushort.MaxValue + 1 - speedStep, (int)nativeSpeed, "the native speed word wraps below zero on the last rising update");
                AssertEqual(diving, state.Action, $"ninja divekick rise ends on update {update}, when the speed turns negative");
                AssertEqual(heading, state.Heading, "the dive goes the rise's way (#2165)");
                AssertEqual(new SignedEightEight(initialDiveSpeed), state.DiveSpeed, "the dive starts again from the initial speed");
                break;
            }
            AssertEqual(initialDiveSpeed / speedStep, risingUpdates, "the rise lasts until the speed passes zero");
        }

        // Drives the production dive with a zero speed: the update lowers it by 64, so the speed word wraps to its
        // negative raw value and that negative speed lands the dive, exactly as the native word does.
        void VerifyDiveSpeedWrap(bool movingRight)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState _) = CreateSystem();
            NinjaSpacePirateEnemyState state = enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.");
            state.DiveSpeed = new SignedEightEight(0);
            RoomLevelData level = VerificationHelpersVerification.CreateEmptyRoom(new(32, 32));
            ushort nativeSpeed = unchecked((ushort)(0 - 64));
            enemies.NinjaSpacePirateBehavior.StepNinjaPirateDive(place, state, level, movingRight ? HorizontalDirection.Right : HorizontalDirection.Left);
            AssertEqual(nativeSpeed, state.DiveSpeed.RawValue, "the dive's speed word wraps from zero to its negative raw value");
            AssertEqual(-64, state.DiveSpeed.InTwoHundredFiftySixthsOfPixel, "the wrapped dive speed reads as minus 64");
            AssertEqual(NinjaSpacePirateAction.DivekickWalkToPost, state.Action, "a negative dive speed lands the dive");
        }

        // Checks that a left or right claw attack spawns its first projectile at the correct horizontal offset.
        void VerifyClawAttack(NinjaSpacePirateSegment program, int expectedFirstXOffset)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Playback =
                new(NinjaSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, 40);
            NinjaSpacePirateEnemyState state = (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0]."));
            AssertEqual(1, enemies.Projectiles.EnemyProjectiles.Count(p => p.IsActive && p.Kind == RoomEnemyProjectileKind.PirateClaw),
                $"ninja Pirate claw program {program} first spawn count");
            EnemyProjectile claw = enemies.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
            AssertEqual(
                unchecked((ushort)(place.Position.X.Whole + expectedFirstXOffset)),
                claw.Position.X.Whole,
                $"ninja Pirate claw program {program} first spawn X");
        }

        // Checks the dive's function switch, shared gold palette, and attack sound.
        void VerifyDive(NinjaSpacePirateSegment program, HorizontalDirection heading)
        {
            (RoomEnemySystem enemies, RoomEnemyPlace place, SamusState samus) = CreateSystem();
            (enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Playback =
                new(NinjaSpacePirateAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);
            place.AnimationClock.StepOnNextUpdate();
            RunFrames(enemies, place, samus, 3);
            NinjaSpacePirateEnemyState diving = enemies.NinjaSpacePirateStates[0] ?? throw new InvalidOperationException("Verification requires enemies.NinjaSpacePirateStates[0].");
            AssertEqual(NinjaSpacePirateAction.DivekickDive, diving.Action, $"ninja Pirate dive program {program} function");
            AssertEqual(heading, diving.Heading, $"ninja Pirate dive program {program} dives its way (#2165)");
            AssertEqual(SuperMetroid.Core.Enemies.SpacePirates.Artwork.NinjaSpacePirateShades.GoldWallFlash, place.Shading,
                $"ninja Pirate dive program {program} gold wall look");
            AssertEqual(NinjaSpacePirateSoundDefinitions.Attack, (enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a pending space pirate sound.")),
                $"ninja Pirate dive program {program} sound");
        }

        // Initializes a ninja Pirate actor with the installed shared palette.
        NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult CreateSystem()
        {
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledFrames(artwork), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
            enemies.VerificationServices().NextRandom = () => 1;
            RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
            place.Bind(EnemyDefinitionId.PirateSilverNinja, place.KeptDefinition);
            place.Bind(place.HeldOccupant().DefinitionId, EnemyDefinitionFixtures.Blank);
            place.Position = place.Position.WithWhole(new(0x0100, 0x0100));
            place.Radius = new(8, 16);
            place.SetSpawnParameter2(EnemyDefinitionId.PirateSilverNinja, 0x0080);
            NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(place.HeldOccupant(), place);
            return new NinjaSpacePirateInstructionProgramDefinitionsCreateSystemResult(enemies, place, new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0100)) });
        }

        // Advances production enemy instructions for the requested number of updates.
        static void RunFrames(
            RoomEnemySystem enemies,
            RoomEnemyPlace place,
            SamusState samus,
            int frames)
        {
            object?[] arguments =
                [place, samus, null, Position16<RoomPixels>.Zero, default(GameplayAction)];
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
