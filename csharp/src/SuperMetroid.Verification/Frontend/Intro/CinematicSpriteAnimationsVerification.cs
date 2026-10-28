using SuperMetroid.Core.Programs;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSpriteNativeListAddresses;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// #1290: every typed cinematic sprite program against the retail bank-$8B lists. Each program must match its native list
/// step by step (durations, poses, counters, branches, pre-instructions and owner cues), and must then play through the
/// real <see cref="IntroDiscoverySprite"/> exactly as an independent reader of the native list does, update by update.
/// </summary>
internal static class CinematicSpriteAnimationsVerification
{
    /// <summary>Updates each program is played for, enough for every list's loops and deletions.</summary>
    private const int PlaybackUpdates = 1200;

    /// <summary>The shared cinematic sprite instructions, by what they do.</summary>
    private enum SharedWord
    {
        /// <summary>The deletion.</summary>
        Delete,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        SetPreInstruction,
        /// <summary>The goto and its target operand.</summary>
        GoTo,
        /// <summary>The timer countdown and its target operand.</summary>
        RepeatWhileCounting,
        /// <summary>The timer load and its count operand.</summary>
        SetCounter,
    }

    /// <summary>The shared instructions' native words.</summary>
    private static readonly Dictionary<ushort, SharedWord> SharedWords = new()
    {
        [(ushort)CinematicSpriteInstruction.Delete] = SharedWord.Delete,
        [(ushort)CinematicSpriteInstruction.Sleep] = SharedWord.Sleep,
        [(ushort)CinematicSpriteInstruction.SetPreInstruction] = SharedWord.SetPreInstruction,
        [(ushort)CinematicSpriteInstruction.Goto] = SharedWord.GoTo,
        [(ushort)CinematicSpriteInstruction.DecrementTimerAndGoto] = SharedWord.RepeatWhileCounting,
        [(ushort)CinematicSpriteInstruction.SetTimer] = SharedWord.SetCounter,
    };

    /// <summary>Every program matches its native list and plays as it does.</summary>
    internal static void VerifyCinematicSpriteAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<CinematicSpriteProgram>().Length, Entries.Length, "every cinematic sprite program has a native entry");
        for (int index = 0; index < Entries.Length; index++)
            AssertEqual(Enum.GetValues<CinematicSpriteProgram>()[index], Entries[index].Program, "the cinematic sprite programs are declared in authored order");
        AssertTrue(Entries.Zip(Entries.Skip(1), (first, second) => first.Entry < second.Entry).All(ascends => ascends), "native cinematic sprite entries ascend");

        int steps = 0, updates = 0;
        foreach ((CinematicSpriteProgram program, ushort entry) in Entries)
        {
            steps += CompareSteps(cartridge, program, entry);
            updates += ComparePlayback(cartridge, program, entry);
        }
        Console.WriteLine($"  cinematic sprite programs: {Entries.Length} programs ({steps} steps, {updates} played updates) match bank $8B.");
    }

    /// <summary>The program whose native list starts at <paramref name="address"/>, if one does.</summary>
    private static CinematicSpriteProgram? ProgramAt(ushort address) =>
        Entries.Any(item => item.Entry == address) ? Entries.Single(item => item.Entry == address).Program : null;

    /// <summary>Reads one native list word.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, ushort address) =>
        cartridge.ReadCartridgeWord((int)new SnesAddress(Bank, address));

    /// <summary>Compares one program's steps with its native list and returns its step count.</summary>
    private static int CompareSteps(IImportCartridgeSource cartridge, CinematicSpriteProgram program, ushort entry)
    {
        var steps = CinematicSpriteAnimations.Programs.Steps(program);
        ushort cursor = entry;
        int index = 0;
        AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue> Next(string what)
        {
            AssertTrue(index < steps.Count, $"{program} has a step for native {what} at $8B:{cursor:X4}");
            return steps[index++];
        }
        while (true)
        {
            string at = $"{program} at $8B:{cursor:X4}";
            if (cursor != entry && ProgramAt(cursor) is { } following)
            {
                var fall = Next("fall-through");
                var fallInto = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.FallIntoStep>(fall, $"{at}: runs on into the next authored program");
                AssertEqual(following, fallInto.Target, $"{at}: fall-through target");
                break;
            }
            ushort word = Word(cartridge, cursor);
            ushort operand = Word(cartridge, unchecked((ushort)(cursor + 2)));
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                var show = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.ShowStep>(Next("frame"), $"{at}: frame");
                AssertEqual(word, show.Duration, $"{at}: duration");
                AssertEqual(operand, CinematicSpritePoseSpritemaps.Of(show.Frame), $"{at}: spritemap");
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            if (OwnerInstructions.TryGetValue(word, out CinematicSpriteCueKind kind))
            {
                var cue = Next("owner instruction");
                var cueStep = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.CueStep>(cue, $"{at}: owner instruction");
                AssertEqual(new CinematicSpriteCue(kind), cueStep.Cue, $"{at}: owner cue");
                cursor = unchecked((ushort)(cursor + 2));
                continue;
            }
            AssertTrue(SharedWords.TryGetValue(word, out SharedWord shared), $"{at}: native word ${word:X4} is known");
            var step = Next(shared.ToString());
            switch (shared)
            {
                case SharedWord.Delete:
                    AssertTrue(step is AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.StopStep, $"{at}: delete");
                    break;
                case SharedWord.Sleep:
                    AssertTrue(step is AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.HoldStep, $"{at}: sleep");
                    break;
                case SharedWord.GoTo:
                    var goTo = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.GoToStep>(step, $"{at}: goto");
                    AssertEqual(operand, Entry(goTo.Target), $"{at}: goto target");
                    break;
                case SharedWord.SetPreInstruction:
                    var cueStep = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.CueStep>(step, $"{at}: pre-instruction install");
                    AssertEqual(CinematicSpriteCueKind.SetPreInstruction, cueStep.Cue.Kind, $"{at}: pre-instruction cue");
                    AssertEqual(operand, cueStep.Cue.PreInstruction.NativeAddress, $"{at}: pre-instruction");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case SharedWord.SetCounter:
                    var setCounter = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.SetCounterStep>(step, $"{at}: timer load");
                    AssertEqual(operand, setCounter.Count, $"{at}: timer");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                case SharedWord.RepeatWhileCounting:
                    var repeat = AssertIs<AnimationProgramStep<CinematicSpriteProgram, CinematicSpritePose, CinematicSpriteCue>.RepeatWhileCountingStep>(step, $"{at}: timer countdown");
                    AssertEqual(operand, Entry(repeat.Target), $"{at}: countdown target");
                    cursor = unchecked((ushort)(cursor + 4));
                    continue;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(SharedWord)} {shared}.");
            }
            break;
        }
        AssertEqual(steps.Count, index, $"{program} ends where its native list does");
        return steps.Count;
    }

    /// <summary>
    /// Plays <paramref name="program"/> through the real actor beside an independent reader of its native list, comparing
    /// liveness, the shown spritemap, the pre-instruction, the loop timer and each update's owner cues; returns the updates.
    /// </summary>
    private static int ComparePlayback(IImportCartridgeSource cartridge, CinematicSpriteProgram program, ushort entry)
    {
        var actor = new IntroDiscoverySprite(default, 0, program);
        var native = new NativeReader(cartridge, entry);
        var cues = new List<CinematicSpriteCueKind>();
        for (int update = 0; update < PlaybackUpdates; update++)
        {
            cues.Clear();
            actor.Step(cues.Add);
            native.Step();
            string at = $"{program} update {update}";
            AssertEqual(native.Active, actor.IsActive, $"{at}: liveness");
            if (!native.Active)
                return update + 1;
            AssertEqual(native.Spritemap, CinematicSpritePoseSpritemaps.Of(actor.Pose), $"{at}: spritemap");
            AssertEqual(native.PreInstruction, actor.PreInstructionPointer, $"{at}: pre-instruction");
            AssertEqual(native.Timer.RawValue, actor.GeneralTimer, $"{at}: loop timer");
            AssertTrue(native.Cues.SequenceEqual(cues), $"{at}: owner cues");
        }
        return PlaybackUpdates;
    }

    /// <summary>An independent reader of a native bank-$8B list with the generic cinematic sprite handler's timer gate.</summary>
    private sealed class NativeReader(IImportCartridgeSource cartridge, ushort cursor)
    {
        /// <summary>The next native word.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates before the next word is read.</summary>
        private WrappingCountdown frameTimer = WrappingCountdown.Start(1);
        /// <summary>Whether the list has deleted the actor.</summary>
        internal bool Active { get; private set; } = true;
        /// <summary>The latest frame's spritemap.</summary>
        internal ushort Spritemap { get; private set; }
        /// <summary>The installed pre-instruction.</summary>
        internal ushort PreInstruction { get; private set; }
        /// <summary>The goto timer word.</summary>
        internal WrappingCountdown Timer { get; private set; }
        /// <summary>The owner instructions the latest update ran.</summary>
        internal List<CinematicSpriteCueKind> Cues { get; } = [];

        /// <summary>Runs one generic handler call.</summary>
        internal void Step()
        {
            Cues.Clear();
            if (!Active)
                return;
            frameTimer = frameTimer.Decrement().Remaining;
            if (!frameTimer.IsZero)
                return;
            for (int command = 0; command < 32; command++)
            {
                ushort word = Word(cartridge, cursor);
                ushort operand = Word(cartridge, unchecked((ushort)(cursor + 2)));
                if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
                {
                    frameTimer = WrappingCountdown.FromRaw(word);
                    Spritemap = operand;
                    cursor = unchecked((ushort)(cursor + 4));
                    return;
                }
                if (OwnerInstructions.TryGetValue(word, out CinematicSpriteCueKind kind))
                {
                    Cues.Add(kind);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                }
                switch (SharedWords[word])
                {
                    case SharedWord.Delete:
                        Active = false;
                        Spritemap = 0;
                        return;
                    case SharedWord.Sleep:
                        return;
                    case SharedWord.SetPreInstruction:
                        PreInstruction = operand;
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.GoTo:
                        cursor = operand;
                        continue;
                    case SharedWord.SetCounter:
                        Timer = WrappingCountdown.FromRaw(operand);
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.RepeatWhileCounting:
                        Timer = Timer.Decrement().Remaining;
                        cursor = !Timer.IsZero ? operand : unchecked((ushort)(cursor + 4));
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(SharedWord)} {SharedWords[word]}.");
                }
            }
            throw new InvalidDataException("A native cinematic sprite list ran thirty-two commands without a frame.");
        }
    }
}
