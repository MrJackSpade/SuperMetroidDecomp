using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectNativeListAddresses;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// #1296: the room PLM programs typed in place of their bank-$84 lists. Each typed program must equal its native list
/// step by step: draws and durations, deletion, sleeps, and the cue of every other instruction with its operands. The
/// families' runtime suites check what those cues do.
/// </summary>
internal static class RoomObjectProgramsVerification
{
    /// <summary>
    /// The glass tube's native lists do what <see cref="NoobTubeObject"/> does (#1554): a tube broken earlier restores the
    /// water and ends; otherwise it links its input wait, arms its Power Bomb wait, shows the intact tube and sleeps; the
    /// input wait links the break, arms its input wait and sleeps; the break stops waiting, locks Samus, cracks, shows each
    /// cracking image, shatters with its sound, debris and earthquake, shows the open tube, sets the broken event,
    /// restores the water, releases Samus and ends. Every image and operand is compared with
    /// <see cref="NoobTubeObjectDefinitions"/>.
    /// </summary>
    internal static void VerifyNoobTubePrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, TerrainChangeFrame frame, string at)
        {
            AssertEqual(frame.Updates, (int)Word(cursor), $"{at} updates");
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(frame.Draw), (int)Word(cursor + 2), $"{at} image");
        }

        int main = NoobTubeList;
        AssertEqual(RoomObjectInstruction.GotoIfEventSet, At(main), "tube first checks whether it is broken");
        AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), (int)Word(main + 2), "tube broken event");
        int alreadyBroken = Word(main + 4);
        AssertEqual(RoomObjectInstruction.LinkInstruction, At(main + 6), "tube links its input wait");
        int waitForInput = Word(main + 8);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(main + 10), "tube arms its Power Bomb wait");
        AssertEqual(NoobTubePhase.WaitingForPowerBomb, RoomObjectFrameBehaviorNativeWords.NoobTube.Decode(Word(main + 12), "tube wait"), "tube Power Bomb wait");
        AssertShown(main + 14, new TerrainChangeFrame(RoomObjectDraw.Of(NoobTubeObjectDefinitions.IntactImage), 1), "intact tube");
        AssertEqual(RoomObjectInstruction.Sleep, At(main + 18), "intact tube waits");

        AssertEqual(RoomObjectInstruction.EnableNoobTubeWaterPhysics, At(alreadyBroken), "broken tube restores the water");
        AssertEqual(RoomObjectInstruction.Delete, At(alreadyBroken + 2), "broken tube ends");

        AssertEqual(RoomObjectInstruction.LinkInstruction, At(waitForInput), "input wait links the break");
        int cursor = Word(waitForInput + 2);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(waitForInput + 4), "tube arms its input wait");
        AssertEqual(NoobTubePhase.WaitingForInput, RoomObjectFrameBehaviorNativeWords.NoobTube.Decode(Word(waitForInput + 6), "tube wait"), "tube input wait");
        AssertEqual(RoomObjectInstruction.Sleep, At(waitForInput + 8), "tube waits for input");

        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, At(cursor), "break stops waiting");
        AssertEqual(RoomObjectInstruction.LockSamus, At(cursor + 2), "break locks Samus");
        AssertEqual(RoomObjectInstruction.SpawnNoobTubeCrack, At(cursor + 4), "break cracks the glass");
        cursor += 6;
        for (int index = 0; index < NoobTubeObjectDefinitions.Cracking.Count; index++, cursor += 4)
            AssertShown(cursor, NoobTubeObjectDefinitions.Cracking[index], $"cracking image {index}");
        AssertEqual(RoomObjectInstruction.QueueSoundLibrary2Maximum6, At(cursor), "the tube shatters with its sound");
        AssertEqual(NoobTubeObjectDefinitions.BreakSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), "tube break sound");
        AssertEqual(SoundRequestPriority.Normal, NoobTubeObjectDefinitions.BreakSoundPriority, "tube break sound priority");
        AssertEqual(RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles, At(cursor + 3), "the tube throws its shards and bubbles");
        AssertEqual(RoomObjectInstruction.TriggerNoobTubeEarthquake, At(cursor + 5), "the tube shakes the room");
        AssertShown(cursor + 7, NoobTubeObjectDefinitions.Shattered, "open tube");
        AssertEqual(RoomObjectInstruction.SetEvent, At(cursor + 11), "the open tube sets its event");
        AssertEqual(NoobTubeObjectDefinitions.BrokenEvent.Number(), (int)Word(cursor + 13), "tube broken event set");
        AssertEqual(RoomObjectInstruction.EnableNoobTubeWaterPhysics, At(cursor + 15), "the open tube restores the water");
        AssertEqual(RoomObjectInstruction.UnlockSamus, At(cursor + 17), "the open tube releases Samus");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 19), "the open tube ends");
        Console.WriteLine("  N00b tube lists match its typed course in bank $84.");
    }

    /// <summary>
    /// The lavaquake's native list does what <see cref="SpeedBoosterEscapeObject"/> does (#1554): three waits in order,
    /// each installed and slept on until it wakes the next, the last never waking.
    /// </summary>
    internal static void VerifySpeedBoosterEscapePrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        SpeedBoosterEscapePhase[] waits =
            [SpeedBoosterEscapePhase.WaitingForSpeedBooster, SpeedBoosterEscapePhase.WaitingForSamusLeft, SpeedBoosterEscapePhase.AdvancingLava];
        for (int index = 0; index < waits.Length; index++)
        {
            int cursor = SpeedBoosterEscapeList + index * 6;
            AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, cursor), $"lavaquake wait {index} is installed");
            AssertEqual(waits[index], RoomObjectFrameBehaviorNativeWords.SpeedBoosterEscape.Decode(cartridge.ReadCartridgeWord(ProgramBank | (cursor + 2)),
                "lavaquake wait"), $"lavaquake wait {index} is the typed object's");
            AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, cursor + 4), $"lavaquake wait {index} sleeps until it wakes the next");
        }
        Console.WriteLine($"  Speed Booster lavaquake list: {waits.Length} waits match bank $84.");
    }

    /// <summary>
    /// The block reactions play as their native lists do (#1554): every image and how long it stays, the sound before it
    /// with its priority, each restore of the saved block, the respawning Grapple block's behaviour restore before its
    /// restored block, and the end after the last image. A bomb block's sound and its shared tail are followed through the
    /// native goto.
    /// </summary>
    internal static void VerifyBlockReactionPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int frames = 0;
        foreach ((BlockReactionChange change, ushort entry) in BlockReactionEntries)
            frames += CompareBlockReaction(cartridge, change, entry, change == BlockReactionChangeDefinitions.RespawningBreakableGrappleBlock);
        Console.WriteLine($"  Block reactions: {frames} typed images match bank $84.");
    }

    /// <summary>The Speed Booster blocks' crumbles play as their native lists do; both restore instructions restore the saved block.</summary>
    internal static void VerifySpeedBoosterBlockPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        int frames = 0;
        foreach ((BlockReactionChange change, ushort entry) in SpeedBoosterBlockEntries)
            frames += CompareBlockReaction(cartridge, change, entry, restoresGrappleBehaviour: false);
        AssertEqual(SoundRequestPriority.WhenIdle, SpeedBoosterBlockObjectDefinitions.BreakSoundPriority, "speed block break sound priority");
        Console.WriteLine($"  Speed Booster blocks: {frames} typed images match bank $84.");
    }

    /// <summary>
    /// Plays the native list at <paramref name="entry"/> beside <paramref name="change"/>, image by image, and returns the
    /// images compared.
    /// </summary>
    private static int CompareBlockReaction(IImportCartridgeSource cartridge, BlockReactionChange change, ushort entry, bool restoresGrappleBehaviour)
    {
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        int cursor = entry;
        int frame = 0;
        SoundEffectId? sound = null;
        SoundRequestPriority priority = SoundRequestPriority.Normal;
        bool grappleRestored = false;
        for (int command = 0; command < 256; command++)
        {
            string at = $"block reaction list $84:{entry:X4} image {frame} at $84:{cursor:X4}";
            int word = Word(cursor);
            if (word < 0x8000)
            {
                AssertTrue(frame < change.Frames.Count, $"{at}: the typed reaction ends before the native list draws");
                BlockReactionFrame expected = change.Frames[frame];
                RoomObjectDraw draw = expected.Draw ?? throw new InvalidDataException($"{at}: the native image draws where the typed one restores the block.");
                AssertEqual(expected.Updates, word, $"{at}: updates");
                AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(draw), (int)Word(cursor + 2), $"{at}: draw");
                AssertEqual(sound, expected.SoundBefore, $"{at}: sound with the image");
                if (sound is not null)
                    AssertEqual(priority, expected.SoundPriority, $"{at}: sound priority");
                sound = null;
                frame++;
                cursor += 4;
                continue;
            }
            RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
            if (instruction == RoomObjectInstruction.Delete)
            {
                AssertEqual(change.Frames.Count, frame, $"{at}: the native list ends after the typed images");
                AssertEqual<SoundEffectId?>(null, sound, $"{at}: no sound is left for the end");
                AssertEqual(restoresGrappleBehaviour, grappleRestored, $"{at}: the Grapple behaviour restore");
                return frame;
            }
            if (instruction == RoomObjectInstruction.Goto)
            {
                cursor = Word(cursor + 2);
                continue;
            }
            if (instruction is RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct or RoomObjectInstruction.QueueSoundLibrary2Maximum3
                or RoomObjectInstruction.QueueSoundLibrary2Maximum6)
            {
                AssertEqual<SoundEffectId?>(null, sound, $"{at}: one sound per image");
                sound = SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2));
                priority = instruction == RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct ? SoundRequestPriority.WhenIdle
                    : instruction == RoomObjectInstruction.QueueSoundLibrary2Maximum3 ? SoundRequestPriority.Low
                    : SoundRequestPriority.Normal;
                cursor += 3;
                continue;
            }
            if (instruction == RoomObjectInstruction.SetObjectBtsToOne)
            {
                AssertTrue(restoresGrappleBehaviour && frame < change.Frames.Count && change.Frames[frame].Draw is null,
                    $"{at}: only the respawning Grapple block restores its behaviour, just before its restored block");
                grappleRestored = true;
                cursor += 2;
                continue;
            }
            if (instruction is RoomObjectInstruction.DrawObjectBlock or RoomObjectInstruction.DrawObjectBlockClone)
            {
                AssertTrue(frame < change.Frames.Count && change.Frames[frame].Draw is null, $"{at}: the typed image restores the saved block");
                AssertEqual(1, change.Frames[frame].Updates, $"{at}: the restored block stays one update");
                AssertEqual<SoundEffectId?>(null, sound, $"{at}: no sound with the restored block");
                frame++;
                cursor += 2;
                continue;
            }
            throw new InvalidDataException($"{at}: native instruction ${word:X4} is no block-reaction step.");
        }
        throw new InvalidDataException($"Block reaction list $84:{entry:X4} ran past its instruction limit without its end.");
    }

    /// <summary>The objects that were the scripted room programs play as their native lists do.</summary>
    internal static void VerifyScriptedRoomPrograms(ISnesAddressSpace rom) => VerifyScriptedTerrainChanges(rom);

    /// <summary>
    /// The room objects that left the scripted program set play as their native lists do (#1554); the critters' wall
    /// marks the critters escaped and the elevatube plays its sound as each ends.
    /// </summary>
    private static void VerifyScriptedTerrainChanges(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var changes = new Dictionary<string, TerrainChange>
        {
            [nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube)] = ScriptedTerrainChangeDefinitions.MaridiaElevatube,
            [nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough)] = ScriptedTerrainChangeDefinitions.CritterWallBreakthrough,
            [nameof(ScriptedTerrainChangeDefinitions.CrateriaEscapePassage)] = ScriptedTerrainChangeDefinitions.CrateriaEscapePassage,
            [nameof(ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak)] = ScriptedTerrainChangeDefinitions.EnemyBreakableBlockBreak,
        };
        int frames = 0;
        foreach ((string name, ushort entry) in ScriptedTerrainChangeEntries)
        {
            frames += NativeTerrainChanges.Compare(cartridge, changes[name], entry, ScriptedTerrainStepAt, name, out bool began,
                out IReadOnlyList<NativeTerrainChanges.NativeStep> atEnd);
            AssertTrue(!began, $"{name} opens nothing as it starts");
            string ending = string.Join(", ", atEnd.Select(step => step.Effect ?? $"{step.Sound} at {step.Priority}"));
            string expected = name == nameof(ScriptedTerrainChangeDefinitions.CritterWallBreakthrough) ? "critters escaped"
                : name == nameof(ScriptedTerrainChangeDefinitions.MaridiaElevatube) ? $"{(SoundEffectId?)EnemySound.Elevatube} at {SoundRequestPriority.Normal}"
                : "";
            AssertEqual(expected, ending, $"{name} acts as it ends exactly as its typed object does");
        }
        Console.WriteLine($"  Scripted terrain changes: {frames} typed frames match bank $84.");
    }

    /// <summary>The enemies' crumble and elevatube sounds at their priorities, the critters' escape and the four-block move right.</summary>
    private static bool ScriptedTerrainStepAt(IImportCartridgeSource cartridge, ushort cursor, out NativeTerrainChanges.NativeStep step)
    {
        RoomObjectInstruction instruction = InstructionAt<RoomObjectInstruction>(cartridge, cursor);
        if (instruction is RoomObjectInstruction.QueueSoundLibrary2Maximum3 or RoomObjectInstruction.QueueSoundLibrary2Maximum6)
        {
            step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero,
                SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)), Begins: false, Length: 3,
                priority: instruction == RoomObjectInstruction.QueueSoundLibrary2Maximum3 ? SoundRequestPriority.Low : SoundRequestPriority.Normal);
            return true;
        }
        if (instruction == RoomObjectInstruction.SetAnimalsEscapedEvent)
        {
            step = new NativeTerrainChanges.NativeStep(Offset16<RoomBlocks>.Zero, Sound: null, Begins: false, Length: 2, Effect: "critters escaped");
            return true;
        }
        if (instruction == RoomObjectInstruction.MoveRightFourBlocks)
        {
            step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(4, 0), Sound: null, Begins: false, Length: 2);
            return true;
        }
        step = default;
        return false;
    }

    /// <summary>
    /// The downward gate's native lists do what <see cref="DownwardGateObject"/> does (#1554): the closed gate shows for
    /// one update, clears its trigger, arms its closed wake and sleeps; past the sleep it wakes its projectile, plays the
    /// movement sound and rises image by image into the open gate, which shows for one update, clears its trigger, arms
    /// its open wake and sleeps; past that sleep it shows the open gate, spawns its moving projectile with the movement
    /// sound and lowers image by image into the closed gate. Every image, hold and operand is compared with
    /// <see cref="DownwardGateMovementDefinitions"/>.
    /// </summary>
    internal static void VerifyDownwardGatePrograms(ISnesAddressSpace rom)
    {
        IReadOnlyDictionary<GateState, ushort> addresses = DownwardGateAddresses(rom);
        Console.WriteLine($"  Downward gate lists: {addresses.Count} typed gate states match bank $84.");
    }

    /// <summary>
    /// The native instruction a downward gate in the typed state of <paramref name="gate"/> rests on at the end of an
    /// update, for comparisons with native gate traces: the instruction after the image it shows, or the sleep it rests
    /// on.
    /// </summary>
    internal static ushort DownwardGateNativeInstruction(ISnesAddressSpace rom, DownwardGateObject gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        downwardGateAddresses ??= DownwardGateAddresses(rom);
        int frame = gate.Phase is DownwardGatePhase.Closing or DownwardGatePhase.Reopening ? gate.Frame : 0;
        return downwardGateAddresses.TryGetValue(new GateState(gate.Phase, frame), out ushort address)
            ? address
            : throw new InvalidDataException($"A downward gate in {gate.Phase} at image {frame} has no native instruction.");
    }

    /// <summary>The native instruction of every typed gate state, walked once.</summary>
    private static Dictionary<GateState, ushort>? downwardGateAddresses;

    /// <summary>
    /// Walks the gate's native lists from its header, checking each against the typed course, and returns the instruction
    /// each typed state rests on.
    /// </summary>
    private static Dictionary<GateState, ushort> DownwardGateAddresses(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, TerrainChangeFrame frame, string at)
        {
            AssertEqual(frame.Updates, (int)Word(cursor), $"{at} updates");
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(frame.Draw), (int)Word(cursor + 2), $"{at} image");
        }
        void AssertMovementSound(int cursor, string at)
        {
            AssertEqual(RoomObjectInstruction.QueueSoundLibrary3Maximum6, At(cursor), $"{at} movement sound");
            AssertEqual(DownwardGateDefinitions.MovementSound, SoundEffectCommands.Decode(SoundEffectLibrary.Room, ReadOperandByte(cartridge, cursor + 2)),
                $"{at} movement sound identity");
            AssertEqual(SoundRequestPriority.Normal, DownwardGateMovementDefinitions.MovementSoundPriority, $"{at} movement sound priority");
        }
        // Each resting list: its image for one update, the trigger cleared, its wake armed, the sleep; the continuation
        // after the sleep follows it.
        int Resting(int list, RoomObjectDraw image, DownwardGatePhase showing, DownwardGatePhase resting, Dictionary<GateState, ushort> into)
        {
            AssertShown(list, new TerrainChangeFrame(image, 1), $"{showing}");
            AssertEqual(RoomObjectInstruction.ClearDownwardGateTrigger, At(list + 4), $"{resting} clears the trigger");
            AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(list + 6), $"{resting} arms its wake");
            AssertEqual(resting, RoomObjectFrameBehaviorNativeWords.DownwardGate.Decode(Word(list + 8), "downward gate wake"), $"{resting} wake");
            AssertEqual(RoomObjectInstruction.Sleep, At(list + 10), $"{resting} sleeps");
            into[new GateState(showing, 0)] = checked((ushort)(list + 4));
            into[new GateState(resting, 0)] = checked((ushort)(list + 10));
            return list + 12;
        }

        var addresses = new Dictionary<GateState, ushort>();
        int closed = Word(RoomObjectNativeWords.Word(RoomObjectKind.DownwardGate) + 2);
        addresses[new GateState(DownwardGatePhase.Starting, 0)] = checked((ushort)closed);
        int reopen = Resting(closed, DownwardGateMovementDefinitions.ClosedImage, DownwardGatePhase.ShowingClosed, DownwardGatePhase.RestingClosed, addresses);
        AssertEqual(RoomObjectInstruction.WakeDownwardGateProjectile, At(reopen), "reopening wakes the gate projectile");
        AssertMovementSound(reopen + 4, "reopening");
        int cursor = reopen + 7;
        for (int index = 0; index < DownwardGateMovementDefinitions.Reopening.Count; index++, cursor += 4)
        {
            AssertShown(cursor, DownwardGateMovementDefinitions.Reopening[index], $"reopening image {index}");
            addresses[new GateState(DownwardGatePhase.Reopening, index)] = checked((ushort)(cursor + 4));
        }
        AssertEqual(RoomObjectInstruction.Goto, At(cursor), "reopening goes on to the open gate");
        int open = Word(cursor + 2);
        int close = Resting(open, DownwardGateMovementDefinitions.OpenImage, DownwardGatePhase.ShowingOpen, DownwardGatePhase.RestingOpen, addresses);
        cursor = close;
        for (int index = 0; index < DownwardGateMovementDefinitions.Closing.Count; index++, cursor += 4)
        {
            if (index == DownwardGateMovementDefinitions.ClosingMovementStart)
            {
                AssertEqual(RoomObjectInstruction.SpawnDownwardGateProjectile, At(cursor), "closing spawns the moving gate");
                AssertEqual(EnemyProjectileNativeWords.Kinds.Word(RoomEnemyProjectileKind.DownwardGateMoving), Word(cursor + 2), "downward gate moving projectile");
                AssertMovementSound(cursor + 4, "closing");
                cursor += 7;
            }
            AssertShown(cursor, DownwardGateMovementDefinitions.Closing[index], $"closing image {index}");
            addresses[new GateState(DownwardGatePhase.Closing, index)] = checked((ushort)(cursor + 4));
        }
        AssertEqual(closed, cursor, "closing runs on into the closed gate");
        return addresses;
    }

    /// <summary>A typed gate state: its phase and, while it moves, the image it shows.</summary>
    /// <param name="Phase">The gate's phase.</param>
    /// <param name="Frame">The image of its movement, or zero at rest.</param>
    private readonly record struct GateState(DownwardGatePhase Phase, int Frame);

    /// <summary>The Tourian elevator access changes play as their native lists do.</summary>
    internal static void VerifyTourianAccessPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(Enum.GetValues<TourianAccessFloor>().Length, TourianAccessEntries.Count, "every Tourian access floor has a native list");
        int frames = 0;
        foreach ((TourianAccessFloor floor, ushort entry) in TourianAccessEntries)
        {
            frames += NativeTerrainChanges.Compare(cartridge, TourianAccessFloors.ForState(floor), entry,
                (IImportCartridgeSource source, ushort cursor, out NativeTerrainChanges.NativeStep step) =>
                {
                    step = new NativeTerrainChanges.NativeStep(new Offset16<RoomBlocks>(0, 1), Sound: null, Begins: false, Length: 2);
                    return InstructionAt<RoomObjectInstruction>(source, cursor) == RoomObjectInstruction.MoveTourianAccessDown;
                }, $"Tourian access {floor}", out bool began);
            AssertTrue(!began, $"Tourian access {floor} opens nothing as it starts");
        }
        Console.WriteLine($"  Tourian access changes: {frames} typed frames match bank $84.");
    }

    /// <summary>
    /// The Wrecked Ship attic's native list does what <see cref="WreckedShipAtticObject"/> does (#1554): it installs a
    /// routine that changes nothing and sleeps for good, so the object only holds its place.
    /// </summary>
    internal static void VerifyWreckedShipAtticPrograms(ISnesAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, InstructionAt<RoomObjectInstruction>(cartridge, WreckedShipAtticList), "attic installs its routine");
        AssertEqual(WreckedShipAtticObjectRomData.NoOpCallback, cartridge.ReadCartridgeWord(ProgramBank | (WreckedShipAtticList + 2)), "attic pre-instruction");
        AssertEqual(RoomObjectInstruction.Sleep, InstructionAt<RoomObjectInstruction>(cartridge, WreckedShipAtticList + 4), "attic sleeps for good");
        Console.WriteLine("  Wrecked Ship attic list: installs its inert routine and sleeps, as its typed object holds its place.");
    }

    /// <summary>The blue caps play as their native lists do (#1554).</summary>
    internal static void VerifyBlueDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareBlueCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Blue door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every coloured cap plays, counts its hits and takes its shot as its native lists do (#1554).</summary>
    internal static void VerifyColoredDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareColoredCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Coloured door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every grey cap, Bomb Torizo's included, plays and counts its hit as its native lists do (#1554).</summary>
    internal static void VerifyGreyDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareGreyCaps(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Grey door caps: {images} typed images match bank $84.");
    }

    /// <summary>Every eye door's eye, body and bottom, both facings, play as their native lists do (#1554).</summary>
    internal static void VerifyEyeDoorPrograms(ISnesAddressSpace rom)
    {
        int images = DoorObjectNativeProgramOracle.CompareEyeDoors(CartridgeImportSource.Require(rom));
        Console.WriteLine($"  Eye doors: {images} typed images match bank $84.");
    }

    /// <summary>
    /// A Brinstar plant's native list plays as its typed plant does (#1554): it installs the hold and counts its chews;
    /// each chew's images, and the sound and damage before them, repeat while the count runs; then the release immunity,
    /// the end of the hold, the closed mouth and the idle plant, and the end. Every image, count and bite is compared
    /// with <paramref name="plant"/>.
    /// </summary>
    internal static void VerifySamusEaterList(ISnesAddressSpace rom, int list, SamusEaterObjectDefinition plant)
    {
        ArgumentNullException.ThrowIfNull(plant);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        ushort Word(int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
        RoomObjectInstruction At(int address) => InstructionAt<RoomObjectInstruction>(cartridge, address);
        void AssertShown(int cursor, SamusEaterDraw image, int updates, string at)
        {
            AssertEqual(updates, (int)Word(cursor), $"{at} updates");
            AssertEqual(SuperMetroid.AssetExtraction.Rooms.RoomObjectDrawAddressCodec.Of(RoomObjectDraw.Of(image)), (int)Word(cursor + 2), $"{at} image");
        }

        string plantName = plant.Ceiling ? "ceiling plant" : "floor plant";
        AssertEqual(RoomObjectInstruction.InstallFrameBehavior, At(list), $"{plantName} first holds Samus");
        AssertEqual(SamusEaterObjectDefinitionsConstants.HoldFrameBehavior, Word(list + 2), $"{plantName} hold");
        AssertEqual(RoomObjectInstruction.SetEightBitTimer, At(list + 4), $"{plantName} counts its chews");
        AssertEqual(plant.Chews, (int)ReadOperandByte(cartridge, list + 6), $"{plantName} chews");
        int chew = list + 7;
        int cursor = chew;
        for (int index = 0; index < plant.Chew.Count; index++)
        {
            SamusEaterChewFrame frame = plant.Chew[index];
            string at = $"{plantName} chew image {index}";
            if (frame.BiteBefore == SamusEaterBite.SoundAndDamage)
            {
                AssertEqual(RoomObjectInstruction.QueueSoundLibrary2Maximum6, At(cursor), $"{at} chew sound");
                AssertEqual(SamusEaterObjectDefinitions.ChewSound, SoundEffectCommands.Decode(SoundEffectLibrary.Enemies, ReadOperandByte(cartridge, cursor + 2)),
                    $"{at} chew sound identity");
                AssertEqual(SoundRequestPriority.Normal, SamusEaterObjectDefinitions.ChewSoundPriority, $"{at} chew sound priority");
                cursor += 3;
            }
            if (frame.BiteBefore != SamusEaterBite.None)
            {
                AssertEqual(RoomObjectInstruction.SamusEaterDamage, At(cursor), $"{at} damage");
                cursor += 2;
            }
            AssertShown(cursor, frame.Image, frame.Updates, at);
            cursor += 4;
        }
        AssertEqual(RoomObjectInstruction.DecrementTimerAndGoto, At(cursor), $"{plantName} repeats its chew");
        AssertEqual(chew, (int)Word(cursor + 2), $"{plantName} repeats from its first chew image");
        AssertEqual(RoomObjectInstruction.SamusEaterReleaseImmunity, At(cursor + 4), $"{plantName} releases Samus");
        AssertEqual(RoomObjectInstruction.ClearFrameBehavior, At(cursor + 6), $"{plantName} stops holding");
        AssertShown(cursor + 8, plant.Released, SamusEaterObjectDefinitions.ReleaseDuration, $"{plantName} closed mouth");
        AssertShown(cursor + 12, plant.Idle, 1, $"{plantName} idle");
        AssertEqual(RoomObjectInstruction.Delete, At(cursor + 16), $"{plantName} ends");
    }
}
