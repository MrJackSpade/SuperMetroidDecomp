using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroMotherBrainExplosionProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;

/// <summary>Checks intro Mother Brain actor and explosion definitions against retail timing and placements.</summary>
internal static class IntroMotherBrainDefinitionsVerification
{
    /// <summary>Compares actor records, explosion offsets, fourth-hit timing, page-two transition, and actor deletion.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyIntroMotherBrainDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        // Native six-byte definition records `$8B:CE55`, `$8B:CF15`, and `$8B:CF1B`; the
        // initial instruction list is the record's third word.
        (ushort Pointer, IntroMotherBrainActorDefinition Definition)[] actors =
        [
            (0xce55, IntroMotherBrainDefinitions.MotherBrain),
            (0xcf15, IntroMotherBrainDefinitions.BigExplosionActor),
            (0xcf1b, IntroMotherBrainDefinitions.SmallExplosionActor),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            (ushort pointer, IntroMotherBrainActorDefinition actual) = actors[index];
            int address = IntroMotherBrainDefinitions.NativeBank | pointer;
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Entry(actual.Program),
                $"intro Mother Brain actor {index} instruction list");
        }

        for (int index = 0; index < IntroMotherBrainDefinitions.BigExplosionCount; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.BigExplosion(index);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigXOffsetReferenceAddress + index * 2))),
                actual.Offset.X, $"intro Mother Brain big explosion {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigYOffsetReferenceAddress + index * 2))),
                actual.Offset.Y, $"intro Mother Brain big explosion {index} Y offset");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigTimerReferenceAddress + index * 2)),
                actual.StartTimer, $"intro Mother Brain big explosion {index} timer");
        }

        for (int index = 0; index < IntroMotherBrainDefinitions.SmallExplosionCount; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.SmallExplosion(index);
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallXOffsetReferenceAddress + index * 2))),
                actual.Offset.X, $"intro Mother Brain small explosion {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallYOffsetReferenceAddress + index * 2))),
                actual.Offset.Y, $"intro Mother Brain small explosion {index} Y offset");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallTimerReferenceAddress + index * 2)),
                actual.StartTimer, $"intro Mother Brain small explosion {index} timer");
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroMotherBrainDefinitions.BigExplosion(
                IntroMotherBrainDefinitions.BigExplosionCount),
            "intro Mother Brain big explosion definition boundary");
        AssertThrows<ArgumentOutOfRangeException>(
            () => IntroMotherBrainDefinitions.SmallExplosion(
                IntroMotherBrainDefinitions.SmallExplosionCount),
            "intro Mother Brain small explosion definition boundary");

        var guarded = NewIntroMotherBrainDefinitionGuard(retail);
        var motherBrain = new IntroMotherBrainSpriteState();
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.X,
            IntroMotherBrainSpriteState.Position.X, "intro Mother Brain compiled X origin");
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.Y,
            IntroMotherBrainSpriteState.Position.Y, "intro Mother Brain compiled Y origin");
        for (int frame = 0; frame < 80; frame++)
        {
            motherBrain.Step();
            int record = CinematicSpriteNativeListAddresses.Entry(CinematicSpriteProgram.IntroMotherBrain) +
                ((frame / 16) % 4) * 4;
            ushort expected = CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank | (record + 2)));
            AssertEqual(expected, motherBrain.SpriteMapPointer,
                $"intro Mother Brain normal frame {frame} selects its native spritemap");
        }
        for (int hit = 0; hit < 4; hit++)
            AssertEqual(hit == 3, motherBrain.RegisterMissileHit(),
                $"intro Mother Brain hit {hit + 1} starts explosion only on the fourth hit");
        var cgram = new SnesCgram();
        var palette = new ushort[SnesCgram.ColorCount];
        for (ushort frame = 1; frame <= 128; frame++)
        {
            motherBrain.RunPreInstruction(cgram, ToColors(palette), frame, introCrossfadeTimer: 1);
            motherBrain.Step();
            if (frame < 128)
                AssertTrue(!motherBrain.PageTwoRequested,
                    $"intro Mother Brain does not request page two before explosion frame {frame}");
        }
        AssertTrue(motherBrain.PageTwoRequested,
            "intro Mother Brain page-two callback runs at explosion frame 128");
        for (int frame = 0; frame < 64; frame++)
        {
            motherBrain.RunPreInstruction(cgram, ToColors(palette), (ushort)(129 + frame),
                introCrossfadeTimer: 1);
            motherBrain.Step();
            int record = CinematicSpriteNativeListAddresses.Entry(CinematicSpriteProgram.IntroMotherBrainPageTwo) +
                (((frame + 1) / 16) % 4) * 4;
            ushort expected = CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank | (record + 2)));
            AssertEqual(expected, motherBrain.SpriteMapPointer,
                $"intro Mother Brain page-two frame {frame} preserves native loop");
        }
        motherBrain.RunPreInstruction(cgram, ToColors(palette), cinematicFrameCounter: 193,
            introCrossfadeTimer: 0);
        AssertTrue(!motherBrain.IsVisible,
            "intro Mother Brain page-two crossfade deletes the actor at timer zero");

        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        AssertEqual(8, explosions.ActiveCount, "intro Mother Brain allocates all explosion actors");
        var explosionActors = explosions.actors;
        for (int frame = 0; frame < 192; frame++)
        {
            explosions.Step(introCrossfadeTimer: 1);
            int smallPhase = frame % LoopFrames(CinematicSpriteProgram.IntroMotherBrainExplosionSmall);
            int bigPhase = frame % LoopFrames(CinematicSpriteProgram.IntroMotherBrainExplosionBig);
            ushort expectedSmall = smallPhase < 18
                ? CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank |
                    (CinematicSpriteNativeListAddresses.Entry(CinematicSpriteProgram.IntroMotherBrainExplosionSmall) +
                        (smallPhase / 3) * 4 + 2)))
                : (ushort)0;
            ushort expectedBig = bigPhase < 36
                ? CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank |
                    (CinematicSpriteNativeListAddresses.Entry(CinematicSpriteProgram.IntroMotherBrainExplosionBig) +
                        (bigPhase / 6) * 4 + 2)))
                : (ushort)0;
            AssertEqual(expectedSmall, explosionActors[0].SpriteMapPointer,
                $"intro first small explosion frame {frame} follows native timing");
            AssertEqual(expectedBig, explosionActors[3].SpriteMapPointer,
                $"intro first big explosion frame {frame} follows native timing");
        }
        AssertEqual(8, explosions.ActiveCount,
            "intro Mother Brain explosion actors survive until page-two crossfade completion");
        explosions.Step(introCrossfadeTimer: 0);
        AssertEqual(0, explosions.ActiveCount,
            "intro Mother Brain explosion actors delete at page-two crossfade completion");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "intro Mother Brain actors never reread compiled definitions, instructions or placement tables");

        Console.WriteLine(
            "  Intro Mother Brain definitions: actor/placement words and 112 instruction/delete bytes match; normal/page-two and both staggered explosion loops are ROM-table independent.");
    }

    /// <summary>Rejects live reads of compiled actor, instruction, and explosion placement bytes.</summary>
    private static CartridgeReadGuard NewIntroMotherBrainDefinitionGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(
            address => address is >= 0x8bcb05 and < 0x8bcb33 or
                >= 0x8bcdab and < 0x8bcdeb or
                >= 0x8bce53 and < 0x8bce55 or
                >= 0x8bce55 and < 0x8bce5b or
                >= 0x8bcf15 and < 0x8bcf21 or
                >= 0x8bb9b6 and < 0x8bb9d4 or
                >= 0x8bb9fd and < 0x8bba0f,
            address => $"Intro Mother Brain reread compiled byte ${address:X6}.");

    /// <summary>Runs intro Mother Brain definition comparison from the command dispatcher.</summary>
    [VerificationCommand("--intro-mother-brain-definitions")]
    internal static int IntroMotherBrainDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroMotherBrainDefinitions), () => VerifyIntroMotherBrainDefinitions());
        return 0;
    }

    /// <summary>The updates one pass of looping <paramref name="program"/> lasts: the sum of its frames' durations.</summary>
    private static int LoopFrames(CinematicSpriteProgram program) =>
        CinematicSpriteAnimations.Programs.Steps(program).Where(step => step.Kind == AnimationProgramStepKind.Show).Sum(step => step.Duration);
}
