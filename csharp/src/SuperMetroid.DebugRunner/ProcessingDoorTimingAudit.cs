using System.Globalization;
using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Measures #422's action and liquid backlogs at the cartridge's actual unread-ring door
/// boundary. Host time is deliberately absent: every result is an emulated frame count.
/// </summary>
internal static class ProcessingDoorTimingAudit
{
    private const int DispatcherCycleFrames = 6;
    private const int MeasurementFrameLimit = 128;
    private const int LiquidProductionFrameLimit = 1024;
    private const byte ControlledAcknowledgementLag = 1;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    public static int Run(string rom, string nativeActionCsv)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        IReadOnlyDictionary<string, SoundEffectId[]> actions = ReadNativeActions(nativeActionCsv);

        var results = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach ((string action, SoundEffectId[] requests) in actions)
        {
            int delay = MeasureMaximumAddedDoorFrames(bus, requests);
            int expected = ExpectedMaximumAddedFrames(requests);
            if (delay != expected)
            {
                throw new InvalidDataException(
                    $"Processing action '{action}' added {delay} door frames; " +
                    $"the original-CPU request snapshot and six-frame dispatcher imply {expected}.");
            }
            results.Add(action, delay);
            Console.WriteLine($"TIMING action={action} requests={Format(requests)} max-added={delay}");
        }

        VerifyLandingActions(bus);
        VerifyCombinedActions(bus, actions);
        VerifyInterruptedCharge(bus);
        VerifyRisingLiquidBacklog(bus);

        Console.WriteLine(
            $"Processing door timing: {results.Count} native action snapshots, compound actions, " +
            "and the retail rising-lava backlog passed at the real unread-ring transition boundary.");
        return 0;
    }

    private static Dictionary<string, SoundEffectId[]> ReadNativeActions(string csv)
    {
        string[] lines = File.ReadAllLines(csv);
        const string header = "action,power_bomb,lib1_count,lib1,lib2_count,lib2,lib3_count,lib3";
        if (lines.Length != 23 || lines[0] != header)
            throw new InvalidDataException("Unexpected #422 original-CPU action trace shape.");

        var actions = new Dictionary<string, SoundEffectId[]>(StringComparer.Ordinal);
        foreach (string line in lines.Skip(1))
        {
            string[] fields = line.Split(',');
            if (fields.Length != 8)
                throw new InvalidDataException($"Malformed #422 native action row: {line}");
            if (fields[1] == "1")
            {
                if (fields[2] != "0" || fields[4] != "0" || fields[6] != "0")
                    throw new InvalidDataException($"Active-Power-Bomb control retained SFX: {line}");
                continue;
            }

            var requests = new List<SoundEffectId>();
            AppendLibrary(requests, SoundEffectLibrary.Library1, fields[2], fields[3]);
            AppendLibrary(requests, SoundEffectLibrary.Library2, fields[4], fields[5]);
            AppendLibrary(requests, SoundEffectLibrary.Library3, fields[6], fields[7]);
            actions.Add(fields[0], requests.ToArray());
        }
        if (actions.Count != ProcessingActionDefinitions.OrderedCases.Count)
            throw new InvalidDataException("Native action trace omitted an inactive-Power-Bomb case.");
        return actions;
    }

    private static void AppendLibrary(
        List<SoundEffectId> target,
        SoundEffectLibrary library,
        string countField,
        string valuesField)
    {
        int count = int.Parse(countField, CultureInfo.InvariantCulture);
        byte[] values = valuesField.Length == 0 ? [] : Convert.FromHexString(valuesField);
        if (values.Length != count)
            throw new InvalidDataException($"Native action trace count {count} does not match '{valuesField}'.");
        foreach (byte value in values)
            target.Add(SoundEffectId.FromCartridge(library, value));
    }

    private static int MeasureMaximumAddedDoorFrames(
        SuperMetroidAddressSpace bus,
        IReadOnlyList<SoundEffectId> actionRequests)
    {
        int maximum = 0;
        for (int phase = 0; phase < DispatcherCycleFrames; phase++)
        {
            int baseline = MeasureDoorFrames(bus, phase, []);
            int action = MeasureDoorFrames(bus, phase, actionRequests);
            maximum = Math.Max(maximum, action - baseline);
        }
        return maximum;
    }

    private static int MeasureDoorFrames(
        SuperMetroidAddressSpace bus,
        int dispatcherPhase,
        IReadOnlyList<SoundEffectId> actionRequests)
    {
        var audio = new CartridgeAudioState();
        audio.AdvanceFrame(bus, default);
        var acknowledgements = new LaggedAcknowledgements(ControlledAcknowledgementLag);

        // One in-flight request per library supplies every reachable relative alignment.
        // The action then precedes the two unconditional door-entry stop calls, exactly as
        // $84:8250 -> $90:F471 and $82:E26C-$E279 order them on the cartridge.
        audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
        audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
        audio.QueueSound(SoundEffectLibrary3Sounds.CancelAll, maximumQueued: 15);
        acknowledgements.Advance(bus, audio);
        for (int frame = 0; frame < dispatcherPhase; frame++)
            acknowledgements.Advance(bus, audio);

        foreach (SoundEffectId request in actionRequests)
            audio.QueueSound(request, maximumQueued: 15);
        QueueDoorEntryStops(audio);
        SetDoorSoundDisable(audio, disabled: true);

        SuperMetroidRuntime runtime = CreateSoundWaitRuntime(bus);
        DoorTransitionState transition = CreateSoundWaitTransition();
        for (int frame = 1; frame <= MeasurementFrameLimit; frame++)
        {
            transition.Step(runtime, audio, controllerInput: 0);
            acknowledgements.Advance(bus, audio);
            if (transition.Phase == DoorTransitionPhase.FadeOutSourcePalette)
                return frame;
        }
        throw new InvalidDataException("Door sound queue did not drain inside the measurement bound.");
    }

    private static int ExpectedMaximumAddedFrames(IReadOnlyList<SoundEffectId> requests)
    {
        int maximumSameLibraryCount = requests
            .GroupBy(request => request.Library)
            .Select(group => group.Count())
            .DefaultIfEmpty(0)
            .Max();
        return maximumSameLibraryCount * DispatcherCycleFrames;
    }

    private static void VerifyCombinedActions(
        SuperMetroidAddressSpace bus,
        IReadOnlyDictionary<string, SoundEffectId[]> actions)
    {
        VerifyCombination(bus, actions, "hud-plus-bomb", expectedFrames: 6,
            ProcessingActionDefinitions.HudSelect,
            ProcessingActionDefinitions.BombExplosion);
        VerifyCombination(bus, actions, "fire-plus-break-spin", expectedFrames: 12,
            ProcessingActionDefinitions.FirePowerBeam,
            ProcessingActionDefinitions.BreakSpin);
        VerifyCombination(bus, actions, "break-spin-charging-plus-bomb", expectedFrames: 12,
            ProcessingActionDefinitions.BreakSpinCharging,
            ProcessingActionDefinitions.BombExplosion);
        VerifyRequestSet(bus, "shinespark-launch-and-door-crash", expectedFrames: 6,
            ShinesparkSounds.Launch,
            ShinesparkSounds.CrashImpact,
            ShinesparkSounds.CrashEcho);
    }

    private static void VerifyCombination(
        SuperMetroidAddressSpace bus,
        IReadOnlyDictionary<string, SoundEffectId[]> actions,
        string name,
        int expectedFrames,
        params string[] actionNames)
    {
        SoundEffectId[] requests = actionNames.SelectMany(name => actions[name]).ToArray();
        int actual = MeasureMaximumAddedDoorFrames(bus, requests);
        if (actual != expectedFrames || actual != ExpectedMaximumAddedFrames(requests))
        {
            throw new InvalidDataException(
                $"Combined action '{name}' added {actual} frames, expected {expectedFrames}.");
        }
        Console.WriteLine($"TIMING combined={name} requests={Format(requests)} max-added={actual}");
    }

    private static void VerifyLandingActions(SuperMetroidAddressSpace bus)
    {
        VerifyLanding(bus, "spin-hard", SamusMovementType.SpinJumping,
            SamusPoseIds.SpinJumpRightPose, impactYSpeed: 5, impactYSubspeed: 0,
            SoundEffectLibrary1Sounds.StopSpinJump,
            SoundEffectLibrary3Sounds.HardLanding);
        VerifyLanding(bus, "wall-soft", SamusMovementType.WallJumping,
            SamusPoseIds.WallJumpLeftPose, impactYSpeed: 0, impactYSubspeed: 1,
            SoundEffectLibrary1Sounds.StopSpinJump,
            SoundEffectLibrary3Sounds.SoftLanding);
        VerifyLanding(bus, "screw-hard", SamusMovementType.SpinJumping,
            SamusPoseIds.ScrewAttackRightPose, impactYSpeed: 5, impactYSubspeed: 0,
            SoundEffectLibrary1Sounds.StopScrewAttack,
            SoundEffectLibrary3Sounds.HardLanding);
        VerifyLanding(bus, "stationary", SamusMovementType.Standing,
            SamusPoseIds.FacingRightNormalPose, impactYSpeed: 0, impactYSubspeed: 0);
    }

    private static void VerifyLanding(
        SuperMetroidAddressSpace bus,
        string name,
        SamusMovementType previousMovement,
        byte previousPose,
        ushort impactYSpeed,
        ushort impactYSubspeed,
        params SoundEffectId[] expected)
    {
        SuperMetroidRuntime runtime = FlatFloorMovementFixture.Create(bus, false);
        SamusState samus = runtime.Samus!;
        samus.LiquidPhysics.BeginFrameSoundRequests(runtime.BombProjectiles.PowerBombExplosion);
        samus.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
            bus, samus, previousMovement, previousPose, impactYSpeed, impactYSubspeed);
        SoundEffectId[] actual = samus.LiquidPhysics.SoundRequests
            .Where(request => !request.SoundSuppressed)
            .Select(request => request.SoundEffect)
            .ToArray();
        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidDataException(
                $"Landing '{name}' published {Format(actual)}, expected {Format(expected)}.");
        }
        int delay = MeasureMaximumAddedDoorFrames(bus, actual);
        int expectedDelay = ExpectedMaximumAddedFrames(expected);
        if (delay != expectedDelay)
            throw new InvalidDataException($"Landing '{name}' added {delay} frames, expected {expectedDelay}.");
        Console.WriteLine($"TIMING landing={name} requests={Format(actual)} max-added={delay}");
    }

    private static void VerifyInterruptedCharge(SuperMetroidAddressSpace bus)
    {
        SuperMetroidRuntime runtime = FlatFloorMovementFixture.Create(bus, false);
        SamusState samus = runtime.Samus!;
        samus.EquippedBeams = (ushort)SamusBeamFlags.Charge;
        samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs).ToNativeWord();
        samus.Pose = SamusPoseIds.FacingRightNormalPose;

        var produced = new List<SoundEffectId>();
        for (int frame = 0; frame < SamusProjectileRomData.Beams.ChargeSoundStartCounter; frame++)
        {
            ushort edge = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            SamusProjectileFrameResult projectile = runtime.Projectiles.StepFrame(
                bus,
                runtime.LevelData!,
                samus,
                (ushort)SnesButton.X,
                edge,
                runtime.Camera!.XPosition,
                runtime.Camera.YPosition,
                runtime.BombProjectiles,
                roomPlms: runtime.Plms);
            samus.ProjectileFlareCounter = runtime.Projectiles.FlareCounter;
            Append(projectile, produced);
        }
        if (runtime.Projectiles.FlareCounter != SamusProjectileRomData.Beams.ChargeSoundStartCounter ||
            produced.LastOrDefault() != SoundEffectLibrary1Sounds.ChargeBeamStart)
        {
            throw new InvalidDataException(
                "Sixteen-frame charge fixture did not reach the cartridge charge-sound boundary.");
        }

        samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
        samus.RefreshCollisionRadii(bus);
        BombProjectileFrameResult cancelled = runtime.BombProjectiles.StepFrame(
            bus, runtime.LevelData!, samus, controllerInput: 0, controllerNewInput: 0);
        if (cancelled.BeamChargeConsumed)
        {
            runtime.Projectiles.CancelChargeForBombSpread();
            samus.ProjectileFlareCounter = 0;
        }
        foreach (SamusSoundRequest request in cancelled.SoundRequests ?? [])
        {
            if (!request.SoundSuppressed)
                produced.Add(request.SoundEffect);
        }
        if (produced.LastOrDefault() != SoundEffectLibrary1Sounds.CancelAll ||
            runtime.Projectiles.FlareCounter != 0 ||
            samus.ProjectileFlareCounter != 0)
        {
            throw new InvalidDataException(
                "Morph interruption did not publish the cartridge cancellation after sixteen charge frames.");
        }

        // The initial power-beam shot has ample time to drain during the sixteen-frame
        // hold. At the immediate interruption/door boundary, the relevant backlog is the
        // just-started sustained charge followed by its cancellation.
        SoundEffectId[] backlog =
        [
            SoundEffectLibrary1Sounds.ChargeBeamStart,
            SoundEffectLibrary1Sounds.CancelAll,
        ];
        int delay = MeasureMaximumAddedDoorFrames(bus, backlog);
        if (delay != 12)
            throw new InvalidDataException($"Interrupted sixteen-frame charge added {delay} frames, expected 12.");
        Console.WriteLine(
            $"TIMING charge-interrupt=16f produced={Format(produced.ToArray())} " +
            $"boundary={Format(backlog)} max-added={delay}");
    }

    private static void Append(SamusProjectileFrameResult frame, List<SoundEffectId> target)
    {
        if (frame.QueuedSoundEffect is { } sound && !frame.QueuedSoundSuppressed)
            target.Add(sound);
        foreach (SamusSoundRequest request in frame.AdditionalSoundRequests ?? [])
        {
            if (!request.SoundSuppressed)
                target.Add(request.SoundEffect);
        }
    }

    private static void VerifyRequestSet(
        SuperMetroidAddressSpace bus,
        string name,
        int expectedFrames,
        params SoundEffectId[] requests)
    {
        int actual = MeasureMaximumAddedDoorFrames(bus, requests);
        if (actual != expectedFrames)
        {
            throw new InvalidDataException(
                $"Request set '{name}' added {actual} frames, expected {expectedFrames}.");
        }
        Console.WriteLine($"TIMING combined={name} requests={Format(requests)} max-added={actual}");
    }

    private static void VerifyRisingLiquidBacklog(SuperMetroidAddressSpace bus)
    {
        SuperMetroidRuntime runtime = FlatFloorMovementFixture.Create(bus, false);
        runtime.LoadCartridgeRoomThroughDoorForVerification(
            CartridgeDoorHeader.Load(bus, RoomEffectSoundAuditDefinitions.RisingLavaDoor));
        runtime.Samus!.InputLocked = true;
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            enemy.Clear();

        var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", PrivateInstance)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", PrivateInstance)!
            .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetField("lastAudioRuntimeGameplayPublication", PrivateInstance)!
            .SetValue(game, (ulong?)runtime.CompletedGameplayAudioPublication);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!
            .SetValue(game, SuperMetroidGameState.MainGameplay);
        var audio = (CartridgeAudioState)typeof(SuperMetroidGame)
            .GetField("audio", PrivateInstance)!.GetValue(game)!;
        audio.AdvanceFrame(bus, default);
        var acknowledgements = new LaggedAcknowledgements(ControlledAcknowledgementLag);

        int requests = 0;
        int peakOccupancy = 0;
        int productionFrames = 0;
        for (; productionFrames < LiquidProductionFrameLimit; productionFrames++)
        {
            game.SetAudioAcknowledgements(acknowledgements.Current);
            FrontendFrame frame = game.Step(0);
            acknowledgements.Observe(frame.AudioCommands);
            requests += runtime.RoomLayer3Fx.SoundRequests.Count;
            peakOccupancy = Math.Max(peakOccupancy, Occupancy(audio, SoundEffectLibrary.Library2));
            if (peakOccupancy == 6)
                break;
        }
        if (peakOccupancy != 6 || requests < 6)
        {
            throw new InvalidDataException(
                $"Retail rising lava produced {requests} quake sounds and peak occupancy " +
                $"{peakOccupancy}; expected the native Max6 backlog.");
        }

        // HitDoorBlock adds these before setting DisableSounds. The wait owner intentionally
        // does not run room FX, so no later liquid request can refill the draining queue.
        QueueDoorEntryStops(audio);
        SetDoorSoundDisable(audio, disabled: true);
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        DoorTransitionState transition = CreateSoundWaitTransition();
        int waitFrames = 0;
        while (transition.Phase == DoorTransitionPhase.WaitForSoundQueues &&
               waitFrames < MeasurementFrameLimit)
        {
            transition.Step(runtime, audio, controllerInput: 0);
            acknowledgements.Advance(bus, audio);
            waitFrames++;
        }
        if (transition.Phase != DoorTransitionPhase.FadeOutSourcePalette)
            throw new InvalidDataException("Retail rising-lava backlog did not reach the fade boundary.");
        if (waitFrames < 30 || waitFrames > 42)
        {
            throw new InvalidDataException(
                $"Retail rising-lava backlog delayed the transition {waitFrames} frames; " +
                "expected the documented approximately-thirty-frame Max6 range.");
        }
        Console.WriteLine(
            $"TIMING liquid-room=$02/$28 production-frames={productionFrames + 1} " +
            $"requests={requests} peak-lib2={peakOccupancy} wait-to-fade={waitFrames}");
    }

    private static void QueueDoorEntryStops(CartridgeAudioState audio)
    {
        audio.QueueSound(SoundEffectLibrary1Sounds.CancelAll, maximumQueued: 15);
        audio.QueueSound(SoundEffectLibrary2Sounds.CancelAll, maximumQueued: 15);
    }

    private static SuperMetroidRuntime CreateSoundWaitRuntime(SuperMetroidAddressSpace bus)
    {
        SuperMetroidRuntime runtime = FlatFloorMovementFixture.Create(bus, false);
        foreach (RoomEnemySlot enemy in runtime.Enemies.Slots)
            enemy.Clear();
        foreach (var projectile in runtime.Enemies.EnemyProjectiles)
            projectile.Clear();
        runtime.Plms.Reset();
        runtime.Samus!.InputLocked = true;
        runtime.Enemies.ElevatorDoorTransitionActive = true;
        return runtime;
    }

    private static DoorTransitionState CreateSoundWaitTransition()
    {
        var transition = new DoorTransitionState();
        typeof(DoorTransitionState).GetProperty(nameof(transition.Phase))!
            .SetValue(transition, DoorTransitionPhase.WaitForSoundQueues);
        return transition;
    }

    private static void SetDoorSoundDisable(CartridgeAudioState audio, bool disabled) =>
        typeof(CartridgeAudioState).GetProperty("DoorTransitionSoundsDisabled", PrivateInstance)!
            .SetValue(audio, disabled);

    private static int Occupancy(CartridgeAudioState audio, SoundEffectLibrary library)
    {
        int queue = SoundEffectLibraries.ToQueueIndex(library);
        byte[] reads = (byte[])typeof(CartridgeAudioState)
            .GetField("_soundReadPositions", PrivateInstance)!.GetValue(audio)!;
        byte[] writes = (byte[])typeof(CartridgeAudioState)
            .GetField("_soundWritePositions", PrivateInstance)!.GetValue(audio)!;
        return (writes[queue] - reads[queue]) & AudioRomData.Queues.SoundIndexMask;
    }

    private static string Format(SoundEffectId[] requests) =>
        requests.Length == 0
            ? "none"
            : string.Join('+', requests.Select(request =>
                $"L{(byte)request.Library}:{request.Value:X2}"));

    /// <summary>Controlled SPC echo model shared by the native queue comparisons.</summary>
    private sealed class LaggedAcknowledgements
    {
        private readonly int lag;
        private readonly List<byte[]> history = [];
        private readonly byte[] ports = new byte[4];

        public LaggedAcknowledgements(int lag) => this.lag = lag;

        public CartridgeAudioAcknowledgements Current
        {
            get
            {
                byte[] source = history.Count > lag ? history[history.Count - lag - 1] : new byte[4];
                return new(source[0], source[1], source[2], source[3]);
            }
        }

        public void Advance(ISnesAddressSpace bus, CartridgeAudioState audio) =>
            Observe(audio.AdvanceFrame(bus, Current));

        public void Observe(IReadOnlyList<CartridgeAudioCommand> commands)
        {
            foreach (CartridgeAudioCommand command in commands)
            {
                if (command.Kind == CartridgeAudioCommandKind.WritePort)
                    ports[command.Port] = command.Value;
            }
            history.Add((byte[])ports.Clone());
        }
    }
}
