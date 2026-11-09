using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the typed door caps (#1554): it walks each cap's native bank-$84 lists beside the sequences, counts
/// and wakes the typed caps hold, comparing every image (its draw and the updates it stays), every sound and where it is
/// requested, and every branch, link, wake, count, sleep, goto and delete that orders them. It also names the phase a
/// recorded grey cap's native instruction stands for. Only verification reads cartridge lists; the port runs the typed
/// caps.
/// </summary>
internal static class DoorObjectNativeProgramOracle
{
    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a draw's duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>The bytes of the queue-sound instruction: its word and the sound's byte.</summary>
    private const int QueueSoundLength = 3;

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
            ushort cursor = CompareSteps(cartridge, lists.Opening, DoorCapSequenceDefinitions.BlueOpening, DoorCapPaint.Blue, facing, $"{at} opening");
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} opening leaves the room");
            cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.BlueClosing, DoorCapPaint.Blue, facing, $"{at} closing");
            VerificationAssertions.AssertEqual(lists.Closed, cursor, $"{at} closing falls into its closed image");
            cursor = Expect(cartridge, lists.Closed, RoomObjectInstruction.SetObjectBtsFromByte, 3, $"{at} is made shootable before its closed image");
            cursor = CompareSteps(cartridge, cursor, DoorCapSequenceDefinitions.BlueClosed, DoorCapPaint.Blue, facing, $"{at} closed");
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} closed leaves the room");
            images += DoorCapSequenceDefinitions.BlueOpening.Count + DoorCapSequenceDefinitions.BlueClosing.Count + DoorCapSequenceDefinitions.BlueClosed.Count;
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
            DoorCapPaint paint = DoorCaps.PaintOf(color);
            string at = $"{color} {facing} cap";

            ushort cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.CapClosing, paint, facing, $"{at} closing");
            VerificationAssertions.AssertEqual(lists.Waiting, cursor, $"{at} closing falls into its first look");

            cursor = ExpectGoto(cartridge, lists.Waiting, RoomObjectInstruction.GotoIfDoorBitSet, DoorObjectNativeListAddresses.Blue(facing).Closed,
                $"{at} turns into its closed blue cap once opened");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.LinkInstruction, lists.Hit, $"{at} wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.ColoredDoorShotWakes[color],
                $"{at} takes its colour's shot");
            cursor = CompareSteps(cartridge, cursor, sequences.Waiting, paint, facing, $"{at} waiting");
            VerificationAssertions.AssertEqual(lists.Sleep, cursor, $"{at} rests after its closed image");
            _ = Expect(cartridge, lists.Sleep, RoomObjectInstruction.Sleep, 2, $"{at} rests");

            VerificationAssertions.AssertEqual(RoomObjectInstruction.IncrementDoorHitCounterAndGoto, InstructionAt<RoomObjectInstruction>(cartridge, lists.Hit),
                $"{at} counts its hit");
            VerificationAssertions.AssertEqual(ColoredDoorDefinitions.HitThresholds[color], (int)ReadOperandByte(cartridge, lists.Hit + 2), $"{at} hit count");
            VerificationAssertions.AssertEqual(lists.Open, Word(cartridge, lists.Hit + 3), $"{at} opens at its count");
            cursor = CompareSteps(cartridge, unchecked((ushort)(lists.Hit + 5)), sequences.Flash, paint, facing, $"{at} flash");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Sleep, $"{at} rests again after its flash");

            cursor = CompareSteps(cartridge, lists.Open, sequences.Opening, paint, facing, $"{at} opening");
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} leaves the room once open");
            images += DoorCapSequenceDefinitions.CapClosing.Count + sequences.Waiting.Count + sequences.Flash.Count + sequences.Opening.Count;
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
            string at = variant.BombTorizo ? "Bomb Torizo's grey cap" : $"grey {facing} cap";

            ushort cursor;
            if (variant.BombTorizo)
            {
                cursor = CompareSteps(cartridge, lists.Closing, GreyDoorDefinitions.BombTorizoHeldOpen, DoorCapPaint.Grey, facing, $"{at} held open");
                cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.GotoIfSamusHasNoBombs, lists.Closing, $"{at} stays open until Samus has Bombs");
                cursor = CompareSteps(cartridge, cursor, GreyDoorDefinitions.BombTorizoClosing, DoorCapPaint.Grey, facing, $"{at} closing");
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Waiting, $"{at} closing goes to its first look");
                images += GreyDoorDefinitions.BombTorizoHeldOpen.Count + GreyDoorDefinitions.BombTorizoClosing.Count;
            }
            else
            {
                cursor = CompareSteps(cartridge, lists.Closing, DoorCapSequenceDefinitions.CapClosing, DoorCapPaint.Grey, facing, $"{at} closing");
                VerificationAssertions.AssertEqual(lists.Waiting, cursor, $"{at} closing falls into its first look");
                images += DoorCapSequenceDefinitions.CapClosing.Count;
            }

            cursor = ExpectGoto(cartridge, lists.Waiting, RoomObjectInstruction.GotoIfDoorBitSet, DoorObjectNativeListAddresses.Blue(facing).Closed,
                $"{at} turns into its closed blue cap once opened");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.LinkInstruction, lists.Activate, $"{at} wakes at its unlock");
            cursor = Expect(cartridge, cursor, RoomObjectInstruction.SetGreyDoorFrameBehavior, 2, $"{at} waits for its condition");
            cursor = CompareSteps(cartridge, cursor, GreyDoorDefinitions.Locked, DoorCapPaint.Grey, facing, $"{at} locked");
            VerificationAssertions.AssertEqual(lists.Sleep, cursor, $"{at} rests after its closed image");
            _ = Expect(cartridge, lists.Sleep, RoomObjectInstruction.Sleep, 2, $"{at} rests");

            cursor = ExpectGoto(cartridge, lists.Activate, RoomObjectInstruction.LinkInstruction, lists.Hit, $"{at} unlocked wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.GreyDoorShotWake, $"{at} unlocked takes any shot");
            VerificationAssertions.AssertEqual(lists.Flash, cursor, $"{at} unlocked falls into its flash");
            cursor = CompareSteps(cartridge, lists.Flash, GreyDoorDefinitions.Flash, DoorCapPaint.Grey, facing, $"{at} flash");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, lists.Flash, $"{at} flash repeats");

            VerificationAssertions.AssertEqual(RoomObjectInstruction.IncrementDoorHitCounterAndGoto, InstructionAt<RoomObjectInstruction>(cartridge, lists.Hit),
                $"{at} counts its hit");
            VerificationAssertions.AssertEqual(GreyDoorDefinitions.HitThreshold, (int)ReadOperandByte(cartridge, lists.Hit + 2), $"{at} hit count");
            VerificationAssertions.AssertEqual(lists.Open, Word(cartridge, lists.Hit + 3), $"{at} opens at its count");
            VerificationAssertions.AssertEqual(lists.Open, unchecked((ushort)(lists.Hit + 5)), $"{at} hit falls into its opening");

            cursor = CompareSteps(cartridge, lists.Open, GreyDoorDefinitions.Opening, DoorCapPaint.Grey, facing, $"{at} opening");
            _ = Expect(cartridge, cursor, RoomObjectInstruction.Delete, 2, $"{at} leaves the room once open");
            images += GreyDoorDefinitions.Locked.Count + GreyDoorDefinitions.Flash.Count + GreyDoorDefinitions.Opening.Count;
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

            cursor = CompareEyeSteps(cartridge, eye.Idle, EyeDoorSequenceDefinitions.Closed, orientation, EyeDoorComponent.Eye, $"{at} closed");
            cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.WakeReach, eye.Activate, $"{at} opens once Samus is near");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Idle, $"{at} stays closed otherwise");

            cursor = ExpectGoto(cartridge, eye.Activate, RoomObjectInstruction.LinkInstruction, eye.Hit, $"{at} wakes at its hit");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.EyeDoorMissileWake, $"{at} takes missiles");
            cursor = CompareEyeSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Opening, orientation, EyeDoorComponent.Eye, $"{at} opening");
            VerificationAssertions.AssertEqual(eye.Watch, cursor, $"{at} opening falls into its watch");

            cursor = ExpectNear(cartridge, eye.Watch, EyeDoorSequenceDefinitions.InFrontReach, eye.Close, $"{at} blinks while Samus is in front");
            cursor = CompareEyeSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Watching, orientation, EyeDoorComponent.Eye, $"{at} watching");
            cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.WakeReach, eye.Watch, $"{at} watches again while Samus is near");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, eye.Idle, $"{at} closes once Samus has left");

            cursor = CompareEyeSteps(cartridge, eye.Close, EyeDoorSequenceDefinitions.Blinking, orientation, EyeDoorComponent.Eye, $"{at} blinking");
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
            cursor = CompareEyeSteps(cartridge, unchecked((ushort)(cursor + 5)), EyeDoorSequenceDefinitions.Flinching, orientation, EyeDoorComponent.Eye, $"{at} flinching");
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

            cursor = CompareEyeSteps(cartridge, eye.Opening, EyeDoorSequenceDefinitions.OpeningFlash, orientation, EyeDoorComponent.Eye, $"{at} flash");
            cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.DecrementTimerAndGoto, eye.Opening, $"{at} flashes again until counted");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, blueClosed, $"{at} turns into its closed blue cap");

            cursor = Expect(cartridge, eye.Opened, makeBlue, 2, $"{at} of an opened door moves onto its blue cap");
            _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, blueClosed, $"{at} of an opened door turns into its closed blue cap");
            images += EyeDoorSequenceDefinitions.Closed.Count + EyeDoorSequenceDefinitions.Opening.Count + EyeDoorSequenceDefinitions.Watching.Count
                + EyeDoorSequenceDefinitions.Blinking.Count + EyeDoorSequenceDefinitions.Flinching.Count + EyeDoorSequenceDefinitions.OpeningFlash.Count;

            foreach (EyeDoorComponent component in (EyeDoorComponent[])[EyeDoorComponent.Door, EyeDoorComponent.Bottom])
            {
                NativeEyeDoorPartLists part = DoorObjectNativeListAddresses.Part(orientation, component);
                string where = $"{orientation} eye door {component}";
                cursor = ExpectGoto(cartridge, part.Start, RoomObjectInstruction.GotoIfDoorBitSet, part.Delete, $"{where} leaves once opened");
                VerificationAssertions.AssertEqual(part.Idle, cursor, $"{where} first look falls into its still pose");
                cursor = ExpectNear(cartridge, part.Idle, EyeDoorSequenceDefinitions.PulseReach, part.Activate, $"{where} pulses once Samus is near");
                cursor = CompareEyeSteps(cartridge, cursor, EyeDoorSequenceDefinitions.Still, orientation, component, $"{where} still");
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, part.Idle, $"{where} looks for Samus again");
                cursor = ExpectGoto(cartridge, part.Activate, RoomObjectInstruction.LinkInstruction, part.Delete, $"{where} wakes at its delete");
                cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.InstallFrameBehavior, DoorsNativeWords.EyeDoorOpenedDoorWake, $"{where} waits for its door");
                VerificationAssertions.AssertEqual(part.Cycle, cursor, $"{where} falls into its pulse");
                cursor = CompareEyeSteps(cartridge, part.Cycle, EyeDoorSequenceDefinitions.Pulse[orientation], orientation, component, $"{where} pulse");
                cursor = ExpectNear(cartridge, cursor, EyeDoorSequenceDefinitions.PulseReach, part.Cycle, $"{where} pulses again while Samus is near");
                _ = ExpectGoto(cartridge, cursor, RoomObjectInstruction.Goto, part.Idle, $"{where} stands still once Samus has left");
                _ = Expect(cartridge, part.Delete, RoomObjectInstruction.Delete, 2, $"{where} leaves the room");
                images += EyeDoorSequenceDefinitions.Still.Count + EyeDoorSequenceDefinitions.Pulse[orientation].Count;
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
    /// Compares the native timed draws from <paramref name="cursor"/> with <paramref name="sequence"/>, each with the
    /// sound requested just before it; returns the cursor after the last draw.
    /// </summary>
    private static ushort CompareSteps(IImportCartridgeSource cartridge, ushort cursor, IReadOnlyList<DoorCapStep> sequence, DoorCapPaint paint,
        ColoredDoorOrientation facing, string at)
    {
        for (int index = 0; index < sequence.Count; index++)
        {
            DoorCapStep step = sequence[index];
            string where = $"{at} image {index} at $84:{cursor:X4}";
            SoundEffectId? sound = null;
            if (InstructionAt<RoomObjectInstruction>(cartridge, cursor) == RoomObjectInstruction.QueueSoundLibrary3Maximum6)
            {
                sound = QueuedLibrary3Sound(cartridge, cursor);
                VerificationAssertions.AssertEqual(SoundRequestPriority.Normal, DoorCapSequenceDefinitions.SoundPriority, $"{where}: sound priority");
                cursor = unchecked((ushort)(cursor + QueueSoundLength));
            }
            ushort duration = Word(cartridge, cursor);
            VerificationAssertions.AssertTrue((duration & InstructionBit) == 0, $"{where}: the native list draws");
            VerificationAssertions.AssertEqual(duration, step.Updates, $"{where}: updates the image stays");
            VerificationAssertions.AssertEqual(Word(cartridge, cursor + 2), RoomObjectDrawAddressCodec.Of(DoorCaps.Image(paint, facing, step.Image)), $"{where}: draw");
            VerificationAssertions.AssertEqual(sound, step.SoundBefore, $"{where}: sound with the image");
            cursor = unchecked((ushort)(cursor + 4));
        }
        return cursor;
    }

    /// <summary>
    /// Compares the native timed draws from <paramref name="cursor"/> with an eye door's <paramref name="sequence"/>, each
    /// with the shot, smoke or sweat spawned just before it; returns the cursor after the last draw.
    /// </summary>
    private static ushort CompareEyeSteps(IImportCartridgeSource cartridge, ushort cursor, IReadOnlyList<EyeDoorStep> sequence,
        EyeDoorOrientation orientation, EyeDoorComponent component, string at)
    {
        // The shot and sweat operands are the byte offsets of the facing's origin and launch pairs ($86:B65B and
        // $86:B6B1), which LookupStream4 ties to the typed facings.
        const ushort LeftOperand = 0;
        const ushort RightShotOperand = 20;
        const ushort RightSweatOperand = 4;
        for (int index = 0; index < sequence.Count; index++)
        {
            EyeDoorStep step = sequence[index];
            string where = $"{at} image {index} at $84:{cursor:X4}";
            switch (step.Before)
            {
                case EyeDoorAction.None:
                    break;
                case EyeDoorAction.Shoot:
                    cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.ShootEyeDoorProjectile,
                        orientation == EyeDoorOrientation.Right ? RightShotOperand : LeftOperand, $"{where}: shot");
                    break;
                case EyeDoorAction.SpawnSweat:
                    cursor = ExpectGoto(cartridge, cursor, RoomObjectInstruction.SpawnEyeDoorSweat,
                        orientation == EyeDoorOrientation.Right ? RightSweatOperand : LeftOperand, $"{where}: sweat");
                    break;
                case EyeDoorAction.SpawnTwoSmoke:
                    cursor = Expect(cartridge, cursor, RoomObjectInstruction.SpawnTwoEyeDoorSmoke, 2, $"{where}: smoke");
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(EyeDoorAction)} {step.Before}.");
            }
            ushort duration = Word(cartridge, cursor);
            VerificationAssertions.AssertTrue((duration & InstructionBit) == 0, $"{where}: the native list draws");
            VerificationAssertions.AssertEqual(duration, step.Updates, $"{where}: updates the image stays");
            VerificationAssertions.AssertEqual(Word(cartridge, cursor + 2),
                RoomObjectDrawAddressCodec.Of(EyeDoorDraws.DrawList(orientation, component, step.Image)), $"{where}: draw");
            cursor = unchecked((ushort)(cursor + 4));
        }
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
