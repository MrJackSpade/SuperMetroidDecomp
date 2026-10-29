using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.SpriteObjects;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>
/// Verification's native view of room sprite objects: the bank-$B4 address of the record a slot shows, derived from the
/// programs' list addresses (a timed record takes four bytes, a hold or delete two, a goto four and a fall-through none),
/// and the native words of its timer, spritemap, attributes and disable flag.
/// </summary>
internal static class RoomSpriteObjectSlotAccess
{
    /// <summary>The bytes a step occupies in its native list.</summary>
    private static int StepBytes(AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue> step) => step switch
    {
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.ShowStep => 4,
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.HoldStep or AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.StopStep => 2,
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.GoToStep => 4,
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.FallIntoStep => 0,
        AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.CueStep or AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.WaitStep or AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.SetCounterStep or AnimationProgramStep<RoomSpriteObjectProgram, RoomSpriteObjectFrame, NoAnimationCue>.RepeatWhileCountingStep => throw new InvalidOperationException($"Room sprite-object programs have no {step} step."),
        _ => throw new InvalidOperationException($"Room sprite-object programs have no {step} step."),
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
        internal ushort SpritemapPointer => self.IsActive ? RoomSpriteObjectVisualDefinitionsConstants.Spritemap(self.Frame) : (ushort)0;

        /// <summary>
        /// The native OBJ attribute word the slot's tile base and the row holding its colours under
        /// <paramref name="enemies"/>' loaded graphics set encode to.
        /// </summary>
        internal ushort GraphicsIndexIn(RoomEnemySystem enemies) =>
            SnesObjAttributeWord.Create(self.Graphics.TileBase, self.Graphics.PaletteRowIn(enemies), priority: 0).Raw;

        /// <summary>The native disable word.</summary>
        internal int DisableFlags => self.UpdatesDisabled ? 1 : 0;
    }

    /// <summary>Native views of the kind selection.</summary>
    extension(RoomSpriteObjectDefinitions)
    {
        /// <summary>The bank-$B4 list address <paramref name="kind"/> starts.</summary>
        internal static ushort InstructionPointer(RoomSpriteObjectKind kind) => RoomSpriteObjectProgramAddresses.Entries[RoomSpriteObjectDefinitions.Programs[kind]];
    }

    /// <summary>Frames by spritemap.</summary>
    extension(RoomSpriteObjectVisualDefinitions)
    {
        /// <summary>The frame whose spritemap is <paramref name="spritemap"/>.</summary>
        internal static RoomSpriteObjectFrame FrameOf(ushort spritemap) =>
            Enum.GetValues<RoomSpriteObjectFrame>().Single(frame => RoomSpriteObjectVisualDefinitionsConstants.Spritemap(frame) == spritemap);
    }
}
