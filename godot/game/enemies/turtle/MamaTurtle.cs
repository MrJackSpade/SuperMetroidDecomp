using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// The Mama Turtle: asleep in her shell, a rounded hill that Samus and the babies can walk over. Bother one of her
/// babies and she wakes, comes out, and spins about the room, bouncing off the walls. Samus can ride the spinning
/// shell, which then climbs high into the room before falling back.
/// </summary>
/// <remarks>
/// Her timelines are "asleep"; "leave_shell_left" and "leave_shell_right", whose cue "enter_shell" sends her back in;
/// "enter_shell_left" and "enter_shell_right", whose cue "rise:left" or "rise:right" lifts her off spinning that way;
/// and "spinning". Her data gives the height of her shell at each pixel out from her middle under "shell_left" and
/// "shell_right". Her solid collider is her shell and her hitbox the part of her that hurts once she is awake.
/// </remarks>
public partial class MamaTurtle : Enemy
{
    /// <summary>Half her height once awake, in pixels.</summary>
    [Export, DataValue("awake_half_height")]
    public int AwakeHalfHeight { get; set; } = 16;

    /// <summary>How many updates she rises before she starts to drift.</summary>
    [Export, DataValue("rise_updates")]
    public int RiseUpdates { get; set; } = 16;

    /// <summary>The speed across she gains each update while hovering, as a number of pixels.</summary>
    [Export, DataValue("hover_quickening")]
    public double HoverQuickening { get; set; } = 0.0625;

    /// <summary>The fastest she drifts, in pixels an update.</summary>
    [Export, DataValue("fastest_hover")]
    public int FastestHover { get; set; } = 3;

    /// <summary>The height in the room, in pixels from its top, above which she stops climbing with a rider.</summary>
    [Export, DataValue("ridden_peak")]
    public int RiddenPeak { get; set; } = 488;

    /// <summary>How far she climbs each update with a rider, in pixels.</summary>
    [Export, DataValue("ridden_climb_speed")]
    public int RiddenClimbSpeed { get; set; } = 7;

    /// <summary>How many updates she hangs at the top of her climb.</summary>
    [Export, DataValue("peak_pause")]
    public int PeakPause { get; set; } = 30;

    /// <summary>The downward speed she gains each update falling, as a number of pixels.</summary>
    [Export, DataValue("fall_quickening")]
    public double FallQuickening { get; set; } = 0.125;

    /// <summary>The fastest she falls, in pixels an update.</summary>
    [Export, DataValue("fastest_fall")]
    public int FastestFall { get; set; } = 4;

    /// <summary>How hard she shakes the room when she hits a wall.</summary>
    [Export, DataValue("wall_shake_strength")]
    public int WallShakeStrength { get; set; } = 1;

    /// <summary>How many updates the room shakes for.</summary>
    [Export, DataValue("wall_shake_updates")]
    public int WallShakeUpdates { get; set; } = 16;

    /// <summary>The sound of her hitting a wall, by name.</summary>
    [Export, DataValue("wall_sound")]
    public string WallSound { get; set; } = "";

    private enum Doing
    {
        Placed,
        PlacedAndWoken,
        Posed,
        Asleep,
        Woken,
        LeavingShell,
        EnteringShell,
        RisingToHover,
        Hovering,
        RisingToPeak,
        HoveringAtPeak,
        Falling,
    }

    /// <summary>
    /// How the push out of her shell is tried, as the cartridge moves an enemy across: only where it would end, and
    /// sloped ground that holds things up by their middle no part of it.
    /// </summary>
    private static readonly MoveOptions PushesAcross =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private readonly List<int> _shellLeft = [];
    private readonly List<int> _shellRight = [];
    private Doing _doing;
    private Countdown _wait;
    private Pixels _quickening;
    private Collider? _hurts;

    /// <summary>Whether she lies in her shell, asleep or only just woken, so that her babies walk over it.</summary>
    public bool LiesInShell => _doing is Doing.Asleep or Doing.Woken;

    /// <summary>Wakes her, if she is asleep.</summary>
    public void Rouse()
    {
        if (_doing == Doing.Placed)
            _doing = Doing.PlacedAndWoken;
        else if (_doing == Doing.Asleep)
            _doing = Doing.Woken;
    }

    /// <summary>
    /// How high her shell stands above her middle, in pixels and negative, under something <paramref name="across"/>
    /// pixels to the right of her middle; null beyond the shell's edge.
    /// </summary>
    public int? ShellSurface(int across)
    {
        int distance = Math.Abs(across);
        if (distance >= _shellLeft.Count)
            return null;
        return across > 0 ? _shellRight[distance - 1] : _shellLeft[distance];
    }

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _shellLeft.Clear();
        _shellLeft.AddRange(data.List("shell_left").Select(height => height.AsInt()));
        _shellRight.Clear();
        _shellRight.AddRange(data.List("shell_right").Select(height => height.AsInt()));
        // Under her middle and to its left a height is read at the distance itself; to the right at one less, as the
        // cartridge's two tables are read. So the right has one height fewer than the left.
        if (_shellLeft.Count == 0 || _shellRight.Count != _shellLeft.Count - 1)
            throw data.Problem("must list at least one shell height to the left, and one fewer to the right than to the left");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        Play("asleep");
        _hurts = EntityNode.CollidersOf(this).FirstOrDefault(collider => collider.Role == ColliderRole.Hitbox);
        if (_hurts is not null)
            _hurts.Enabled = false;
        SetShellHeight(0);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue == "enter_shell")
        {
            _doing = Doing.EnteringShell;
        }
        else if (cue is "rise:left" or "rise:right")
        {
            _doing = Doing.RisingToHover;
            Velocity = Velocity.WithX(Velocity.X.WithWhole(cue == "rise:left" ? -1 : 1));
            _wait = Countdown.Of(RiseUpdates);
            Play("spinning", restart: true);
        }
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        // Asleep she hurts nobody; awake, only her hitbox does, and only while she leaves her shell or is out of it
        // and moving: not while she poses or turns to go back in.
        bool hurts = _doing is Doing.LeavingShell or Doing.RisingToHover or Doing.Hovering or Doing.RisingToPeak
            or Doing.HoveringAtPeak or Doing.Falling;
        if (hurts && _hurts is { Enabled: true } && !Room.Player.IsInvincible)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Placed:
                _doing = Doing.Asleep;
                return;

            case Doing.PlacedAndWoken:
                _doing = Doing.Woken;
                return;

            case Doing.Posed:
                return;

            case Doing.Asleep:
                // Her shell is as tall as it stands under Samus, so that she walks over its curve.
                int? surface = ShellSurface(Target.X.Whole - Position.X.Whole);
                SetShellHeight(surface is { } height ? -height : 0);
                if (surface is null || Solid is not { Enabled: true } shell)
                    return;
                PixelRect rider = Room.Player.BodyRect;
                int sunk = rider.Bottom - shell.RoomRect.Top;
                if (sunk > 0 && Room.Player.Position.Y.Whole < Position.Y.Whole)
                    ShiftPlayer(PixelOffset.FromWhole(0, -sunk));
                return;

            case Doing.Woken:
                _doing = Doing.LeavingShell;
                if (_hurts is not null)
                    _hurts.Enabled = true;
                return;

            case Doing.LeavingShell:
                // Every other update of the short count, which a door's scroll starts over.
                if (Room.Session.Clock.ShortUpdates % 2 != 0)
                    return;
                // She shudders up out of the ground a pixel at a time.
                bool ridden = IsRiddenByPlayer;
                MoveCarrying(PixelOffset.FromWhole(Position.Y.Whole % 2 == 0 ? -1 : 1, -1));
                if (ridden)
                    ShiftPlayer(PixelOffset.FromWhole(-1, 0));
                SetShellHeight(AwakeHalfHeight);
                if (Room.Mover.MoveAcross(Shell(), Pixels.FromWhole(1), PushesAcross).Blocked)
                    return;
                Play("leave_shell_left", restart: true);
                _doing = Doing.Posed;
                return;

            case Doing.EnteringShell:
                Play(Position.X.Whole >= Target.X.Whole ? "enter_shell_left" : "enter_shell_right", restart: true);
                _doing = Doing.Posed;
                return;

            case Doing.RisingToHover:
                if (Blocked(PixelOffset.FromWhole(0, -1)))
                    return;
                MoveCarrying(PixelOffset.FromWhole(0, -1));
                if (_wait.IsOver)
                    return;
                _wait = _wait.Stepped();
                if (_wait.IsRunning)
                    return;
                _quickening = Position.X.Whole < Target.X.Whole ? Pixels.FromNumber(HoverQuickening) : -Pixels.FromNumber(HoverQuickening);
                Velocity = Velocity.WithX(Pixels.Zero);
                _doing = Doing.Hovering;
                return;

            case Doing.Hovering:
                var drift = new PixelOffset(Velocity.X, Pixels.Zero);
                if (StoppedFlush((Position.X + Velocity.X).Whole - Position.X.Whole, across: true))
                {
                    // Off a wall she comes back the other way, by the cartridge's own lopsided arithmetic.
                    Pixels before = Velocity.X;
                    Velocity = Velocity.WithX(Reversed(before));
                    _quickening = Reversed(_quickening);
                    if (before.Whole == -1)
                        _quickening -= Pixels.FromWhole(1);
                    Room.Shake(WallShakeStrength, WallShakeUpdates);
                    Sound(WallSound);
                    return;
                }

                MoveCarrying(drift);
                if (IsRiddenByPlayer)
                    _doing = Doing.RisingToPeak;
                Pixels faster = Velocity.X + _quickening;
                if (faster.Whole >= FastestHover || faster.Whole <= -FastestHover)
                    faster = faster.WithWhole(Velocity.X.IsNegative ? -FastestHover : FastestHover);
                Velocity = Velocity.WithX(faster);
                return;

            case Doing.RisingToPeak:
                if (Position.Y.Whole < RiddenPeak)
                {
                    _wait = Countdown.Of(PeakPause);
                    _doing = Doing.HoveringAtPeak;
                    Velocity = Velocity.WithY(Pixels.Zero);
                }
                else if (IsRiddenByPlayer)
                {
                    MoveCarrying(PixelOffset.FromWhole(0, -RiddenClimbSpeed));
                }
                else
                {
                    _doing = Doing.Falling;
                    Velocity = Velocity.WithY(Pixels.Zero);
                }

                return;

            case Doing.HoveringAtPeak:
                if (_wait.IsOver)
                    return;
                _wait = _wait.Stepped();
                if (_wait.IsOver)
                    _doing = Doing.Falling;
                return;

            case Doing.Falling:
                if (Velocity.Y < Pixels.FromWhole(FastestFall))
                    Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(FallQuickening));
                // She falls by the whole pixels of her speed alone.
                if (!StoppedFlush(Velocity.Y.Whole, across: false))
                {
                    MoveCarrying(PixelOffset.FromWhole(0, Velocity.Y.Whole));
                    return;
                }

                Play(Velocity.X.IsNegative ? "leave_shell_left" : "leave_shell_right", restart: true);
                _doing = Doing.Posed;
                return;

            default:
                throw new InvalidOperationException($"Undefined Mama Turtle stage {_doing}.");
        }
    }

    /// <summary>Makes her shell, and the part of her that hurts, stand <paramref name="halfHeight"/> pixels each way from her middle.</summary>
    private void SetShellHeight(int halfHeight)
    {
        if (Solid is not { } shell)
            return;
        shell.Rect = new PixelRect(-shell.Size.X / 2, -halfHeight, shell.Size.X, halfHeight * 2);
        shell.Enabled = halfHeight > 0;
    }

    /// <summary>Her shell, which she meets the room's blocks with: she has no other body.</summary>
    private Collider Shell() => Solid ?? throw new InvalidOperationException($"The Mama Turtle \"{Name}\" has no shell collider.");

    /// <summary>Whether terrain stops her shell moving by <paramref name="offset"/>.</summary>
    private bool Blocked(PixelOffset offset)
    {
        if (Solid is not { } shell)
            return false;
        PixelRect moved = shell.RoomRect.MovedBy(offset.X.Whole, offset.Y.Whole);
        return Room.Terrain.IsSolidIn(moved, SlopeTreatment.Wall);
    }

    /// <summary>
    /// Whether terrain stops her shell moving <paramref name="pixels"/> whole pixels across, or down. Stopped, she is
    /// put against what stopped her as the cartridge puts her: as far as she can go, at the far edge of her last pixel
    /// moving right or down and at its near edge moving left or up.
    /// </summary>
    private bool StoppedFlush(int pixels, bool across)
    {
        PixelOffset By(int distance) => across ? PixelOffset.FromWhole(distance, 0) : PixelOffset.FromWhole(0, distance);
        if (pixels == 0 || !Blocked(By(pixels)))
            return false;
        int way = Math.Sign(pixels), free = 0;
        while (free + way != pixels && !Blocked(By(free + way)))
            free += way;
        Pixels from = across ? Position.X : Position.Y;
        Pixels to = Pixels.FromParts(from.Whole + free, way > 0 ? Pixels.FractionsPerPixel - 1 : 0);
        MoveCarrying(across ? new PixelOffset(to - from, Pixels.Zero) : new PixelOffset(Pixels.Zero, to - from));
        return true;
    }

    /// <summary>A speed turned round as the cartridge turns it: its whole pixels complemented and its fine part negated.</summary>
    private static Pixels Reversed(Pixels speed) =>
        Pixels.FromParts(~speed.Whole, (Pixels.FractionsPerPixel - speed.Fraction) % Pixels.FractionsPerPixel);
}
