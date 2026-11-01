using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.TestSupport.MovieReplay.NativePlmListDefinitions;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the door caps' typed PLM programs: the door sounds and grey caps' instructions as their native
/// bank-$84 lists encode them, and the native instruction pointer a grey cap's typed cursor stands for in recordings.
/// </summary>
internal static class DoorPlmNativeProgramOracle
{
    /// <summary>The library-3 sound a queue-sound instruction at <paramref name="cursor"/> names.</summary>
    internal static SoundEffectId QueuedLibrary3Sound(IImportCartridgeSource cartridge, int cursor) =>
        SoundEffectCommands.Decode(SoundEffectLibrary.Library3, ReadOperandByte(cartridge, cursor + 2));

    /// <summary>A door sound as its cue: the open or the close sound.</summary>
    internal static TCue DoorSoundCue<TCue>(SoundEffectId sound, TCue open, TCue close) =>
        sound == BlueDoorPlmProgramDefinitions.OpenSound ? open :
        sound == BlueDoorPlmProgramDefinitions.CloseSound ? close :
        throw new InvalidOperationException($"Door sound {sound} is neither the open nor the close sound.");

    /// <summary>The draw list a grey cap's frame draws.</summary>
    internal static PlmDraw GreyCapDraw(ColoredDoorOrientation orientation, DoorCapFrame frame) => frame switch
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
        var entries = DoorPlmNativeListAddresses.GreyEntries(variant);
        ushort EntryOf(GreyDoorProgram program) => entries.Single(entry => entry.Program == program).Entry;
        switch (InstructionAt<RoomPlmInstruction>(cartridge, cursor))
        {
            case RoomPlmInstruction.GotoIfDoorBitSet:
                VerificationAssertions.AssertEqual(DoorPlmNativeListAddresses.BlueEntries.Single(e => e.Program == BlueDoorPlmProgramDefinitions.ClosedPrograms[variant.Orientation]).Entry,
                    Word(2), $"grey {variant} becomes its blue cap");
                cue = GreyDoorCue.BranchIfOpened;
                length = 4;
                return true;
            case RoomPlmInstruction.LinkInstruction:
                ushort target = Word(2);
                VerificationAssertions.AssertTrue(target == EntryOf(GreyDoorProgram.Activate) || target == EntryOf(GreyDoorProgram.Hit), $"grey {variant} link target");
                cue = target == EntryOf(GreyDoorProgram.Activate) ? GreyDoorCue.LinkActivate : GreyDoorCue.LinkHit;
                length = 4;
                return true;
            case RoomPlmInstruction.SetGreyDoorFrameBehavior:
                cue = GreyDoorCue.ArmCondition;
                length = 2;
                return true;
            case RoomPlmInstruction.InstallFrameBehavior:
                VerificationAssertions.AssertEqual(DoorsNativeWords.GreyDoorFrameBehaviors.Word(GreyDoorFrameBehavior.WakeWhenShot), Word(2), $"grey {variant} shot wake");
                cue = GreyDoorCue.ArmShotWake;
                length = 4;
                return true;
            case RoomPlmInstruction.IncrementDoorHitCounterAndGoto:
                VerificationAssertions.AssertEqual(GreyDoorPlmProgramDefinitions.HitThreshold, ReadOperandByte(cartridge, cursor + 2), "grey door hit threshold");
                VerificationAssertions.AssertEqual(EntryOf(GreyDoorProgram.Open), Word(3), $"grey {variant} opens at its threshold");
                cue = GreyDoorCue.CountHit;
                length = 5;
                return true;
            case RoomPlmInstruction.QueueSoundLibrary3Maximum6:
                cue = DoorSoundCue(QueuedLibrary3Sound(cartridge, cursor), GreyDoorCue.QueueOpenSound, GreyDoorCue.QueueCloseSound);
                length = 3;
                return true;
            case RoomPlmInstruction.GotoIfSamusHasNoBombs:
                VerificationAssertions.AssertEqual(EntryOf(GreyDoorProgram.Closing), Word(2), "Bomb Torizo door repeats its closing");
                cue = GreyDoorCue.RepeatUntilSamusHasBombs;
                length = 4;
                return true;
            case RoomPlmInstruction.Sleep:
            case RoomPlmInstruction.Delete:
            case RoomPlmInstruction.ClearFrameBehavior:
            case RoomPlmInstruction.IncrementArgumentAndGotoIfGreaterOrEqual:
            case RoomPlmInstruction.GotoIfSamusNear:
            case RoomPlmInstruction.Goto:
            case RoomPlmInstruction.DecrementTimerAndGoto:
            case RoomPlmInstruction.SetEightBitTimer:
            case RoomPlmInstruction.CopyFromRamToVram:
            case RoomPlmInstruction.GotoIfAreaBossBitSet:
            case RoomPlmInstruction.GotoIfEventSet:
            case RoomPlmInstruction.SetEvent:
            case RoomPlmInstruction.SetPlmBtsFromByte:
            case RoomPlmInstruction.DrawPlmBlock:
            case RoomPlmInstruction.DrawPlmBlockClone:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum6:
            case RoomPlmInstruction.ClearDownwardGateTrigger:
            case RoomPlmInstruction.SpawnDownwardGateProjectile:
            case RoomPlmInstruction.WakeDownwardGateProjectile:
            case RoomPlmInstruction.ShootEyeDoorProjectile:
            case RoomPlmInstruction.SpawnEyeDoorSweat:
            case RoomPlmInstruction.SpawnTwoEyeDoorSmoke:
            case RoomPlmInstruction.SpawnEyeDoorSmoke:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingRight:
            case RoomPlmInstruction.MoveUpAndMakeBlueDoorFacingLeft:
            case RoomPlmInstruction.DamageDraygonCannonFacingRight:
            case RoomPlmInstruction.DamageDraygonCannonFacingLeft:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum3:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1:
            case RoomPlmInstruction.QueueSoundLibrary2Maximum1Direct:
            case RoomPlmInstruction.SetPlmBtsToOne:
            case RoomPlmInstruction.GotoIfRoomArgumentLess:
            case RoomPlmInstruction.SpawnFourMotherBrainGlassShards:
            case RoomPlmInstruction.SpawnTorizoStatueBreaking:
            case RoomPlmInstruction.QueueSongOneMusicTrack:
            case RoomPlmInstruction.EnableNoobTubeWaterPhysics:
            case RoomPlmInstruction.SpawnNoobTubeCrack:
            case RoomPlmInstruction.TriggerNoobTubeEarthquake:
            case RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles:
            case RoomPlmInstruction.LockSamus:
            case RoomPlmInstruction.UnlockSamus:
            case RoomPlmInstruction.MoveRightFourBlocks:
            case RoomPlmInstruction.SetAnimalsEscapedEvent:
            case RoomPlmInstruction.SamusEaterDamage:
            case RoomPlmInstruction.SamusEaterReleaseImmunity:
            case RoomPlmInstruction.MoveTourianAccessDown:
            case RoomPlmInstruction.TransformSpikesToSlopes:
            case RoomPlmInstruction.RevertSlopesToSpikes:
            case RoomPlmInstruction.SetLoweredAcidHeight:
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
    internal static ushort GreyDoorNativeInstruction(ISnesAddressSpace rom, GreyDoorPlmAnimation door) =>
        GreyDoorAddresses(rom, door.Variant)[new ProgramStep<GreyDoorProgram>(door.Cursor.Program, door.Cursor.Position)];

    /// <summary>The native list a grey cap's typed program begins at.</summary>
    internal static ushort GreyDoorNativeEntry(GreyDoorVariant variant, GreyDoorProgram program) =>
        DoorPlmNativeListAddresses.GreyEntries(variant).Single(entry => entry.Program == program).Entry;

    /// <summary>Grey cap <paramref name="variant"/>'s step addresses.</summary>
    private static IReadOnlyDictionary<ProgramStep<GreyDoorProgram>, ushort> GreyDoorAddresses(ISnesAddressSpace rom, GreyDoorVariant variant)
    {
        if (!greyDoorAddresses.TryGetValue(variant, out var addresses))
        {
            addresses = StepAddresses(CartridgeImportSource.Require(rom), GreyDoorPlmProgramDefinitions.Sets[variant],
                DoorPlmNativeListAddresses.GreyEntries(variant), frame => GreyCapDraw(variant.Orientation, frame),
                (IImportCartridgeSource source, ushort cursor, out GreyDoorCue cue, out int length) => GreyDoorCueAt(source, cursor, variant, out cue, out length),
                $"grey {variant}");
            greyDoorAddresses[variant] = addresses;
        }
        return addresses;
    }
}
