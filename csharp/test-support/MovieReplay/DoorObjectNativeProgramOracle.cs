using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the typed door caps (#1554): it walks each cap's native bank-$84 lists beside the sequences, counts
/// and wakes the typed caps hold, comparing every image (its draw and the updates it stays, a picture the list draws again
/// straight after itself one image of both lengths, #2480), every sound and where it is requested, and every branch, link, wake, count, sleep, goto and delete that orders them. It also names the phase a
/// recorded grey cap's native instruction stands for. Only verification reads cartridge lists; the port runs the typed
/// caps.
/// </summary>
internal static class DoorObjectNativeProgramOracle
{
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a draw's duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>The bytes of the queue-sound instruction: its word and the sound's byte.</summary>
    private const int QueueSoundLength = 3;

    /// <summary>How many times the cartridge's unlocked grey cap list writes its flash out before it repeats.</summary>
    private const int CartridgeGreyFlashesBeforeRepeat = 3;

    /// <summary>The bytes of an open native list, from its sound to its delete: one sound, four images and the delete.</summary>
    private const int OpenListLength = 21;

    /// <summary>The library-3 sound a queue-sound instruction at <paramref name="cursor"/> names.</summary>
    internal static SoundEffectId QueuedLibrary3Sound(IImportCartridgeSource cartridge, int cursor) =>
        SoundEffectCommands.Decode(SoundEffectLibrary.Room, ReadOperandByte(cartridge, cursor + 2));

    /// <summary>Every blue cap's lists equal its typed sequences; returns the images compared.</summary>
    internal static int CompareBlueCaps(IImportCartridgeSource cartridge)
    {
        int images = 0;
        foreach (ColoredDoorOrientation facing in Enum.GetValues<ColoredDoorOrientation>())
        {
            NativeBlueDoorLists lists = DoorObjectNativeListAddresses.Blue(facing);
            string at = $"blue {facing} cap";
            ushort cursor = CompareSteps(cartridge, lists.Opening, DoorCapSequenceDefinitions.BlueOpening, DoorCapPaint.Blue, facing, $"{at} opening", ref images);
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} opening leaves the room");
            cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.BlueClosing, DoorCapPaint.Blue, facing, $"{at} closing", ref images);
            VerificationAssertions.AssertEqual(lists.Closed, cursor, $"{at} closing falls into its closed image");
            cursor = Expect(cartridge, lists.Closed, RoomObjectInstruction.SetObjectBtsFromByte, 3, $"{at} is made shootable before its closed image");
            cursor = CompareSteps(cartridge, cursor, DoorCapSequenceDefinitions.BlueClosed, DoorCapPaint.Blue, facing, $"{at} closed", ref images);
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} closed leaves the room");
        }
        return images;
    }

    /// <summary>Every coloured cap's lists equal its typed sequences, count and wake; returns the images compared.</summary>
    internal static int CompareColoredCaps(IImportCartridgeSource cartridge)
    {
        int images = 0;
        foreach (ColoredDoorColor color in Enum.GetValues<ColoredDoorColor>())
        foreach (ColoredDoorOrientation facing in Enum.GetValues<ColoredDoorOrientation>())
        {
            NativeColoredDoorLists lists = DoorObjectNativeListAddresses.Colored(color, facing);
            ColoredDoorSequences sequences = ColoredDoorDefinitions.Sequences[new ColoredDoorVariant(color, facing)];
            DoorCapPaint paint = ColoredDoorDefinitions.Colors[color].Paint;
            string at = $"{color} {facing} cap";

            ushort cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.CapClosing, paint, facing, $"{at} closing", ref images);
            VerificationAssertions.AssertEqual(lists.Waiting, cursor, $"{at} closing falls into its first look");

            cursor = ExpectGoto(cartridge, lists.Waiting, RoomObjectInstruction.GotoIfDoorBitSet, DoorObjectNativeListAddresses.Blue(facing).Closed,
                $"{at} turns into its closed blue cap once opened");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.LinkInstruction, lists.Hit, $"{at} wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.ColoredDoorShotWakes[color],
                $"{at} takes its colour's shot");
            cursor = CompareSteps(cartridge, cursor, sequences.Waiting, paint, facing, $"{at} waiting", ref images);
            VerificationAssertions.AssertEqual(lists.Sleep, cursor, $"{at} rests after its closed image");
            _ = Expect(cartridge, lists.Sleep, RoomObjectInstruction.Sleep, 2, $"{at} rests");

            VerificationAssertions.AssertEqual(RoomObjectInstruction.IncrementDoorHitCounterAndGoto, InstructionAt<RoomObjectInstruction>(cartridge, lists.Hit),
                $"{at} counts its hit");
            VerificationAssertions.AssertEqual(ColoredDoorDefinitions.Colors[color].HitsToOpen, (int)ReadOperandByte(cartridge, lists.Hit + 2), $"{at} hit count");
            VerificationAssertions.AssertEqual(lists.Open, Word(cartridge, lists.Hit + 3), $"{at} opens at its count");
            cursor = CompareSteps(cartridge, unchecked((ushort)(lists.Hit + 5)), sequences.Flash, paint, facing, $"{at} flash", ref images);
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Sleep, $"{at} rests again after its flash");

            cursor = CompareSteps(cartridge, lists.Open, sequences.Opening, paint, facing, $"{at} opening", ref images);
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} leaves the room once open");
        }
        return images;
    }

    /// <summary>Every grey cap's lists, Bomb Torizo's included, equal its typed sequences and count; returns the images compared.</summary>
    internal static int CompareGreyCaps(IImportCartridgeSource cartridge)
    {
        int images = 0;
        foreach (GreyDoorVariant variant in DoorObjectNativeListAddresses.GreyVariants)
        {
            NativeGreyDoorLists lists = DoorObjectNativeListAddresses.Grey(variant);
            ColoredDoorOrientation facing = variant.Orientation;
            string at = variant.Closing == GreyDoorClosing.AfterBombs ? "Bomb Torizo's grey cap" : $"grey {facing} cap";

            ushort cursor;
            if (variant.Closing == GreyDoorClosing.AfterBombs)
            {
                cursor = CompareSteps(cartridge, lists.Closing, GreyDoorDefinitions.BombTorizoHeldOpen, DoorCapPaint.Grey, facing, $"{at} held open", ref images);
                cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.GotoIfSamusHasNoBombs, lists.Closing, $"{at} stays open until Samus has Bombs");
                cursor = CompareSteps(cartridge, cursor, GreyDoorDefinitions.BombTorizoClosing, DoorCapPaint.Grey, facing, $"{at} closing", ref images);
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Waiting, $"{at} closing goes to its first look");
            }
            else
            {
                cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.CapClosing, DoorCapPaint.Grey, facing, $"{at} closing", ref images);
                VerificationAssertions.AssertEqual(lists.Waiting, cursor, $"{at} closing falls into its first look");
            }

            cursor = ExpectGoto(cartridge, lists.Waiting, RoomObjectInstruction.GotoIfDoorBitSet, DoorObjectNativeListAddresses.Blue(facing).Closed,
                $"{at} turns into its closed blue cap once opened");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.LinkInstruction, lists.Activate, $"{at} wakes at its unlock");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SetGreyDoorFrameBehavior, 2, $"{at} waits for its condition");
            cursor = CompareSteps(cartridge, cursor, GreyDoorDefinitions.Locked, DoorCapPaint.Grey, facing, $"{at} locked", ref images);
            VerificationAssertions.AssertEqual(lists.Sleep, cursor, $"{at} rests after its closed image");
            _ = Expect(cartridge, lists.Sleep, RoomObjectInstruction.Sleep, 2, $"{at} rests");

            cursor = ExpectGoto(cartridge, lists.Activate, RoomObjectInstruction.LinkInstruction, lists.Hit, $"{at} unlocked wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.GreyDoorShotWake, $"{at} unlocked takes any shot");
            VerificationAssertions.AssertEqual(lists.Flash, cursor, $"{at} unlocked falls into its flash");
            // The cartridge's list writes the flash out three times before it repeats, which plays exactly as the port's one
            // flash repeated (#2572).
            cursor = lists.Flash;
            for (int flash = 0; flash < CartridgeGreyFlashesBeforeRepeat; flash++)
                cursor = CompareSteps(cartridge, cursor, GreyDoorDefinitions.Flash, DoorCapPaint.Grey, facing, $"{at} flash {flash}", ref images);
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Flash, $"{at} flash repeats");

            VerificationAssertions.AssertEqual(RoomObjectInstruction.IncrementDoorHitCounterAndGoto, InstructionAt<RoomObjectInstruction>(cartridge, lists.Hit),
                $"{at} counts its hit");
            // The port opens a grey cap at the first shot after its condition holds.
            VerificationAssertions.AssertEqual(1, (int)ReadOperandByte(cartridge, lists.Hit + 2), $"{at} opens at its first hit");
            VerificationAssertions.AssertEqual(lists.Open, Word(cartridge, lists.Hit + 3), $"{at} opens at its count");
            VerificationAssertions.AssertEqual(lists.Open, unchecked((ushort)(lists.Hit + 5)), $"{at} hit falls into its opening");

            cursor = CompareSteps(cartridge, lists.Open, GreyDoorDefinitions.Opening, DoorCapPaint.Grey, facing, $"{at} opening", ref images);
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} leaves the room once open");
        }
        return images;
    }

    /// <summary>
    /// Every eye door's lists, both facings, equal the typed eye's and parts' sequences, nearness, count and wakes;
    /// returns the images compared.
    /// </summary>
    internal static int CompareEyeDoors(IImportCartridgeSource cartridge)
    {
        int images = 0;
        foreach (EyeDoorOrientation orientation in Enum.GetValues<EyeDoorOrientation>())
        {
            NativeEyeDoorEyeLists eye = DoorObjectNativeListAddresses.Eye(orientation);
            ushort blueClosed = DoorObjectNativeListAddresses.Blue(orientation == EyeDoorOrientation.Left ? ColoredDoorOrientation.Left : ColoredDoorOrientation.Right).Closed;
            RoomObjectInstruction makeBlue = orientation == EyeDoorOrientation.Left
                ? RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft
                : RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight;
            string at = $"{orientation} eye";

            ushort cursor = ExpectGoto(cartridge, eye.Start, RoomObjectInstruction.GotoIfDoorBitSet, eye.Opened, $"{at} turns blue once opened");
            VerificationAssertions.AssertEqual(eye.Idle, cursor, $"{at} first look falls into its closed eye");

            cursor = CompareEyeSteps(cartridge, eye.Idle, EyeDoorSequenceDefinitions.Closed, orientation, $"{at} closed", ref images);
            cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.WakeReach, eye.Activate, $"{at} opens once Samus is near");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Idle, $"{at} stays closed otherwise");

            cursor = ExpectGoto(cartridge, eye.Activate, RoomObjectInstruction.LinkInstruction, eye.Hit, $"{at} wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.EyeDoorMissileWake, $"{at} takes missiles");
            cursor = CompareEyeSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Opening, orientation, $"{at} opening", ref images);
            VerificationAssertions.AssertEqual(eye.Watch, cursor, $"{at} opening falls into its watch");

            cursor = ExpectNear(cartridge, eye.Watch, EyeDoorSequenceDefinitions.InFrontReach, eye.Close, $"{at} blinks while Samus is in front");
            cursor = CompareEyeSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Watching, orientation, $"{at} watching", ref images);
            cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.WakeReach, eye.Watch, $"{at} watches again while Samus is near");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Idle, $"{at} closes once Samus has left");

            cursor = CompareEyeSteps(cartridge, eye.Close, EyeDoorSequenceDefinitions.Blinking, orientation, $"{at} blinking", ref images);
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Watch, $"{at} watches after its blink");

            VerificationAssertions.AssertEqual(RoomObjectInstruction.QueueSoundLibrary2Maximum6, InstructionAt<RoomObjectInstruction>(cartridge, eye.Hit), $"{at} hit sounds");
            VerificationAssertions.AssertEqual(EyeDoorSequenceDefinitions.HitSound,
                SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, eye.Hit + 2)), $"{at} hit sound");
            VerificationAssertions.AssertEqual(SoundRequestPriority.Normal, EyeDoorSequenceDefinitions.HitSoundPriority, $"{at} hit sound priority");
            cursor = Expect(cartridge, unchecked((ushort)(eye.Hit + QueueSoundLength)), RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{at} hit smokes");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{at} hit smokes again");
            VerificationAssertions.AssertEqual(RoomObjectInstruction.IncrementDoorHitCounterAndGoto, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"{at} counts its hit");
            VerificationAssertions.AssertEqual(EyeDoorSequenceDefinitions.HitThreshold, (int)ReadOperandByte(cartridge, cursor + 2), $"{at} hit count");
            VerificationAssertions.AssertEqual(eye.Destroyed, Word(cartridge, cursor + 3), $"{at} is destroyed at its count");
            cursor = CompareEyeSteps(cartridge, unchecked((ushort)(cursor + 5)), EyeDoorSequenceDefinitions.Flinching, orientation, $"{at} flinching", ref images);
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Watch, $"{at} watches after its flinch");

            cursor = Expect(cartridge, eye.Destroyed, RoomObjectInstruction.ClearFrameBehavior, 2, $"{at} destroyed takes no more shots");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnEyeDoorSmoke, 2, $"{at} destroyed smokes");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnEyeDoorSmoke, 2, $"{at} destroyed smokes again");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{at} destroyed scatters smoke");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{at} destroyed scatters smoke again");
            cursor = Expect(cartridge, cursor, makeBlue, 2, $"{at} destroyed moves onto its blue cap");
            VerificationAssertions.AssertEqual(RoomObjectInstruction.SetEightBitTimer, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"{at} counts its flashes");
            VerificationAssertions.AssertEqual(EyeDoorSequenceDefinitions.OpeningFlashes, (int)ReadOperandByte(cartridge, cursor + 2), $"{at} flashes");
            VerificationAssertions.AssertEqual(eye.Opening, unchecked((ushort)(cursor + 3)), $"{at} destroyed falls into its flashes");

            // The typed destroyed eye plays its flash its count of times; the native list counts its replays down.
            VerificationAssertions.AssertEqual(EyeDoorSequenceDefinitions.OpeningFlashes, EyeDoorSequenceDefinitions.Destroyed.Phases.Single().Plays,
                $"{at} flashes its count of times");
            cursor = CompareEyeSteps(cartridge, eye.Opening, EyeDoorSequenceDefinitions.Destroyed, orientation, $"{at} flash",
                ref images, onePlay: true);
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.DecrementTimerAndGoto, eye.Opening, $"{at} flashes again until counted");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, blueClosed, $"{at} turns into its closed blue cap");

            cursor = Expect(cartridge, eye.Opened, makeBlue, 2, $"{at} of an opened door moves onto its blue cap");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, blueClosed, $"{at} of an opened door turns into its closed blue cap");

            foreach (EyeDoorBodyPart bodyPart in Enum.GetValues<EyeDoorBodyPart>())
            {
                var component = new EyeDoorPart(bodyPart, orientation);
                NativeEyeDoorPartLists part = DoorObjectNativeListAddresses.Part(component);
                string where = $"{orientation} eye door {bodyPart}";
                cursor = ExpectGoto(cartridge, part.Start, RoomObjectInstruction.GotoIfDoorBitSet, part.Delete, $"{where} leaves once opened");
                VerificationAssertions.AssertEqual(part.Idle, cursor, $"{where} first look falls into its still pose");
                cursor = ExpectNear(cartridge, part.Idle, EyeDoorSequenceDefinitions.PulseReach, part.Activate, $"{where} pulses once Samus is near");
                cursor = CompareEyePartSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Still, component, $"{where} still", ref images);
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, part.Idle, $"{where} looks for Samus again");
                cursor = ExpectGoto(cartridge, part.Activate, RoomObjectInstruction.LinkInstruction, part.Delete, $"{where} wakes at its delete");
                cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.EyeDoorOpenedDoorWake, $"{where} waits for its door");
                VerificationAssertions.AssertEqual(part.Cycle, cursor, $"{where} falls into its pulse");
                cursor = CompareEyePartSteps(cartridge, part.Cycle, EyeDoorSequenceDefinitions.Pulse[orientation], component, $"{where} pulse", ref images);
                cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.PulseReach, part.Cycle, $"{where} pulses again while Samus is near");
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, part.Idle, $"{where} stands still once Samus has left");
                _ = Expect(cartridge, part.Delete, RoomObjectInstruction.Delete, 2, $"{where} leaves the room");
            }
        }
        return images;
    }

    /// <summary>
    /// The phase a recorded grey cap of <paramref name="variant"/> is in when its native instruction pointer rests at
    /// <paramref name="instructionPointer"/>: the list holding the instruction it last ran, which ends just before the
    /// pointer (a sleep rests on itself, after the closed image it follows).
    /// </summary>
    internal static GreyDoorPhase GreyDoorPhaseAt(GreyDoorVariant variant, ushort instructionPointer)
    {
        NativeGreyDoorLists lists = DoorObjectNativeListAddresses.Grey(variant);
        int last = instructionPointer - 1;
        if (last >= lists.Closing && last < lists.Waiting)
            return GreyDoorPhase.Closing;
        if (last >= lists.Waiting && last < lists.Activate)
            return GreyDoorPhase.Locked;
        if (last >= lists.Activate && last < lists.Hit)
            return GreyDoorPhase.Flashing;
        if (last >= lists.Hit && last < lists.Open + OpenListLength)
            return GreyDoorPhase.Opening;
        throw new InvalidDataException($"Recorded {variant} grey cap rests at $84:{instructionPointer:X4}, outside its lists.");
    }

    /// <summary>Whether a recorded grey cap of <paramref name="variant"/> rests on its sleep.</summary>
    internal static bool GreyDoorRestsAt(GreyDoorVariant variant, ushort instructionPointer) =>
        instructionPointer == DoorObjectNativeListAddresses.Grey(variant).Sleep;

    /// <summary>
    /// Compares the native timed draws from <paramref name="cursor"/> with one play of <paramref name="animation"/> (#2480):
    /// each sound with the queue-sound instruction just before the next draw, and each picture with the draw records that
    /// show it, a picture the list draws again straight after itself, with nothing between, one picture of both lengths.
    /// Adds the native draws compared to <paramref name="images"/>; returns the cursor after the last draw.
    /// </summary>
    private static ushort CompareSteps(IImportCartridgeSource cartridge, ushort cursor, Animation<DoorCapSegment> animation, DoorCapPaint paint,
        ColoredDoorOrientation facing, string at, ref int images)
    {
        IReadOnlyList<AnimationBeat<DoorCapFrame, SoundEffectId>> beats = DoorCapSequenceDefinitions.Segments.PlayedBeats(animation);
        for (int index = 0; index < beats.Count; index++)
        {
            string where = $"{at} beat {index} at $84:{cursor:X4}";
            switch (beats[index])
            {
                case AnimationBeat<DoorCapFrame, SoundEffectId>.Event sound:
                    VerificationAssertions.AssertEqual(RoomObjectInstruction.QueueSoundLibrary3Maximum6, InstructionAt<RoomObjectInstruction>(cartridge, cursor),
                        $"{where}: the native list sounds");
                    VerificationAssertions.AssertEqual(SoundRequestPriority.Normal, DoorCapSequenceDefinitions.SoundPriority, $"{where}: sound priority");
                    VerificationAssertions.AssertEqual(sound.Raised, QueuedLibrary3Sound(cartridge, cursor), $"{where}: sound");
                    cursor = unchecked((ushort)(cursor + QueueSoundLength));
                    break;
                case AnimationBeat<DoorCapFrame, SoundEffectId>.Frame frame:
                    cursor = CompareDraws(cartridge, cursor, RoomObjectDrawAddressCodec.Of(DoorCaps.Image(paint, facing, frame.Shown)), frame.Duration, where, ref images);
                    break;
                default:
                    throw new InvalidOperationException($"{where}: a door cap's animation holds only pictures and sounds.");
            }
        }
        return cursor;
    }

    /// <summary>
    /// Compares the native timed draws from <paramref name="cursor"/> with an eye door's <paramref name="animation"/>, each
    /// action with the shot, smoke or sweat spawned just before the next draw, as <see cref="CompareSteps"/> compares a
    /// cap's; a repeated phase is compared once when <paramref name="onePlay"/>, as the native list it replays holds it once.
    /// Adds the native draws compared to <paramref name="images"/>; returns the cursor after the last draw.
    /// </summary>
    private static ushort CompareEyeSteps(IImportCartridgeSource cartridge, ushort cursor, Animation<EyeDoorSegment> animation,
        EyeDoorOrientation orientation, string at, ref int images, bool onePlay = false)
    {
        // The shot and sweat operands are the byte offsets of the facing's origin and launch pairs ($86:B65B and
        // $86:B6B1), which LookupStream4 ties to the typed facings.
        const ushort LeftOperand = 0;
        const ushort RightShotOperand = 20;
        const ushort RightSweatOperand = 4;
        AnimationSegments<EyeDoorSegment, EyeDoorObjectFrame, EyeDoorAction> segments = EyeDoorSequenceDefinitions.Segments;
        IReadOnlyList<AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>> beats = onePlay
            ? [.. animation.Phases.SelectMany(phase => segments.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))]
            : segments.PlayedBeats(animation);
        for (int index = 0; index < beats.Count; index++)
        {
            string where = $"{at} beat {index} at $84:{cursor:X4}";
            switch (beats[index])
            {
                case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Event { Raised: EyeDoorAction.Shoot }:
                    cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.ShootEyeDoorProjectile,
                        orientation == EyeDoorOrientation.Right ? RightShotOperand : LeftOperand, $"{where}: shot");
                    break;
                case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Event { Raised: EyeDoorAction.SpawnSweat }:
                    cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.SpawnEyeDoorSweat,
                        orientation == EyeDoorOrientation.Right ? RightSweatOperand : LeftOperand, $"{where}: sweat");
                    break;
                case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Event { Raised: EyeDoorAction.SpawnTwoSmoke }:
                    cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{where}: smoke");
                    break;
                case AnimationBeat<EyeDoorObjectFrame, EyeDoorAction>.Frame frame:
                    cursor = CompareDraws(cartridge, cursor, RoomObjectDrawAddressCodec.Of(EyeDoorDraws.Of(orientation, frame.Shown)),
                        frame.Duration, where, ref images);
                    break;
                default:
                    throw new InvalidOperationException($"{where}: an eye door's animation holds only pictures and its actions.");
            }
        }
        return cursor;
    }

    /// <summary>
    /// Compares the native timed draws from <paramref name="cursor"/> with one play of an eye door's body's or bottom's
    /// <paramref name="animation"/>, which raises nothing (#2481); adds the native draws compared to
    /// <paramref name="images"/> and returns the cursor after the last draw.
    /// </summary>
    private static ushort CompareEyePartSteps(IImportCartridgeSource cartridge, ushort cursor, Animation<EyeDoorPartSegment> animation,
        EyeDoorPart part, string at, ref int images)
    {
        IReadOnlyList<AnimationBeat<EyeDoorPartPose, NoAnimationCue>> beats = EyeDoorSequenceDefinitions.PartSegments.PlayedBeats(animation);
        for (int index = 0; index < beats.Count; index++)
        {
            string where = $"{at} beat {index} at $84:{cursor:X4}";
            if (beats[index] is not AnimationBeat<EyeDoorPartPose, NoAnimationCue>.Frame frame)
                throw new InvalidOperationException($"{where}: an eye door's body or bottom only draws.");
            cursor = CompareDraws(cartridge, cursor, RoomObjectDrawAddressCodec.Of(EyeDoorDraws.Of(part, frame.Shown)), frame.Duration, where, ref images);
        }
        return cursor;
    }

    /// <summary>
    /// Compares the native draw records from <paramref name="cursor"/> with one typed picture drawing <paramref name="draw"/>
    /// for <paramref name="updates"/> updates: its first record, and each record straight after it drawing the same picture
    /// until their lengths add up to the picture's (#2480). Adds the records compared to <paramref name="images"/>; returns the
    /// cursor after the last.
    /// </summary>
    private static ushort CompareDraws(IImportCartridgeSource cartridge, ushort cursor, ushort draw, int updates, string at, ref int images)
    {
        int shown = 0;
        do
        {
            ushort duration = Word(cartridge, cursor);
            VerificationAssertions.AssertTrue((duration & InstructionBit) == 0, $"{at}: the native list draws at $84:{cursor:X4}");
            VerificationAssertions.AssertEqual(Word(cartridge, cursor + 2), draw, $"{at}: draw at $84:{cursor:X4}");
            shown += duration;
            images++;
            cursor = unchecked((ushort)(cursor + 4));
        }
        while (shown < updates);
        VerificationAssertions.AssertEqual(updates, shown, $"{at}: updates the image stays");
        return cursor;
    }

    /// <summary>
    /// Asserts the Samus-nearness branch at <paramref name="cursor"/>: its reach across and up or down, and the list it
    /// continues at; returns the cursor after it.
    /// </summary>
    private static ushort ExpectNear(IImportCartridgeSource cartridge, ushort cursor, Extent16<RoomBlocks> reach, ushort target, string at)
    {
        VerificationAssertions.AssertEqual(RoomObjectInstruction.GotoIfSamusNear, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"{at} at $84:{cursor:X4}");
        VerificationAssertions.AssertEqual(reach, new Extent16<RoomBlocks>(ReadOperandByte(cartridge, cursor + 2), ReadOperandByte(cartridge, cursor + 3)),
            $"{at}: reach");
        VerificationAssertions.AssertEqual(target, Word(cartridge, cursor + 4), $"{at}: target");
        return unchecked((ushort)(cursor + 6));
    }

    /// <summary>
    /// Asserts <paramref name="instruction"/>, <paramref name="length"/> bytes long with its operands, at <paramref name="cursor"/>;
    /// returns the cursor after it.
    /// </summary>
    private static ushort Expect(IImportCartridgeSource cartridge, ushort cursor, RoomObjectInstruction instruction, int length, string at)
    {
        VerificationAssertions.AssertEqual(instruction, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"{at} at $84:{cursor:X4}");
        return unchecked((ushort)(cursor + length));
    }

    /// <summary>
    /// Asserts <paramref name="instruction"/> at <paramref name="cursor"/> with its word operand <paramref name="operand"/>; returns
    /// the cursor after it.
    /// </summary>
    private static ushort ExpectGoto(IImportCartridgeSource cartridge, ushort cursor, RoomObjectInstruction instruction, ushort operand, string at)
    {
        VerificationAssertions.AssertEqual(instruction, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"{at} at $84:{cursor:X4}");
        VerificationAssertions.AssertEqual(operand, Word(cartridge, cursor + 2), $"{at}: operand at $84:{cursor:X4}");
        return unchecked((ushort)(cursor + 4));
    }

    /// <summary>The native word at a bank-$84 offset.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
}
