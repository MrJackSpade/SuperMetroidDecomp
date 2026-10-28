using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>
/// One test a room's state-selection program makes before offering a state: a persistent event, a defeated boss of the
/// room's area, or the equipment Samus carries.
/// </summary>
internal abstract record RoomStateCondition
{
    /// <summary>Satisfied once <paramref name="eventNumber"/> is set.</summary>
    internal static RoomStateCondition Event(EventNumber eventNumber) => new EventSet(eventNumber);

    /// <summary>Satisfied once any boss of <paramref name="bosses"/> in the room's area is dead.</summary>
    internal static RoomStateCondition BossDead(BossBits bosses) => new BossesDead(bosses);

    /// <summary>Satisfied while Samus carries power bombs.</summary>
    internal static RoomStateCondition PowerBombs { get; } = new PowerBombsCarried();

    /// <summary>Satisfied while Samus carries the morph ball and missiles.</summary>
    internal static RoomStateCondition MorphBallAndMissiles { get; } = new MorphBallAndMissilesCarried();

    /// <summary>Whether the condition holds for <paramref name="selection"/>.</summary>
    internal abstract bool IsMet(RoomStateSelectionContext selection);

    /// <summary>A persistent event test.</summary>
    private sealed record EventSet(EventNumber Number) : RoomStateCondition
    {
        /// <inheritdoc/>
        internal override bool IsMet(RoomStateSelectionContext selection) => selection.IsEventSet(Number);
    }

    /// <summary>A defeated-boss test against the room's area.</summary>
    private sealed record BossesDead(BossBits Bosses) : RoomStateCondition
    {
        /// <inheritdoc/>
        internal override bool IsMet(RoomStateSelectionContext selection) => selection.IsBossDead(Bosses);
    }

    /// <summary>The power-bomb equipment test.</summary>
    private sealed record PowerBombsCarried : RoomStateCondition
    {
        /// <inheritdoc/>
        internal override bool IsMet(RoomStateSelectionContext selection) => selection.HasPowerBombs;
    }

    /// <summary>The morph-ball-and-missiles equipment test.</summary>
    private sealed record MorphBallAndMissilesCarried : RoomStateCondition
    {
        /// <inheritdoc/>
        internal override bool IsMet(RoomStateSelectionContext selection) => selection.HasMorphBallAndMissiles;
    }
}
