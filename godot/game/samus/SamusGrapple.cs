using Engine.Actors;
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
/// <remarks>
/// Reaching out, it goes as the cartridge's does. The update it is fired on only begins it. On each update after, with
/// the attack still held, its length is counted on by a set amount, and at its reach it is given up; what can be
/// latched or is hurt by it is looked for at its end as it stands; then its end moves in equal substeps, a quarter of
/// its speed each, and the block at the end is asked after each: one that holds the line takes it at once, but one
/// that only stops it gives the line up only if it is the last substep's, so a thin wall crossed within an update
/// does not stop it. A line given up while reaching out ends on the update after, when its end is heard. While it is
/// out her beams' colours are those of the beams its data names, and her own again as it ends.
/// </remarks>
public partial class SamusGrapple : Node2D
{
    /// <summary>How far the line reaches out each update, as a number of pixels, fired straight across, up or down.</summary>
    [Export]
    public double ExtendSpeed { get; set; } = 12;

    /// <summary>How far the line reaches out along each axis each update, fired diagonally.</summary>
    [Export]
    public double DiagonalExtendSpeed { get; set; } = 8.5;

    /// <summary>How much its length is counted on each update it reaches out.</summary>
    [Export]
    public int LengthEachUpdate { get; set; } = 12;

    /// <summary>How many equal substeps its end moves in each update, the block at the end asked after each.</summary>
    [Export]
    public int Substeps { get; set; } = 4;

    /// <summary>The length at which a line that has met nothing is given up.</summary>
    [Export]
    public int Reach { get; set; } = 128;

    /// <summary>The damage the tip of the line does to what it touches.</summary>
    [Export]
    public int Damage { get; set; } = 1;

    /// <summary>The beams whose colours her beams' colours show while the line is out, as her colours name them.</summary>
    [Export]
    public string FiringBeams { get; set; } = "ice";

    /// <summary>The colour the line is drawn in.</summary>
    [Export]
    public Color Colour { get; set; } = new(0.4f, 0.8f, 1f);

    private enum Phase
    {
        /// <summary>Not out.</summary>
        Inactive,

        /// <summary>Reaching out.</summary>
        Firing,

        /// <summary>Given up while reaching out: it ends on the next update.</summary>
        CancelPending,

        /// <summary>Holding something.</summary>
        Latched,
    }

    private Samus? _samus;
    private Phase _phase;
    private PixelPosition _tip;
    private int _across;
    private int _down;

    /// <summary>Where the line leaves her, from her place, as it was fired.</summary>
    private PixelOffset _fromHer;

    /// <summary>How far the line's end has moved from where it left her, its part of a pixel kept.</summary>
    private PixelOffset _reached;

    /// <summary>The line's length as it is counted while it reaches out.</summary>
    private int _length;

    /// <summary>Whether this update's step of the line is already spent: by its firing, which only begins it, or by its being let go.</summary>
    private bool _stepSpent;
    private ILatchable? _held;
    private Block? _heldBlock;
    private GrappleSwingRules? _swing;

    /// <summary>Whether the line is out: reaching, given up and not yet ended, or latched.</summary>
    public bool IsOut => _phase != Phase.Inactive;

    /// <summary>Whether the line has hold of something.</summary>
    public bool IsLatched => _phase == Phase.Latched;

    /// <summary>Where the line holds, when latched.</summary>
    public PixelPosition Anchor => _held?.LatchPoint ?? _tip;

    /// <summary>The line's length as it was counted when it took hold.</summary>
    public int Length => _length;

    /// <summary>The numbers of the swing a latched line holds her in.</summary>
    public GrappleSwingRules Swing => _swing ?? throw new InvalidOperationException("The grapple beam's values were not read.");

    /// <summary>Reads the grapple's values from the group of Samus's data file that gives them.</summary>
    public void ReadValues(DataNode data)
    {
        ExtendSpeed = data.Distance("extend_speed").ToNumber();
        DiagonalExtendSpeed = data.Distance("extend_diagonal_speed").ToNumber();
        LengthEachUpdate = data.Int("length_each_update");
        Substeps = data.Int("substeps");
        Reach = data.Int("reach");
        Damage = data.Int("damage");
        FiringBeams = data.Text("firing_beams");
        _swing = GrappleSwingRules.Read(data.Get("swing"));
    }

    /// <summary>Fires the line the way <paramref name="samus"/> aims, if it is not already out. It begins to reach out on the next update.</summary>
    public void Fire(Samus samus)
    {
        if (IsOut)
            return;
        _samus = samus;
        (_across, _down) = samus.AimDirection;
        _fromHer = samus.GrapplePoint - samus.Position;
        _tip = samus.GrapplePoint;
        _reached = PixelOffset.Zero;
        _length = 0;
        _phase = Phase.Firing;
        _stepSpent = true;
        samus.Room.Session.Sounds.Request(Sounds.GrappleFire, channel: "weapons", priority: SoundPriority.WhenIdle);

        // Its firing, like a beam shot's, takes back the asking again of a charge's sound.
        samus.ForgetChargingSoundResume();
        samus.Colours.ShowBeams(samus.Colours.BeamsOf(FiringBeams));
    }

    /// <summary>Lets go of whatever the line holds and draws it in. Its end is heard whenever it was out, latched or not.</summary>
    public void Release()
    {
        if (IsOut && _samus is not null)
        {
            _samus.Room.Session.Sounds.Request(Sounds.GrappleRelease, channel: "weapons", priority: SoundPriority.Essential);
            _samus.ShowBeamColours();
        }

        _held?.Unlatched();
        _held = null;
        if (_heldBlock is not null && _samus is not null)
            _samus.Room.Terrain.LetGoOf(_heldBlock);
        _heldBlock = null;
        _phase = Phase.Inactive;
        QueueRedraw();
    }

    /// <summary>
    /// Plays one update of the line. Reaching out, it is counted on, looks at its end for what it can latch or
    /// hurt, and moves its end in substeps, asking the block there after each. Letting go of the attack, its reach,
    /// or a block that stops it at the last substep gives it up, and it ends on the update after.
    /// </summary>
    public void Step(Samus samus)
    {
        // The update a line is fired on only begins it, and one let go on this update ends on the next.
        if (_stepSpent)
        {
            _stepSpent = false;
            QueueRedraw();
            return;
        }

        switch (_phase)
        {
            case Phase.Inactive:
                return;
            case Phase.CancelPending:
                Release();
                return;
            case Phase.Latched:
                // A block that has broken away holds nothing.
                if (_heldBlock is { Broken: true })
                    Release();
                else
                    QueueRedraw();
                return;
        }

        if (!samus.Input.IsHeld(GameAction.Attack) || samus.Selected != Equipment.GrappleBeam)
        {
            GiveUp();
            return;
        }

        _length += LengthEachUpdate;
        if (_length >= Reach)
        {
            GiveUp();
            return;
        }

        // What can be latched, or is hurt by the line, is looked for at its end as it stands, before the end moves.
        Room room = samus.Room;
        var hit = new Hit(Damage, HitSide.Player, new HitKind(HitKinds.Grapple)) { Source = samus };
        var around = PixelRect.Around(_tip.X.Whole, _tip.Y.Whole, 2, 2);
        ILatchable? latchable = room.Collision.Overlapping(around, ColliderRole.Hurtbox, except: samus)
            .Concat(room.Collision.Overlapping(around, ColliderRole.Solid, except: samus))
            .Select(collider => collider.CollisionOwner as ILatchable)
            .FirstOrDefault(found => found is { CanBeLatched: true });
        if (latchable is not null)
        {
            Latch(samus, latchable, null);
            return;
        }

        room.DeliverHit(hit, around, from: samus);

        // The end moves a quarter of the line's speed at a time, the part of a pixel kept from one to the next.
        bool diagonal = _across != 0 && _down != 0;
        Pixels each = Pixels.FromNumber((diagonal ? DiagonalExtendSpeed : ExtendSpeed) / Substeps);
        bool stopped = false;
        for (int substep = 0; substep < Substeps; substep++)
        {
            _reached += new PixelOffset(each * _across, each * _down);
            PixelPosition end = samus.Position + _reached;
            _tip = PixelPosition.FromWhole(end.X.Whole, end.Y.Whole) + _fromHer;
            var point = new PixelRect(_tip.X.Whole, _tip.Y.Whole, 1, 1);
            Block? block = room.Terrain.BlocksIn(point).FirstOrDefault(found => found.Type.Contact is BlockContact.Latch or BlockContact.LatchOnce);
            if (block is not null)
            {
                _tip = PixelPosition.FromWhole(block.Rect.Left + block.Rect.Width / 2, block.Rect.Top + block.Rect.Height / 2);
                Latch(samus, null, block);
                return;
            }

            // A block the line's touch sets off is set off, and stops the line if it was solid as the line met it.
            stopped = room.Terrain.IsSolidIn(point, SlopeTreatment.Wall);
            room.Terrain.TakeHit(hit, point);
        }

        // Only the block at the last substep's end gives the line up.
        if (stopped)
            GiveUp();
        QueueRedraw();
    }

    /// <summary>
    /// Lets go as her swing ends, by her letting go or by a drop: the line stays as it is for this update, still
    /// holding what it held, and ends on the next, when its end is heard and her beams' colours are her own again.
    /// </summary>
    public void EndOnNextUpdate()
    {
        if (!IsOut)
            return;
        _tip = Anchor;
        (_phase, _stepSpent) = (Phase.CancelPending, true);
        QueueRedraw();
    }

    /// <summary>Gives up a line that is reaching out: it stays as it is for this update and ends on the next.</summary>
    private void GiveUp()
    {
        _phase = Phase.CancelPending;
        QueueRedraw();
    }

    /// <summary>Takes hold of something and hands Samus to her swing.</summary>
    private void Latch(Samus samus, ILatchable? held, Block? block)
    {
        _held = held;
        _heldBlock = block;
        _phase = Phase.Latched;
        held?.Latched();
        if (block is not null)
            samus.Room.Terrain.LatchedOnto(block);
        samus.Room.Session.Sounds.Request(Sounds.GrappleLatch, channel: "weapons");
        // Seized, she holds the line but hangs where she is held. Otherwise the way the line was fired and what she
        // was doing say whether she swings from it or is held in place: moving up or down she always swings; else
        // she swings only from a line fired upward, and one fired level or downward holds her as she stands or
        // crouches.
        if (samus.State is GrabbedState)
        {
            QueueRedraw();
            return;
        }

        bool swings = !samus.Velocity.Y.IsZero || _down < 0;
        samus.ChangeState(swings ? new GrapplingState() : new GrappleLockedState(samus.State is CrouchingState));
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
