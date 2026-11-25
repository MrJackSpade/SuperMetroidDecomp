using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Data;
using Godot;

namespace Game;

/// <summary>
/// Samus's grapple beam: a line fired the way she aims that reaches out until it finds something to latch onto, runs
/// out of reach, or meets a wall. Latched, it holds her while she swings; the swinging itself is her
/// <see cref="GrapplingState"/>. It is a child of Samus, and what it latches onto it asks only through
/// <see cref="ILatchable"/> and the terrain's block rules.
/// </summary>
public partial class SamusGrapple : Node2D
{
    /// <summary>How far the line reaches out each update, as a number of pixels, fired straight across, up or down.</summary>
    [Export]
    public double ExtendSpeed { get; set; } = 12;

    /// <summary>How far the line reaches out along each axis each update, fired diagonally.</summary>
    [Export]
    public double DiagonalExtendSpeed { get; set; } = 8.5;

    /// <summary>The furthest the line reaches, in pixels.</summary>
    [Export]
    public int Reach { get; set; } = 144;

    /// <summary>The damage the tip of the line does to what it touches.</summary>
    [Export]
    public int Damage { get; set; } = 1;

    /// <summary>The colour the line is drawn in.</summary>
    [Export]
    public Color Colour { get; set; } = new(0.4f, 0.8f, 1f);

    private Samus? _samus;
    private bool _extending;
    private PixelPosition _tip;
    private PixelPosition _from;
    private int _across;
    private int _down;
    private Pixels _length;
    private Pixels _reached;
    private ILatchable? _held;
    private Block? _heldBlock;

    /// <summary>Whether the line is out: reaching, or latched.</summary>
    public bool IsOut => _extending || IsLatched;

    /// <summary>Whether the line has hold of something.</summary>
    public bool IsLatched { get; private set; }

    /// <summary>Where the line holds, when latched.</summary>
    public PixelPosition Anchor => _held?.LatchPoint ?? _tip;

    /// <summary>How far the line had reached out, in whole pixels, when it took hold.</summary>
    public int Length => _length.Whole;

    private GrappleSwingRules? _swing;

    /// <summary>The numbers of the swing a latched line holds her in.</summary>
    public GrappleSwingRules Swing => _swing ?? throw new InvalidOperationException("The grapple beam's values were not read.");

    /// <summary>Reads the grapple's values from the group of Samus's data file that gives them.</summary>
    public void ReadValues(DataNode data)
    {
        ExtendSpeed = data.Distance("extend_speed").ToNumber();
        DiagonalExtendSpeed = data.Distance("extend_diagonal_speed").ToNumber();
        Reach = data.Int("reach");
        Damage = data.Int("damage");
        _swing = GrappleSwingRules.Read(data.Get("swing"));
    }

    /// <summary>Fires the line the way <paramref name="samus"/> aims, if it is not already out.</summary>
    public void Fire(Samus samus)
    {
        if (IsOut)
            return;
        _samus = samus;
        (_across, _down) = samus.AimDirection;
        _from = samus.GrapplePoint;
        _tip = _from;
        _length = Pixels.Zero;
        _reached = Pixels.Zero;
        _extending = true;
        samus.Room.Session.Sounds.Request(Sounds.GrappleFire, channel: "weapons", priority: SoundPriority.WhenIdle);

        // Its firing, like a beam shot's, takes back the asking again of a charge's sound.
        samus.ForgetChargingSoundResume();
    }

    /// <summary>Lets go of whatever the line holds and draws it in. Its end is heard whenever it was out, latched or not.</summary>
    public void Release()
    {
        if (IsOut && _samus is not null)
            _samus.Room.Session.Sounds.Request(Sounds.GrappleRelease, channel: "weapons", priority: SoundPriority.Essential);
        _held?.Unlatched();
        _held = null;
        if (_heldBlock is not null && _samus is not null)
            _samus.Room.Terrain.LetGoOf(_heldBlock);
        _heldBlock = null;
        IsLatched = false;
        _extending = false;
        QueueRedraw();
    }

    /// <summary>
    /// Plays one update of the line while it reaches out: it grows, hurts what its tip touches, and latches onto the
    /// first thing that can be latched. Letting go of the attack, a wall, or running out of reach draws it in.
    /// </summary>
    public void Step(Samus samus)
    {
        if (!_extending)
        {
            // A block that has broken away holds nothing.
            if (IsLatched && _heldBlock is { Broken: true })
                Release();
            if (IsLatched)
                QueueRedraw();
            return;
        }

        if (!samus.Input.IsHeld(GameAction.Attack) || samus.Selected != Equipment.GrappleBeam)
        {
            Release();
            return;
        }

        Room room = samus.Room;
        _from = samus.GrapplePoint;

        // The line's length is counted along either axis; its fraction of a pixel carries over to the next update.
        _reached += Pixels.FromNumber(_across != 0 && _down != 0 ? DiagonalExtendSpeed : ExtendSpeed);
        int pixels = _reached.Whole - _length.Whole;
        for (int step = 0; step < pixels; step++)
        {
            _length += Pixels.FromWhole(1);
            _tip = _from + PixelOffset.FromWhole(_across * _length.Whole, _down * _length.Whole);
            var tip = PixelRect.Around(_tip.X.Whole, _tip.Y.Whole, 2, 2);

            ILatchable? latchable = room.Collision.Overlapping(tip, ColliderRole.Hurtbox, except: samus)
                .Concat(room.Collision.Overlapping(tip, ColliderRole.Solid, except: samus))
                .Select(collider => collider.CollisionOwner as ILatchable)
                .FirstOrDefault(found => found is { CanBeLatched: true });
            if (latchable is not null)
            {
                Latch(samus, latchable, null);
                return;
            }

            Block? block = room.Terrain.BlocksIn(tip).FirstOrDefault(found => found.Type.Contact is BlockContact.Latch or BlockContact.LatchOnce);
            if (block is not null)
            {
                _tip = PixelPosition.FromWhole(block.Rect.Left + block.Rect.Width / 2, block.Rect.Top + block.Rect.Height / 2);
                Latch(samus, null, block);
                return;
            }

            var hit = new Hit(Damage, HitSide.Player, new HitKind(HitKinds.Grapple)) { Source = samus };
            if (room.DeliverHit(hit, tip, from: samus).Any(result => result.Result != HitResult.Ignored) ||
                room.Terrain.IsSolidIn(tip, SlopeTreatment.Wall) || _length.Whole >= Reach)
            {
                Release();
                return;
            }
        }

        QueueRedraw();
    }

    /// <summary>Takes hold of something and hands Samus to her swing.</summary>
    private void Latch(Samus samus, ILatchable? held, Block? block)
    {
        _held = held;
        _heldBlock = block;
        _extending = false;
        IsLatched = true;
        held?.Latched();
        if (block is not null)
            samus.Room.Terrain.LatchedOnto(block);
        samus.Room.Session.Sounds.Request(Sounds.GrappleLatch, channel: "weapons");
        // Seized, she holds the line but hangs where she is held; otherwise she swings from it.
        if (samus.State is not GrabbedState)
            samus.ChangeState(new GrapplingState());
        QueueRedraw();
    }

    /// <inheritdoc/>
    public override void _Draw()
    {
        if (!IsOut || _samus is null)
            return;
        PixelPosition end = IsLatched ? Anchor : _tip;
        PixelOffset toMuzzle = _samus.GrapplePoint - _samus.Position;
        var from = new Vector2(toMuzzle.X.ToDisplay(), toMuzzle.Y.ToDisplay());
        PixelOffset toEnd = end - _samus.Position;
        DrawLine(from, new Vector2(toEnd.X.ToDisplay(), toEnd.Y.ToDisplay()), Colour, 2);
    }
}
