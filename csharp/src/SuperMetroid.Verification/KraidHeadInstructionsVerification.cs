using System.Reflection;
using SuperMetroid.Core.Hardware;
using static KraidContourCaseMappingsVerification;
using static KraidHeadCommandMappingVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

/// <summary>Retail-data and typed-WRAM checks for Kraid head instruction programs.</summary>
internal static class KraidHeadInstructionsVerification
{
    /// <summary>Checks command mapping, entry timers, production frame interpretation, and mutable growth resume selection.</summary>
    internal static void VerifyKraidHeadInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => unchecked((ushort)(
            SnesLittleEndian.Word(KraidHeadInstructionDefinitionsRomByte(rom, address), KraidHeadInstructionDefinitionsRomByte(rom, address + 1))));

        Suite(nameof(VerifyKraidHeadCommandMapping), () => VerifyKraidHeadCommandMapping(rom));
        Suite(nameof(VerifyKraidGrowthResumeCases), () => VerifyKraidGrowthResumeCases(rom));

        AssertEqual(Word(0xa796d2), KraidHeadInstructionDefinitions.RoarEntryTimer,
            "Native roar entry timer");
        AssertEqual(Word(0xa7974a), KraidHeadInstructionDefinitions.EyeGlowEntryTimer,
            "Native glow entry timer");
        AssertEqual(Word(0xa79764), KraidHeadInstructionDefinitions.DeathEntryTimer,
            "Native death entry timer");

        Suite(nameof(VerifyProductionInterpreter), () => VerifyProductionInterpreter(rom));
        Suite(nameof(VerifyTimerAndGrowthConsumers), () => VerifyTimerAndGrowthConsumers(rom));

        AssertThrows<InvalidDataException>(
            () => KraidHeadInstructionDefinitions.Resolve(0x9788),
            "Kraid head catalog rejects following mouth geometry");
        AssertThrows<InvalidDataException>(
            () => KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(rom, 0x8000),
            "Kraid head tilemap resolver rejects unrelated upper-ROM code");
        AssertThrows<ArgumentOutOfRangeException>(
            () => KraidHeadInstructionDefinitions.ReadGrowthSelectionWord(rom, 0x3ffe),
            "Kraid head low-half alias rejects unmapped expansion space");

        Console.WriteLine(
            "Kraid head programs: 28 commands, 91 stream words, both sound callbacks, all entry timers, mutable low-half growth selection, and production interpretation pass with the private streams forbidden.");
    }

    /// <summary>Reads one retail byte for the independent entry-timer oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid head instruction definitions")]
    private static byte KraidHeadInstructionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Executes each compiled head command and checks timed frames, hitboxes, tilemap uploads, and sound effects.</summary>
    private static void VerifyProductionInterpreter(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (KraidHeadInstructionDefinition definition in
            KraidHeadInstructionDefinitions.All)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
            enemies.BindFixtureBus(guard);
            enemies.Presentation._vram = new SnesVram();
            Func<RoomEnemySlot, KraidEnemyState, ushort> execute = enemies.KraidBehavior.ExecuteKraidHeadInstruction;
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            var state = new KraidEnemyState();
            body.VariableB = definition.Pointer;

            ushort result = execute(body, state);
            if (definition.Kind == KraidHeadInstructionKind.Terminate)
            {
                AssertEqual(ushort.MaxValue, result,
                    $"Kraid terminator ${definition.Pointer:X4} result");
                AssertEqual(definition.Pointer, body.VariableB,
                    $"Kraid terminator ${definition.Pointer:X4} keeps cursor");
            }
            else
            {
                KraidHeadInstructionDefinition displayed = definition.Kind ==
                    KraidHeadInstructionKind.Frame
                    ? definition
                    : KraidHeadInstructionDefinitions.Resolve(
                        unchecked((ushort)(definition.Pointer + 2)));
                AssertEqual((ushort)1, result,
                    $"Kraid command ${definition.Pointer:X4} reaches timed frame");
                AssertEqual(displayed.Duration, body.VariableC,
                    $"Kraid command ${definition.Pointer:X4} installs duration");
                AssertEqual(displayed.Tilemap, state.CurrentHeadTilemap,
                    $"Kraid command ${definition.Pointer:X4} installs tilemap");
                AssertEqual(displayed.VulnerableHitbox, state.VulnerableMouthHitbox,
                    $"Kraid command ${definition.Pointer:X4} installs vulnerable hitbox");
                AssertEqual(displayed.InvulnerableHitbox, state.InvulnerableMouthHitbox,
                    $"Kraid command ${definition.Pointer:X4} installs invulnerable hitbox");
                AssertEqual(unchecked((ushort)(displayed.Pointer + 8)), body.VariableB,
                    $"Kraid command ${definition.Pointer:X4} advances cursor");
                AssertEqual(1, state.HeadTilemapUploadCount,
                    $"Kraid command ${definition.Pointer:X4} uploads one head tilemap");
            }

            if (definition.Kind is KraidHeadInstructionKind.RoarSound or
                KraidHeadInstructionKind.DyingSound)
            {
                AssertEqual(unchecked((byte)definition.SoundId),
                    (enemies.KraidBehavior.LastKraidSoundEffect ?? throw new InvalidOperationException("Verification requires enemies.KraidBehavior.LastKraidSoundEffect.")).SoundEffect.Value,
                    $"Kraid sound command ${definition.Pointer:X4} queues exact sound");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"Kraid command ${definition.Pointer:X4} avoids compiled stream");
        }
    }

    /// <summary>Checks roar, glow, death, and all 16-bit growth-resume timer inputs against native selection.</summary>
    private static void VerifyTimerAndGrowthConsumers(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem { TileArtwork = InstalledFixtures.EnemyTiles };
        var state = new KraidEnemyState();
        enemies.KraidBehavior.Kraid = state;
        var guard = new KraidHeadProgramReadGuard(rom);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        enemies.BindFixtureBus(guard);
        enemies.Presentation._cgram = new SnesCgram();
        enemies.Presentation._vram = new SnesVram();
        var growth = new Func<RoomEnemySlot, KraidEnemyState, bool>(enemies.KraidBehavior.TryBeginKraidGrowth);
        var glow = new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.InitializeKraidEyeGlow);
        var death = new Action<RoomEnemySlot, KraidEnemyState>(enemies.KraidBehavior.InitializeKraidDeath);
        var combat = new Action<RoomEnemySlot, KraidEnemyState, VramWriteQueue?, ushort>(enemies.KraidBehavior.RunKraidCombatFunction);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        state.InitialHealth = 1000;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = NativeCountdown.FromRaw((ushort)raw);
            body.VariableC = 999;
            combat(body, state, null, 0);
            AssertEqual(raw == 1
                    ? KraidHeadInstructionDefinitions.RoarEntryTimer
                    : (ushort)999,
                body.VariableC, "Roar timer loads only on thinker expiry");

            body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
            state.ThinkingTimer = NativeCountdown.FromRaw((ushort)raw);
            body.VariableC = 999;
            combat(body, state, null, 0);
            AssertEqual(raw == 1
                    ? KraidHeadInstructionDefinitions.RoarEntryTimer
                    : (ushort)999,
                body.VariableC, "Second-phase roar timer loads only on thinker expiry");

            body.Health = 1;
            body.VariableB = 0x1000;
            guard.MutableTilemap = (ushort)raw;
            AssertTrue(growth(body, state), "Growth threshold admits timer setup");
            KraidHeadResumeDefinition expected =
                NativeKraidGrowthResume(rom, (ushort)raw);
            AssertEqual(expected.Timer, body.VariableC,
                "Growth timer follows selected native resume row");
            AssertEqual(expected.Pointer, body.VariableB,
                "Growth resume cursor remains paired with timer");
        }

        glow(body, state);
        AssertEqual(
            unchecked((ushort)(KraidHeadInstructionDefinitions.EyeGlowEntryTimer - 1)),
            body.VariableC, "Eye glow consumes one timer tick on initialization");
        state.HurtFrame = 1;
        body.VariableC = 999;
        death(body, state);
        AssertEqual((ushort)999, body.VariableC,
            "Active hurt frame delays death timer setup");
        state.HurtFrame = 0;
        death(body, state);
        AssertEqual(KraidHeadInstructionDefinitions.DeathEntryTimer, body.VariableC,
            "Death initialization installs native timer without ticking");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Kraid timer/growth consumers avoid compiled head programs");
        AssertEqual(0, guard.UntypedLowHalfReadAttempts,
            "Kraid growth reads its live low-half alias through typed WRAM");
    }

    /// <summary>Guards compiled head-program bytes while observing Kraid's typed live-tilemap WRAM read.</summary>
    [CpuBusBoundary("exact bus test: witnesses that Kraid reads the $A7:1002 live tilemap alias only through typed WRAM reads, never the generic bus, with compiled head-program bytes forbidden")]
    internal sealed class KraidHeadProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Live low-half tilemap alias supplied to Kraid's growth resume path.</summary>
        public ushort MutableTilemap { get; set; }
        /// <summary>Counts attempts to reread the compiled head stream.</summary>
        public int ForbiddenReadAttempts { get; private set; }
        /// <summary>Counts invalid generic-bus reads of the typed low-half WRAM alias.</summary>
        public int UntypedLowHalfReadAttempts { get; private set; }

        /// <summary>Rejects head-stream bytes and low-half alias reads before delegating other cartridge data.</summary>
        public byte ReadCartridgeByte(int address)
        {
            if (address is 0xa71002 or 0xa71003)
            {
                UntypedLowHalfReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid read live WRAM alias ${address:X6} through the cartridge reader.");
            }
            if (address is >= 0xa796d2 and < 0xa79788)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid reread compiled head-program byte ${address:X6}.");
            }
            return source.ReadCartridgeByte(address);
        }

        /// <summary>Serves the mutable tilemap alias directly and forwards other WRAM reads.</summary>
        public byte ReadWorkRamByte(int address) => address switch
        {
            0xa71002 => (byte)MutableTilemap,
            0xa71003 => SnesLittleEndian.HighByte(MutableTilemap),
            _ => (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose WRAM.")).ReadWorkRamByte(address),
        };

        /// <summary>Forwards SRAM reads to the mutable source.</summary>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose SRAM.")).ReadSaveRamByte(address);

        /// <summary>Preserves ordinary writes through the guarded address space.</summary>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    /// <summary>Runs the retail-backed Kraid head instruction verification command.</summary>
    [VerificationCommand("--kraid-head-instruction-definitions")]
    internal static int KraidHeadInstructionDefinitionsCommand()
    {
        Suite(nameof(VerifyKraidHeadInstructionDefinitions), () => VerifyKraidHeadInstructionDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
