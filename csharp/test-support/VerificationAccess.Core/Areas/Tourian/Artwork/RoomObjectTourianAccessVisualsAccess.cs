using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian.Artwork;

/// <summary>Verification access to <see cref="RoomObjectTourianAccessVisuals"/> members production does not use.</summary>
internal static class RoomObjectTourianAccessVisualsAccess
{
    /// <summary>Exposes <see cref="RoomObjectTourianAccessVisuals"/> state that only verification reads.</summary>
    extension(RoomObjectTourianAccessVisuals self)
    {
        /// <summary>The picture <paramref name="draw"/> shows.</summary>
        /// <exception cref="InvalidDataException">The installation lacks the frame.</exception>
        internal RoomObjectFrameImage Image(RoomObjectDraw draw) => self.Images.Image(draw);
    }
}
