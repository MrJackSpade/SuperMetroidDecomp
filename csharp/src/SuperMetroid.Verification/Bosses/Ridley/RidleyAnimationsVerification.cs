using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.Ridley.RidleyNativeListAddresses;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// #1287: Ridley's typed body, breakup-fragment and fireball programs against the retail cartridge. Each typed
/// program must match its native list step by step, and the body's facing and low-energy branches must redirect as
/// the native gotos do. The step-by-step walk also yields each typed step's native instruction address, which
/// <see cref="RidleyNativeInstructionPointers"/> uses to compare typed cursors with recorded native pointers.
/// </summary>
internal static class RidleyAnimationsVerification
{
    /// <summary>Every body, breakup and fireball program matches its native list, and the body's branches redirect natively.</summary>
    internal static void VerifyRidleyAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<RidleyBodyProgram>().Length, BodyEntries.Length, "every typed Ridley body program has a native entry");
        for (int index = 0; index < BodyEntries.Length; index++)
            AssertEqual((RidleyBodyProgram)index, BodyEntries[index].Program, "the Ridley body programs are declared in authored order");
        AssertTrue(BodyEntries.Zip(BodyEntries.Skip(1)).All(pair => pair.First.Entry < pair.Second.Entry), "native Ridley body entries ascend");
        AssertEqual(Enum.GetValues<RidleyFireballProgram>().Length, FireballEntries.Length, "every typed fireball program has a native entry");

        var body = BodyStepAddresses(cartridge);
        BreakupStepAddresses(cartridge);
        var fireball = FireballStepAddresses(cartridge);
        AssertEqual(RidleyFireballVisualDefinitions.ProjectileFrameCount, RidleyFireballVisualDefinitions.ProjectileFrames.Count,
            "the fireball catalog lists every frame");

        VerifyFacingBranch();
        VerifyLowEnergyBranch();
        Console.WriteLine($"  Ridley programs: {body.Count} body programs ({body.Values.Sum(steps => steps.Length)} steps), " +
            $"{Enum.GetValues<RidleyBreakupFrame>().Length} breakup programs and {fireball.Count} fireball programs match their native lists; " +
            "the facing and low-energy branches redirect natively.");
    }

    /// <summary>Compares every body program with its native list and returns each typed step's native address.</summary>
    internal static Dictionary<RidleyBodyProgram, ushort[]> BodyStepAddresses(IImportCartridgeSource cartridge) =>
        BodyEntries.ToDictionary(item => item.Program, item => CompareBodyProgram(cartridge, item.Program, item.Entry));

    /// <summary>Compares one body program with its native list and returns each typed step's native address.</summary>
    private static ushort[] CompareBodyProgram(IImportCartridgeSource cartridge, RidleyBodyProgram program, ushort entry)
    {
        var steps = RidleyAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            string at = $"Ridley {program} step {index}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && BodyProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, step.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = (BodyBankNumber << 16) | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            switch (NativeEnemyListDefinitions.Classify(word))
            {
                case NativeEnemyListDefinitions.WordKind.Frame:
                    AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: frame");
                    AssertEqual((int)word, step.Duration, $"{at}: duration");
                    AssertEqual(operand, (ushort)step.Frame, $"{at}: spritemap");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case NativeEnemyListDefinitions.WordKind.Sleep:
                    AssertEqual(AnimationProgramStepKind.Hold, step.Kind, $"{at}: sleep");
                    AssertEqual(steps.Count, index + 1, $"{at}: sleep ends the program");
                    return addresses;
                case NativeEnemyListDefinitions.WordKind.Callback:
                {
                    AssertTrue(BodyInstructions.TryGetValue(word, out var instruction), $"{at}: native instruction ${word:X4} is known");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: instruction");
                    RidleyBodyCue cue = step.Cue;
                    AssertEqual(instruction.Kind, cue.Kind, $"{at}: cue");
                    switch (instruction.Kind)
                    {
                        case RidleyBodyCueKind.GoToUnlessFacingLeft:
                        case RidleyBodyCueKind.GoToIfCeresAndSamusLowEnergy:
                            AssertEqual(BodyProgramAt(operand), cue.Target, $"{at}: goto target");
                            break;
                        case RidleyBodyCueKind.SetFeetDistance:
                            // The native operand is the pose's byte offset into the word-sized claw-height table.
                            AssertEqual((int)operand, cue.FeetDistanceIndex * sizeof(ushort), $"{at}: claw-height pose");
                            break;
                        case RidleyBodyCueKind.Move:
                            AssertEqual(unchecked((short)operand), cue.Displacement.X, $"{at}: X move");
                            AssertEqual(unchecked((short)cartridge.ReadCartridgeWord(address + 4)), cue.Displacement.Y, $"{at}: Y move");
                            break;
                    }
                    cursor = unchecked((ushort)(cursor + 2 + 2 * instruction.Operands));
                    continue;
                }
                case NativeEnemyListDefinitions.WordKind.GoTo:
                case NativeEnemyListDefinitions.WordKind.Stop:
                case NativeEnemyListDefinitions.WordKind.SetCounter:
                case NativeEnemyListDefinitions.WordKind.RepeatWhileCounting:
                case NativeEnemyListDefinitions.WordKind.Wait:
                    throw new InvalidDataException($"{at}: Ridley's body lists use no common gotos, stops, counters or waits.");
                default:
                    throw new InvalidOperationException($"Undefined {nameof(NativeEnemyListDefinitions.WordKind)}.");
            }
        }
        throw new InvalidDataException($"Ridley {program} has steps past its native list.");
    }

    /// <summary>
    /// Compares every breakup program with its six-byte native list: one frame and a sleep. A breakup program's
    /// steps sit at the list entry and its sleep.
    /// </summary>
    internal static void BreakupStepAddresses(IImportCartridgeSource cartridge)
    {
        foreach (RidleyBreakupFrame frame in Enum.GetValues<RidleyBreakupFrame>())
        {
            ushort entry = BreakupList(frame);
            int address = (BodyBankNumber << 16) | entry;
            var steps = RidleyBreakupAnimations.Programs.Steps(frame);
            string at = $"Ridley breakup {frame}";
            AssertEqual(2, steps.Count, $"{at}: one frame and a hold");
            AssertEqual(AnimationProgramStepKind.Show, steps[0].Kind, $"{at}: frame");
            AssertEqual((int)cartridge.ReadCartridgeWord(address), steps[0].Duration, $"{at}: duration");
            AssertEqual(cartridge.ReadCartridgeWord(address + 2), RidleyBreakupFrameDefinitionsConstants.Spritemap(steps[0].Frame), $"{at}: spritemap");
            AssertEqual(NativeEnemyListDefinitions.WordKind.Sleep, NativeEnemyListDefinitions.Classify(cartridge.ReadCartridgeWord(address + 4)), $"{at}: sleep");
            AssertEqual(AnimationProgramStepKind.Hold, steps[1].Kind, $"{at}: hold");
        }
    }

    /// <summary>The native list of breakup <paramref name="frame"/>.</summary>
    internal static ushort BreakupList(RidleyBreakupFrame frame) => BreakupEntry(frame);

    /// <summary>Compares every fireball program with its native bank-$86 list and returns each typed step's native address.</summary>
    internal static Dictionary<RidleyFireballProgram, ushort[]> FireballStepAddresses(IImportCartridgeSource cartridge) =>
        FireballEntries.ToDictionary(item => item.Program, item => CompareFireballProgram(cartridge, item.Program, item.Entry));

    /// <summary>Compares one fireball program with its native list and returns each typed step's native address.</summary>
    private static ushort[] CompareFireballProgram(IImportCartridgeSource cartridge, RidleyFireballProgram program, ushort entry)
    {
        var steps = RidleyFireballAnimations.Programs.Steps(program);
        var addresses = new ushort[steps.Count];
        ushort cursor = entry;
        for (int index = 0; index < steps.Count; index++)
        {
            string at = $"Ridley fireball {program} step {index}";
            addresses[index] = cursor;
            var step = steps[index];
            if (cursor != entry && FireballProgramAt(cursor) is { } next)
            {
                AssertEqual(AnimationProgramStepKind.FallInto, step.Kind, $"{at}: runs on into the next authored program");
                AssertEqual(next, step.Target, $"{at}: fall-through target");
                AssertEqual(steps.Count, index + 1, $"{at}: fall-through ends the program");
                return addresses;
            }
            int address = NativeEnemyProjectileListDefinitions.ProjectileBank | cursor;
            ushort word = cartridge.ReadCartridgeWord(address);
            ushort operand = cartridge.ReadCartridgeWord(address + 2);
            if ((word & NativeEnemyListDefinitions.InstructionBit) == 0)
            {
                AssertEqual(AnimationProgramStepKind.Show, step.Kind, $"{at}: frame");
                AssertEqual((int)word, step.Duration, $"{at}: duration");
                AssertEqual(unchecked((ushort)(cursor + 2)), EnemyProjectileFrames.ProgramFrameKey(RidleyFireballVisualDefinitions.ShownProjectileFrames[step.Frame]), $"{at}: presentation key");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            switch ((EnemyProjectileInstruction)word)
            {
                case EnemyProjectileInstruction.ClearPreInstruction:
                    AssertEqual(RidleyFireballCue.ClearPreInstruction, step.Cue, $"{at}: clear pre-instruction");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                case EnemyProjectileInstruction.PreInstructionInY:
                    AssertEqual(EnemyProjectilePreInstructionAccess.Word(EnemyProjectilePreInstruction.RidleyFireball), operand, $"{at}: the fireball's moving pre-instruction");
                    AssertEqual(RidleyFireballCue.BeginFlight, step.Cue, $"{at}: begin flight");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case EnemyProjectileInstruction.GotoY:
                    AssertEqual(AnimationProgramStepKind.GoTo, step.Kind, $"{at}: goto");
                    AssertEqual(FireballProgramAt(operand), step.Target, $"{at}: goto target");
                    AssertEqual(steps.Count, index + 1, $"{at}: goto ends the program");
                    return addresses;
                case EnemyProjectileInstruction.Delete:
                    AssertEqual(RidleyFireballCue.Delete, step.Cue, $"{at}: delete");
                    AssertEqual(AnimationProgramStepKind.Hold, steps[index + 1].Kind, $"{at}: deletion ends the program");
                    AssertEqual(steps.Count, index + 2, $"{at}: deletion ends the program");
                    addresses[index + 1] = cursor;
                    return addresses;
                default:
                    AssertTrue(FireballCallbacks.TryGetValue(word, out RidleyFireballCue callback), $"{at}: native callback ${word:X4} is known");
                    AssertEqual(AnimationProgramStepKind.Cue, step.Kind, $"{at}: callback");
                    AssertEqual(callback, step.Cue, $"{at}: cue");
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
            }
        }
        throw new InvalidDataException($"Ridley fireball {program} has steps past its native list.");
    }

    /// <summary>Creates Ridley in slot zero of a fresh enemy system, facing <paramref name="facing"/>.</summary>
    private static (RoomEnemySystem Enemies, RidleyEnemyState State, RoomEnemySlot Body) NewRidley(EnemyDefinitionId definition, RidleyFacing facing)
    {
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        var state = new RidleyEnemyState { FacingDirection = facing };
        enemies.RidleyEncounter.Ridley = state;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(definition, body.Definition);
        return (enemies, state, body);
    }

    /// <summary>Runs one body update, expiring the current frame first.</summary>
    private static void Pass(RoomEnemySlot body, SamusState? samus)
    {
        body.InstructionTimer = NativeCountdown.Start(1);
        EnemyInstructionInterpreter.ProcessInstructions(body, samus, level: null, Position16<RoomPixels>.Zero, controllerInput: 0);
    }

    /// <summary>A right-facing Ridley starting a left-facing entry runs its right-facing program in the same update.</summary>
    private static void VerifyFacingBranch()
    {
        (RoomEnemySystem enemies, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Right);
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoarFacingRight, state.BodyProgram.Program, "a right-facing Ridley roars facing right");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Right, body.SpritemapPointer, "the right-facing roar shows its first frame at once");
        AssertEqual((ushort)6, body.InstructionTimer.RawValue, "the right-facing roar's first frame lasts six updates");

        (enemies, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.OpeningRoar);
        Pass(body, samus: null);
        AssertEqual(RidleyBodyProgram.OpeningRoar, state.BodyProgram.Program, "a left-facing Ridley keeps the left-facing roar");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Left, body.SpritemapPointer, "the left-facing roar shows its first frame");
    }

    /// <summary>
    /// In Ceres, a Samus under 30 energy cuts the fireballing short to its closing tail and shortens the hover to eight
    /// updates; Lower Norfair Ridley ignores her energy.
    /// </summary>
    private static void VerifyLowEnergyBranch()
    {
        (RoomEnemySystem enemies, RidleyEnemyState state, RoomEnemySlot body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        state.Roaring = true;
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 29 });
        AssertEqual(RidleyBodyProgram.FireballingEnd, state.BodyProgram.Program, "low energy in Ceres skips to the fireballing tail");
        AssertEqual((ushort)8, state.FunctionTimer.RawValue, "low energy shortens the hover to eight updates");
        AssertTrue(!state.Roaring, "the fireballing tail clears the roaring flag");
        AssertEqual((ushort)RidleyCollisionDefinitions.BodyFrame.Left, body.SpritemapPointer, "the tail shows the standing pose");

        (enemies, state, body) = NewRidley(EnemyDefinitionId.RidleyCeres, RidleyFacing.Left);
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 30 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "thirty energy keeps the full fireballing");

        (enemies, state, body) = NewRidley(EnemyDefinitionId.Ridley, RidleyFacing.Left);
        CeresRidleyBehavior.StartRidleyProgram(state, body, RidleyBodyProgram.Fireballing);
        Pass(body, new SamusState { Health = 1 });
        AssertEqual(RidleyBodyProgram.Fireballing, state.BodyProgram.Program, "Lower Norfair Ridley ignores Samus's energy");
    }

    /// <summary>Runs Ridley's program parity check against the repository cartridge.</summary>
    [VerificationCommand("--ridley-animations")]
    internal static int RidleyAnimationsCommand()
    {
        Suite(nameof(VerifyRidleyAnimations), () => VerifyRidleyAnimations(LoadRepositoryRom()));
        return 0;
    }
}
