using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadCommandMappingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-data and typed-WRAM checks for Kraid head instruction programs.</summary>
internal static class KraidHeadInstructionsVerification
{
    /// <summary>Checks the head programs against the cartridge, production interpretation, and the timer and growth consumers.</summary>
    internal static void VerifyKraidHeadInstructionDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyKraidHeadCommandMapping), () => VerifyKraidHeadCommandMapping(rom));
        Suite(nameof(VerifyKraidGrowthResumeCases), () => VerifyKraidGrowthResumeCases(rom));
        Suite(nameof(VerifyProductionInterpreter), () => VerifyProductionInterpreter(rom));
        Suite(nameof(VerifyTimerAndGrowthConsumers), () => VerifyTimerAndGrowthConsumers(rom));

        var unstarted = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
        Func<KraidEnemyState, KraidHeadAnimationResult> run = unstarted.KraidBehavior.Head.RunKraidHeadAnimationStep;
        AssertThrows<InvalidDataException>(() => run(new KraidEnemyState()),
            "an unstarted Kraid head program fails loudly, as an unset native cursor did");

        Console.WriteLine(
            "Kraid head programs: 28 native records, both cries, all entry cursors and delays, every growth resume, and production interpretation pass with the private streams forbidden.");
    }

    /// <summary>Executes the head program from every native cursor and checks poses, durations, image uploads, cries and the resting cursor.</summary>
    private static void VerifyProductionInterpreter(CartridgeImportAddressSpace rom)
    {
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(rom));
        foreach (KraidHeadNativeRecord record in native.Records)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            Func<KraidEnemyState, KraidHeadAnimationResult> execute = enemies.KraidBehavior.Head.RunKraidHeadAnimationStep;
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            var state = new KraidEnemyState { HeadPlayback = record.Playback };

            KraidHeadAnimationResult result = execute(state);
            if (record.IsTerminator)
            {
                AssertEqual(KraidHeadAnimationResult.AnimationEnded, result, $"Kraid terminator ${record.Pointer:X4} result");
                AssertEqual(record.Playback, state.HeadPlayback, $"Kraid terminator ${record.Pointer:X4} keeps its holding place");
            }
            else
            {
                // A cry runs on into the pose authored after it.
                KraidHeadNativeRecord displayed = record.IsPose
                    ? record
                    : native.RecordAt(native.PlaybackAt(unchecked((ushort)(record.Pointer + 2))));
                AssertEqual(KraidHeadAnimationResult.PoseShown, result, $"Kraid command ${record.Pointer:X4} reaches timed frame");
                AssertEqual(displayed.Opcode, state.BodyState.HeadTimer, $"Kraid command ${record.Pointer:X4} installs duration");
                KraidHeadFrame shown = state.DisplayedHeadFrame
                    ?? throw new InvalidOperationException("Verification requires a displayed Kraid head pose.");
                AssertEqual(displayed.Tilemap, KraidHeadFrameDefinitions.PresentationKey(shown),
                    $"Kraid command ${record.Pointer:X4} displays its pose's image");
                AssertEqual(displayed.OuterShape, KraidNativeWords.MouthHitboxes.Word(shown.VulnerableShape), $"Kraid command ${record.Pointer:X4} outer shape");
                AssertEqual(unchecked((ushort)(displayed.Pointer + 8)), native.PointerAt(state.HeadPlayback ?? throw new InvalidOperationException("Verification requires a started Kraid head.")),
                    $"Kraid command ${record.Pointer:X4} advances cursor");
            }

            if (!record.IsPose && !record.IsTerminator)
            {
                AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, (byte)record.Sound),
                    enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a Kraid sound request."),
                    $"Kraid sound command ${record.Pointer:X4} queues exact sound");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts, $"Kraid command ${record.Pointer:X4} avoids compiled stream");
        }
    }

    /// <summary>Checks roar, glow, death, and all 16-bit growth-resume timer inputs against native selection.</summary>
    private static void VerifyTimerAndGrowthConsumers(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = InstalledFixtures.BossArtwork };
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        enemies.KraidBehavior.Parts.RegisterInRoomOrder(enemies.SlotTable);
        var guard = new KraidHeadProgramReadGuard(rom);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        var glow = new Action<KraidEnemyState>(enemies.KraidBehavior.Head.InitializeKraidEyeGlow);
        var death = new Action<KraidEnemyState>(enemies.KraidBehavior.Death.InitializeKraidDeath);
        var combat = new Action<RoomEnemySlot, KraidEnemyState, ushort>((combatBody, combatState, cameraX) =>
            enemies.KraidBehavior.RunKraidCombatPhase(combatBody, combatState, cameraX));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        state.InitialHealth = 1000;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            state.BodyState.Phase = KraidPhase.MainloopThinking;
            state.ThinkingTimer = raw;
            state.BodyState.HeadTimer = 999;
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? (ushort)KraidHeadAnimations.RoarEntryDuration
                    : (ushort)999,
                state.BodyState.HeadTimer, "Roar timer loads only on thinker expiry");

            state.BodyState.Phase = KraidPhase.SecondPhaseThinking;
            state.ThinkingTimer = raw;
            state.BodyState.HeadTimer = 999;
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? (ushort)KraidHeadAnimations.RoarEntryDuration
                    : (ushort)999,
                state.BodyState.HeadTimer, "Second-phase roar timer loads only on thinker expiry");
        }

        glow(state);
        AssertEqual(
            unchecked(KraidHeadAnimations.EyeGlowEntryDuration - 1),
            state.BodyState.HeadTimer, "Eye glow consumes one timer tick on initialization");
        state.HurtFrame = 1;
        state.BodyState.HeadTimer = 999;
        death(state);
        AssertEqual(999, state.BodyState.HeadTimer,
            "Active hurt frame delays death timer setup");
        state.HurtFrame = 0;
        death(state);
        AssertEqual(KraidHeadAnimations.DeathEntryDuration, state.BodyState.HeadTimer,
            "Death initialization installs native timer without ticking");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid timer/growth consumers avoid compiled head programs");
    }

    /// <summary>Guards compiled head-program bytes while observing Kraid's typed live-tilemap WRAM read.</summary>
    internal sealed class KraidHeadProgramReadGuard(ISnesAddressSpace source) :
        ISnesWritableAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Counts attempts to reread the compiled head stream.</summary>
        public int ForbiddenReadAttempts { get; private set; }

        /// <summary>Rejects head-stream bytes and low-half alias reads before delegating other cartridge data.</summary>
        public byte ReadCartridgeByte(int address)
        {
            if (address is >= 0xa796d2 and < 0xa79788)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid reread compiled head-program byte ${address:X6}.");
            }
            return source.ReadCartridgeByte(address);
        }

        /// <summary>Forwards WRAM reads to the mutable source.</summary>
        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose WRAM.")).ReadWorkRamByte(address);

        /// <summary>Preserves ordinary writes through the guarded address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Runs the retail-backed Kraid head instruction verification command.</summary>
    [VerificationCommand("--kraid-head-instruction-definitions")]
    internal static int KraidHeadInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
