using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Baby Metroid's fatal blackout, palette recovery, and one-shot SPC death cry.</summary>
internal static class BabyMetroidDeathCryVerification
{
    /// <summary>Compares all affected CGRAM entries and verifies exactly one native library-three sound publication.</summary>
    internal static int VerifyBabyMetroidDeathCry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        var installation = RepositoryInstallation.Installation;
        // The Baby's scene runs while Mother Brain's encounter is loaded; the room shows the shades it lays (#1512).
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        // Mother Brain is placed as her room's population places her, and her late fight brings in the Baby, whose own main
        // AI then runs its cutscene.
        // Her late fight and the Baby read the room's random numbers, which stand at zero here.
        enemies.VerificationServices().ReadRandom = () => 0;
        enemies.VerificationServices().NextRandom = () => 0;
        MotherBrainEnemyState motherBrain = MotherBrainBabyArrival.PlaceMotherBrain(enemies, samus);
        MotherBrainRainbowBeamAttackSequence lateFight = MotherBrainBabyArrival.BringInBaby(motherBrain, samus);
        BabyMetroidCutsceneEntity entity = MotherBrainBabyArrival.BabyOf(lateFight);
        ushort[] Shown() => MotherBrainLook.Over(motherBrain, InstalledFixtures.ImportMaps, InstalledFixtures.BossArtwork,
            [.. Enumerable.Range(0, 256).Select(index => (ushort)(index + 1))]);
        // The Baby's health lives in the place the final beam's spawn makes it in (#2572).
        BabyMetroidCutsceneState baby = lateFight.Baby ?? throw new InvalidOperationException("Verification requires the final beam's spawn to make the Baby.");
        baby.Phase = new BabyMetroidCutscenePhase.Flying.FinalCharge();
        baby.Health = 79;
        // The room shows the colours the Baby's arrival laid over it; the fatal blow and the recovery change only the
        // background's.
        ushort[] arrived = Shown();
        enemies.BeginEnemySoundRequestFrame();
        entity.Place.RunMainAi(samus);
        AssertEqual(0, enemies.Audio.SoundRequests.Count, "The live final charge does not emit a death cry early");
        ushort[] charged = Shown();
        for (int index = 0; index < 256; index++)
            AssertEqual(arrived[index], charged[index], "Live final charge preserves room colors");
        baby.ApplyMotherBrainOnionRingHit(MotherBrainAttackDamage.OnionRing);
        entity.Place.RunMainAi(samus);
        AssertEqual(new BabyMetroidCutscenePhase.Flying.TakeFinalBlow(), baby.Phase, "Fatal hit enters native shake phase");
        ushort[] blackout = Shown();
        for (int index = 0; index < 256; index++)
            AssertEqual(IsBlackoutColor(index) ? (ushort)0 : arrived[index], blackout[index],
                $"Fatal blow blacks out only native background colors: {index}");
        AssertEqual(1, enemies.Audio.SoundRequests.Count, "Fatal transition emits exactly one death cry");
        var request = enemies.Audio.SoundRequests.Single();
        AssertEqual(SoundEffectLibrary.Room, request.SoundEffect.Library, "Death cry belongs to library three");
        AssertEqual<SoundEffectId>(RoomSound.BabyMetroidDeath, request.SoundEffect, "Death cry is the Baby Metroid's death (native sequence 19)");
        AssertEqual(SoundRequestPriority.Normal, request.Priority, "Death cry is requested at normal priority");
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.BeginEnemySoundRequestFrame();
            entity.Place.RunMainAi(samus);
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "Fatal shake does not replay the death cry");
        }
        baby.Phase = new BabyMetroidCutscenePhase.Flying.FinalCutscene();
        for (int frame = 0; frame < 7; frame++)
        {
            // Each update of the final cutscene shows its row of the recovering lights, which the colours below observe.
            entity.Place.RunMainAi(samus);
            ushort[] cgram = Shown();
            for (int index = 0; index < 256; index++)
            {
                int offset = index is >= 49 and <= 62 ? index - 49 : 14 + index - 81;
                ushort expected = IsBlackoutColor(index)
                    ? bus.ReadCartridgeWord(0xadf3d3 - frame * 0x38 + offset * 2)
                    : arrived[index];
                AssertEqual(expected, cgram[index], $"Recovery row {frame} restores exact cartridge colors and preserves other palettes: {index}");
            }
        }
        var audio = AudioQueues.Standalone();
        var renderer = AudioSessionAccess.CreateSynthesizing(installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(MusicDataSet.MotherBrain)]);
        audio.QueueSound(request.SoundEffect, request.Priority);
        // The CPU rewrites the port each update until the SPC echoes it ($82:8A55); the SPC starts a
        // sound only when its poll latches a changed value, so the starts it publishes are what it received.
        var starts = new List<SoundEffectId>();
        for (int frame = 0; frame < 8; frame++)
            starts.AddRange(renderer.Advance(audio.AdvanceFrame(renderer.Acknowledgements))
                .OfType<AudioEvent.StartSoundEffect>().Select(audioEvent => audioEvent.SoundEffect));
        AssertEqual(1, starts.Count(sound => sound == request.SoundEffect), "SPC receives exactly one library-three death cry");
        AssertEqual(0, starts.Count(sound => sound.Library != SoundEffectLibrary.Room), "Death cry never reaches the wrong library");
        Console.WriteLine("Baby Metroid fatal blow: exact 28-color blackout, seven cartridge recovery rows, untouched sprite palettes, one-shot death cry and SPC publication passed.");
        return 0;

        static bool IsBlackoutColor(int index) => index is >= 49 and <= 62 or >= 81 and <= 94;
    }

    /// <summary>Runs the fatal-blow palette and audio verification.</summary>
    [VerificationCommand("--baby-metroid-death-cry")]
    internal static int BabyMetroidDeathCryCommand()
    {
        return VerifyBabyMetroidDeathCry();
    }
}
