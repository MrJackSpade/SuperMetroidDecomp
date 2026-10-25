using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hud.GameplayMessageBoxDefinitions;

namespace SuperMetroid.Core.Hud;

/// <summary>Chooses the button glyph a message panel shows for a binding, and where in the panel it goes (#1416).</summary>
public static class GameplayMessageButtonGlyphs
{
    /// <summary>The glyph for <paramref name="binding"/>, testing A, B, X, Y, Select, L, R in that order; the first matching button wins even for a malformed multibit binding.</summary>
    public static ushort ResolveGlyphWord(SnesButton binding) => binding switch
    {
        var bits when (bits & SnesButton.A) != 0 => Buttons.A,
        var bits when (bits & SnesButton.B) != 0 => Buttons.B,
        var bits when (bits & SnesButton.X) != 0 => Buttons.X,
        var bits when (bits & SnesButton.Y) != 0 => Buttons.Y,
        var bits when (bits & SnesButton.Select) != 0 => Buttons.Select,
        var bits when (bits & SnesButton.L) != 0 => Buttons.L,
        var bits when (bits & SnesButton.R) != 0 => Buttons.R,
        SnesButton.None or
            SnesButton.R or
            SnesButton.L or
            SnesButton.X or
            SnesButton.A or
            SnesButton.Right or
            SnesButton.Left or
            SnesButton.Down or
            SnesButton.Up or
            SnesButton.Start or
            SnesButton.Select or
            SnesButton.Y or
            SnesButton.B => Buttons.UnknownGlyph.Raw,
        _ => Buttons.UnknownGlyph.Raw,
    };

    /// <summary>The byte offset in <paramref name="messageId"/>'s panel where the button glyph goes; zero for a message without one.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> has no glyph-table entry.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="messageId"/> is the table's dummy boundary.</exception>
    public static ushort SpecialGlyphByteOffset(GameplayMessageId messageId)
    {
        if ((uint)((byte)messageId - 1) >= Buttons.GlyphTableEntries) throw new ArgumentOutOfRangeException(nameof(messageId));
        return messageId switch
        {
            GameplayMessageId.MissileTank or GameplayMessageId.SuperMissileTank or GameplayMessageId.Bombs => Buttons.AmmunitionPanelGlyphOffset,
            GameplayMessageId.PowerBombTank or GameplayMessageId.GrappleBeam or GameplayMessageId.XrayScope => Buttons.SelectedItemPanelGlyphOffset,
            GameplayMessageId.SpeedBooster => Buttons.SpeedBoosterPanelGlyphOffset,
            GameplayMessageId.None or GameplayMessageId.EnergyTank or GameplayMessageId.VariaSuit or
                GameplayMessageId.SpringBall or GameplayMessageId.MorphBall or GameplayMessageId.ScrewAttack or
                GameplayMessageId.HiJumpBoots or GameplayMessageId.SpaceJump or GameplayMessageId.ChargeBeam or
                GameplayMessageId.IceBeam or GameplayMessageId.WaveBeam or GameplayMessageId.SpazerBeam or
                GameplayMessageId.PlasmaBeam or GameplayMessageId.MapDataAccessCompleted or
                GameplayMessageId.EnergyRechargeCompleted or GameplayMessageId.MissileRechargeCompleted or
                GameplayMessageId.SaveConfirmation or GameplayMessageId.SaveCompleted or
                GameplayMessageId.ReserveTank or GameplayMessageId.GravitySuit or
                GameplayMessageId.GunshipSaveConfirmation => 0,
            _ => throw new InvalidOperationException($"Undefined GameplayMessageId {messageId}."),
        };
    }
}
