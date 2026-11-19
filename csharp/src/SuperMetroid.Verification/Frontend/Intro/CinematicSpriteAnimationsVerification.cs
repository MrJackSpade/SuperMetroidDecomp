using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.TestSupport.MovieReplay;
using static SuperMetroid.Verification.Frontend.Intro.CinematicSpriteNativeListAddresses;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// #1290, #1600, #2196, #2197: every typed cinematic animation an actor starts against the retail bank-$8B lists it
/// replaces. Each family's animations play through the real <see cref="CinematicActor{TAnimation, TFrame, TCue}"/> exactly as
/// an independent reader of the native lists does, update by update: the actor's liveness, the picture or typed text it
/// shows, the plays a counted phase has left and the family's own cues. The typed segments are written as the game shows
/// them, a still picture held and an entry and its loop one animation, so they are compared by what they play, not by the
/// lists' layout.
/// </summary>
internal static class CinematicSpriteAnimationsVerification
{
    /// <summary>Updates each animation is played for, enough for every list's loops and deletions.</summary>
    private const int PlaybackUpdates = 1200;

    /// <summary>The shared cinematic sprite instructions, by what they do.</summary>
    private enum SharedWord
    {
        /// <summary>The deletion.</summary>
        Delete,
        /// <summary>The sleep.</summary>
        Sleep,
        /// <summary>The pre-instruction install and its routine operand.</summary>
        SetFrameBehavior,
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
        [(ushort)CinematicSpriteInstruction.SetFrameBehavior] = SharedWord.SetFrameBehavior,
        [(ushort)CinematicSpriteInstruction.Goto] = SharedWord.GoTo,
        [(ushort)CinematicSpriteInstruction.DecrementTimerAndGoto] = SharedWord.RepeatWhileCounting,
        [(ushort)CinematicSpriteInstruction.SetTimer] = SharedWord.SetCounter,
    };

    /// <summary>Every family's animations play as their native lists do, and every native start is an animation an actor starts.</summary>
    internal static void VerifyCinematicSpriteAnimations(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var compared = new List<Enum>();
        int updates = 0;
        updates += CompareFamily(cartridge, IntroMotherBrainAnimations.MotherBrain, MotherBrainCues, Pictures, compared);
        updates += CompareFamily(cartridge, IntroMotherBrainAnimations.Explosions, NativeCinematicCues<NoAnimationCue>.None, Pictures, compared);
        updates += CompareFamily(cartridge, IntroBabyAnimations.Discovery, DiscoveryCues, Pictures, compared);
        updates += CompareFamily(cartridge, IntroBabyAnimations.Laboratory, LaboratoryCues, Pictures, compared);
        updates += CompareFamily(cartridge, IntroEggEffectAnimations.Effects, NativeCinematicCues<NoAnimationCue>.None, Pictures, compared);
        updates += CompareFamily(cartridge, IntroRinkaAnimations.Rinka, RinkaCues, Pictures, compared);
        updates += CompareFamily(cartridge, IntroRinkaAnimations.Spawner, RinkaSpawnerCues, Pictures, compared);
        updates += CompareFamily(cartridge, IntroNarrationAnimations.Narration, NativeCinematicCues<NoAnimationCue>.None, Pictures, compared);
        updates += CompareFamily(cartridge, CeresAnimations.Scenes, CeresCues, Pictures, compared);
        updates += CompareFamily(cartridge, CeresAnimations.ExplosionSpawner, ExplosionSpawnerCues, Pictures, compared);
        updates += CompareFamily(cartridge, EndingSceneAnimations.Pictures, EndingSceneCues, Pictures, compared);
        updates += CompareFamily(cartridge, EndingTextAnimations.Texts, EndingSceneCues, TypedTexts, compared);
        updates += CompareFamily(cartridge, EndingRewardAnimations.Gesture, RewardGestureCues, Pictures, compared);
        updates += CompareFamily(cartridge, EndingRewardAnimations.Jump, RewardJumpCues, Pictures, compared);
        updates += CompareFamily(cartridge, EndingLogoAnimations.Logo, LogoCues, Pictures, compared);
        AssertEqual(compared.Count, compared.Distinct().Count(), "no animation is compared twice");
        AssertTrue(Starts.Keys.All(compared.Contains) && compared.All(Starts.ContainsKey),
            "every native start is an animation an actor of some family starts, and every such animation has its native start");
        Console.WriteLine($"  cinematic sprite animations: {compared.Count} animations ({updates} played updates) match bank $8B.");
    }

    /// <summary>Whether the native spritemap word <paramref name="native"/> draws <paramref name="pose"/>.</summary>
    private static bool Pictures(CinematicSpritePose? pose, ushort native) => CinematicSpritePoseSpritemaps.Shows(pose, native);

    /// <summary>Whether the native spritemap word <paramref name="native"/> is the step that shows <paramref name="typed"/>.</summary>
    private static bool TypedTexts(EndingTextTyping? typed, ushort native) =>
        typed is { } shown ? CinematicSpritemapSources.Of(shown) == native : native == 0;

    /// <summary>Plays every animation an actor of <paramref name="animations"/>' family starts against its native list; returns the updates.</summary>
    private static int CompareFamily<TAnimation, TFrame, TCue>(IImportCartridgeSource cartridge, CinematicAnimations<TAnimation, TFrame, TCue> animations,
        NativeCinematicCues<TCue> cues, Func<TFrame?, ushort, bool> shows, List<Enum> compared)
        where TAnimation : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        int updates = 0;
        foreach (TAnimation animation in animations.Starts)
        {
            compared.Add(animation);
            updates += ComparePlayback(cartridge, animations, cues, shows, animation);
        }
        return updates;
    }

    /// <summary>Reads one native list word.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, ushort address) =>
        cartridge.ReadCartridgeWord((int)new SnesAddress(Bank, address));

    /// <summary>
    /// Plays <paramref name="animation"/> through the real actor beside an independent reader of its native list, comparing
    /// liveness, the shown frame, the plays a repeated phase has left (#2006) and each update's owner cues, a routine install
    /// among them; returns the updates.
    /// </summary>
    private static int ComparePlayback<TAnimation, TFrame, TCue>(IImportCartridgeSource cartridge, CinematicAnimations<TAnimation, TFrame, TCue> animations,
        NativeCinematicCues<TCue> cues, Func<TFrame?, ushort, bool> shows, TAnimation animation)
        where TAnimation : struct, Enum
        where TFrame : struct
        where TCue : notnull
    {
        var actor = new CinematicActor<TAnimation, TFrame, TCue>(default, IntroShades.Drawn(IntroSpriteColouring.DiscoveryActors), animations, animation);
        var native = new NativeReader<TCue>(cartridge, Start(animation), cues);
        var raised = new List<TCue>();
        for (int update = 0; update < PlaybackUpdates; update++)
        {
            raised.Clear();
            actor.Step(raised.Add);
            native.Step();
            string at = $"{animation} update {update}";
            AssertEqual(native.Active, actor.IsActive, $"{at}: liveness");
            if (!native.Active)
                return update + 1;
            AssertTrue(shows(actor.Shown, native.Spritemap), $"{at}: spritemap ${native.Spritemap:X4} draws {actor.Shown}");
            // A list that repeats by its goto counter counts the plays a repeated phase has left in it; one whose installed
            // routine raises the repeated cue keeps no such count, and its repeats are compared as the cues each update raises.
            if (NativeEnemyAnimationOracle.NativePlaysLeft(actor.Playback) is { } playsLeft && native.CountsRepeats)
                AssertEqual<int>(native.Timer.RawValue, playsLeft, $"{at}: plays left");
            AssertTrue(native.Cues.SequenceEqual(raised), $"{at}: owner cues");
        }
        return PlaybackUpdates;
    }

    /// <summary>An independent reader of a native bank-$8B list with the generic cinematic sprite handler's timer gate.</summary>
    /// <typeparam name="TCue">The cues of the list's family.</typeparam>
    private sealed class NativeReader<TCue>(IImportCartridgeSource cartridge, ushort cursor, NativeCinematicCues<TCue> cues)
        where TCue : notnull
    {
        /// <summary>The next native word.</summary>
        private ushort cursor = cursor;
        /// <summary>Updates before the next word is read.</summary>
        private WrappingCountdown frameTimer = WrappingCountdown.Start(1);
        /// <summary>Whether the list has deleted the actor.</summary>
        internal bool Active { get; private set; } = true;
        /// <summary>The latest frame's spritemap.</summary>
        internal ushort Spritemap { get; private set; }
        /// <summary>The goto timer word.</summary>
        internal WrappingCountdown Timer { get; private set; }
        /// <summary>Whether the list has set its goto timer, which then counts its repeats.</summary>
        internal bool CountsRepeats { get; private set; }
        /// <summary>The owner instructions the latest update ran, as the family's cues.</summary>
        internal List<TCue> Cues { get; } = [];
        /// <summary>The installed routine that raises cues of its own, which runs before the list on every update, if any.</summary>
        private INativeCinematicRoutine<TCue>? routine;

        /// <summary>Runs one generic handler call: the installed routine, then the list once its timer runs out.</summary>
        internal void Step()
        {
            Cues.Clear();
            if (!Active)
                return;
            if (routine is { } running)
                Cues.AddRange(running.Step());
            frameTimer = frameTimer.Decrement().Remaining;
            if (!frameTimer.IsZero)
                return;
            for (int command = 0; command < 32; command++)
            {
                ushort word = Word(cartridge, cursor);
                ushort operand = Word(cartridge, unchecked((ushort)(cursor + 2)));
                if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
                {
                    frameTimer = WrappingCountdown.Start(word);
                    Spritemap = operand;
                    cursor = unchecked((ushort)(cursor + 4));
                    return;
                }
                if (cues.Instructions.TryGetValue(word, out TCue? cue))
                {
                    Cues.Add(cue);
                    cursor = unchecked((ushort)(cursor + 2));
                    continue;
                }
                AssertTrue(SharedWords.TryGetValue(word, out SharedWord shared), $"native word ${word:X4} at $8B:{cursor:X4} is the family's or shared");
                switch (shared)
                {
                    case SharedWord.Delete:
                        Active = false;
                        Spritemap = 0;
                        return;
                    case SharedWord.Sleep:
                        return;
                    case SharedWord.SetFrameBehavior:
                        // A routine whose updates raise the family's cues runs from the next update on (#2489).
                        if (cues.Routines.TryGetValue(operand, out Func<IImportCartridgeSource, INativeCinematicRoutine<TCue>>? reader))
                        {
                            routine = reader(cartridge);
                            cursor = unchecked((ushort)(cursor + 4));
                            continue;
                        }
                        // Installing a routine starts its motion, which the actor's owner runs on the cue Core raises.
                        if (!cues.Installs.TryGetValue(operand, out TCue? starting))
                            throw new InvalidDataException($"Routine ${operand:X4} installed at $8B:{cursor:X4} starts none of the family's motions.");
                        Cues.Add(starting);
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.GoTo:
                        cursor = operand;
                        continue;
                    case SharedWord.SetCounter:
                        Timer = WrappingCountdown.Start(operand);
                        CountsRepeats = true;
                        cursor = unchecked((ushort)(cursor + 4));
                        continue;
                    case SharedWord.RepeatWhileCounting:
                        Timer = Timer.Decrement().Remaining;
                        cursor = !Timer.IsZero ? operand : unchecked((ushort)(cursor + 4));
                        continue;
                    default:
                        throw new InvalidOperationException($"Undefined {nameof(SharedWord)} {shared}.");
                }
            }
            throw new InvalidDataException("A native cinematic sprite list ran thirty-two commands without a frame.");
        }
    }
}
