using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Kraid.KraidArmCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidFootCollisionDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Kraid.KraidNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Kraid;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;

/// <summary>
/// #1284: Kraid's typed foot, arm, lint, fingernail and rock programs against the retail cartridge. The typed part
/// programs must match the native bank-$A7 lists step by step; the runner must switch the arm to its slow swing
/// exactly as the native callback does; and the rocks must match their bank-$86 lists update by update.
/// </summary>
internal static class KraidAnimationsVerification
{
    /// <summary>
    /// Every part program's typed steps equal its native list, the arm switches to its slow swing below half health,
    /// and the arm and foot collision tables still cover every typed frame.
    /// </summary>
    internal static void VerifyKraidPartAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<KraidPartProgram>().Length, PartEntries.Length, "every typed Kraid part program has a native entry");
        for (int index = 0; index < PartEntries.Length; index++)
            AssertEqual((KraidPartProgram)index, PartEntries[index].Program, "the Kraid part programs are declared in authored order");
        AssertTrue(PartEntriesAscend, "native Kraid part entries ascend");

        int stepCount = 0;
        foreach ((KraidPartProgram program, ushort entry) in PartEntries)
            stepCount += CompareProgram(cartridge, program, entry);

        VerifyArmSpeedSwitch();
        Suite(nameof(VerifyKraidArmCollisionDefinitions), () => VerifyKraidArmCollisionDefinitions(rom));
        Suite(nameof(VerifyKraidFootCollisionDefinitions), () => VerifyKraidFootCollisionDefinitions());
        Console.WriteLine($"  Kraid part programs: {PartEntries.Length} programs and {stepCount} steps match bank $A7; the arm's slow-swing switch matches the native callback.");
    }

    /// <summary>Compares one typed part program with its native list and returns its typed step count.</summary>
    private static int CompareProgram(IImportCartridgeSource cartridge, KraidPartProgram program, ushort entry)
    {
        var steps = KraidAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        for (int command = 0; command < 512; command++)
        {
            string at = $"Kraid {program} step {index}";
            if (cursor != entry && PartProgramAt(cursor) is { } next)
            {
                var fallInto = AssertIs<AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.FallIntoStep>(steps[index], $"{at}: runs on into the next authored program");
                AssertEqual(next, fallInto.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return steps.Count;
            }
            int address = (PartBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    var show = AssertIs<AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.ShowStep>(steps[index], $"{at}: frame");
                    AssertEqual(word, show.Duration, $"{at}: duration");
                    AssertEqual(operand, KraidPartFrameToolingDefinitions.Spritemap(show.Frame), $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    index++;
                    continue;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    var goTo = AssertIs<AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.GoToStep>(steps[index], $"{at}: goto");
                    AssertEqual(PartProgramAt(operand), goTo.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertTrue(steps[index] is AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.HoldStep, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Callback:
                    AssertTrue(PartCallbacks.TryGetValue(word, out KraidPartCue? cue), $"{at}: native callback ${word:X4} is known");
                    cursor = unchecked((ushort)(cursor + 2));
                    if (cue is not { } expected)
                        continue; // The no-operation callback has no typed step.
                    var cueStep = AssertIs<AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.CueStep>(steps[index], $"{at}: callback");
                    AssertEqual(expected, cueStep.Cue, $"{at}: cue");
                    index++;
                    continue;
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Kraid's part lists use no stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Kraid {program} ran past its native list.");
    }

    /// <summary>Creates Kraid's body and arm in a fresh enemy system.</summary>
    private static NewKraidResult NewKraid()
    {
        var enemies = new RoomEnemySystem(new EnemySession(), services: new VerificationRoomEnemyServices(null));
        var state = new KraidEnemyState { InitialHealth = 104 };
        enemies.KraidBehavior.Access.Kraid = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Kraid, body.Definition);
        RoomEnemySlot arm = enemies.SlotTable.Slots[1];
        arm.Bind(EnemyDefinitionId.KraidArm, arm.Definition);
        return new NewKraidResult(enemies, state, body, arm);
    }

    /// <summary>
    /// At full health the arm's normal swing loops; below half health the pause's callback starts the slow swing and
    /// shows its first frame in the same update, and once slow the callback keeps it slow.
    /// </summary>
    private static void VerifyArmSpeedSwitch()
    {
        (_, KraidEnemyState state, RoomEnemySlot body, RoomEnemySlot arm) = NewKraid();
        body.Health = 104;
        state.Parts[1].Cursor = AnimationProgramCursor<KraidPartProgram>.Start(KraidPartProgram.ArmNormalPause);
        arm.InstructionTimer = WrappingCountdown.Start(1);
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidPartProgram.ArmNormal, state.Parts[1].Cursor.Program, "a healthy arm loops its normal swing");

        body.Health = 51;
        state.Parts[1].Cursor = AnimationProgramCursor<KraidPartProgram>.Start(KraidPartProgram.ArmNormalPause);
        arm.InstructionTimer = WrappingCountdown.Start(1);
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidPartProgram.ArmSlow, state.Parts[1].Cursor.Program, "below half health the pause starts the slow swing");
        AssertEqual(KraidPartFrameToolingDefinitions.Spritemap(new KraidPartFrame(KraidPartFrameKind.Arm, 0)), arm.SpritemapPointer, "the slow swing's first frame shows at once");
        AssertEqual((ushort)8, arm.InstructionTimer.RawValue, "the slow swing's first frame lasts eight updates");

        state.Parts[1].Cursor = new(KraidPartProgram.ArmSlow, KraidAnimations.Programs.Steps(KraidPartProgram.ArmSlow).Count - 3);
        arm.InstructionTimer = WrappingCountdown.Start(1);
        Pass(arm);
        Pass(arm, forceExpiry: true);
        AssertEqual(KraidPartProgram.ArmSlow, state.Parts[1].Cursor.Program, "a slow arm's callback keeps the slow swing");
    }

    /// <summary>Runs one arm update, optionally expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot part, bool forceExpiry = false)
    {
        if (forceExpiry)
            part.InstructionTimer = WrappingCountdown.Start(1);
        EnemyInstructionInterpreter.ProcessInstructions(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>
    /// The real rock producers start typed programs that match their native lists, and the spat rock's burst sets
    /// its palette pre-instruction and deletes on the native updates. The Kago bug shares the rocks' pose.
    /// </summary>
    internal static void VerifyKraidRockAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<KraidRockProgram>().Length, RockEntries.Length, "every typed rock program has a native entry");
        VerifyRockProducers();
        foreach ((KraidRockProgram program, ushort entry) in RockEntries)
        {
            (RoomEnemySystem enemies, _, _, _) = NewKraid();
            enemies.VerificationServices().ReadRandom = () => 32;
            AssertTrue(enemies.KraidBehavior.Head.SpawnKraidSpitRock(NewRockSource()), "the real spit producer allocates a rock");
            RoomEnemyProjectileSlot rock = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == RoomEnemyProjectileKind.KraidSpitRock);
            rock.StartAnimation(new KraidRockAnimation(program));
            var native = new NativeEnemyProjectileListDefinitions.Reader(entry, rock.InstructionTimer.RawValue);
            for (int update = 0; update < 40 && !native.Deleted; update++)
            {
                enemies.Projectiles.ProcessEnemyProjectileInstructions(rock, null, Position16<RoomPixels>.Zero);
                native.Step(cartridge);
                string at = $"Kraid rock {program} update {update}";
                AssertEqual(!native.Deleted, rock.IsActive, $"{at}: deletion");
                if (native.Deleted)
                    break;
                AssertEqual(native.Timer, rock.InstructionTimer.RawValue, $"{at}: timer");
                AssertEqual(native.Operand, rock.PresentationOperandAddress, $"{at}: frame");
                if (native.PreInstruction is { } preInstruction)
                    AssertEqual(preInstruction, rock.PreInstructionWord, $"{at}: pre-instruction");
            }
            AssertEqual(program == KraidRockProgram.SpitRockShot, native.Deleted, $"Kraid rock {program}: only the burst deletes its rock");
        }

        AssertEqual(KraidRockVisualDefinitions.ProjectileFrameCount, KraidRockVisualDefinitions.ProjectileFrames.Count, "the rock catalog lists every frame");
        Console.WriteLine("  Kraid rock programs: the poses and burst match bank $86 every update, with the palette pre-instruction and deletion.");
    }

    /// <summary>A Kraid body position and art for the real rock producers.</summary>
    private static RoomEnemySlot NewRockSource() => new(0, EnemySlotFixtures.Registry)
    {
        Position = FixedPosition<RoomPixels>.FromWhole(new(128, 224)),
        TileIndexBase = 512,
        PaletteRow = 6,
    };

    /// <summary>Every real rock producer, and Kago's bug, starts the typed pose its native list starts.</summary>
    private static void VerifyRockProducers()
    {
        (RoomEnemySystem enemies, _, _, _) = NewKraid();
        ushort random = 0;
        enemies.VerificationServices().ReadRandom = () => random;
        RoomEnemySlot source = NewRockSource();
        random = 32;
        AssertTrue(enemies.KraidBehavior.Head.SpawnKraidSpitRock(source), "the real spit producer allocates its rock");
        random = 32;
        AssertTrue(enemies.KraidBehavior.Background.SpawnKraidCeilingRock(96), "the real ceiling producer allocates its rock");
        var state = new KraidEnemyState();
        random = 0;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source, state);
        random = 16;
        enemies.KraidBehavior.Rise.RequestKraidRisingRock(source, state);
        random = 2;
        AssertTrue(enemies.KagoBehavior.SpawnKagoBug(source), "the real Kago producer allocates its bug");
        foreach ((RoomEnemyProjectileKind kind, KraidRockProgram program) in new[]
                 {
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidSpitRock, KraidRockProgram.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidCeilingRock, KraidRockProgram.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidRisingRockLeft, KraidRockProgram.SharedPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KraidRisingRockRight, KraidRockProgram.RisingRightPose),
                     new VerifyRockProducersCase(RoomEnemyProjectileKind.KagoBug, KraidRockProgram.SharedPose),
                 })
        {
            RoomEnemyProjectileSlot projectile = enemies.Projectiles.EnemyProjectiles.Single(item => item.Kind == kind);
            AssertTrue(projectile.Animation is KraidRockAnimation started && started.Program == program, $"{kind} starts the {program} program");
        }
    }

    /// <summary>Runs Kraid's part program parity check against the repository cartridge.</summary>
    [VerificationCommand("--kraid-part-animations")]
    internal static int KraidPartAnimationsCommand()
    {
        Suite(nameof(VerifyKraidPartAnimations), () => VerifyKraidPartAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Kraid's rock program parity check against the repository cartridge.</summary>
    [VerificationCommand("--kraid-rock-animations")]
    internal static int KraidRockAnimationsCommand()
    {
        Suite(nameof(VerifyKraidRockAnimations), () => VerifyKraidRockAnimations(LoadRepositoryRom()));
        return 0;
    }
}
