using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadCommandMappingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses;
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

        // A head always plays an animation from the update it stands on (#2554), so there is no unstarted head to refuse.

        Console.WriteLine(
            "Kraid head programs: the 19 native records of the three started lists, both cries, all entry cursors and delays, every growth resume, and production interpretation pass with the private streams forbidden.");
    }

    /// <summary>
    /// Starts Kraid's death as the first update of his dying does: his body enters his death's first phase, whose update
    /// starts the death beside <paramref name="samus"/>.
    /// </summary>
    internal static void StartKraidDeath(KraidBehavior behavior, KraidEnemyState state, SuperMetroid.Core.Samus.SamusState samus)
    {
        state.BodyState.DeathPhase = KraidDeathPhase.Initialize;
        behavior.Death.RunKraidDeathPhase(state.Parts.Body, state, (KraidStageRun.Dying)state.BodyState.Run, samus);
    }

    /// <summary>Executes the head program from every native cursor and checks poses, durations, image uploads, cries and the resting cursor.</summary>
    private static void VerifyProductionInterpreter(CartridgeImportAddressSpace rom)
    {
        var native = new KraidHeadNativeCursors(rom);
        foreach (KraidHeadNativeRecord record in native.Records)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            Func<KraidEnemyState, KraidHeadAnimationResult> execute = enemies.KraidBehavior.Head.PlayToNextPose;
            RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
            var state = new KraidEnemyState(KraidPartsFixtures.Standalone());
            state.BodyState.FightPhase = KraidFightPhase.FirstPhaseThinking;
            state.HeadPlayback = record.Playback;

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
                AssertEqual(displayed.Opcode, state.HeadHold.UpdatesLeft, $"Kraid command ${record.Pointer:X4} holds the pose for its duration");
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
                AssertEqual(SoundEffectCommands.DecodeCartridge(SoundEffectLibrary.Enemies, (byte)record.Sound),
                    enemies.Audio.LatestSound ?? throw new InvalidOperationException("Verification requires a Kraid sound request."),
                    $"Kraid sound command ${record.Pointer:X4} queues exact sound");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts, $"Kraid command ${record.Pointer:X4} avoids compiled stream");
        }
    }

    /// <summary>Checks roar, glow, death, and all 16-bit growth-resume timer inputs against native selection.</summary>
    private static void VerifyTimerAndGrowthConsumers(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(InstalledFixtures.EnemyTiles), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: InstalledFixtures.BossArtwork);
        // His setup reads whether his room's boss is defeated, which this room's is not, and writes the screen
        // scrolls his fight frames, which this room takes.
        enemies.VerificationServices().IsAreaBossDefeated ??= () => false;
        enemies.VerificationServices().SetRoomScrollState ??= (_, _) => { };
        KraidEnemyState state = enemies.PlaceKraid();
        var guard = new KraidHeadProgramReadGuard(rom);
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        var glow = new Action<KraidEnemyState>(glowState => enemies.KraidBehavior.Head.InitializeKraidEyeGlow(glowState, (KraidStageRun.Fighting)glowState.BodyState.Run));
        var death = new Action<KraidEnemyState>(deathState => StartKraidDeath(enemies.KraidBehavior, deathState, RoomEnemyFrameFixtures.StandaloneSamus()));
        var combat = new Action<RoomEnemyPlace, KraidEnemyState, ushort>((combatBody, combatState, cameraX) =>
            enemies.KraidBehavior.RunKraidBodyStage(combatBody, combatState, cameraX, RoomEnemyFrameFixtures.StandaloneSamus()));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        state.InitialHealth = 1000;
        // A thinking wait at zero has run out and holds (#2538): only the update its last count runs out on roars. The counts
        // are the run-out wait, its last count, the counts either side of the sign bit and the widest, where a wrapping or
        // signed wait would differ.
        foreach (int raw in (int[])[0, 1, 2, 3, 0x7ffe, 0x7fff, 0x8000, 0x8001, 0xfffe, ushort.MaxValue])
        {
            state.BodyState.FightPhase = KraidFightPhase.FirstPhaseThinking;
            state.ThinkingWait.SetUpdatesLeft(raw);
            state.HeadHold.LastFor(999);
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Delay
                    : 999,
                state.HeadHold.UpdatesLeft, "the roar starts only on the thinking's last update");

            state.BodyState.FightPhase = KraidFightPhase.SecondPhaseThinking;
            state.ThinkingWait.SetUpdatesLeft(raw);
            state.HeadHold.LastFor(999);
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? KraidHeadAnimations.Starting(KraidHeadSegment.Roar).Delay
                    : 999,
                state.HeadHold.UpdatesLeft, "the grown roar starts only on the thinking's last update");
        }

        glow(state);
        AssertEqual(
            KraidHeadAnimations.Starting(KraidHeadSegment.EyeGlow).Delay - 1,
            state.HeadHold.UpdatesLeft, "the eye glow's start takes the first update of its opening pose's hold");
        state.HeadHold.LastFor(999);
        death(state);
        AssertEqual(KraidHeadAnimations.Starting(KraidHeadSegment.Death).Delay, state.HeadHold.UpdatesLeft,
            "the death's start holds its opening pose for its whole duration");
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
