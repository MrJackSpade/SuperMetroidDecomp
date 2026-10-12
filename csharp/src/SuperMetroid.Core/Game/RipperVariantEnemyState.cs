using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed view of the common Ripper-family motion words plus GRipper's two private patrol
/// bounds. The three species reuse the same native variables C/D/E for fractional speed,
/// signed whole speed, and byte offset into the bank-$A0 linear-speed table.
/// </summary>
public sealed class RipperVariantEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores minimum xpositions.</summary>
    private readonly ushort[] _minimumXPositions;
    /// <summary>Stores maximum xpositions.</summary>
    private readonly ushort[] _maximumXPositions;

    /// <summary>Initializes a new ripper variant enemy state instance.</summary>
    internal RipperVariantEnemyState(
        RoomEnemySlot slot,
        ushort[] minimumXPositions,
        ushort[] maximumXPositions)
    {
        _slot = slot;
        _minimumXPositions = minimumXPositions;
        _maximumXPositions = maximumXPositions;
    }

    /// <summary>Common variable C: low/fractional word of signed 16.16 X velocity.</summary>
    public ushort XSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Common variable D: signed whole-pixel word of 16.16 X velocity.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Common variable E: byte offset into <c>$A0:8187</c>.</summary>
    public ushort SpeedTableByteOffset
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>GRipper extra word $00: exclusive left patrol reversal threshold.</summary>
    public ushort MinimumXPosition
    {
        get => _minimumXPositions[_slot.SlotIndex];
        internal set => _minimumXPositions[_slot.SlotIndex] = value;
    }

    /// <summary>GRipper extra word $01: inclusive right patrol reversal threshold.</summary>
    public ushort MaximumXPosition
    {
        get => _maximumXPositions[_slot.SlotIndex];
        internal set => _maximumXPositions[_slot.SlotIndex] = value;
    }
}
