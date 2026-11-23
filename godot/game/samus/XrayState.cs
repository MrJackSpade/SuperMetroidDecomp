using Engine.Actors;
using Engine.Actors.Components;
using Engine.Input;
using Game.Data;

namespace Game;

/// <summary>
/// Looking through the X-ray scope: while the run action is held with the scope selected she stands, or crouches,
/// still; up and down swing the scope's cone, and what the cone passes over is shown for what it is. Everything else
/// in the room waits.
/// </summary>
public sealed class XrayState : PlayerState, IUntouchable, ISamusLook
{
    /// <inheritdoc/>
    public string Look { get; private set; } = "xray_standing";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        Look = owner.Form == Player.Crouching ? "xray_crouching" : "xray_standing";
        samus.Xray?.Open(samus);
        owner.Room.Session.Sounds.Request(Sounds.Xray, channel: "samus");
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        var samus = (Samus)owner;
        if (!owner.Input.IsHeld(Actions.Run) || samus.Selected != Equipment.XrayScope || !owner.IsOnGround)
            return owner.Form == Player.Crouching ? new CrouchingState() : owner.StateAtRest();
        int swing = (owner.Input.IsHeld(GameAction.Down) ? 1 : 0) - (owner.Input.IsHeld(GameAction.Up) ? 1 : 0);
        samus.Xray?.Look(samus, swing);
        return this;
    }

    /// <inheritdoc/>
    public override void Exit(Player owner)
    {
        ((Samus)owner).Xray?.Close();
        owner.Room.Session.Sounds.Request(Sounds.XrayEnd, channel: "samus");
    }
}
