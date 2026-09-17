using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Compares the sound-queue producers named by the Processing technique against
/// original-CPU execution of their cartridge routines.
/// </summary>
internal static class ProcessingActionAudit
{
    public static int Run(string rom, string nativeCsv)
    {
        string[] expected = File.ReadAllLines(nativeCsv);
        if (expected.Length != 23 ||
            expected[0] != "action,power_bomb,lib1_count,lib1,lib2_count,lib2,lib3_count,lib3")
        {
            throw new InvalidDataException("Unexpected #422 original-CPU action trace shape.");
        }

        var rows = new List<string>();
        foreach (string action in ProcessingActionDefinitions.OrderedCases)
        foreach (bool powerBombActive in new[] { false, true })
        {
            string row = RunCase(rom, action, powerBombActive);
            rows.Add(row);
            string native = expected[rows.Count];
            if (row != native)
            {
                throw new InvalidDataException(
                    $"Processing action mismatch at row {rows.Count}: managed={row}; original={native}.");
            }
        }

        Console.WriteLine(
            $"Processing actions: all {rows.Count} original-CPU queue snapshots match, including active-Power-Bomb controls.");
        return 0;
    }

    private static string RunCase(string rom, string action, bool powerBombActive)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var runtime = FlatFloorMovementFixture.Create(bus, false);
        SamusState samus = runtime.Samus!;
        SamusPowerBombExplosionState explosion = runtime.BombProjectiles.PowerBombExplosion;
        if (powerBombActive)
        {
            explosion.Arm();
            explosion.Spawn(samus.XPosition, samus.YPosition);
        }

        var sounds = new List<SoundEffectId>();
        void Add(SamusSoundRequest request)
        {
            if (!request.SoundSuppressed)
                sounds.Add(request.SoundEffect);
        }
        void AddMovementSounds()
        {
            foreach (SamusSoundRequest request in samus.LiquidPhysics.SoundRequests)
                Add(request);
        }

        samus.LiquidPhysics.BeginFrameSoundRequests(explosion);
        switch (action)
        {
            case ProcessingActionDefinitions.HudSelect:
                samus.Missiles = samus.MaxMissiles = 5;
                samus.SelectedHudItem = 1;
                runtime.Hud.UpdateGameplayCounters(
                    bus, samus, soundSuppressed: explosion.IsActive);
                if (runtime.Hud.SelectionSoundRequestedThisFrame &&
                    !runtime.Hud.SelectionSoundSuppressedThisFrame)
                    sounds.Add(SoundEffectLibrary1Sounds.HudWeaponSelect);
                break;

            case ProcessingActionDefinitions.SpinStart:
                samus.Pose = SamusPoseIds.MovingRightNormalPose;
                samus.EquippedItems = 0x0020;
                samus.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpRightPose);
                AddMovementSounds();
                break;

            case ProcessingActionDefinitions.SpaceJumpCheck:
                samus.Pose = SamusPoseIds.SpinJumpRightPose;
                samus.EquippedItems = 0x0220;
                samus.ApplySpinJumpDirectionTransition(bus, SamusPoseIds.SpinJumpLeftPose);
                AddMovementSounds();
                break;

            case ProcessingActionDefinitions.ScrewAttackControl:
                samus.Pose = SamusPoseIds.SpinJumpRightPose;
                samus.EquippedItems = 0x0228;
                samus.ApplySpinJumpDirectionTransition(bus, SamusPoseIds.SpinJumpLeftPose);
                AddMovementSounds();
                break;

            case ProcessingActionDefinitions.BreakSpin:
                samus.Pose = SamusPoseIds.FacingRightNormalPose;
                SamusPostDrawAudio.Step(bus, samus, SamusMovementType.SpinJumping, 0);
                AddMovementSounds();
                break;

            case ProcessingActionDefinitions.BreakSpinCharging:
                samus.Pose = SamusPoseIds.FacingRightNormalPose;
                samus.ProjectileFlareCounter =
                    SamusProjectileRomData.Beams.ChargeSoundStartCounter;
                SamusPostDrawAudio.Step(
                    bus, samus, SamusMovementType.SpinJumping, (ushort)SnesButton.X);
                SamusPostDrawAudio.Step(
                    bus, samus, SamusMovementType.NormalJumping, (ushort)SnesButton.X);
                AddMovementSounds();
                break;

            case ProcessingActionDefinitions.FirePowerBeam:
                AddProjectileSound(FireBeam(runtime, bus, SamusBeamFlags.None, holdFrames: 1), sounds);
                break;

            case ProcessingActionDefinitions.FireWaveBeam:
                AddProjectileSound(FireBeam(runtime, bus, SamusBeamFlags.Wave, holdFrames: 1), sounds);
                break;

            case ProcessingActionDefinitions.ReleaseCharge:
                AddProjectileSound(FireBeam(
                    runtime, bus, SamusBeamFlags.Charge,
                    SamusProjectileRomData.Beams.FullyChargedCounter,
                    release: true), sounds);
                break;

            case ProcessingActionDefinitions.BombExplosion:
                samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
                samus.EquippedItems =
                    (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs).ToNativeWord();
                samus.RefreshCollisionRadii(bus);
                samus.InitializeAnimation(bus);
                BombProjectileFrameResult planted = runtime.BombProjectiles.StepFrame(
                    bus, runtime.LevelData!, samus,
                    (ushort)SnesButton.X, (ushort)SnesButton.X);
                if (planted.PlacedSlot is not int bombIndex)
                    throw new InvalidDataException("Processing bomb fixture did not place a bomb.");
                runtime.BombProjectiles.Slots[bombIndex].BombTimer = 1;
                BombProjectileFrameResult expired = runtime.BombProjectiles.StepFrame(
                    bus, runtime.LevelData!, samus, 0, 0);
                foreach (SamusSoundRequest request in expired.SoundRequests ?? [])
                    Add(request);
                break;

            case ProcessingActionDefinitions.ShinesparkActivation:
                FireBeam(runtime, bus, SamusBeamFlags.Charge,
                    SamusProjectileRomData.Beams.ChargeSoundStartCounter);
                if (!samus.Shinespark.TryStoreFromSpeedBooster(
                        SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
                    throw new InvalidDataException("Processing fixture could not store shine.");
                samus.Shinespark.BindProjectileOwners(runtime.Projectiles, explosion);
                samus.Shinespark.BeginWindup(samus);
                if (samus.Shinespark.ConsumeChargeCancellationSoundRequest() &&
                    !samus.Shinespark.ChargeCancellationSoundSuppressed)
                    sounds.Add(SoundEffectLibrary1Sounds.CancelAll);
                samus.Shinespark.BeginDirectionalLaunch(
                    bus, samus, SamusPoseIds.ShinesparkVerticalRightPose);
                if (samus.Shinespark.ConsumeLaunchSoundRequest() &&
                    !samus.Shinespark.LaunchSoundSuppressed)
                    sounds.Add(ShinesparkSounds.Launch);
                break;

            default:
                throw new InvalidDataException($"Unknown Processing action '{action}'.");
        }

        return Format(action, powerBombActive, sounds);
    }

    private static SamusProjectileFrameResult FireBeam(
        SuperMetroidRuntime runtime,
        SuperMetroidAddressSpace bus,
        SamusBeamFlags beams,
        int holdFrames,
        bool release = false)
    {
        SamusState samus = runtime.Samus!;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.EquippedBeams = (ushort)beams;
        SamusProjectileFrameResult result = default;
        for (int frame = 0; frame < holdFrames; frame++)
        {
            ushort edge = frame == 0 ? (ushort)SnesButton.X : (ushort)0;
            runtime.BombProjectiles.StepFrame(
                bus, runtime.LevelData!, samus, (ushort)SnesButton.X, edge);
            result = runtime.Projectiles.StepFrame(
                bus, runtime.LevelData!, samus,
                (ushort)SnesButton.X, edge,
                runtime.Camera!.XPosition, runtime.Camera.YPosition,
                runtime.BombProjectiles,
                roomPlms: runtime.Plms);
            samus.ProjectileFlareCounter = runtime.Projectiles.FlareCounter;
        }
        if (!release)
            return result;

        runtime.BombProjectiles.StepFrame(bus, runtime.LevelData!, samus, 0, 0);
        result = runtime.Projectiles.StepFrame(
            bus, runtime.LevelData!, samus, 0, 0,
            runtime.Camera!.XPosition, runtime.Camera.YPosition,
            runtime.BombProjectiles,
            roomPlms: runtime.Plms);
        samus.ProjectileFlareCounter = runtime.Projectiles.FlareCounter;
        return result;
    }

    private static void AddProjectileSound(
        SamusProjectileFrameResult frame,
        List<SoundEffectId> sounds)
    {
        if (frame.QueuedSoundEffect is { } sound && !frame.QueuedSoundSuppressed)
            sounds.Add(sound);
        foreach (SamusSoundRequest request in frame.AdditionalSoundRequests ?? [])
        {
            if (!request.SoundSuppressed)
                sounds.Add(request.SoundEffect);
        }
    }

    private static string Format(
        string action,
        bool powerBombActive,
        IReadOnlyList<SoundEffectId> sounds)
    {
        string Field(SoundEffectLibrary library)
        {
            byte[] values = sounds
                .Where(sound => sound.Library == library)
                .Select(sound => sound.Value)
                .ToArray();
            return $"{values.Length},{Convert.ToHexString(values)}";
        }
        return $"{action},{(powerBombActive ? 1 : 0)}," +
            $"{Field(SoundEffectLibrary.Library1)}," +
            $"{Field(SoundEffectLibrary.Library2)}," +
            Field(SoundEffectLibrary.Library3);
    }
}

/// <summary>Stable row identities in the #422 original-CPU Processing action trace.</summary>
internal static class ProcessingActionDefinitions
{
    public const string HudSelect = "hud-select";
    public const string SpinStart = "spin-start";
    public const string SpaceJumpCheck = "space-jump-check";
    public const string ScrewAttackControl = "screw-attack-control";
    public const string BreakSpin = "break-spin";
    public const string BreakSpinCharging = "break-spin-charging";
    public const string FirePowerBeam = "fire-power-beam";
    public const string FireWaveBeam = "fire-wave-beam";
    public const string ReleaseCharge = "release-charge";
    public const string BombExplosion = "bomb-explosion";
    public const string ShinesparkActivation = "shinespark-activation";

    public static IReadOnlyList<string> OrderedCases { get; } =
    [
        HudSelect,
        SpinStart,
        SpaceJumpCheck,
        ScrewAttackControl,
        BreakSpin,
        BreakSpinCharging,
        FirePowerBeam,
        FireWaveBeam,
        ReleaseCharge,
        BombExplosion,
        ShinesparkActivation,
    ];
}
