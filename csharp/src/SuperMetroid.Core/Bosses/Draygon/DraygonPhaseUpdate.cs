using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One update of Draygon's body, as his phase's step runs it (#2554).</summary>
/// <param name="draygon">Draygon's behaviour, whose components run the phase.</param>
/// <param name="state">Draygon's encounter.</param>
/// <param name="samus">Samus, whom the fight reads and moves.</param>
/// <param name="frameCounter">The frame counter Draygon times his turrets and smoke by.</param>
[RoomLifetime]
internal sealed partial class DraygonPhaseUpdate(DraygonBehavior draygon, DraygonEnemyState state, SamusState samus, int frameCounter)
{
    /// <summary>Draygon's behaviour, whose components run the phase.</summary>
    internal DraygonBehavior Draygon { get; } = draygon;

    /// <summary>Draygon's encounter.</summary>
    internal DraygonEnemyState State { get; } = state;

    /// <summary>Samus, whom the fight reads and moves.</summary>
    internal SamusState Samus { get; } = samus;

    /// <summary>The frame counter Draygon times his turrets and smoke by.</summary>
    internal int FrameCounter { get; } = frameCounter;
}
