namespace SuperMetroid.Core.Game;

/// <summary>
/// One complete fourteen-byte <c>EprojDef</c> record from bank <c>$86</c>.
/// </summary>
/// <param name="PreInstruction">Per-frame callback installed before the first frame-list tick.</param>
/// <param name="InitialInstructionList">Initial mixed animation/behavior program.</param>
/// <param name="RadiusWord">Packed X radius in the low byte and Y radius in the high byte.</param>
/// <param name="Properties">Damage and collision/draw property bits.</param>
/// <param name="TouchInstructionList">Optional list installed when the actor touches Samus.</param>
/// <param name="ShotInstructionList">List installed when a destructible actor is shot.</param>
internal readonly record struct EnemyProjectileDefinition(
    ushort PreInstruction,
    ushort InitialInstructionList,
    ushort RadiusWord,
    ushort Properties,
    ushort TouchInstructionList,
    ushort ShotInstructionList)
{
    /// <summary>The collision half-extents decoded from the packed radius word (X low byte, Y high byte).</summary>
    internal Extent16<RoomPixels> Radius => new(unchecked((byte)RadiusWord), SnesLittleEndian.HighByte(RadiusWord));
    /// <summary>Low twelve property bits used as Samus contact damage.</summary>
    internal ushort Damage => unchecked((ushort)(Properties & 0x0fff));
    /// <summary>Property bit $1000 selects the high OAM draw pass.</summary>
    internal EnemyProjectileDrawPriority DrawPriority =>
        (Properties & 0x1000) != 0
            ? EnemyProjectileDrawPriority.High
            : EnemyProjectileDrawPriority.Low;
    /// <summary>Cleared bit $2000 permits the projectile's damage value to affect Samus.</summary>
    internal bool CanDamageSamus => (Properties & 0x2000) == 0;
    /// <summary>Set bit $4000 retains the actor after touching Samus.</summary>
    internal bool PersistsOnSamusContact => (Properties & 0x4000) != 0;
    /// <summary>Set bit $8000 lets this projectile intercept Samus's shots.</summary>
    internal bool BlocksSamusProjectiles => (Properties & 0x8000) != 0;
}
