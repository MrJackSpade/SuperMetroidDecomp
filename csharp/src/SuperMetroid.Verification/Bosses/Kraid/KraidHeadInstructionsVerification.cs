using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidContourCaseMappingsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidHeadCommandMappingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail-data and typed-WRAM checks for Kraid head instruction programs.</summary>
internal static class KraidHeadInstructionsVerification
{
    /// <summary>Checks the head programs against the cartridge, production interpretation, and the timer and growth consumers.</summary>
    internal static void VerifyKraidHeadInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyKraidHeadCommandMapping), () => VerifyKraidHeadCommandMapping(rom));
        Suite(nameof(VerifyKraidGrowthResumeCases), () => VerifyKraidGrowthResumeCases(rom));
        Suite(nameof(VerifyProductionInterpreter), () => VerifyProductionInterpreter(rom));
        Suite(nameof(VerifyTimerAndGrowthConsumers), () => VerifyTimerAndGrowthConsumers(rom));

        var unstarted = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        Func<RoomEnemySlot, KraidEnemyState, ushort> run = unstarted.KraidBehavior.Head.ExecuteKraidHeadInstruction;
        AssertThrows<InvalidDataException>(() => run(unstarted.SlotTable.Slots[0], new KraidEnemyState()),
            "an unstarted Kraid head program fails loudly, as an unset native cursor did");

        Console.WriteLine(
            "Kraid head programs: 28 native records, both cries, all entry cursors and delays, every growth resume, and production interpretation pass with the private streams forbidden.");
    }

    /// <summary>Executes the head program from every native cursor and checks poses, durations, image uploads, cries and the resting cursor.</summary>
    private static void VerifyProductionInterpreter(SuperMetroidAddressSpace rom)
    {
        var native = new KraidHeadNativeCursors(CartridgeImportSource.Require(rom));
        foreach (KraidHeadNativeRecord record in native.Records)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
            Func<RoomEnemySlot, KraidEnemyState, ushort> execute = enemies.KraidBehavior.Head.ExecuteKraidHeadInstruction;
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            var state = new KraidEnemyState { HeadProgram = record.Cursor };

            ushort result = execute(body, state);
            if (record.IsTerminator)
            {
                AssertEqual(ushort.MaxValue, result, $"Kraid terminator ${record.Pointer:X4} result");
                AssertEqual(record.Cursor, state.HeadProgram, $"Kraid terminator ${record.Pointer:X4} keeps cursor");
            }
            else
            {
                // A cry runs on into the pose authored after it.
                KraidHeadNativeRecord displayed = record.IsPose
                    ? record
                    : native.RecordAt(native.CursorAt(unchecked((ushort)(record.Pointer + 2))));
                AssertEqual((ushort)1, result, $"Kraid command ${record.Pointer:X4} reaches timed frame");
                AssertEqual(displayed.Opcode, state.Parts[0].HeadTimer, $"Kraid command ${record.Pointer:X4} installs duration");
                KraidHeadFrame shown = state.DisplayedHeadFrame
                    ?? throw new InvalidOperationException("Verification requires a displayed Kraid head pose.");
                AssertEqual(displayed.Tilemap, KraidHeadFrameDefinitions.PresentationKey(shown),
                    $"Kraid command ${record.Pointer:X4} displays its pose's image");
                AssertEqual(displayed.OuterShape, (ushort)shown.VulnerableShape, $"Kraid command ${record.Pointer:X4} outer shape");
                AssertEqual(native.CursorAt(unchecked((ushort)(displayed.Pointer + 8))), state.HeadProgram,
                    $"Kraid command ${record.Pointer:X4} advances cursor");
            }

            if (!record.IsPose && !record.IsTerminator)
            {
                AssertEqual(SoundEffectCommands.Decode(SoundEffectLibrary.Library2, (byte)record.Sound),
                    (enemies.KraidBehavior.Access.LastKraidSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.KraidBehavior.LastKraidSoundEffect.")).SoundEffect,
                    $"Kraid sound command ${record.Pointer:X4} queues exact sound");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts, $"Kraid command ${record.Pointer:X4} avoids compiled stream");
        }
    }

    /// <summary>Checks roar, glow, death, and all 16-bit growth-resume timer inputs against native selection.</summary>
    private static void VerifyTimerAndGrowthConsumers(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = InstalledFixtures.EnemyTiles, BossArtwork = InstalledFixtures.BossArtwork, AreaArtwork = InstalledFixtures.AreaArtwork };
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Access.Kraid = state;
        var guard = new KraidHeadProgramReadGuard(rom);
        enemies.Presentation.BindColours(new GameplayColours(new SpriteColourSets()));
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(new SnesVram()));
        var glow = new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Head.InitializeKraidEyeGlow);
        var death = new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.Death.InitializeKraidDeath);
        var combat = new Action<RoomEnemySlot, KraidEnemyState, ushort>(enemies.KraidBehavior.RunKraidCombatFunction);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        state.InitialHealth = 1000;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            state.Parts[0].Function = KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = WrappingCountdown.FromRaw((ushort)raw);
            state.Parts[0].HeadTimer = 999;
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? (ushort)KraidHeadAnimations.RoarEntryDuration
                    : (ushort)999,
                state.Parts[0].HeadTimer, "Roar timer loads only on thinker expiry");

            state.Parts[0].Function = KraidAiFunction.SecondPhaseThinking;
            state.ThinkingTimer = WrappingCountdown.FromRaw((ushort)raw);
            state.Parts[0].HeadTimer = 999;
            combat(body, state, 0);
            AssertEqual(raw == 1
                    ? (ushort)KraidHeadAnimations.RoarEntryDuration
                    : (ushort)999,
                state.Parts[0].HeadTimer, "Second-phase roar timer loads only on thinker expiry");
        }

        glow(body, state);
        AssertEqual(
            unchecked(KraidHeadAnimations.EyeGlowEntryDuration - 1),
            state.Parts[0].HeadTimer, "Eye glow consumes one timer tick on initialization");
        state.HurtFrame = 1;
        state.Parts[0].HeadTimer = 999;
        death(body, state);
        AssertEqual(999, state.Parts[0].HeadTimer,
            "Active hurt frame delays death timer setup");
        state.HurtFrame = 0;
        death(body, state);
        AssertEqual(KraidHeadAnimations.DeathEntryDuration, state.Parts[0].HeadTimer,
            "Death initialization installs native timer without ticking");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid timer/growth consumers avoid compiled head programs");
    }

    /// <summary>Guards compiled head-program bytes while observing Kraid's typed live-tilemap WRAM read.</summary>
    [CpuBusBoundary("exact bus test: witnesses that Kraid reads the $A7:1002 live tilemap alias only through typed WRAM reads, never the generic bus, with compiled head-program bytes forbidden")]
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
