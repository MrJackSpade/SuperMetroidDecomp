using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hud.GameplayMessageBoxDefinitions;

namespace SuperMetroid.Core.Hud;

/// <summary>Chooses the button glyph a message panel shows for a binding, and where in the panel it goes (#1416).</summary>
public static class GameplayMessageButtonGlyphs
{
    /// <summary>The glyph for <paramref name="binding"/>, testing A, B, X, Y, Select, L, R in that order; the first matching button wins even for a malformed multibit binding.</summary>
    public static BackgroundTile ResolveGlyph(ControllerButton binding) => binding switch
    {
        var bits when (bits & ControllerButton.A) != ControllerButton.None => Buttons.A,
        var bits when (bits & ControllerButton.B) != ControllerButton.None => Buttons.B,
        var bits when (bits & ControllerButton.X) != ControllerButton.None => Buttons.X,
        var bits when (bits & ControllerButton.Y) != ControllerButton.None => Buttons.Y,
        var bits when (bits & ControllerButton.Select) != ControllerButton.None => Buttons.Select,
        var bits when (bits & ControllerButton.L) != ControllerButton.None => Buttons.L,
        var bits when (bits & ControllerButton.R) != ControllerButton.None => Buttons.R,
        ControllerButton.None or
            ControllerButton.R or
            ControllerButton.L or
            ControllerButton.X or
            ControllerButton.A or
            ControllerButton.Right or
            ControllerButton.Left or
            ControllerButton.Down or
            ControllerButton.Up or
            ControllerButton.Start or
            ControllerButton.Select or
            ControllerButton.Y or
            ControllerButton.B => Buttons.UnknownGlyph,
        _ => Buttons.UnknownGlyph,
    };

    /// <summary>The byte offset in <paramref name="messageId"/>'s panel where the button glyph goes; zero for a message without one.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messageId"/> is no message, or a message the glyph table does not list.</exception>
    public static ushort SpecialGlyphByteOffset(GameplayMessageId messageId)
    {
        if (!Enum.IsDefined(messageId) || messageId is GameplayMessageId.None or GameplayMessageId.GunshipSaveConfirmation)
            throw new ArgumentOutOfRangeException(nameof(messageId));
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
