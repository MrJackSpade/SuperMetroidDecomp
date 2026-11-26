using Engine.Actors;
using Engine.Input;
using Engine.Shared;
using Engine.World;
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

    /// <summary>How many more updates the scope takes to open, in which the run action is not looked at.</summary>
    private Countdown _opening = Countdown.Over;

    /// <summary>Once the run action has been found let go, how many more updates she is held; none while the scope is not closing.</summary>
    private int? _closing;

    /// <summary>Whether the cone has begun to open: the first update after the warm-up that finds the run action held only begins it.</summary>
    private bool _coneBegun;

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override bool InControl => false;

    /// <summary>Time stands still for everything but her: the enemies, every shot and pickup, and the room itself wait.</summary>
    public override Stopped Stops => Stopped.Enemies | Stopped.Projectiles | Stopped.Room;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        owner.StopAcross();
        _stance = owner.Form == Player.Crouching ? "xray_crouching" : "xray_standing";
        Look = LookAt(samus, 0);
        (_opening, _closing, _coneBegun) = (Countdown.Of(samus.XrayWarmUpUpdates), null, false);
        samus.Xray?.Open(samus);
        // The X-ray's colouring of her ends a stored shine, as it ends a crystal flash's.
        samus.SpendShine();
        owner.Room.Session.Sounds.Request(Sounds.Xray, channel: "samus");
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        if (!owner.IsOnGround)
            return AtRest(owner);

        // While it opens the run action is not looked at, so a scope taken up is held at least that long.
        if (_opening.IsRunning)
        {
            _opening = _opening.Stepped();
            return null;
        }

        // Closing, she and the room are held its updates more, and the update after them lets go.
        if (_closing is { } left)
        {
            if (left == 0)
                return AtRest(owner);
            _closing = left - 1;
            return null;
        }

        // Only letting go of the run ends it; choosing another item meanwhile does not.
        if (!owner.Input.IsHeld(Actions.Run))
        {
            _closing = samus.XrayClosingUpdates;
            return null;
        }

        // The cartridge spends one update with no cone after the warm-up, and opens it from the next.
        if (!_coneBegun)
        {
            _coneBegun = true;
            return null;
        }

        // Up wins when up and down are both held.
        int swing = owner.Input.IsHeld(GameAction.Up) ? -1 : owner.Input.IsHeld(GameAction.Down) ? 1 : 0;
        samus.Xray?.Look(samus, swing);
        Look = LookAt(samus, samus.Xray?.Swing ?? 0);
        return null;
    }

    /// <summary>The state she is left in as the scope lets go: crouched if she crouched, else at rest.</summary>
    private static PlayerState AtRest(Player owner) => owner.Form == Player.Crouching ? new CrouchingState() : owner.StateAtRest();

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
