using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Verification's native view of room sprite objects: the bank-$B4 address of the record an object shows, derived from the
/// segments' list addresses (a timed record takes four bytes, and the lists' frames come first), and the native words of
/// its timer, spritemap, attributes and disable flag.
/// </summary>
internal static class RoomSpriteObjectAccess
{
    /// <summary>The bytes a beat occupies in its native list: only timed frames, four bytes each.</summary>
    private static int BeatBytes(AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue> beat) => beat switch
    {
        AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Frame => 4,
        AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Pause or AnimationBeat<RoomSpriteObjectFrame, NoAnimationCue>.Event => throw new InvalidOperationException($"Room sprite-object segments have no {beat} beat."),
        _ => throw new InvalidOperationException($"Room sprite-object segments have no {beat} beat."),
    };

    /// <summary>The bank-$B4 address of beat <paramref name="beat"/> of <paramref name="segment"/>.</summary>
    internal static ushort RecordAddress(RoomSpriteObjectSegment segment, int beat) =>
        checked((ushort)(RoomSpriteObjectProgramAddresses.Entries[segment] +
            RoomSpriteObjectAnimations.Segments.Beats(segment).Take(beat).Sum(BeatBytes)));

    /// <summary>Native views of a sprite object.</summary>
    extension(RoomSpriteObject self)
    {
        /// <summary>
        /// The bank-$B4 address of the record the object shows, or zero once it has ended: the beat before the place its
        /// animation stands at, in the segment of the phase it plays; a held animation rests past its phase's last beat and
        /// shows that beat.
        /// </summary>
        internal ushort InstructionPointer => self.Playback is { } playback
            ? RecordAddress(playback.Animation.Phases[playback.Place.Phase].Segment,
                playback.Animation.Phases[playback.Place.Phase].FirstBeat
                    + Math.Min(playback.Place.Beat, playback.Animation.Phases[playback.Place.Phase].BeatCount) - 1)
            : (ushort)0;

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
        internal static ushort InstructionPointer(RoomSpriteObjectKind kind) => RoomSpriteObjectProgramAddresses.Entries[RoomSpriteObjectDefinitions.Animations[kind]];
    }

    /// <summary>Frames by spritemap.</summary>
    extension(RoomSpriteObjectVisualDefinitions)
    {
        /// <summary>The frame whose spritemap is <paramref name="spritemap"/>.</summary>
        internal static RoomSpriteObjectFrame FrameOf(ushort spritemap) =>
            Enum.GetValues<RoomSpriteObjectFrame>().Single(frame => RoomSpriteObjectVisualDefinitionsConstants.Spritemap(frame) == spritemap);
    }
}
