using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKraidHeadInstructionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

        VerifyKraidHeadCommandMapping(rom);

        AssertEqual(Word(0xa796d2), KraidHeadInstructionDefinitions.RoarEntryTimer,
            "Native roar entry timer");
        AssertEqual(Word(0xa7974a), KraidHeadInstructionDefinitions.EyeGlowEntryTimer,
            "Native glow entry timer");
        AssertEqual(Word(0xa79764), KraidHeadInstructionDefinitions.DeathEntryTimer,
            "Native death entry timer");

        VerifyProductionInterpreter(rom);
        VerifyTimerAndGrowthConsumers(rom);

        AssertThrows<InvalidDataException>(
            () => KraidHeadInstructionDefinitions.Resolve(0x9788),
            "Kraid head catalog rejects following mouth geometry");
        AssertThrows<InvalidDataException>(
            () => KraidHeadInstructionDefinitions.ResolveFrameTilemap(rom, 0x8000),
            "Kraid head tilemap resolver rejects unrelated upper-ROM code");
        AssertThrows<ArgumentOutOfRangeException>(
            () => KraidHeadInstructionDefinitions.ResolveFrameTilemap(rom, 0x3ffe),
            "Kraid head low-half alias rejects unmapped expansion space");

        Console.WriteLine(
            "Kraid head programs: 28 commands, 91 stream words, both sound callbacks, all entry timers, mutable low-half growth selection, and production interpretation pass with the private streams forbidden.");
    }

    private static void VerifyProductionInterpreter(SuperMetroidAddressSpace rom)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        foreach (KraidHeadInstructionDefinition definition in
            KraidHeadInstructionDefinitions.All)
        {
            var guard = new KraidHeadProgramReadGuard(rom);
            var enemies = new RoomEnemySystem();
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

    private static void VerifyTimerAndGrowthConsumers(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
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
        var combat = Method<Action<RoomEnemySlot, KraidEnemyState, VramWriteQueue?>>(
            "RunKraidCombatFunction");
        RoomEnemySlot body = enemies.Slots[0];
        state.HealthEighthThresholds[6] = 875;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = (ushort)raw;
            body.VariableC = 999;
            combat(body, state, null);
            AssertEqual(raw == 1
                    ? KraidHeadInstructionDefinitions.RoarEntryTimer
                    : (ushort)999,
                body.VariableC, "Roar timer loads only on thinker expiry");

            body.VariableA = (ushort)KraidAiFunction.SecondPhaseThinking;
            state.ThinkingTimer = (ushort)raw;
            body.VariableC = 999;
            combat(body, state, null);
            AssertEqual(raw == 1
                    ? KraidHeadInstructionDefinitions.RoarEntryTimer
                    : (ushort)999,
                body.VariableC, "Second-phase roar timer loads only on thinker expiry");

            body.Health = 1;
            body.VariableB = 0x1000;
            guard.MutableTilemap = (ushort)raw;
            AssertTrue(growth(body, state), "Growth threshold admits timer setup");
            KraidHeadResumeDefinition expected =
                KraidHeadInstructionDefinitions.GrowthResume((ushort)raw);
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

    private sealed class KraidHeadProgramReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public ushort MutableTilemap { get; set; }
        public int ForbiddenReadAttempts { get; private set; }
        public int UntypedLowHalfReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

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

        public byte ReadWorkRamByte(int address) => address switch
        {
            0xa71002 => (byte)MutableTilemap,
            0xa71003 => (byte)(MutableTilemap >> 8),
            _ => (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose WRAM.")).ReadWorkRamByte(address),
        };

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Kraid head guard source does not expose SRAM.")).ReadSaveRamByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
