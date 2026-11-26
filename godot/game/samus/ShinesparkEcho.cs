using Engine.Actors;
using Engine.Actors.Components;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of the two echoes of Samus thrown off as a shinespark ends. While she is held where she crashed they circle
/// her, out and round and back in, doing no harm; then they are let go, and fly straight out from her, hurting what
/// they pass through, until they have left the screen. One still on the screen when it has flown 256 pixels out is
/// back on her and flies out again, as the cartridge places it by its distance counted within 256. It has no picture
/// of its own: it repeats the whole picture her body last showed, in her colours, and only on the clock's odd updates.
/// </summary>
public partial class ShinesparkEcho : Projectile
{
    /// <summary>What a flying echo's distance from her is counted within.</summary>
    private const int ReachCountedWithin = 256;

    private int _angle;
    private int _radius;
    private bool _flying;
    private ActorVisual? _picture;

    /// <summary>What the first echo leaves its height to on each update it flies; none for the second.</summary>
    private Compatibility.CrashOrbitTravel? _leavesHeightTo;

    /// <summary>Puts the echo at an angle and distance from her, while it circles.</summary>
    public void Circle(Player samus, int angle, int radius)
    {
        ArgumentNullException.ThrowIfNull(samus);
        (_angle, _radius, _flying) = (angle, radius, false);
        Place(samus);
    }

    /// <summary>Lets the echo go, straight out from her at an angle; the first leaves its height to <paramref name="leavesHeightTo"/> as it flies.</summary>
    public void Fly(int angle, Compatibility.CrashOrbitTravel? leavesHeightTo) =>
        (_angle, _radius, _flying, _leavesHeightTo) = (angle, 0, true, leavesHeightTo);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => _flying && base.DeliversHitNow;

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        _picture = new ActorVisual { Name = "Picture" };
        AddChild(_picture);
        _picture.Bind(room.Content);
    }

    /// <summary>
    /// What turns it away leaves it as it was: it is no shot of a weapon to be launched anew. The cartridge makes it
    /// an ordinary beam shot flying the turned way, which is not carried (#3375).
    /// </summary>
    protected override void OnTurned(Player player, int across, int down)
    {
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Velocity = PixelVelocity.Zero;

        // It is shown as she was last shown, on the clock's odd updates.
        if (_picture is { } picture && Launcher is Samus shown && shown.GetNodeOrNull<SamusVisual>("Visual") is { } visual)
            visual.ShowEchoOn(shown, picture, shows: room.Session.Clock.Updates % 2 != 0);
        if (!_flying || Launcher is not Player samus)
            return;

        // It is always placed from where she is now: it is an echo of her, not a shot with a course of its own.
        _radius += 8;
        Place(samus);
        int x = Position.X.Whole - room.Session.Camera.Left, y = Position.Y.Whole - room.Session.Camera.Top;
        bool onScreen = x >= 0 && x < 256 && y >= 0 && y < 256;
        if (onScreen)
            _leavesHeightTo?.LeaveHeight(Position.Y.Whole);
        else
            _leavesHeightTo?.Clear();
        if (!onScreen)
            Leave(room);
    }

    /// <summary>Takes the echo away, for a shinespark that ends some other way.</summary>
    public void Dismiss(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!HasEnded)
            Leave(room);
    }

    private void Place(Player samus)
    {
        // Flying, it is placed by how far it has flown counted within 256.
        // Each way it lies from her is the reach times the cartridge's own sine, cut toward nothing.
        int reach = _flying ? _radius % ReachCountedWithin : _radius;
        Position = PixelPosition.FromWhole(
            samus.Position.X.Whole + Compatibility.CartridgeAngles.ReachCutShort(_angle, reach),
            samus.Position.Y.Whole + Compatibility.CartridgeAngles.ReachCutShort(_angle - Turns.PerTurn / 4, reach));
    }
}
