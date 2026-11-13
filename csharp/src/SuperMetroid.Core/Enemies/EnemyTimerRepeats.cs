using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy's frame timer as its animation's repeat counter (#1600): the timer its AI shares with its counted loops.</summary>
/// <param name="slot">The enemy.</param>
internal readonly struct EnemyTimerRepeats(RoomEnemySlot slot) : IAnimationRepeatCounter
{
    /// <inheritdoc />
    public WrappingCountdown Repeats
    {
        get => slot.Timer;
        set => slot.Timer = value;
    }
}
