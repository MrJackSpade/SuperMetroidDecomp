using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Debugger-visible names for the six common enemy words aliased by bank-$A8's Work Robot.
/// The values deliberately remain native words: laser speed is signed 8.8, while falling
/// speed is split into the same signed-whole/fraction pair consumed by bank $A0 movement.
/// </summary>
public sealed class WorkRobotEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new work robot enemy state instance.</summary>
    internal WorkRobotEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// True only when definition $E8FF saw the area's boss bit during initialization.
    /// Definition $E8FF can therefore own a deactivated state before Phantoon is defeated.
    /// </summary>
    public bool Powered { get; internal set; }

    /// <summary>Gets or sets variable A, the signed 8.8 horizontal velocity assigned to newly fired robot lasers.</summary>
    public ushort LaserXVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets or sets variable B, the wrapping countdown that gates the next laser attempt.</summary>
    public ushort LaserCooldown
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>
    /// Gets or sets the whole-pixel room position saved during a temporary ledge probe: X in
    /// variable C, Y in variable D.
    /// </summary>
    public Position16<RoomPixels> BackupPosition
    {
        get => new(_slot.VariableC, _slot.VariableD);
        internal set
        {
            _slot.VariableC = value.X;
            _slot.VariableD = value.Y;
        }
    }

    /// <summary>Gets or sets variable E, the fractional low word of the robot's signed 16.16 falling velocity.</summary>
    public ushort YSubvelocity
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Gets or sets variable F, the signed whole-pixel word of the robot's 16.16 falling velocity.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}
