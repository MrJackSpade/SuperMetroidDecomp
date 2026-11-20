using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>What one wall-turret firing does (#1686): fire from one of the wall cannons, or hold fire.</summary>
internal abstract record DraygonTurretTurn
{
    /// <summary>Only the nested cases derive.</summary>
    private DraygonTurretTurn()
    {
    }

    /// <summary>The firing holds fire: no cannon shoots.</summary>
    internal static DraygonTurretTurn HoldsFire { get; } = new Holding();

    /// <summary>The firing fires from <paramref name="Cannon"/>, unless the cannon is disabled.</summary>
    /// <param name="Cannon">The cannon.</param>
    internal sealed record FiresFrom(DraygonWallCannon Cannon) : DraygonTurretTurn;

    /// <summary>The firing that holds fire.</summary>
    private sealed record Holding : DraygonTurretTurn;
}
