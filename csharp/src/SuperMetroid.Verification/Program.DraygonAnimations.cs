using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static DraygonNativeListAddresses;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;

/// <summary>
/// #1283: Draygon's typed body, eye, tail, arms and projectile programs against the retail cartridge. The typed
/// programs must match the native bank-$A5 and bank-$86 lists step by step, and the real runner must match them
/// update by update: timers, frames, the loop counter, cues, part starts and deletion.
/// </summary>
internal static class DraygonAnimationsVerification
{
    /// <summary>
    /// Every part program's typed steps equal its native list; the runner then plays the reset that starts all four
    /// parts and the dying sequence, with its counted explosions, beside the native list.
    /// </summary>
    internal static void VerifyDraygonAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<DraygonProgram>().Length, Entries.Length, "every typed Draygon program has a native entry");
        for (int index = 0; index < Entries.Length; index++)
            AssertEqual((DraygonProgram)index, Entries[index].Program, "the Draygon programs are declared in authored order");
        AssertTrue(EntriesAscend, "native Draygon entries ascend");

        var drawn = new HashSet<ushort>();
        int stepCount = 0;
        foreach ((DraygonProgram program, ushort entry) in Entries)
            stepCount += CompareProgram(cartridge, program, entry, drawn);
        AssertEqual(DraygonFrameDefinitions.FrameCount, drawn.Count, "the Draygon programs draw every catalog frame");
        for (int index = 0; index < DraygonFrameDefinitions.FrameCount; index++)
            AssertTrue(drawn.Contains(new DraygonFrame(index).Spritemap), $"Draygon catalog frame {index} is drawn by a native list");

        VerifyReset();
        VerifyDying(cartridge);
        Console.WriteLine($"  Draygon programs: {Entries.Length} programs and {stepCount} steps match bank $A5; the reset and dying sequence match the runner.");
    }

    /// <summary>Compares one typed program with its native list and returns its step count.</summary>
    private static int CompareProgram(IImportCartridgeSource cartridge, DraygonProgram program, ushort entry, HashSet<ushort> drawn)
    {
        var steps = DraygonAnimations.Programs.Steps(program);
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            string at = $"Draygon {program} step {index}";
            if (index > 0 && ProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, step.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return steps.Count;
            }

            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: frame");
                    AssertEqual((int)word, step.Duration, $"{at}: duration");
                    AssertEqual(operand, step.Frame.Spritemap, $"{at}: spritemap");
                    drawn.Add(operand);
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.GoTo:
                    AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                    AssertEqual(ProgramAt(operand), step.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, step.Kind, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.Stop:
                    AssertEqual(AnimationProgramStepKind.Stop, step.Kind, $"{at}: stop");
                    AssertEqual(steps.Count, index + 1, $"{at}: stop ends the program");
                    return steps.Count;
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                    AssertEqual(AnimationProgramStepKind.SetCounter, step.Kind, $"{at}: counter load");
                    AssertEqual((int)operand, step.Duration, $"{at}: repeat count");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                    AssertEqual(AnimationProgramStepKind.RepeatWhileCounting, step.Kind, $"{at}: counted repeat");
                    AssertEqual(ProgramAt(operand), step.Target, $"{at}: repeat target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Wait:
                    AssertEqual(AnimationProgramStepKind.Wait, step.Kind, $"{at}: wait");
                    AssertEqual((int)operand, step.Duration, $"{at}: wait length");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    int count = OperandWords(word);
                    var operands = new ushort[count];
                    for (int operandIndex = 0; operandIndex < count; operandIndex++)
                        operands[operandIndex] = cartridge.ReadCartridgeWord(address + 2 + 2 * operandIndex);
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: callback");
                    AssertEqual(CueOf(word, operands), step.Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 2 + 2 * count));
                    continue;
                }
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Draygon {program} ran past its typed steps.");
    }

    /// <summary>Creates Draygon's four linked parts in a fresh enemy system.</summary>
    private static (RoomEnemySystem Enemies, DraygonEnemyState State) NewDraygon()
    {
        var enemies = new RoomEnemySystem();
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.EnemyDefinitionPointer = EnemyDefinitionId.DraygonBody;
        body.Position = body.Position.WithWhole(new(256, 384));
        var state = new DraygonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1],
            Tail = enemies.SlotTable.Slots[2],
            Arms = enemies.SlotTable.Slots[3],
        };
        state.Eye.EnemyDefinitionPointer = EnemyDefinitionId.DraygonEye;
        state.Tail.EnemyDefinitionPointer = EnemyDefinitionId.DraygonTail;
        state.Arms.EnemyDefinitionPointer = EnemyDefinitionId.DraygonArms;
        foreach (RoomEnemySlot part in new[] { body, state.Eye, state.Tail, state.Arms })
            state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.Sleep));
        enemies.DraygonBehavior.Draygon = state;
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 4660);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 4660);
        return (enemies, state);
    }

    /// <summary>Runs one update of <paramref name="part"/> through the production interpreter.</summary>
    private static void Pass(RoomEnemySystem enemies, RoomEnemySlot part) =>
        enemies.Instructions.ProcessInstructions(part, samus: null, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);

    /// <summary>
    /// The body's reset starts all four parts in the same update, gives the eye its tracking function and the
    /// HUD-draw interrupt, and keeps the body itself in the reset, whose own frame replaces its started program.
    /// </summary>
    private static void VerifyReset()
    {
        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        state.SetCursor(state.Body, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.BodyFacingRightReset));
        state.Body.InstructionTimer = NativeCountdown.Start(1);
        Pass(enemies, state.Body);
        AssertEqual(AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.EyeFacingRightIdle), state.Cursor(state.Eye), "the reset starts the eye");
        AssertEqual(AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.TailFacingRightIdle), state.Cursor(state.Tail), "the reset starts the tail");
        AssertEqual(AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.ArmsFacingRightIdle), state.Cursor(state.Arms), "the reset starts the arms");
        foreach (RoomEnemySlot part in new[] { state.Eye, state.Tail, state.Arms })
            AssertEqual((ushort)1, part.InstructionTimer.RawValue, "the reset's started parts run on their next update");
        AssertEqual(DraygonProgram.BodyFacingRightReset, state.Cursor(state.Body).Program, "the body's own frame keeps it in the reset");
        AssertEqual(new DraygonFrame(63).Spritemap, state.Body.SpritemapPointer, "the reset shows the right-facing body frame");
        AssertEqual((ushort)DraygonEyeFunction.FacingRight, state.Eye.VariableA, "the reset gives the eye its tracking function");
        AssertEqual(RoomLoadingInterruptCommand.BeginHudDraw, state.RoomLoadingIrqCommand, "the reset selects the HUD-draw interrupt");
    }

    /// <summary>
    /// The arms' dying program runs on into the body's dying explosions: the cry, the intangible body, eight counted
    /// explosion rounds with their waits, the paralysis of the tail, then the explosion loop, update by update beside
    /// the native list, including the loop counter in the arms' timer word.
    /// </summary>
    private static void VerifyDying(IImportCartridgeSource cartridge)
    {
        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        RoomEnemySlot arms = state.Arms;
        state.SetCursor(arms, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.ArmsFacingLeftDying));
        arms.InstructionTimer = NativeCountdown.Start(1);
        var native = new NativeEnemyListDefinitions.Reader(BodyBankNumber, Entry(DraygonProgram.ArmsFacingLeftDying), 1, OperandWords);
        bool paralysed = false;
        for (int update = 0; update < 400; update++)
        {
            state.LastSoundLibrary2 = null;
            state.LastSoundLibrary3 = null;
            Pass(enemies, arms);
            List<NativeEnemyListDefinitions.Callback> callbacks = native.Step(cartridge);
            string at = $"Draygon dying update {update}";
            AssertEqual(native.Timer, arms.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Spritemap, arms.SpritemapPointer, $"{at}: frame");
            AssertEqual(native.NewFrame, arms.ExtraProperties.HasAny(EnemyExtraProperties.NewInstructionFrame), $"{at}: new-frame flag");
            AssertEqual(native.Counter, arms.Timer.RawValue, $"{at}: loop counter");
            DraygonCue[] cues = [.. callbacks.Select(callback => CueOf(callback.Word, callback.Operands))];
            AssertEqual(cues.OfType<DraygonCue.QueueSound>().LastOrDefault(sound => sound.Library == SoundEffectLibrary.Library2)?.Sound,
                state.LastSoundLibrary2, $"{at}: library-2 sound");
            AssertEqual(cues.OfType<DraygonCue.QueueSound>().LastOrDefault(sound => sound.Library == SoundEffectLibrary.Library3)?.Sound,
                state.LastSoundLibrary3, $"{at}: library-3 sound");
            if (cues.Any(cue => cue is DraygonCue.ParalyseTailAndArms))
            {
                paralysed = true;
                AssertEqual(AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.Sleep), state.Cursor(state.Tail), $"{at}: the tail sleeps");
                AssertEqual(DraygonProgram.BodyDyingExplosionsLoop, state.Cursor(arms).Program,
                    $"{at}: the running arms keep their own program, as the native final store replaces the paralysis");
            }
        }
        AssertTrue(paralysed, "the dying sequence reaches the paralysis");
        AssertTrue(state.Body.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "the dying sequence makes the body intangible");
    }

    /// <summary>
    /// Every projectile program's typed steps equal its native bank-$86 list, and the goop touch sticks the goop to
    /// Samus and shows its first frame in the same update.
    /// </summary>
    internal static void VerifyDraygonProjectileAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<DraygonProjectileProgram>().Length, ProjectileEntries.Length, "every projectile program has a native entry");
        foreach ((DraygonProjectileProgram program, ushort entry) in ProjectileEntries)
            CompareProjectileProgram(cartridge, program, entry);
        AssertEqual(DraygonProjectileVisualDefinitions.ProjectileFrameCount, DraygonProjectileVisualDefinitions.ProjectileFrames.Count,
            "the projectile catalog lists every frame");

        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        enemies.DraygonBehavior.SpawnDraygonGoop(state, movingRight: true);
        RoomEnemyProjectileSlot goop = enemies.Projectiles.EnemyProjectiles.Single(projectile => projectile.Kind == RoomEnemyProjectileKind.DraygonGoop);
        AssertTrue(goop.Animation is DraygonProjectileAnimation, "the real goop producer starts the typed goop program");
        goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileProgram.GoopTouch));
        var samus = new SamusState();
        enemies.Projectiles.ProcessEnemyProjectileInstructions(goop, samus, Position16<RoomPixels>.Zero);
        AssertEqual((ushort)EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus, goop.PreInstruction, "the goop touch sticks the goop to Samus");
        AssertEqual(DraygonProjectileVisualDefinitions.PresentationKey(new(DraygonProjectileFrameKind.Goop, 0)), goop.PresentationOperandAddress,
            "the goop touch shows the goop's first frame in the same update");
        AssertEqual((ushort)10, goop.InstructionTimer.RawValue, "the goop's first frame lasts ten updates");
        Console.WriteLine("  Draygon projectile programs: goop, burst, touch, bloom and flight match bank $86; the touch sticks the goop at once.");
    }

    /// <summary>Compares one typed projectile program with its native bank-$86 list.</summary>
    private static void CompareProjectileProgram(IImportCartridgeSource cartridge, DraygonProjectileProgram program, ushort entry)
    {
        var steps = DraygonProjectileAnimations.Programs.Steps(program);
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            var step = steps[index];
            string at = $"Draygon projectile {program} step {index}";
            if (index > 0 && ProjectileProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, step.Target, $"{at}: fall-through target");
                return;
            }
            ushort word = cartridge.ReadCartridgeWord(ProjectileBank | cursor);
            ushort operand = cartridge.ReadCartridgeWord(ProjectileBank | (cursor + 2));
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: frame");
                AssertEqual((int)word, step.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), DraygonProjectileVisualDefinitions.PresentationKey(step.Frame), $"{at}: frame operand");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (IsProjectileGoTo(word))
            {
                if (ProjectileProgramAt(operand) is { } target)
                {
                    AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                    AssertEqual(target, step.Target, $"{at}: goto target");
                    return;
                }
                // A goto into the shared deletion list continues with its delete instruction.
                cursor = operand;
                index--;
                continue;
            }
            AssertTrue(ProjectileCallbacks.TryGetValue(word, out DraygonProjectileCue cue), $"{at}: native callback ${word:X4} is known");
            AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: callback");
            AssertEqual(cue, step.Cue, $"{at}: cue");
            if (cue == DraygonProjectileCue.Delete)
            {
                AssertEqual(AnimationProgramStepKind.Hold, steps[index + 1].Kind, $"{at}: nothing follows the deletion");
                return;
            }
            cursor = unchecked((ushort)(cursor + 2));
        }
        throw new InvalidDataException($"Draygon projectile {program} ran past its typed steps.");
    }

    /// <summary>Runs Draygon's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--draygon-instruction-mechanics")]
    internal static int DraygonInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonAnimations), () => VerifyDraygonAnimations(LoadRepositoryRom()));
        return 0;
    }

    /// <summary>Runs Draygon's projectile program parity check against the repository cartridge.</summary>
    [VerificationCommand("--draygon-projectile-instruction-mechanics")]
    internal static int DraygonProjectileInstructionMechanicsCommand()
    {
        Suite(nameof(VerifyDraygonProjectileAnimations), () => VerifyDraygonProjectileAnimations(LoadRepositoryRom()));
        return 0;
    }
}
