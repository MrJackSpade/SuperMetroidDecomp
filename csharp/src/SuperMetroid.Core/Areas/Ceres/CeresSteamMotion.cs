using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Where a steam vent is drawn, which its action (<see cref="CeresSteamAction"/>) selects as it initializes: where it
/// stands, or turned with the elevator shaft it stands in. Collision always uses where the vent stands.
/// </summary>
internal abstract partial record CeresSteamMotion
{
    /// <summary>Closes the hierarchy to the motions declared here.</summary>
    private CeresSteamMotion()
    {
    }

    /// <summary>Places the vent's drawing for this update.</summary>
    /// <param name="place">The vent's place.</param>
    internal abstract void Move(RoomEnemyPlace place);

    /// <summary>A vent drawn where it stands: its room position is also its draw position, so it changes nothing.</summary>
    internal sealed partial record HoldsItsPlace : CeresSteamMotion
    {
        /// <summary>The one such motion.</summary>
        internal static HoldsItsPlace Instance { get; } = new();

        /// <summary>Only <see cref="Instance"/> exists.</summary>
        private HoldsItsPlace()
        {
        }

        /// <inheritdoc/>
        internal override void Move(RoomEnemyPlace place)
        {
        }
    }

    /// <summary>
    /// A vent inside the turning elevator shaft: it is drawn where the shaft's tilt turns the point it stands on, the same
    /// turn Samus is drawn with, while it collides where it stands.
    /// </summary>
    /// <param name="Shaft">The shaft's turn, which the shaft's elevator door starts as the room loads.</param>
    [RoomLifetime]
    internal sealed partial record RidesShaftTurn(CeresElevatorShaftRoomMainState Shaft) : CeresSteamMotion
    {
        /// <inheritdoc/>
        internal override void Move(RoomEnemyPlace place)
        {
            Position16<RoomPixels> standing = Ensure.NotNull(place).Position.Whole;
            // Only the turned point's distance from where the vent stands is kept; drawing applies it later.
            place.DrawOffset = Shaft.Transform.Transform(standing) - standing;
        }
    }
}
