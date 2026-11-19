using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Abilities;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// What each kind of background maps to (#2544): a consumer implements one method for each, and a background calls the one
/// for its kind (<see cref="ShownRoomBackground.Map"/>), so every kind is mapped and none is left to a fallback.
/// </summary>
/// <typeparam name="TResult">What each kind of background maps to.</typeparam>
public interface IShownRoomBackgroundMapping<out TResult>
{
    /// <summary>The value of a background showing nothing.</summary>
    TResult Nothing();

    /// <summary>The value of the room's background plane.</summary>
    /// <param name="plane">The background plane with its pictures.</param>
    TResult RoomPlane(RoomBackgroundLayer plane);

    /// <summary>The value of the scrolling sky's rows.</summary>
    /// <param name="sky">The sky's rows, drawn from its image.</param>
    TResult Sky(SkyPictures sky);

    /// <summary>The value of a boss's body.</summary>
    /// <param name="body">The body's pictures, placed where they show.</param>
    TResult BossBody(ShownBossBody body);

    /// <summary>The value of the room's library background image.</summary>
    /// <param name="image">The image, repeated across and down.</param>
    TResult LibraryImage(RoomBackgroundImagePlane image);

    /// <summary>The value of the X-ray's reveal.</summary>
    /// <param name="reveal">The revealed blocks, drawn from the room's block pictures.</param>
    TResult XrayReveal(XrayRevealPlane reveal);
}
