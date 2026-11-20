using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// What a room's written pictures are drawn from, bound to the room at its load (#1392, #1466, #2554): a room whose enemies
/// write warning text over it, Mother Brain's or Ceres Ridley's, is bound to its tileset's written picture sheet, which
/// the load requires it to have; every other room writes nothing over itself and has no sheet to draw from.
/// </summary>
public abstract class RoomWrittenPictures
{
    /// <summary>Only the two below exist.</summary>
    private RoomWrittenPictures()
    {
    }

    /// <summary>The enemies whose fights write warning text over their rooms: Mother Brain's escape and Ceres's self-destruct.</summary>
    internal static ImmutableArray<EnemyDefinitionId> Writers { get; } =
        [EnemyDefinitionId.MotherBrainBody, EnemyDefinitionId.RidleyCeres];

    /// <summary>The room's tileset's written picture sheet, over which its enemies write their warning text.</summary>
    /// <param name="pictures">The sheet's resolver.</param>
    public sealed class Sheet(WrittenPictureResolver pictures) : RoomWrittenPictures
    {
        /// <summary>The sheet's resolver.</summary>
        private readonly WrittenPictureResolver _pictures = Ensure.NotNull(pictures);

        /// <summary>The pictures <paramref name="overlay"/> writes over the room, as the room's appearance shows them.</summary>
        /// <param name="overlay">The text written over the room.</param>
        /// <param name="appearance">The room's appearance.</param>
        internal ImmutableArray<PlacedWrittenPicture> Resolve(RoomWarningText overlay, RoomAppearanceView appearance) =>
            _pictures.Resolve(overlay, appearance);
    }

    /// <summary>A room no enemy writes text over, which draws no written pictures.</summary>
    public sealed class Unwritten : RoomWrittenPictures
    {
        /// <summary>The one room-without-writing value.</summary>
        public static Unwritten Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private Unwritten()
        {
        }
    }
}
