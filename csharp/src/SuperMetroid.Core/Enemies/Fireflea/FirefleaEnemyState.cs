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
    /// <summary>The slot's Fireflea extension words, kept for the room across occupants.</summary>
    private readonly FirefleaExtensionWords _words;

    /// <summary>Binds a physical enemy record to its three parallel extra-WRAM state arrays.</summary>
    internal FirefleaEnemyState(
        RoomEnemySlot slot, FirefleaExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<FirefleaProgram> Program { get; set; }

    /// <summary>
    /// The signed angular speed selected from the linear speed table, in 256ths of a turn per update with a 16-bit
    /// fraction. Its fraction and whole halves stay in variables A and B, which other enemies' aliased enemy-RAM reads
    /// can observe. The circle advances by its middle sixteen bits, an 8.8 turn delta.
    /// </summary>
    public SnesSignedSixteenSixteen AngularSpeed
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableB), _slot.VariableA);
        internal set
        {
            _slot.VariableA = value.Fraction;
            _slot.VariableB = value.WholeWord;
        }
    }

    /// <summary>
    /// Variable C: the movement radius in pixels, 8 through 64, selected by parameter two's high byte. The slot variable
    /// keeps the exact value for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int Radius
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>
    /// Variable D: the circle angle, wrapping around the turn. Main AI uses its table index as the eight-bit circle angle.
    /// </summary>
    public SnesAngle Angle
    {
        get => SnesAngle.FromRaw(_slot.VariableD);
        internal set => _slot.VariableD = value.RawValue;
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

    /// <summary>Per-slot upper extent of vertical motion, the spawn Y minus the radius with room-coordinate wraparound; only its Y is meaningful. Passing above it, by the wrapped spatial difference, reverses speed after movement.</summary>
    public Position16<RoomPixels> MinimumYExtent
    {
        get => _words.MinimumYExtent;
        internal set => _words.MinimumYExtent = value;
    }

    /// <summary>Per-slot lower extent of vertical motion, the spawn Y plus the radius with room-coordinate wraparound; only its Y is meaningful. Reaching or passing it, by the wrapped spatial difference, reverses speed without clamping.</summary>
    public Position16<RoomPixels> MaximumYExtent
    {
        get => _words.MaximumYExtent;
        internal set => _words.MaximumYExtent = value;
    }

    /// <summary>Byte offset of the current speed entry in the linear speed table, 0 through 2044: eight per speed selector, plus four for the negative half, which reversal toggles.</summary>
    public int SpeedTableIndex
    {
        get => _words.SpeedTableIndex;
        internal set => _words.SpeedTableIndex = value;
    }

    /// <summary>Native init0/parameter-one bit $0002 selects circular motion; otherwise the actor moves vertically between its saved extrema.</summary>
    public bool UsesCircularMovement => (_slot.Parameter1 & FirefleaMovementDefinitions.CircularMovementParameterBit) != 0;
}
