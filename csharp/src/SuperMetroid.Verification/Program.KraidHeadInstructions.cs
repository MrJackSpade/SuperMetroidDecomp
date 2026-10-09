using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks the compiled Kraid head instruction stream and its runtime timer and growth consumers.</summary>
    /// <param name="rom">Retail cartridge image used as the oracle for native instruction and growth-resume data.</param>
    private static void VerifyKraidHeadInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

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

    /// <summary>Runs each compiled head command through the production interpreter while rejecting reads of its source stream.</summary>
    /// <param name="rom">Retail cartridge image supplying unrelated runtime reads made by the interpreter.</param>
    private static void VerifyProductionInterpreter(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (KraidHeadInstructionDefinition definition in
            KraidHeadInstructionDefinitions.All)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
            var execute = typeof(RoomEnemySystem)
                .GetMethod("ExecuteKraidHeadInstruction", flags)!
                .CreateDelegate<Func<RoomEnemySlot, KraidEnemyState, ushort>>(enemies);
            RoomEnemySlot body = enemies.Slots[0];
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
                    enemies.LastKraidSoundEffect!.Value.SoundEffect.Value,
                    $"Kraid sound command ${definition.Pointer:X4} queues exact sound");
            }
            AssertEqual(0, guard.ForbiddenReadAttempts,
                $"Kraid command ${definition.Pointer:X4} avoids compiled stream");
        }
    }

    /// <summary>Checks native entry-timer behavior and every low-half growth selection through production consumers.</summary>
    /// <param name="rom">Retail cartridge image used to determine the matching native growth timer and cursor.</param>
    private static void VerifyTimerAndGrowthConsumers(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem { TileArtwork = RepositoryInstallation.EnemyTiles };
        var state = new KraidEnemyState();
        var guard = new KraidHeadProgramReadGuard(rom);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, new SnesVram());
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, flags)!.CreateDelegate<T>(enemies);
        var growth = Method<Func<RoomEnemySlot, KraidEnemyState, bool>>(
            "TryBeginKraidGrowth");
        var glow = Method<Action<RoomEnemySlot, KraidEnemyState>>(
            "InitializeKraidEyeGlow");
        var death = Method<Action<RoomEnemySlot, KraidEnemyState>>(
            "InitializeKraidDeath");
        var combat = Method<Action<RoomEnemySlot, KraidEnemyState, VramWriteQueue?, ushort>>(
            "RunKraidCombatFunction");
        RoomEnemySlot body = enemies.Slots[0];
        state.InitialHealth = 1000;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = (ushort)raw;
            body.VariableC = 999;
            combat(body, state, null, 0);
            AssertEqual(raw == 1
                    ? KraidHeadInstructionDefinitions.RoarEntryTimer
                    : (ushort)999,
                body.VariableC, "Roar timer loads only on thinker expiry");

            body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
            state.ThinkingTimer = (ushort)raw;
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

    /// <summary>Test address-space proxy that detects forbidden instruction-stream and untyped live-WRAM reads.</summary>
    /// <param name="source">Underlying cartridge and memory bus for permitted reads and forwarded writes.</param>
    private sealed class KraidHeadProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Live growth-selection word returned by typed reads of the native low-half WRAM alias.</summary>
        public ushort MutableTilemap { get; set; }
        /// <summary>Number of attempted reads from bytes belonging to the compiled head instruction stream.</summary>
        public int ForbiddenReadAttempts { get; private set; }
        /// <summary>Number of attempted generic-bus reads of the low-half alias reserved for typed WRAM access.</summary>
        public int UntypedLowHalfReadAttempts { get; private set; }

        /// <summary>Routes cartridge imports through the same read filter used by generic bus accesses.</summary>
        /// <param name="address">Cartridge address being read.</param>
        /// <returns>The permitted byte from the underlying source.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Counts and rejects accesses to the compiled stream or the untyped low-half WRAM alias.</summary>
        /// <param name="address">Address requested through the generic bus.</param>
        /// <returns>The byte from the underlying source when the address is permitted.</returns>
        public byte ReadByte(int address)
        {
            if (address is 0xa71002 or 0xa71003)
            {
                UntypedLowHalfReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid read live WRAM alias ${address:X6} through the generic bus.");
            }
            if (address is >= 0xa796d2 and < 0xa79788)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Kraid reread compiled head-program byte ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        /// <summary>Provides the configured live selection word for its WRAM alias and forwards other WRAM reads.</summary>
        /// <param name="address">WRAM address requested by the production consumer.</param>
        /// <returns>The selected byte of the configured word or the underlying WRAM value.</returns>
        public byte ReadWorkRamByte(int address) => address switch
        {
            0xa71002 => (byte)MutableTilemap,
            0xa71003 => (byte)(MutableTilemap >> 8),
            _ => (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose WRAM.")).ReadWorkRamByte(address),
        };

        /// <summary>Forwards save-RAM reads to the underlying mutable-memory source.</summary>
        /// <param name="address">Save-RAM address requested by the consumer.</param>
        /// <returns>The byte returned by the underlying source.</returns>
        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose SRAM.")).ReadSaveRamByte(address);

        /// <summary>Forwards memory writes without applying the read restrictions.</summary>
        /// <param name="address">Address receiving the write.</param>
        /// <param name="value">Byte to write at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
