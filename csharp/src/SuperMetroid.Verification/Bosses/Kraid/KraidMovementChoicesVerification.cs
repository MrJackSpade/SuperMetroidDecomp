using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks Kraid's second-phase target and think-timer selection against native tables.</summary>
internal static class KraidMovementChoicesVerification
{
    /// <summary>Compares the selected destination X for every RNG word and position row.</summary>
    private static void VerifyKraidMovementTargets(ushort[] positions,
        Func<ushort, ushort, KraidMovementChoice> expected) =>
        Suite(nameof(VerifyKraidMovementField), () => VerifyKraidMovementField(positions, expected, value => value.TargetX));

    /// <summary>Compares the selected think delay for every RNG word and position row.</summary>
    private static void VerifyKraidMovementTimers(ushort[] positions,
        Func<ushort, ushort, KraidMovementChoice> expected) =>
        Suite(nameof(VerifyKraidMovementField), () => VerifyKraidMovementField(positions, expected, value => value.ThinkTimer));

    /// <summary>Exercises all weighted choices, including the fallback position row.</summary>
    private static void VerifyKraidMovementField(ushort[] positions,
        Func<ushort, ushort, KraidMovementChoice> expected,
        Func<KraidMovementChoice, int> field)
    {
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            foreach (ushort position in positions)
                AssertEqual(field(expected(position, (ushort)raw)),
                    field(KraidMovementChoices.Select(position, (ushort)raw)),
                    "Every RNG word in each native movement row");
            for (int choice = 0; choice < 8; choice++)
            {
                ushort random = (ushort)(choice * 4);
                AssertEqual(field(expected((ushort)raw, random)),
                    field(KraidMovementChoices.Select((ushort)raw, random)),
                    "Every position and weighted choice, including fallback");
            }
        }
    }

    /// <summary>Compares native records and optionally follows the production thinking transition without advancing RNG.</summary>
    internal static void VerifyKraidMovementChoices(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        ushort Word(int a) => SnesLittleEndian.Word(KraidMovementChoicesRomByte(rom, a), KraidMovementChoicesRomByte(rom, a + 1));
        var positions = new ushort[6];
        var targets = new ushort[6, 5];
        var timers = new ushort[6, 5];
        for (int row = 0; row < 6; row++)
        {
            int record = EnemyRomTablePointers.Kraid.SecondPhaseMovementRecords + row * 4;
            positions[row] = Word(record);
            int pointer = 0xa70000 | Word(record + 2);
            for (int choice = 0; choice < 5; choice++)
            {
                targets[row, choice] = Word(pointer + choice * 4);
                timers[row, choice] = Word(pointer + choice * 4 + 2);
            }
        }
        KraidMovementChoice Expected(ushort x, ushort random)
        {
            int row = Array.IndexOf(positions, x);
            if (row < 0) row = 1;
            int choice = Math.Min((random & 28) / 4, 4);
            return new(targets[row, choice], timers[row, choice]);
        }
        Suite(nameof(VerifyKraidMovementTargets), () => VerifyKraidMovementTargets(positions, Expected));
        Suite(nameof(VerifyKraidMovementTimers), () => VerifyKraidMovementTimers(positions, Expected));
        if (definitionsOnly)
        {
            Console.WriteLine("Kraid movement selectors: both fields, every RNG word per row and every position/choice pass native-data proofs.");
            return;
        }
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState();
        var body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        var foot = enemies.SlotTable.Slots[5];
        var part = new KraidPartState();
        enemies.KraidBehavior.Access.Kraid = state;
        ushort rng = 0;
        int reads = 0;
        enemies.VerificationServices().ReadRandom = () => { reads++; return rng; };
        enemies.VerificationServices().NextRandom = () => throw new InvalidOperationException("Movement choice must not advance RNG.");
        Action<RoomEnemySlot, RoomEnemySlot, KraidPartState> think = enemies.KraidBehavior.Limbs.RunKraidSecondPhaseThinking;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            body.Position = body.Position.WithWholeX((ushort)raw);
            for (int choice = 0; choice < 8; choice++)
            {
                rng = (ushort)(choice * 4);
                reads = 0;
                part.ThinkTimer = WrappingCountdown.Start(1);
                foot.InstructionTimer = WrappingCountdown.Start(99);
                var expected = Expected(body.Position.X.Whole, rng);
                think(body, foot, part);
                bool right = unchecked((short)(expected.TargetX - raw)) >= 0;
                AssertEqual(expected.TargetX, state.WalkTarget.X, "Actual target includes fallback row");
                AssertEqual(expected.ThinkTimer, part.ThinkTimer.RawValue, "Actual thinking delay includes weighted final choice");
                AssertEqual((right ? KraidAiFunction.FootSecondPhaseWalkingRight : KraidAiFunction.FootSecondPhaseWalkingLeft), state.Parts[5].Function, "Native signed walking direction");
                AssertEqual(
                    AnimationProgramCursor<KraidPartProgram>.Start(right ? KraidPartProgram.FootWalkingBackward : KraidPartProgram.FootWalkingForward),
                    state.Parts[foot.SlotIndex].Cursor,
                    "Chosen direction starts matching animation");
                AssertEqual((ushort)1, foot.InstructionTimer.RawValue, "Choice restarts animation timer");
                AssertEqual(1, reads, "Choice reads current RNG once");
            }
            part.ThinkTimer = WrappingCountdown.FromRaw((ushort)raw);
            reads = 0;
            think(body, foot, part);
            if (raw != 1)
            {
                AssertEqual(unchecked((ushort)(raw - 1)), part.ThinkTimer.RawValue, "Only zero after decrement triggers choice");
                AssertEqual(0, reads, "Unexpired thinking does not read RNG");
            }
        }
        Console.WriteLine("Kraid choices: all indirect native pairs, every RNG word per row, 524288 actual coordinate/choice transitions and all timer words pass bus-free.");
    }

    /// <summary>Reads one native movement-table byte for the independent selection oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Kraid movement choices")]
    private static byte KraidMovementChoicesRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
