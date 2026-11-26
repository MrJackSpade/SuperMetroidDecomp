using Engine.Actors;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game;

/// <summary>
/// One of the four lights of a special attack: what a full charge lets loose when power bombs are selected and
/// Samus carries just one of the wave, ice, spazer and plasma beams. Each kind moves its lights its own way about
/// her: the ice ones circle her and then fly outward; the plasma ones swell out, draw in and burst away; the spazer
/// ones wheel overhead, meet and rain down; the wave ones swing to and fro through her like weights on springs.
/// While any is out she can neither fire nor charge.
/// </summary>
public partial class SpecialAttack : Projectile
{
    /// <summary>The kinds of special attack.</summary>
    public enum Kinds
    {
        /// <summary>Lights that swing through her.</summary>
        Wave,

        /// <summary>Lights that circle her, then fly outward.</summary>
        Ice,

        /// <summary>Lights that wheel overhead and rain down.</summary>
        Spazer,

        /// <summary>A ring of lights that swells, draws in and bursts away.</summary>
        Plasma,
    }

    /// <summary>Which kind this light belongs to.</summary>
    [Export]
    public Kinds Kind { get; set; }

    /// <summary>The sound of the attack coming to its end, by name: the ice lights flying out, the spazer lights meeting, the wave lights going.</summary>
    [Export]
    public string EndSound { get; set; } = "";

    /// <summary>The sound of the wave lights swinging back through her.</summary>
    [Export]
    public string PulseSound { get; set; } = "";

    private enum Stage
    {
        First,
        Second,
        Third,
        Last,
    }

    /// <summary>How long the ice lights circle and the wave lights swing.</summary>
    private const int Lifetime = 600;

    /// <summary>How long she is kept from firing each update a light is out.</summary>
    private const int HoldUpdates = 2;

    /// <summary>Where each wave light starts, from her.</summary>
    private static readonly (int X, int Y)[] WaveStarts = [(128, 128), (128, -128), (-128, -128), (-128, 128)];

    private int _part;
    private int _angle;
    private int _turn;
    private int _radius;
    private int _age;
    private Stage _stage;
    private PixelVelocity _swing;
    private SpecialAttack? _partner;
    private long? _partnerSpentOn;

    /// <summary>
    /// Sets the light going as the <paramref name="part"/>th of its four, turning the way Samus faces. A spazer light
    /// is paired with the other of <paramref name="lights"/> that wheels its way.
    /// </summary>
    public void Begin(int part, bool facingRight, IReadOnlyList<SpecialAttack> lights)
    {
        ArgumentNullException.ThrowIfNull(lights);
        if (part is < 0 or > 3)
            throw new ArgumentOutOfRangeException(nameof(part), part, "A special attack has four lights.");
        if (lights.Count != 4)
            throw new ArgumentException("A special attack has four lights.", nameof(lights));
        (_part, _age, _stage, _swing) = (part, 0, Stage.First, PixelVelocity.Zero);
        _partner = Kind == Kinds.Spazer ? lights[part ^ 2] : null;
        switch (Kind)
        {
            case Kinds.Ice:
                (_angle, _radius, _turn) = (part * 64, 32, facingRight ? 4 : -4);
                break;
            case Kinds.Plasma:
                (_angle, _radius, _turn) = (part * 64, 40, facingRight ? 4 : -4);
                break;
            case Kinds.Spazer:
                // Two wheel one way and two the other.
                (_angle, _radius, _turn) = (0, 40, part % 2 == 0 ? 4 : -4);
                break;
            case Kinds.Wave:
                break;
            default:
                throw new InvalidOperationException($"There is no special attack of kind {Kind}.");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (Launcher is not Player samus)
            throw new InvalidOperationException("A special attack belongs to the player who let it loose.");

        // A spazer light goes on the update after its partner was spent before it fell, as the cartridge has it.
        if (_partnerSpentOn is { } spentOn && room.Session.UpdateNumber > spentOn)
        {
            Leave(room);
            return;
        }

        Velocity = PixelVelocity.Zero;
        if (_age == 0 && Kind == Kinds.Wave)
            Position = samus.Position + PixelOffset.FromWhole(WaveStarts[_part].X, WaveStarts[_part].Y);
        samus.Weapons.WaitAtLeast(HoldUpdates);
        samus.Weapons.DropCharge();
        _age++;
        switch (Kind)
        {
            case Kinds.Ice:
                StepIce(room, samus);
                break;
            case Kinds.Plasma:
                StepPlasma(room, samus);
                break;
            case Kinds.Spazer:
                StepSpazer(room, samus);
                break;
            default:
                StepWave(room, samus);
                break;
        }
    }

    private void StepIce(Room room, Player samus)
    {
        Place(samus, 0);
        if (_stage == Stage.First)
        {
            _angle += _turn;
            if (_age < Lifetime)
                return;

            // Their circling done, they fly outward.
            (_stage, _radius) = (Stage.Second, 40);
            Sound(room, EndSound);
            return;
        }

        if (HasLeftView(room))
        {
            Leave(room);
            return;
        }

        _angle += _turn;
        _radius += 8;
    }

    private void StepPlasma(Room room, Player samus)
    {
        Place(samus, 0);
        _angle += _turn;
        switch (_stage)
        {
            case Stage.First:
                _radius += 4;
                if (_radius >= 192)
                    _stage = Stage.Second;
                break;
            case Stage.Second:
                _radius -= 4;
                if (_radius < 45)
                    _stage = Stage.Third;
                break;
            default:
                if (HasLeftView(room))
                    Leave(room);
                else
                    _radius += 4;
                break;
        }
    }

    private void StepSpazer(Room room, Player samus)
    {
        int screenY = Position.Y.Whole - room.Session.Camera.Top;
        switch (_stage)
        {
            case Stage.First:
                // They wheel half-way round her, then leap out wide above her.
                Place(samus, 0);
                _angle += _turn;
                if (Turns.Wrap(_angle) == 128)
                    (_stage, _radius, _turn) = (Stage.Second, 160, _turn / 2);
                break;

            case Stage.Second:
                // They wheel in toward a point overhead and meet there.
                Place(samus, 114);
                if (Position.Y.Whole - room.Session.Camera.Top < 16)
                {
                    BeginFall();
                    break;
                }

                _angle += _turn;
                _radius -= 5;
                if (_radius > 0)
                    break;
                (_radius, _turn, _angle, _stage) = (0, -_turn, _angle + 128, Stage.Third);
                if (_part == 0)
                    Sound(room, EndSound);
                break;

            case Stage.Third:
                // They wheel out again, and fall.
                Place(samus, 114);
                if (Position.Y.Whole - room.Session.Camera.Top < 16)
                {
                    BeginFall();
                    break;
                }

                _angle += _turn;
                _radius += 5;
                if (_radius >= 96)
                    BeginFall();
                break;

            default:
                Position += PixelOffset.FromWhole(0, 8);
                if (screenY >= 248)
                    Leave(room);
                break;
        }
    }

    /// <summary>A spazer light spent before it falls takes its partner with it.</summary>
    protected override void OnEnded(Room room)
    {
        if (_partner is { HasEnded: false } partner && _stage != Stage.Last)
            partner._partnerSpentOn = room.Session.UpdateNumber;
    }

    /// <summary>
    /// The shot a leading light becomes as it starts to fall: the trailing pair's, an uncharged shot with its own
    /// harm and pictures. Null for a light that falls as it is.
    /// </summary>
    public ShotType? FallsAs { get; set; }

    private void BeginFall()
    {
        Position += PixelOffset.FromWhole(_part < 2 ? 16 : -16, 0);
        _stage = Stage.Last;
        if (FallsAs is { } echo)
            BecomeShotOf(echo);
    }

    private void StepWave(Room room, Player samus)
    {
        if (_age >= Lifetime)
        {
            Sound(room, EndSound);
            Leave(room);
            return;
        }

        // Each is drawn toward her along each axis, and so swings through her and back.
        Pixels before = _swing.X;
        _swing = _swing.WithX(Drawn(_swing.X, samus.Position.X.Whole - Position.X.Whole));
        Position = new PixelPosition(Position.X + _swing.X, Position.Y);
        _swing = _swing.WithY(Drawn(_swing.Y, samus.Position.Y.Whole - Position.Y.Whole));
        Position = new PixelPosition(Position.X, Position.Y + _swing.Y);
        if (_part == 3 && before.IsNegative != _swing.X.IsNegative)
            Sound(room, PulseSound);
    }

    /// <summary>A speed drawn a little further toward where she is, up to its limit.</summary>
    private static Pixels Drawn(Pixels speed, int toward)
    {
        Pixels limit = Pixels.FromWhole(8), gain = Pixels.FromNumber(0.25);
        if (toward < 0)
            return speed > -limit ? speed - gain : speed;
        return speed < limit ? speed + gain : speed;
    }

    /// <summary>Puts the light at its angle and distance from her, or from a point above her.</summary>
    private void Place(Player samus, int above)
    {
        // Each way it lies from her is the radius times the cartridge's own sine, cut toward nothing.
        Position = PixelPosition.FromWhole(
            samus.Position.X.Whole + Compatibility.CartridgeAngles.ReachCutShort(_angle, _radius),
            samus.Position.Y.Whole + Compatibility.CartridgeAngles.ReachCutShort(_angle - Turns.PerTurn / 4, _radius) - above);
    }

    private bool HasLeftView(Room room)
    {
        int x = Position.X.Whole - room.Session.Camera.Left, y = Position.Y.Whole - room.Session.Camera.Top;
        return x < -32 || x >= 288 || y < 16 || y >= 256;
    }

    private static void Sound(Room room, string sound)
    {
        if (sound.Length > 0)
            room.Session.Sounds.Request(sound, channel: "weapons");
    }
}
