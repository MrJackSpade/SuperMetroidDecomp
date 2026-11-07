using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;
using static SuperMetroid.TestSupport.MovieReplay.NativeObjectListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the door caps' typed PLM programs: the door sounds and grey caps' instructions as their native
/// bank-$84 lists encode them, and the native instruction pointer a grey cap's typed cursor stands for in recordings.
/// </summary>
internal static class DoorObjectNativeProgramOracle
{
    /// <summary>The library-3 sound a queue-sound instruction at <paramref name="cursor"/> names.</summary>
    internal static SoundEffectId QueuedLibrary3Sound(IImportCartridgeSource cartridge, int cursor) =>
        SoundEffectCommands.Decode(SoundEffectLibrary.Library3, ReadOperandByte(cartridge, cursor + 2));

    /// <summary>A door sound as its cue: the open or the close sound.</summary>
    internal static TCue DoorSoundCue<TCue>(SoundEffectId sound, TCue open, TCue close) =>
        sound == BlueDoorObjectProgramDefinitions.OpenSound ? open :
        sound == BlueDoorObjectProgramDefinitions.CloseSound ? close :
        throw new InvalidOperationException($"Door sound {sound} is neither the open nor the close sound.");

    /// <summary>The draw list a grey cap's frame draws.</summary>
    internal static RoomObjectDraw GreyCapDraw(ColoredDoorOrientation orientation, DoorCapFrame frame) => frame switch
    {
        DoorCapFrame.Clear => DoorCaps.Clear(orientation),
        DoorCapFrame.Closed0 => DoorCaps.Grey(orientation, 0),
        DoorCapFrame.Closed1 => DoorCaps.Grey(orientation, 1),
        DoorCapFrame.Closed2 => DoorCaps.Grey(orientation, 2),
        DoorCapFrame.Closed3 => DoorCaps.Grey(orientation, 3),
        DoorCapFrame.BlueFlash => DoorCaps.Blue(orientation, 0),
        _ => throw new InvalidOperationException($"Undefined {nameof(DoorCapFrame)} {frame}."),
    };

    /// <summary>A grey cap's branch, link, wake, hit-count, sound and Bombs-wait instructions with their operands.</summary>
    internal static bool GreyDoorCueAt(IImportCartridgeSource cartridge, ushort cursor, GreyDoorVariant variant, out GreyDoorCue cue, out int length)
    {
        ushort Word(int offset) => cartridge.ReadCartridgeWord(ProgramBank | (cursor + offset));
        var entries = DoorObjectNativeListAddresses.GreyEntries(variant);
        ushort EntryOf(GreyDoorProgram program) => entries.Single(entry => entry.Program == program).Entry;
        switch (InstructionAt<RoomObjectInstruction>(cartridge, cursor))
        {
            case RoomObjectInstruction.GotoIfDoorBitSet:
                VerificationAssertions.AssertEqual(DoorObjectNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorObjectProgramDefinitions.ClosedPrograms[variant.Orientation]).Entry,
                    Word(2), $"grey {variant} becomes its blue cap");
                cue = GreyDoorCue.BranchIfOpened;
                length = 4;
                return true;
            case RoomObjectInstruction.LinkInstruction:
                ushort target = Word(2);
                VerificationAssertions.AssertTrue(target == EntryOf(GreyDoorProgram.Activate) || target == EntryOf(GreyDoorProgram.Hit), $"grey {variant} link target");
                cue = target == EntryOf(GreyDoorProgram.Activate) ? GreyDoorCue.LinkActivate : GreyDoorCue.LinkHit;
                length = 4;
                return true;
            case RoomObjectInstruction.SetGreyDoorFrameBehavior:
                cue = GreyDoorCue.ArmCondition;
                length = 2;
                return true;
            case RoomObjectInstruction.InstallFrameBehavior:
                VerificationAssertions.AssertEqual(DoorsNativeWords.GreyDoorFrameBehaviors.Word(GreyDoorFrameBehavior.WakeWhenShot), Word(2), $"grey {variant} shot wake");
                cue = GreyDoorCue.ArmShotWake;
                length = 4;
                return true;
            case RoomObjectInstruction.IncrementDoorHitCounterAndGoto:
                VerificationAssertions.AssertEqual(GreyDoorObjectProgramDefinitions.HitThreshold, ReadOperandByte(cartridge, cursor + 2), "grey door hit threshold");
                VerificationAssertions.AssertEqual(EntryOf(GreyDoorProgram.Open), Word(3), $"grey {variant} opens at its threshold");
                cue = GreyDoorCue.CountHit;
                length = 5;
                return true;
            case RoomObjectInstruction.QueueSoundLibrary3Maximum6:
                cue = DoorSoundCue(QueuedLibrary3Sound(cartridge, cursor), GreyDoorCue.QueueOpenSound, GreyDoorCue.QueueCloseSound);
                length = 3;
                return true;
            case RoomObjectInstruction.GotoIfSamusHasNoBombs:
                VerificationAssertions.AssertEqual(EntryOf(GreyDoorProgram.Closing), Word(2), "Bomb Torizo door repeats its closing");
                cue = GreyDoorCue.RepeatUntilSamusHasBombs;
                length = 4;
                return true;
            case RoomObjectInstruction.Sleep:
            case RoomObjectInstruction.Delete:
            case RoomObjectInstruction.ClearFrameBehavior:
            case RoomObjectInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomObjectInstruction.GotoIfSamusNear:
            case RoomObjectInstruction.Goto:
            case RoomObjectInstruction.DecrementTimerAndGoto:
            case RoomObjectInstruction.SetEightBitTimer:
            case RoomObjectInstruction.CopyFromRamToVram:
            case RoomObjectInstruction.GotoIfAreaBossBitSet:
            case RoomObjectInstruction.GotoIfEventSet:
            case RoomObjectInstruction.SetEvent:
            case RoomObjectInstruction.SetObjectBtsFromByte:
            case RoomObjectInstruction.DrawObjectBlock:
            case RoomObjectInstruction.DrawObjectBlockClone:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum6:
            case RoomObjectInstruction.ClearDownwardGateTrigger:
            case RoomObjectInstruction.SpawnDownwardGateProjectile:
            case RoomObjectInstruction.WakeDownwardGateProjectile:
            case RoomObjectInstruction.ShootEyeDoorProjectile:
            case RoomObjectInstruction.SpawnEyeDoorSweat:
            case RoomObjectInstruction.SpawnTwoEyeDoorSmoke:
            case RoomObjectInstruction.SpawnEyeDoorSmoke:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomObjectInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomObjectInstruction.DamageDraygonCannonFacingRight:
            case RoomObjectInstruction.DamageDraygonCannonFacingLeft:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum3:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1:
            case RoomObjectInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomObjectInstruction.SetObjectBtsToOne:
            case RoomObjectInstruction.GotoIfRoomArgumentLess:
            case RoomObjectInstruction.SpawnFourMotherBrainGlassShards:
            case RoomObjectInstruction.SpawnTorizoStatueBreaking:
            case RoomObjectInstruction.QueueSongOneMusicTrack:
            case RoomObjectInstruction.EnableNoobTubeWaterPhysics:
            case RoomObjectInstruction.SpawnNoobTubeCrack:
            case RoomObjectInstruction.TriggerNoobTubeEarthquake:
            case RoomObjectInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomObjectInstruction.LockSamus:
            case RoomObjectInstruction.UnlockSamus:
            case RoomObjectInstruction.MoveRightFourBlocks:
            case RoomObjectInstruction.SetAnimalsEscapedEvent:
            case RoomObjectInstruction.SamusEaterDamage:
            case RoomObjectInstruction.SamusEaterReleaseImmunity:
            case RoomObjectInstruction.MoveTourianAccessDown:
            case RoomObjectInstruction.TransformSpikesToSlopes:
            case RoomObjectInstruction.RevertSlopesToSpikes:
            case RoomObjectInstruction.SetLoweredAcidHeight:
            default:
                cue = default;
                length = 0;
                return false;
        }
    }

    /// <summary>Each grey cap's step addresses, walked once.</summary>
    private static readonly Dictionary<GreyDoorVariant, IReadOnlyDictionary<ProgramStep<GreyDoorProgram>, ushort>> greyDoorAddresses = [];

    /// <summary>
    /// The native instruction pointer a grey cap's typed program stands for: the native instruction its cursor rests on,
    /// for comparisons with native movie snapshots.
    /// </summary>
    internal static ushort GreyDoorNativeInstruction(ISnesAddressSpace rom, GreyDoorObjectAnimation door) =>
        GreyDoorAddresses(rom, door.Variant)[new ProgramStep<GreyDoorProgram>(door.Cursor.Program, door.Cursor.Position)];

    /// <summary>The native list a grey cap's typed program begins at.</summary>
    internal static ushort GreyDoorNativeEntry(GreyDoorVariant variant, GreyDoorProgram program) =>
        DoorObjectNativeListAddresses.GreyEntries(variant).Single(entry => entry.Program == program).Entry;

    /// <summary>Grey cap <paramref name="variant"/>'s step addresses.</summary>
    private static IReadOnlyDictionary<ProgramStep<GreyDoorProgram>, ushort> GreyDoorAddresses(ISnesAddressSpace rom, GreyDoorVariant variant)
    {
        if (!greyDoorAddresses.TryGetValue(variant, out var addresses))
        {
            addresses = StepAddresses(CartridgeImportSource.Require(rom), GreyDoorObjectProgramDefinitions.Sets[variant],
                DoorObjectNativeListAddresses.GreyEntries(variant), frame => GreyCapDraw(variant.Orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out GreyDoorCue cue, out int length) => GreyDoorCueAt(source, cursor, variant, out cue, out length),
                $"grey {variant}");
            greyDoorAddresses[variant] = addresses;
        }
        return addresses;
    }
}
