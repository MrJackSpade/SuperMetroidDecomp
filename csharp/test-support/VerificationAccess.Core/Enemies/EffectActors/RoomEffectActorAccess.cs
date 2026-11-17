using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// Verification's native view of room sprite objects: the bank-$B4 address of the record an object shows, derived from the
/// segments' list addresses (a timed record takes four bytes, and the lists' frames come first), and the native words of
/// its timer, spritemap, attributes and disable flag.
/// </summary>
internal static class RoomEffectActorAccess
{
    /// <summary>
    /// The bytes beat <paramref name="index"/> of <paramref name="segment"/> occupies in its native list: a timed frame or a
    /// still picture four bytes a record, a frame the list shows in two records in a row (#2275) eight.
    /// </summary>
    private static int BeatBytes(RoomEffectActorSegment segment, int index) => RoomEffectActorAnimations.Segments.Beats(segment)[index] switch
    {
        AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.Frame or AnimationBeat<RoomEffectActorPicture, NoAnimationCue>.HeldFrame =>
            4 * RoomEffectActorProgramAddresses.RecordsOf(segment, index),
        var beat => throw new InvalidOperationException($"Room sprite-object segments have no {beat} beat."),
    };

    /// <summary>The bank-$B4 address of beat <paramref name="beat"/> of <paramref name="segment"/>.</summary>
    internal static ushort RecordAddress(RoomEffectActorSegment segment, int beat) =>
        checked((ushort)(RoomEffectActorProgramAddresses.SegmentEntries[segment] +
            Enumerable.Range(0, beat).Sum(index => BeatBytes(segment, index))));

    /// <summary>Native views of a sprite object.</summary>
    extension(RoomEffectActor self)
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
        internal ushort SpritemapPointer => self.IsActive ? RoomEffectActorVisualDefinitionsConstants.Spritemap(self.Frame) : (ushort)0;

        /// <summary>
        /// The native OBJ attribute word the character index the object's owner draws from and the row holding its colours
        /// under the graphics set <paramref name="enemies"/>' room draws under encode to.
        /// </summary>
        internal ushort GraphicsIndexIn(RoomEnemySystem enemies) =>
            SnesObjAttributeWord.Create(self.Graphics.NativeTileBaseIn(enemies), self.Graphics.PaletteRowIn(enemies), priority: 0).Raw;

        /// <summary>The native disable word.</summary>
        internal int DisableFlags => self.UpdatesDisabled ? 1 : 0;
    }
}
