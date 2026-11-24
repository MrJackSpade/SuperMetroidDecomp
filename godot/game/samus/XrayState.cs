using Engine.Actors;
using Engine.Input;
using Game.Data;

namespace Game;

/// <summary>
/// Looking through the X-ray scope: taken up with the scope selected, while the run action is held she stands, or
/// crouches, still; up and down swing the scope's cone, and what the cone passes over is shown for what it is. Everything else
/// in the room waits.
/// </summary>
public sealed class XrayState : PlayerState, IUntouchable, ISamusLook
{
    /// <inheritdoc/>
    public string Look { get; private set; } = "xray_standing_level";

    /// <summary>The names of the ways she holds her head as the scope swings, from looking up to looking down.</summary>
    private static readonly string[] Bands = ["up", "raised", "level", "lowered", "down"];

    private string _stance = "xray_standing";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        _stance = owner.Form == Player.Crouching ? "xray_crouching" : "xray_standing";
        Look = LookAt(samus, 0);
        samus.Xray?.Open(samus);
        // The X-ray's colouring of her ends a stored shine, as it ends a crystal flash's.
        samus.SpendShine();
        owner.Room.Session.Sounds.Request(Sounds.Xray, channel: "samus");
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        // Only letting go of the run ends it; choosing another item meanwhile does not, as on the cartridge.
        if (!owner.Input.IsHeld(Actions.Run) || !owner.IsOnGround)
            return owner.Form == Player.Crouching ? new CrouchingState() : owner.StateAtRest();
        int swing = (owner.Input.IsHeld(GameAction.Down) ? 1 : 0) - (owner.Input.IsHeld(GameAction.Up) ? 1 : 0);
        samus.Xray?.Look(samus, swing);
        Look = LookAt(samus, samus.Xray?.Swing ?? 0);
        return null;
    }

    /// <summary>
    /// Her look with the scope swung so far from level: her head is held one of five ways, changing where the scope's
    /// angle from straight up passes each of the angles her data gives.
    /// </summary>
    private string LookAt(Samus samus, int swing)
    {
        int fromUp = 64 + swing;
        int band = samus.XrayAimBands.Count(from => fromUp >= from);
        if (band >= Bands.Length)
            throw new InvalidOperationException("Samus's data gives the X-ray scope more angles at which her look changes than she has looks.");
        return $"{_stance}_{Bands[band]}";
    }

    /// <inheritdoc/>
    public override void Exit(Player owner)
    {
        ((Samus)owner).Xray?.Close();
        owner.Room.Session.Sounds.Request(Sounds.XrayEnd, channel: "samus");
    }
}
