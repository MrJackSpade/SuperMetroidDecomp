using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Selects what a pass drawing the room's background draws.</summary>
internal static class D3D11RoomSources
{
    /// <summary>Added to a source when pictures written over the background cover it (Tiles.hlsl SourceWrittenOverBackground).</summary>
    internal const uint WrittenOverBackgroundFlag = 256;

    /// <summary>
    /// The source of what the background shows (#2544): a boss body (#1352), the room's background plane, the X-ray reveal
    /// (#1392), the sky's rows, the room's library background image (#1346, #1347, #1403) or nothing, with
    /// <see cref="WrittenOverBackgroundFlag"/> when pictures are written over the background (#1348).
    /// </summary>
    internal static uint BackgroundSourceOf(RoomBackgrounds rooms) =>
        (uint)rooms.Background.Map(SourceOfBackground.Instance) | (rooms.BackgroundText is null ? 0 : WrittenOverBackgroundFlag);

    /// <summary>The source each kind of background draws from.</summary>
    private sealed class SourceOfBackground : IShownRoomBackgroundMapping<D3D11RoomSource>
    {
        /// <summary>The one mapping.</summary>
        internal static SourceOfBackground Instance { get; } = new();

        /// <inheritdoc/>
        public D3D11RoomSource Nothing() => D3D11RoomSource.Nothing;

        /// <inheritdoc/>
        public D3D11RoomSource RoomPlane(RoomBackgroundLayer plane) => D3D11RoomSource.Background;

        /// <inheritdoc/>
        public D3D11RoomSource Sky(SkyPictures sky) => D3D11RoomSource.Sky;

        /// <inheritdoc/>
        public D3D11RoomSource BossBody(ShownBossBody body) => D3D11RoomSource.BossBody;

        /// <inheritdoc/>
        public D3D11RoomSource LibraryImage(RoomBackgroundImagePlane image) => D3D11RoomSource.LibraryBackground;

        /// <inheritdoc/>
        public D3D11RoomSource XrayReveal(XrayRevealPlane reveal) => D3D11RoomSource.XrayReveal;
    }
}
