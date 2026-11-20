using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Frontend.Intro;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

/// <summary>Checks intro Mother Brain actor and explosion definitions against retail timing and placements.</summary>
internal static class IntroMotherBrainDefinitionsVerification
{
    /// <summary>Compares actor records, explosion offsets, fourth-hit timing, page-two transition, and actor deletion.</summary>
    internal static void VerifyIntroMotherBrainDefinitions()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));

        // Native six-byte definition records `$8B:CE55`, `$8B:CF15`, and `$8B:CF1B`; the
        // initial instruction list is the record's third word.
        IntroMotherBrainDefinitionsActor[] actors =
        [
            new IntroMotherBrainDefinitionsActor(0xce55, IntroMotherBrainDefinitions.MotherBrain.Animation),
            new IntroMotherBrainDefinitionsActor(0xcf15, IntroMotherBrainDefinitions.BigExplosionActor.Animation),
            new IntroMotherBrainDefinitionsActor(0xcf1b, IntroMotherBrainDefinitions.SmallExplosionActor.Animation),
        ];
        for (int index = 0; index < actors.Length; index++)
        {
            (ushort pointer, Enum actual) = actors[index];
            int address = IntroMotherBrainDefinitions.NativeBank | pointer;
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((address + 4)), CinematicSpriteNativeListAddresses.Start(actual),
                $"intro Mother Brain actor {index} instruction list");
        }

        // The native tables list each size's explosions in spawn order (#1979: each placement is named).
        AssertEqual(5, IntroMotherBrainDefinitions.BigExplosions.Count, "intro Mother Brain big explosion count");
        AssertEqual(3, IntroMotherBrainDefinitions.SmallExplosions.Count, "intro Mother Brain small explosion count");
        for (int index = 0; index < IntroMotherBrainDefinitions.BigExplosions.Count; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.BigExplosions[index];
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigXOffsetReferenceAddress + index * 2))),
                actual.Offset.X, $"intro Mother Brain big explosion {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigYOffsetReferenceAddress + index * 2))),
                actual.Offset.Y, $"intro Mother Brain big explosion {index} Y offset");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.BigTimerReferenceAddress + index * 2)),
                actual.StartDelayFrames, $"intro Mother Brain big explosion {index} timer");
        }

        for (int index = 0; index < IntroMotherBrainDefinitions.SmallExplosions.Count; index++)
        {
            IntroMotherBrainExplosionPlacement actual = IntroMotherBrainDefinitions.SmallExplosions[index];
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallXOffsetReferenceAddress + index * 2))),
                actual.Offset.X, $"intro Mother Brain small explosion {index} X offset");
            AssertEqual(unchecked((short)CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallYOffsetReferenceAddress + index * 2))),
                actual.Offset.Y, $"intro Mother Brain small explosion {index} Y offset");
            AssertEqual(CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.SmallTimerReferenceAddress + index * 2)),
                actual.StartDelayFrames, $"intro Mother Brain small explosion {index} timer");
        }

        var guarded = NewIntroMotherBrainDefinitionGuard(retail);
        var motherBrain = new IntroMotherBrainSpriteState();
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.X,
            IntroMotherBrainSpriteState.Position.X, "intro Mother Brain compiled X origin");
        AssertEqual(IntroMotherBrainDefinitions.MotherBrainOrigin.Y,
            IntroMotherBrainSpriteState.Position.Y, "intro Mother Brain compiled Y origin");
        for (int frame = 0; frame < 80; frame++)
        {
            motherBrain.Step();
            int record = CinematicSpriteNativeListAddresses.Start(IntroMotherBrainAnimation.Pulsing) +
                ((frame / 16) % 4) * 4;
            ushort expected = CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank | (record + 2)));
            AssertEqual(expected, CinematicSpritePoseSpritemaps.Of(motherBrain.Pose),
                $"intro Mother Brain normal frame {frame} selects its native spritemap");
        }
        for (int hit = 0; hit < 4; hit++)
            AssertEqual(hit == 3, motherBrain.RecordMissileHit(),
                $"intro Mother Brain hit {hit + 1} starts explosion only on the fourth hit");
        for (ushort frame = 1; frame <= 128; frame++)
        {
            motherBrain.RunBehavior(frame, crossfadeFinishing: false);
            motherBrain.Step();
            if (frame < 128)
                AssertTrue(!motherBrain.NextPageRequested,
                    $"intro Mother Brain does not request page two before explosion frame {frame}");
        }
        AssertTrue(motherBrain.NextPageRequested,
            "intro Mother Brain page-two callback runs at explosion frame 128");
        for (int frame = 0; frame < 64; frame++)
        {
            motherBrain.RunBehavior((ushort)(129 + frame),
                crossfadeFinishing: false);
            motherBrain.Step();
            // Page two's list follows the page-two start's owner instruction and cross-fade install, six bytes in, and loops by itself.
            int record = CinematicSpriteNativeListAddresses.Start(IntroMotherBrainAnimation.StartingNextPage) + 6 +
                (((frame + 1) / 16) % 4) * 4;
            ushort expected = CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank | (record + 2)));
            AssertEqual(expected, CinematicSpritePoseSpritemaps.Of(motherBrain.Pose),
                $"intro Mother Brain page-two frame {frame} preserves native loop");
        }
        motherBrain.RunBehavior(cinematicFrameCounter: 193,
            crossfadeFinishing: true);
        AssertTrue(!motherBrain.IsVisible,
            "intro Mother Brain page-two crossfade deletes the actor at timer zero");

        var explosions = IntroMotherBrainExplosionSystem.SpawnFourthHitExplosions();
        AssertEqual(8, explosions.ActiveCount, "intro Mother Brain allocates all explosion actors");
        var explosionActors = explosions.actors;
        for (int frame = 0; frame < 192; frame++)
        {
            explosions.Step(crossfadeFinishing: false);
            int smallPhase = frame % LoopFrames(IntroMotherBrainExplosionAnimation.Small);
            int bigPhase = frame % LoopFrames(IntroMotherBrainExplosionAnimation.Big);
            ushort expectedSmall = smallPhase < 18
                ? CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank |
                    (CinematicSpriteNativeListAddresses.Start(IntroMotherBrainExplosionAnimation.Small) +
                        (smallPhase / 3) * 4 + 2)))
                : (ushort)0;
            ushort expectedBig = bigPhase < 36
                ? CartridgeImportSource.Require(retail).ReadCartridgeWord((IntroMotherBrainDefinitions.NativeBank |
                    (CinematicSpriteNativeListAddresses.Start(IntroMotherBrainExplosionAnimation.Big) +
                        (bigPhase / 6) * 4 + 2)))
                : (ushort)0;
            AssertEqual(expectedSmall, CinematicSpritePoseSpritemaps.Of(explosionActors[0].Pose),
                $"intro first small explosion frame {frame} follows native timing");
            AssertEqual(expectedBig, CinematicSpritePoseSpritemaps.Of(explosionActors[3].Pose),
                $"intro first big explosion frame {frame} follows native timing");
        }
        AssertEqual(8, explosions.ActiveCount,
            "intro Mother Brain explosion actors survive until page-two crossfade completion");
        explosions.Step(crossfadeFinishing: true);
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

    /// <summary>
    /// #1385: each of the first four missiles left of X=$54 is killed and counted, the fourth starting the explosion;
    /// after it, pre-instruction <c>$B80F</c> tests no missiles, so a fifth is neither killed nor counted.
    /// </summary>
    internal static void VerifyIntroMotherBrainMissilesAfterFourthHit()
    {
        var motherBrain = new IntroMotherBrainSpriteState();
        var shots = new SamusProjectileSystem(GameplayAudioRequests.Standalone(), new BeamChargeState(), InstalledFixtures.SamusOverreads, new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork(projectileFrames: InstalledFixtures.Projectiles.FrameBindings), new global::SuperMetroid.Core.Fx.RoomEarthquake());
        var bombs = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        SamusProjectile missile = shots.Shots[0];
        for (int hit = 1; hit <= 5; hit++)
        {
            missile.Type = SamusProjectileType.Of(SamusProjectileFamily.Missile);
            missile.Position = missile.Position.WithWhole(new(0x40, 0x80));
            bool fourth = motherBrain.TakeMissileHit(shots.EnemyImpacts, bombs);
            AssertEqual(hit == 4, fourth, $"missile {hit} starts the explosion only on the fourth hit");
            AssertEqual(hit <= 4, missile.Type.IsFamily(SamusProjectileFamily.MissileExplosion),
                $"missile {hit} is killed only while $8B:B786 tests missiles");
            AssertEqual(Math.Min(hit, 4), PrivateState.Field<IntroMotherBrainSpriteState, int>(motherBrain, "hitCount"), $"missile {hit} leaves the hit count at most four");
        }
        Console.WriteLine("Intro Mother Brain: four missiles hit and the fourth starts the explosion; a fifth passes through uncounted.");
    }

    /// <summary>Runs intro Mother Brain definition comparison from the command dispatcher.</summary>
    [VerificationCommand("--intro-mother-brain-definitions")]
    internal static int IntroMotherBrainDefinitionsCommand()
    {
        Suite(nameof(VerifyIntroMotherBrainDefinitions), () => VerifyIntroMotherBrainDefinitions());
        Suite(nameof(VerifyIntroMotherBrainMissilesAfterFourthHit), () => VerifyIntroMotherBrainMissilesAfterFourthHit());
        return 0;
    }

    /// <summary>The updates one pass of looping <paramref name="program"/> lasts: the sum of its frames' durations.</summary>
    private static int LoopFrames(IntroMotherBrainExplosionAnimation program) =>
        IntroMotherBrainAnimations.Explosions.Segments.Beats(program).OfType<AnimationBeat<CinematicSpritePose, NoAnimationCue>.Frame>().Sum(frame => frame.Duration);
}
