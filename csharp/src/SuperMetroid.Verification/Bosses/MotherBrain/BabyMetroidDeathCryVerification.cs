using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
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
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Baby Metroid death-cry palette frames")]
    internal static int VerifyBabyMetroidDeathCry()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem(new EnemySession());
        var installation = RepositoryInstallation.Installation;
        enemies.BossArtworkBinding.MotherBrainRoomColors = InstalledFixtures.Maps.MotherBrainRoomColors;
        var cgram = new SamusColourRig();
        for (int index = 0; index < 256; index++) cgram.SetColor(index, Bgr555.FromWord((ushort)(index + 1)));
        enemies.Presentation.BindCgram(cgram.Gameplay);
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]);
        var baby = new BabyMetroidCutsceneState();
        baby.Phase = BabyMetroidCutscenePhase.FinalCharge;
        baby.Health = 79;
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        var samus = new SamusState();
        Action<MotherBrainEnemyState, BabyMetroidCutsceneStepResult> apply = enemies.BabyMetroidCutsceneBehavior.ApplyBabyMetroidFrameEffects;
        apply(state, baby.Step(samus, sequence, HeadOf(sequence)));
        AssertEqual(0, enemies.Audio.SoundRequests.Count, "The live final charge does not emit a death cry early");
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(index + 1), cgram.Colors[index], "Live final charge preserves room colors");
        baby.ApplyMotherBrainOnionRingHit(80);
        var fatal = baby.Step(samus, sequence, HeadOf(sequence));
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, fatal.PhaseAfter, "Fatal hit enters native shake phase");
        apply(state, fatal);
        for (int index = 0; index < 256; index++)
            AssertEqual((ushort)(IsBlackoutColor(index) ? 0 : index + 1), cgram.Colors[index],
                $"Fatal blow blacks out only native background colors: {index}");
        AssertEqual(1, enemies.Audio.SoundRequests.Count, "Fatal transition emits exactly one death cry");
        var request = enemies.Audio.SoundRequests.Single();
        AssertEqual(SoundEffectLibrary.Library3, request.SoundEffect.Library, "Death cry belongs to library three");
        AssertEqual(0x19, request.SoundEffect.Value, "Death cry retains native sequence 19");
        AssertEqual(6, request.MaximumQueued, "Death cry uses native Max6");
        for (int frame = 0; frame < 3; frame++)
        {
            enemies.BeginEnemySoundRequestFrame();
            apply(state, baby.Step(samus, sequence, HeadOf(sequence)));
            AssertEqual(0, enemies.Audio.SoundRequests.Count, "Fatal shake does not replay the death cry");
        }
        baby.Phase = BabyMetroidCutscenePhase.FinalCutscene;
        for (int frame = 0; frame < 7; frame++)
        {
            var recovery = baby.Step(samus, sequence, HeadOf(sequence));
            AssertEqual(frame, (recovery.RecoveryLightsFrame ?? throw new InvalidOperationException("Verification requires recovery.RecoveryLightsFrame.")), "Recovery publishes each native light row");
            apply(state, recovery);
            for (int index = 0; index < 256; index++)
            {
                int offset = index is >= 49 and <= 62 ? index - 49 : 14 + index - 81;
                ushort expected = IsBlackoutColor(index)
                    ? bus.ReadCartridgeWord(0xadf3d3 - frame * 0x38 + offset * 2)
                    : (ushort)(index + 1);
                AssertEqual(expected, cgram.Colors[index], $"Recovery row {frame} restores exact cartridge colors and preserves other palettes: {index}");
            }
        }
        var audio = new CartridgeAudioState();
        var renderer = CartridgeAudioSessionAccess.CreateSynthesizing(installation.LoadAudio());
        renderer.RenderFrame(audio.AdvanceFrame(renderer.Acknowledgements));
        renderer.RenderFrame([new AudioRequest.ChangeMusicDataSet(AudioBank.MotherBrain)]);
        audio.QueueSound(request.SoundEffect, request.MaximumQueued);
        // The CPU rewrites the port each update until the SPC echoes it ($82:8A55); the SPC starts a
        // sound only when its poll latches a changed value, so the starts it publishes are what it received.
        var starts = new List<SoundEffectId>();
        for (int frame = 0; frame < 8; frame++)
            starts.AddRange(renderer.Advance(audio.AdvanceFrame(renderer.Acknowledgements))
                .OfType<AudioEvent.StartSoundEffect>().Select(audioEvent => audioEvent.SoundEffect));
        AssertEqual(1, starts.Count(sound => sound == request.SoundEffect), "SPC receives exactly one library-three death cry");
        AssertEqual(0, starts.Count(sound => sound.Library != SoundEffectLibrary.Library3), "Death cry never reaches the wrong library");
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
