namespace SuperMetroid.Core.Enemies.Ripper;

/// <summary>
/// Typed view of the common Ripper-family motion words plus GRipper's two private patrol
/// bounds. The three species reuse the same native variables C/D/E for fractional speed,
/// signed whole speed, and byte offset into the bank-$A0 linear-speed table.
/// </summary>
public sealed class RipperVariantEnemyState
{
    /// <summary>Native enemy variables C, D, and E for the current Ripper slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's Ripper extension words, kept for the room across occupants.</summary>
    private readonly RipperExtensionWords _words;

    /// <summary>Binds a Ripper slot and the GRipper patrol-bound storage shared by the enemy system.</summary>
    internal RipperVariantEnemyState(
        RoomEnemySlot slot, RipperExtensionWords words)
    {
        _slot = slot;
        _words = words;
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
        get => _words.MinimumXPosition;
        internal set => _words.MinimumXPosition = value;
    }

    /// <summary>GRipper extra word $01: inclusive right patrol reversal threshold.</summary>
    public ushort MaximumXPosition
    {
        get => _words.MaximumXPosition;
        internal set => _words.MaximumXPosition = value;
    }
}
