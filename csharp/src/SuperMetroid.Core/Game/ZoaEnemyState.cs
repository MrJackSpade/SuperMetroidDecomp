namespace SuperMetroid.Core.Game;

/// <summary>Same-bank dispatch words used by Zoa AI $A3:B482-$B536.</summary>
public enum ZoaEnemyFunction : ushort
{
    /// <summary>$A3:B482, Function_Zoa_WaitForSamusToGetNear: remain hidden until Samus is fewer than 128 horizontal pixels away, then select the facing rise list.</summary>
    WaitForSamus = 0xb482,
    /// <summary>$A3:B4A8, Function_Zoa_Rising: become visible and move upward 0.5 pixel per NTSC AI update while Samus is above; reaching her height selects horizontal launch.</summary>
    Rising = 0xb4a8,
    /// <summary>$A3:B4D6, Function_Zoa_Shooting: launch the enemy itself horizontally using instruction-controlled speeds; once its center leaves the camera square, hide and restore its spawn whole coordinates.</summary>
    Shooting = 0xb4d6,
}

/// <summary>
/// Named view of Zoa's common slot words and its one parallel $7E:7800 speed-table word.
/// “Shooting” is the ROM's name for the creature launching itself horizontally; it does not
/// create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Initializes a new zoa enemy state instance.</summary>
    internal ZoaEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>
    /// Whole-pixel population position captured at initialization and restored when a launch
    /// ends offscreen, stored in native enemy variables A (X) and B (Y). Only the whole words
    /// are reset: the subpixel fractions accumulated in flight survive.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_slot.VariableA, _slot.VariableB);
        internal set
        {
            _slot.VariableA = value.X;
            _slot.VariableB = value.Y;
        }
    }

    /// <summary>Element index zero through three in the instruction-list pointer table.</summary>
    public ZoaAnimationSelector InstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Last installed animation selector, initially zero; $A3:B537 compares it with the requested selector so unchanged lists keep their timer and loop state.</summary>
    public ZoaAnimationSelector PreviousInstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Same-bank AI dispatch word in slot variable F, initialized to wait and transitioned through rise, horizontal launch, and offscreen return to wait.</summary>
    public ZoaEnemyFunction Function
    {
        get => (ZoaEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Byte offset zero, four, eight, or twelve into the five-row X-speed table.</summary>
    public ushort XSpeedTableIndex { get; internal set; }
}
