using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Where a sequenced room object's animations stand (#2558): it decides what follows on its next update, it plays an
/// animation, or it rests until what it waits for starts another, on the frame it last showed or showing none.
/// </summary>
/// <typeparam name="TSegment">The object's segment identity.</typeparam>
/// <typeparam name="TFrame">What one of its frames draws.</typeparam>
/// <typeparam name="TEvent">What the object does as its animation passes an event.</typeparam>
internal abstract partial class SequencedRoomObjectProgress<TSegment, TFrame, TEvent>
    where TSegment : struct
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Only the cases below are progress.</summary>
    private SequencedRoomObjectProgress()
    {
    }

    /// <summary>
    /// The object decides what follows on its next update, showing nothing more: it was just placed, or it ended its
    /// animation early to decide again.
    /// </summary>
    internal sealed partial class Deciding : SequencedRoomObjectProgress<TSegment, TFrame, TEvent>
    {
        /// <summary>The object deciding on its next update.</summary>
        internal static Deciding OnNextUpdate { get; } = new();

        /// <summary>Only <see cref="OnNextUpdate"/> decides.</summary>
        private Deciding()
        {
        }
    }

    /// <summary>The object plays an animation of its frames.</summary>
    /// <param name="player">The animation playing.</param>
    internal sealed partial class Playing(AnimationPlayer<TSegment, TFrame, TEvent> player) : SequencedRoomObjectProgress<TSegment, TFrame, TEvent>
    {
        /// <summary>The animation playing.</summary>
        internal AnimationPlayer<TSegment, TFrame, TEvent> Player { get; } = Ensure.NotNull(player);
    }

    /// <summary>The object's animation has ended with nothing to follow: it rests on the frame it last showed.</summary>
    /// <param name="shown">The frame it last showed.</param>
    internal sealed partial class Resting(TFrame shown) : SequencedRoomObjectProgress<TSegment, TFrame, TEvent>
    {
    }

    /// <summary>The object's animation has ended with nothing to follow, its last step showing no frame: it rests showing none.</summary>
    internal sealed partial class RestingUnshown : SequencedRoomObjectProgress<TSegment, TFrame, TEvent>
    {
        /// <summary>The object resting showing no frame.</summary>
        internal static RestingUnshown Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> rests showing no frame.</summary>
        private RestingUnshown()
        {
        }
    }
}
