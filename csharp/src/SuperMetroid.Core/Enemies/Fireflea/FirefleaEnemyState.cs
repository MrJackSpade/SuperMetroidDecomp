using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>
/// Typed debugger view of Fireflea's six common-slot words and three per-slot words in
/// extra WRAM <c>$7E:7800</c>. The latter stay in parallel arrays because they are genuinely
/// outside the 64-byte enemy record on the cartridge; flattening them into generic vars
/// would misrepresent the memory contract this decompilation is intended to expose.
/// </summary>
public sealed class FirefleaEnemyState
{
    /// <summary>Common 64-byte enemy record holding variables A through F.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Per-slot extra-WRAM upper Y extents for vertical oscillation.</summary>
    private readonly ushort[] _minimumYPositions;
    /// <summary>Per-slot extra-WRAM lower Y extents for vertical oscillation.</summary>
    private readonly ushort[] _maximumYPositions;
    /// <summary>Per-slot native byte offsets into the angular speed table.</summary>
    private readonly ushort[] _speedTableIndexes;

    /// <summary>Binds a physical enemy record to its three parallel extra-WRAM state arrays.</summary>
    internal FirefleaEnemyState(
        RoomEnemySlot slot,
        ushort[] minimumYPositions,
        ushort[] maximumYPositions,
        ushort[] speedTableIndexes)
    {
        _slot = slot;
        _minimumYPositions = minimumYPositions;
        _maximumYPositions = maximumYPositions;
        _speedTableIndexes = speedTableIndexes;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FirefleaProgram> Program { get; set; }

    /// <summary>Fractional word from the selected signed 16.16 angular speed entry.</summary>
    public ushort SubAngleDelta
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed whole word from the selected angular speed entry.</summary>
    public short AngleDelta
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>Movement radius selected by parameter-two's high byte.</summary>
    public ushort Radius
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Circle angle in 8.8 form. Main AI uses the high byte as its eight-bit table angle.
    /// </summary>
    public ushort Angle
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native Fireflea.XCenter/YCenter (variables E/F at $0FB0/$0FB2 plus slot offset): initial whole-pixel room position used as the fixed circular-motion center.</summary>
    public Position16<RoomPixels> Center
    {
        get => new(_slot.VariableE, _slot.VariableF);
        internal set
        {
            _slot.VariableE = value.X;
            _slot.VariableF = value.Y;
        }
    }

    /// <summary>Per-slot extra-WRAM upper extent, initialized to Y minus radius in wrapped room pixels for vertical motion; crossing below it reverses speed after movement.</summary>
    public ushort MinimumYPosition
    {
        get => _minimumYPositions[_slot.SlotIndex];
        internal set => _minimumYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Per-slot extra-WRAM lower extent, initialized to Y plus radius in wrapped room pixels for vertical motion; reaching or passing it reverses speed without clamping.</summary>
    public ushort MaximumYPosition
    {
        get => _maximumYPositions[_slot.SlotIndex];
        internal set => _maximumYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Native byte offset into the linear speed table, not a host array index.</summary>
    public ushort SpeedTableIndex
    {
        get => _speedTableIndexes[_slot.SlotIndex];
        internal set => _speedTableIndexes[_slot.SlotIndex] = value;
    }

    /// <summary>Native init0/parameter-one bit $0002 selects circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => (_slot.Parameter1 & FirefleaMovementDefinitions.CircularMovementParameterBit) != 0;
}
