using SuperMetroid.AssetExtraction.Samus;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.VerificationImporters;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks Samus's typed running cadences and the Speed Booster stage selection against the cartridge's (#1473).</summary>
internal static class RunningCadenceVerification
{
    /// <summary>The delay the getaway's wings leave for the unauthored cadence's first frame in this check; later frames take one tick more each.</summary>
    private const byte ResidueDelay = 0x35;

    /// <summary>
    /// Checks every authored cadence's delays and restart countdowns against the cartridge's, the unauthored cadence's
    /// residue reads, and the production stage interceptor over every counter it can hold.
    /// </summary>
    internal static void VerifyRunningCadence(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertSequenceEqual(SamusRunningCadenceCodec.OrdinaryDelays(cartridge), SamusRunningCadenceDefinitions.OrdinaryDelays, "ordinary running delays");
        IReadOnlyList<IReadOnlyList<int>> stages = SamusRunningCadenceDefinitions.BoostStageDelays;
        for (int stage = 0; stage < stages.Count; stage++)
            AssertSequenceEqual(SamusRunningCadenceCodec.BoostDelays(cartridge, stage), stages[stage], $"boost stage {stage} delays");
        for (int selection = 0; selection <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; selection++)
            AssertEqual(SamusRunningCadenceCodec.CountdownAfterAdvance(cartridge, selection), SamusRunningCadences.CountdownAfterAdvance(selection),
                $"restart countdown after selection {selection}");
        AssertThrows<InvalidDataException>(() => SamusRunningCadences.CountdownAfterAdvance(SamusRunningCadenceDefinitions.MaximumSoundQueueSelection + 1),
            "no selection lies past what the echo's queue request reports");

        // The unauthored cadence times each running frame by the delays the getaway's wings last left behind.
        var residue = new UnauthoredBoostCadence();
        residue.TakeDelaysLeftByWings([.. Enumerable.Range(0, UnauthoredBoostCadence.FrameCount).Select(frame => ResidueDelay + frame)]);
        for (int frame = 0; frame < UnauthoredBoostCadence.FrameCount; frame++)
            AssertEqual(ResidueDelay + frame, SamusRunningCadences.Delay(residue, SamusRunningCadence.UnauthoredBoost, frame),
                $"unauthored cadence frame {frame} takes the delay the wings left");
        AssertThrows<ArgumentOutOfRangeException>(() => SamusRunningCadences.Delay(residue, SamusRunningCadence.Ordinary, SamusRunningCadenceCodec.FrameCount),
            "a running frame past the tenth has no delay");

        var speed = new SamusHorizontalSpeedState();
        for (int stage = 0; stage <= SamusSpeedBoostCounter.FullStage; stage++)
        {
            speed.SpeedBoostCounter = new SamusSpeedBoostCounter(stage, 0);
            for (int frame = 0; frame < SamusRunningCadenceCodec.FrameCount; frame++)
                AssertEqual(stages[stage][frame], speed.SpeedBoosterDelay(residue, frame), "the counter's stage selects its cadence");
        }

        if (definitionsOnly) return;

        // Exercise every stored counter through the real command interceptor. A sound call reporting occupancy five
        // selects the unauthored cadence.
        foreach (bool momentum in new[] { false, true })
        foreach (bool running in new[] { false, true })
        foreach (bool dash in new[] { false, true })
        for (int stage = 0; stage <= 4; stage++)
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            int raw = LittleEndianBytes.Word(unchecked((byte)low), unchecked((byte)stage));
            bool admitted = momentum && running && dash;
            ushort decremented = unchecked((ushort)(raw - 1));
            bool intercept = admitted && (byte)decremented == 0;
            ushort next = admitted ? decremented : (ushort)raw;
            bool sound = intercept && (next & 0x0400) == 0 && ((next + 0x0100) & 0x0400) != 0;
            if (intercept && (next & 0x0400) == 0) next = unchecked((ushort)(next + 0x0100));
            int selection = sound ? SamusRunningCadenceCodec.UnauthoredSelection : LittleEndianBytes.HighByte(next);
            // Only an intercepted advance starts a cadence; without one the timer is cleared and no stream is read.
            int firstDelay = !intercept ? 0
                : selection == SamusRunningCadenceCodec.UnauthoredSelection ? ResidueDelay
                : SamusRunningCadenceCodec.BoostDelays(cartridge, selection)[0];
            ushort expectedTimer = intercept ? unchecked((ushort)(raw + firstDelay)) : (ushort)0;
            if (intercept)
                next = (ushort)(LittleEndianBytes.HighByteInPlace(next) | SamusRunningCadenceCodec.CountdownAfterAdvance(cartridge, selection));
            int calls = 0;
            speed.HasRunningMomentum = momentum; speed.SpeedBoostCounter = new SamusSpeedBoostCounter(stage, low);
            speed.EchoSoundRequested = false; speed.ContactDamageIndex = SamusContactDamageIndex.None;
            int frame = 7;
            if (admitted && raw == 0)
            {
                // The cartridge would wrap the whole word to stage 255; the port's stage-zero counter has no
                // stage to borrow from, so the countdown fails instead.
                AssertThrows<InvalidOperationException>(() => speed.TryAdvanceSpeedBoosterAnimationStage(residue,
                    SamusMovementType.Running, ControllerButton.B, 0, ref frame, out _, () => SoundQueueAdmission.Admitted(5)),
                    "A stage-zero counter with no cadence cannot borrow a stage");
                continue;
            }
            bool actual = speed.TryAdvanceSpeedBoosterAnimationStage(residue,
                running ? SamusMovementType.Running : SamusMovementType.Standing,
                dash ? ControllerButton.B : 0, raw, ref frame, out int timer,
                () => { calls++; return SoundQueueAdmission.Admitted(5); });
            AssertEqual(intercept, actual, "Native boost interception gates and low-byte countdown");
            AssertEqual(SamusSnapshotWords.DecodeSpeedBoostCounter(next), speed.SpeedBoostCounter, "Native counter decrement, stage advance and cadence reset");
            AssertEqual(intercept ? 0 : 7, frame, "Only intercepted command resets frame");
            AssertEqual(expectedTimer, timer, "Selected first-frame delay and wrapping frame buffer");
            AssertEqual(sound ? 1 : 0, calls, "Echo call only on bit-two stage entry");
            AssertTrue(!speed.EchoSoundRequested, "Synchronous queue path does not also defer sound");
            AssertEqual(SamusContactDamageIndex.None, speed.ContactDamageIndex, "Animation cannot publish movement-owned boost contact damage");
        }

        // The queue's report, not the already-published stage, selects both the restart countdown and the first delay.
        for (int selection = 0; selection <= SamusRunningCadenceDefinitions.MaximumSoundQueueSelection; selection++)
        for (int low = 0; low <= byte.MaxValue; low++)
        {
            int buffer = LittleEndianBytes.Word(unchecked((byte)low), unchecked((byte)selection));
            speed.HasRunningMomentum = true; speed.SpeedBoostCounter = new SamusSpeedBoostCounter(3, 1);
            int frame = 9;
            speed.TryAdvanceSpeedBoosterAnimationStage(residue, SamusMovementType.Running, ControllerButton.B,
                buffer, ref frame, out int timer, () => SoundQueueAdmission.Admitted(selection));
            AssertEqual(new SamusSpeedBoostCounter(SamusSpeedBoostCounter.FullStage, SamusRunningCadenceCodec.CountdownAfterAdvance(cartridge, selection)),
                speed.SpeedBoostCounter, "All reported selections restart the countdown the cartridge's restart word holds");
            int expectedDelay = selection == SamusRunningCadenceCodec.UnauthoredSelection
                ? ResidueDelay
                : SamusRunningCadenceCodec.BoostDelays(cartridge, selection)[0];
            AssertEqual(unchecked((ushort)(buffer + expectedDelay)), timer, "Queue-selected first delay, including the residue's");
        }

        speed.HasRunningMomentum = true;
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(3, 1);
        int invalidFrame = 9;
        AssertThrows<InvalidDataException>(
            () => speed.TryAdvanceSpeedBoosterAnimationStage(
                residue,
                SamusMovementType.Running,
                ControllerButton.B,
                0,
                ref invalidFrame,
                out _,
                () => SoundQueueAdmission.Admitted(6)),
            "An occupancy the queue cannot admit selects no cadence");

        speed = new SamusHorizontalSpeedState();
        speed.HandleExtraRunSpeed(SamusMovementType.Running, ControllerButton.B, true);
        AssertEqual(new SamusSpeedBoostCounter(0, 1), speed.SpeedBoostCounter, "First running frame seeds native countdown");
        AssertEqual(0x1000, speed.ExtraRunSpeed.Fraction, "Cadence migration leaves run acceleration intact");
        speed.SpeedBoostCounter = new SamusSpeedBoostCounter(0, 0); speed.SpecialPaletteTimer = WrappingCountdown.Start(8); speed.CyclePaletteStep = 3;
        speed.ReconcilePauseSpeedBoosterState(true);
        AssertEqual(new SamusSpeedBoostCounter(0, 1), speed.SpeedBoostCounter, "Pause equipment reconciliation seeds native countdown");
        AssertEqual(0, speed.SpecialPaletteTimer.RawValue, "Pause reconciliation palette timer differs from initial run");
        AssertEqual(0, speed.CyclePaletteStep, "Pause reconciliation palette frame reset");
        Console.WriteLine("Running cadence: six typed cadences and six restarts match the cartridge, the unauthored cadence reads the residue, and 10240 command/gate cases and 1536 queue reports pass.");
    }
}
