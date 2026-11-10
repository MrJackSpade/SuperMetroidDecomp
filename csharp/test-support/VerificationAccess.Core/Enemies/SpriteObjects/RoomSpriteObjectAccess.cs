using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Verification's native view of room sprite objects: the bank-$B4 address of the record an object shows, derived from the
/// programs' list addresses (a timed record takes four bytes, a hold or delete two, a goto four and a fall-through none),
/// and the native words of its timer, spritemap, attributes and disable flag.
/// </summary>
internal static class RoomSpriteObjectAccess
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

    /// <summary>Native views of a sprite object.</summary>
    extension(RoomSpriteObject self)
    {
        /// <summary>The bank-$B4 address of the record the object shows, or zero once it has ended.</summary>
        internal ushort InstructionPointer => self.Cursor is { } cursor ? RecordAddress(cursor.Program, cursor.Position - 1) : (ushort)0;

        /// <summary>The native timer word while the object counts down; a holding object counts none.</summary>
        internal ushort FrameHold => self.Holds
            ? throw new InvalidOperationException($"Sprite object {self.Kind} holds its frame and counts no timer.")
            : checked((ushort)self.Timer);

        /// <summary>The bank-$B4 spritemap the object shows, or zero once it has ended.</summary>
        internal ushort SpritemapPointer => self.IsActive ? RoomSpriteObjectVisualDefinitionsConstants.Spritemap(self.Frame) : (ushort)0;

        /// <summary>
        /// The native OBJ attribute word the character index the object's owner draws from and the row holding its colours
        /// under the graphics set <paramref name="enemies"/>' room draws under encode to.
        /// </summary>
        internal ushort GraphicsIndexIn(RoomEnemySystem enemies) =>
            SnesObjAttributeWord.Create(self.Graphics.NativeTileBaseIn(enemies), self.Graphics.PaletteRowIn(enemies), priority: 0).Raw;

        /// <summary>The native disable word.</summary>
        internal int DisableFlags => self.UpdatesDisabled ? 1 : 0;
    }

    /// <summary>Views of the room's sprite objects.</summary>
    extension(RoomSpriteObjects self)
    {
        /// <summary>The instance <paramref name="spawned"/>'s draws carry: its position among the live objects.</summary>
        internal int DrawInstanceOf(RoomSpriteObject spawned)
        {
            int instance = self.Live.ToList().IndexOf(spawned);
            return instance >= 0 ? instance : throw new InvalidOperationException($"Sprite object {spawned.Kind} is not live.");
        }
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
