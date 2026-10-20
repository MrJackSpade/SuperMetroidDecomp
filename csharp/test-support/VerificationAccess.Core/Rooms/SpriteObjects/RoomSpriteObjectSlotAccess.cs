using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// Verification's native view of room sprite objects: the bank-$B4 address of the record a slot shows, derived from the
/// programs' list addresses (a timed record takes four bytes, a hold or delete two, a goto four and a fall-through none),
/// and the native words of its timer, spritemap, attributes and disable flag.
/// </summary>
internal static class RoomSpriteObjectSlotAccess
{
    /// <summary>The bytes a step occupies in its native list.</summary>
    private static int StepBytes(AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> step) => step.Kind switch
    {
        AnimationProgramStepKind.Show => 4,
        AnimationProgramStepKind.Hold or AnimationProgramStepKind.Stop => 2,
        AnimationProgramStepKind.GoTo => 4,
        AnimationProgramStepKind.FallInto => 0,
        AnimationProgramStepKind.Cue or
            AnimationProgramStepKind.Wait or
            AnimationProgramStepKind.SetCounter or
            AnimationProgramStepKind.RepeatWhileCounting => throw new InvalidOperationException($"Room sprite-object programs have no {step.Kind} step."),
        _ => throw new InvalidOperationException($"Room sprite-object programs have no {step.Kind} step."),
    };

    /// <summary>The bank-$B4 address of step <paramref name="position"/> of <paramref name="program"/>.</summary>
    internal static ushort RecordAddress(RoomSpriteObjectProgram program, int position) =>
        checked((ushort)(RoomSpriteObjectProgramAddresses.Entries[program] +
            RoomSpriteObjectAnimations.Programs.Steps(program).Take(position).Sum(StepBytes)));

    /// <summary>Native views of a slot.</summary>
    extension(RoomSpriteObjectSlot self)
    {
        /// <summary>The bank-$B4 address of the record the slot shows, or zero for a free slot.</summary>
        internal ushort InstructionPointer => self.Cursor is { } cursor ? RecordAddress(cursor.Program, cursor.Position - 1) : (ushort)0;

        /// <summary>The native timer word.</summary>
        internal ushort InstructionTimer => self.Timer.RawValue;

        /// <summary>The bank-$B4 spritemap the slot shows, or zero for a free slot.</summary>
        internal ushort SpritemapPointer => self.IsActive ? RoomSpriteObjectVisualDefinitions.Spritemap(self.Frame) : (ushort)0;

        /// <summary>The native OBJ attribute word.</summary>
        internal ushort GraphicsIndex => self.Attributes.Raw;

        /// <summary>The native disable word.</summary>
        internal int DisableFlags => self.UpdatesDisabled ? 1 : 0;
    }

    /// <summary>Native views of the kind selection.</summary>
    extension(RoomSpriteObjectDefinitions)
    {
        /// <summary>The bank-$B4 list address <paramref name="kind"/> starts.</summary>
        internal static ushort InstructionPointer(RoomSpriteObjectKind kind) => RoomSpriteObjectProgramAddresses.Entries[RoomSpriteObjectDefinitions.Program(kind)];
    }

    /// <summary>Frames by spritemap.</summary>
    extension(RoomSpriteObjectVisualDefinitions)
    {
        /// <summary>The frame whose spritemap is <paramref name="spritemap"/>.</summary>
        internal static RoomSpriteObjectFrame FrameOf(ushort spritemap) =>
            Enum.GetValues<RoomSpriteObjectFrame>().Single(frame => RoomSpriteObjectVisualDefinitions.Spritemap(frame) == spritemap);
    }
}
