using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The room buffer's slots the background's pictures are packed in (#2544): the one slot of the kind the frame's background
/// shows holds its pictures, and the others stay empty.
/// </summary>
/// <param name="Plane">The room's background plane, when the background shows it.</param>
/// <param name="Sky">The sky's rows, when the background shows them.</param>
/// <param name="Reveal">The X-ray's reveal, when the background shows it.</param>
/// <param name="Image">The room's library background image, when the background shows it.</param>
internal readonly record struct D3D11BackgroundSlots(RoomBackgroundLayer? Plane, SkyPictures? Sky, XrayRevealPlane? Reveal,
    RoomBackgroundImagePlane? Image)
{
    /// <summary>The slots of each kind of background.</summary>
    internal static IShownRoomBackgroundMapping<D3D11BackgroundSlots> Of { get; } = new SlotsOfBackground();

    /// <summary>Fills the slot of each kind of background.</summary>
    private sealed class SlotsOfBackground : IShownRoomBackgroundMapping<D3D11BackgroundSlots>
    {
        /// <inheritdoc/>
        public D3D11BackgroundSlots Nothing() => default;

        /// <inheritdoc/>
        public D3D11BackgroundSlots RoomPlane(RoomBackgroundLayer plane) => new() { Plane = plane };

        /// <inheritdoc/>
        public D3D11BackgroundSlots Sky(SkyPictures sky) => new() { Sky = sky };

        /// <inheritdoc/>
        public D3D11BackgroundSlots LibraryImage(RoomBackgroundImagePlane image) => new() { Image = image };

        /// <inheritdoc/>
        public D3D11BackgroundSlots XrayReveal(XrayRevealPlane reveal) => new() { Reveal = reveal };
    }
}
