using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Content;
using Engine.Core;
using Engine.Input;
using Engine.Shared;
using Engine.World;
using Game.Compatibility;
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
/// latched or is hurt by it is looked for at its end as it stands, and an enemy it neither holds nor destroys stops
/// it there; then its end moves in equal substeps, a quarter of
/// its speed each, and the block at the end is asked after each: one that holds the line takes it at once, but one
/// that only stops it gives the line up only if it is the last substep's, so a thin wall crossed within an update
/// does not stop it. A line given up while reaching out ends on the update after, when its end is heard. While it is
/// out her beams' colours are those of the beams its data names, and her own again as it ends.
/// <para>
/// It is shown by the cartridge's own pictures, in her colours: a chain of the line's pictures from where it leaves
/// her toward its end, one for every so many pixels of its counted length and at least one, each showing the
/// picture before its neighbour's in a pattern that steps on as one; and its end's picture at its end, stepping
/// through its own. Which set of the line's pictures shows is by the line's angle. Nothing of it shows on the
/// update it is given up or let go.
/// </para>
/// </remarks>
public partial class SamusGrapple : Node2D
{
    /// <summary>How far the line reaches out each update, as a number of pixels, fired straight across, up or down.</summary>
    [Export]
    public double ExtendSpeed { get; set; } = 11.953125;

    /// <summary>How far the line reaches out along each axis each update, fired diagonally.</summary>
    [Export]
    public double DiagonalExtendSpeed { get; set; } = 8.484375;

    /// <summary>How much its length is counted on each update it reaches out.</summary>
    [Export]
    public int LengthEachUpdate { get; set; } = 12;

    /// <summary>How many equal substeps its end moves in each update, the block at the end asked after each.</summary>
    [Export]
    public int Substeps { get; set; } = 4;

    /// <summary>The length at which a line that has met nothing is given up.</summary>
    [Export]
    public int Reach { get; set; } = 128;

    /// <summary>How far past an enemy's own size, each way, the line's end still reaches it, in pixels: the size of the line's end itself.</summary>
    private const int EnemyReach = 8;

    /// <summary>The damage the tip of the line does to what it touches.</summary>
    [Export]
    public int Damage { get; set; } = 1;

    /// <summary>The beams whose colours her beams' colours show while the line is out, as her colours name them.</summary>
    [Export]
    public string FiringBeams { get; set; } = "ice";

    /// <summary>How near the line's end a pickup must lie, across and down, for the line to take it up.</summary>
    [Export]
    public int PickupReach { get; set; } = 16;

    /// <summary>How many updates she is safe, and how many knocked back, once the line has touched what shocks it.</summary>
    [Export]
    public int ShockInvincibleUpdates { get; set; } = 96;

    /// <inheritdoc cref="ShockInvincibleUpdates"/>
    [Export]
    public int ShockKnockbackUpdates { get; set; } = 5;

    /// <summary>How many updates a pickup must have lain before the line can take it up.</summary>
    [Export]
    public int PickupDelay { get; set; } = 16;

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

    /// <summary>
    /// The line's pictures: how many pixels of its length each stands for, the updates each picture of its pattern
    /// shows and its first, the sets of its pictures with the set each angle step shows, its end's pictures and the
    /// updates each shows, and where a picture's corner lies from its middle.
    /// </summary>
    private sealed record LinePictures(
        int Spacing, int PictureUpdates, int FirstPictureUpdates, IReadOnlyList<IReadOnlyList<string>> Rope, IReadOnlyList<int> RopeOfAngle,
        IReadOnlyList<string> End, int EndUpdates, Vector2I Offset);

    private LinePictures? _line;
    private readonly List<ActorVisual> _links = [];
    private ActorVisual? _end;

    /// <summary>Where the line's pattern and its end's pictures stand, and the updates each has left.</summary>
    private int _ropePicture;
    private int _ropeLeft;
    private int _endPicture;
    private int _endLeft;

    /// <summary>The line's angle step and its length as whoever holds her by it last gave them, for its pictures.</summary>
    private int _lineAngle;
    private int _lineLength;

    /// <summary>Whether the line is out: reaching, given up and not yet ended, or latched.</summary>
    public bool IsOut => _phase != Phase.Inactive;

    /// <summary>Whether the line has hold of something.</summary>
    public bool IsLatched => _phase == Phase.Latched;

    /// <summary>Whether the flare where the line leaves her shows: while it reaches out and while it holds, and not on the update a line given up ends.</summary>
    public bool ShowsFlare => _phase is Phase.Firing or Phase.Latched;

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
        PickupReach = data.Int("pickup_reach");
        PickupDelay = data.Int("pickup_delay");
        ShockInvincibleUpdates = data.Int("shock_invincible_updates");
        ShockKnockbackUpdates = data.Int("shock_knockback_updates");
        Reach = data.Int("reach");
        Damage = data.Int("damage");
        FiringBeams = data.Text("firing_beams");
        _swing = GrappleSwingRules.Read(data.Get("swing"));
        DataNode line = data.Get("line");
        _line = new LinePictures(
            line.Int("spacing"), line.Int("picture_updates"), line.Int("first_picture_updates"),
            [.. line.List("rope").Select(set => (IReadOnlyList<string>)[.. set.Items().Select(picture => picture.AsText())])],
            [.. line.List("rope_of_angle").Select(set => set.AsInt())],
            [.. line.List("end").Select(picture => picture.AsText())], line.Int("end_updates"),
            new Vector2I(line.Get("offset").Int("x"), line.Get("offset").Int("y")));
    }

    /// <summary>
    /// The line's angle step, clockwise from straight up as her place lies from what holds it, and its length, as
    /// the state that holds her by it has them: its pictures go by these.
    /// </summary>
    public void ShowLineAs(int angleStep, int length) => (_lineAngle, _lineLength) = (Turns.Wrap(angleStep), length);

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

        // Its pictures start over: the pattern's first holds an update longer than the rest, as the first drawing
        // of a line only takes the pattern up. Its angle is that of her place from where it is headed.
        if (_line is { } line)
            (_ropePicture, _ropeLeft, _endPicture, _endLeft) = (0, line.FirstPictureUpdates, 0, line.EndUpdates);
        ShowLineAs(CartridgeAngles.AngleClockwiseFromUp(-_across, -_down), 0);
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
    }

    /// <summary>
    /// Plays one update of the line. Reaching out, it is counted on, looks at its end for what it can latch or
    /// hurt, and moves its end in substeps, asking the block there after each. Letting go of the attack, its reach,
    /// or a block that stops it at the last substep gives it up, and it ends on the update after.
    /// </summary>
    public void Step(Samus samus)
    {
        StepLine(samus);
        TakePickups(samus);
        ShowLine(samus);
    }

    /// <summary>
    /// While the line is out its end takes up every pickup that lies within its reach of it, across and down, once
    /// the pickup has lain its delay. A pickup is judged as its own turn of this update will find it: a turn older,
    /// and not at all on the update its time ends.
    /// </summary>
    private void TakePickups(Samus samus)
    {
        if (!IsOut)
            return;
        PixelPosition end = Anchor;
        foreach (Pickup pickup in samus.Room.Pickups.ToList())
        {
            int left = pickup.UpdatesLeft - 1;
            if (left <= 0 || left >= pickup.Type.LastsUpdates - PickupDelay)
                continue;
            if (Math.Abs(pickup.Position.X.Whole - end.X.Whole) < PickupReach && Math.Abs(pickup.Position.Y.Whole - end.Y.Whole) < PickupReach)
                pickup.Collect();
        }
    }

    /// <summary>Plays the line's own update.</summary>
    private void StepLine(Samus samus)
    {
        // The update a line is fired on only begins it, and one let go on this update ends on the next.
        if (_stepSpent)
        {
            _stepSpent = false;
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

        _lineLength = _length;

        // What can be latched, or is hurt by the line, is looked for at its end as it stands, before the end moves.
        Room room = samus.Room;
        var hit = new Hit(Damage, HitSide.Player, new HitKind(HitKinds.Grapple)) { Source = samus };
        var around = PixelRect.Around(_tip.X.Whole, _tip.Y.Whole, 2, 2);

        // An enemy is met by its own size and so far more each way, whatever its pose gives of it to be hit, as the
        // cartridge measures the line's end against it: the first so reached in the room's order, one no hit can
        // land on for the moment passed over. What is no enemy is met where the line's end lies.
        var reach = PixelRect.Around(_tip.X.Whole, _tip.Y.Whole, EnemyReach, EnemyReach);
        Enemy? reached = room.Enemies.FirstOrDefault(enemy => !enemy.IsDead && !enemy.IsSafeFromHits && enemy.OwnRect is { } own && own.Overlaps(reach));
        ILatchable? latchable = reached is ILatchable { CanBeLatched: true } enemyHeld
            ? enemyHeld
            : room.Collision.Overlapping(around, ColliderRole.Hurtbox, except: samus)
                .Concat(room.Collision.Overlapping(around, ColliderRole.Solid, except: samus))
                .Where(collider => collider.CollisionOwner is not Enemy)
                .Select(collider => collider.CollisionOwner as ILatchable)
                .FirstOrDefault(found => found is { CanBeLatched: true });
        if (latchable is not null)
        {
            Latch(samus, latchable, null);
            return;
        }

        // An enemy the line neither holds nor destroys stops it there: the line is given up before its end moves.
        // One it destroys, and anything that takes no notice of it, lets it go on.
        // The hit lands on the enemy reached, on whatever of it is there to be hit, and on whatever else lies at the
        // line's end that is no enemy.
        var met = new List<(IHittable Target, HitResult Result)>(room.DeliverHit(hit, around, from: samus, landsOn: target => target is not Enemy));
        if (reached?.OwnRect is { } size)
            met.AddRange(room.DeliverHit(hit, size, from: samus, landsOn: target => ReferenceEquals(target, reached)));

        // What shocks the line hurts her through it, safe from her last hurt or not: its damage, which her suit
        // divides; her safety for the line's own count; and a knockback away from the way she faces, whichever side
        // of her it is on. The line is given up.
        if (met.Select(struck => struck.Target).OfType<IShocksGrapple>().FirstOrDefault() is { } shocking)
        {
            samus.Sap(new Hit(shocking.GrappleShock, HitSide.Enemies, HitKind.Contact));
            samus.BeInvincibleFor(ShockInvincibleUpdates);
            samus.KnockBack(samus.Facing, ShockKnockbackUpdates);
            GiveUp();
            return;
        }

        if (met.Any(struck => struck.Target is Enemy && struck.Result is HitResult.Absorbed or HitResult.Dud or HitResult.TurnedBack))
        {
            GiveUp();
            return;
        }

        // A solid enemy with nothing of it there for the hit to land on stops the line too: the cartridge's line
        // meets an enemy by its size, whatever it is made of, and none that is solid lets it pass.
        if (reached is not null && !met.Any(struck => ReferenceEquals(struck.Target, reached))
            && EntityNode.CollidersOf(reached).Any(solid => solid.Enabled && solid.Role == ColliderRole.Solid))
        {
            GiveUp();
            return;
        }

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
    }

    /// <summary>Gives up a line that is reaching out: it stays as it is for this update and ends on the next.</summary>
    private void GiveUp()
    {
        _phase = Phase.CancelPending;
    }

    /// <summary>Takes hold of something and hands Samus to her swing.</summary>
    private void Latch(Samus samus, ILatchable? held, Block? block)
    {
        _held = held;
        _heldBlock = block;
        _phase = Phase.Latched;
        held?.Latched(_tip);
        if (block is not null)
            samus.Room.Terrain.LatchedOnto(block);
        samus.Room.Session.Sounds.Request(Sounds.GrappleLatch, channel: "weapons");
        // Seized, she holds the line but hangs where she is held. Otherwise the way the line was fired and what she
        // was doing say whether she swings from it or is held in place: moving up or down she always swings; else
        // she swings only from a line fired upward, and one fired level or downward holds her as she stands or
        // crouches.
        if (samus.State is GrabbedState)
        {
            return;
        }

        bool swings = !samus.Velocity.Y.IsZero || _down < 0;
        samus.ChangeState(swings ? new GrapplingState() : new GrappleLockedState(samus.State is CrouchingState));
    }

    /// <summary>
    /// Shows the line as it stands once its update is played. Reaching out or holding, its end's pictures step on;
    /// and with any length the line's pattern steps on and its pictures are laid from where it leaves her toward its
    /// end, a spacing apart along the way from the one to the other, the part of a pixel kept from one to the next.
    /// Given up or let go, nothing of it shows.
    /// </summary>
    private void ShowLine(Samus samus)
    {
        if (_line is not { } line)
            return;
        int links = 0;
        bool shown = _phase is Phase.Firing or Phase.Latched;
        if (shown)
        {
            if (--_endLeft == 0)
                (_endPicture, _endLeft) = ((_endPicture + 1) % line.End.Count, line.EndUpdates);
            shown = _lineLength > 0;
        }

        if (shown)
        {
            IReadOnlyList<string> pictures = line.Rope[line.RopeOfAngle[_lineAngle]];
            if (--_ropeLeft == 0)
                (_ropePicture, _ropeLeft) = ((_ropePicture + 1) % pictures.Count, line.PictureUpdates);
            // The line is laid from where its flare lies: swinging, the point her body hangs from; held in place and
            // reaching out, the place her data gives the flare at her arm.
            PixelPosition from = samus.GrappleFlarePoint;
            PixelPosition end = IsLatched ? Anchor : _tip;
            int toward = CartridgeAngles.AngleClockwiseFromUp(end.X.Whole - from.X.Whole, end.Y.Whole - from.Y.Whole);
            long across = (long)CartridgeAngles.SignedSine(toward) * line.Spacing * 256;
            long down = (long)CartridgeAngles.SignedSine(toward - Turns.PerTurn / 4) * line.Spacing * 256;
            long x = (long)from.X.Whole * Pixels.FractionsPerPixel, y = (long)from.Y.Whole * Pixels.FractionsPerPixel;
            links = Math.Max(1, _lineLength / line.Spacing);
            for (int link = 0; link < links; link++)
            {
                // Each link beyond the one by her shows the picture before its neighbour's.
                int picture = ((_ropePicture - link) % pictures.Count + pictures.Count) % pictures.Count;
                Place(samus, LinkAt(samus, link, line), pictures[picture], (int)Math.Floor((double)x / Pixels.FractionsPerPixel), (int)Math.Floor((double)y / Pixels.FractionsPerPixel));
                x += across;
                y += down;
            }

            Place(samus, _end ??= NewPicture(samus, "End", line), line.End[_endPicture], end.X.Whole, end.Y.Whole);
        }
        else
        {
            _end?.Show("");
        }

        for (int link = links; link < _links.Count; link++)
            _links[link].Show("");
    }

    /// <summary>The picture of the line's link of a number, made the first time it is wanted.</summary>
    private ActorVisual LinkAt(Samus samus, int link, LinePictures line)
    {
        while (_links.Count <= link)
            _links.Add(NewPicture(samus, $"Link{_links.Count + 1}", line));
        return _links[link];
    }

    private ActorVisual NewPicture(Samus samus, string name, LinePictures line)
    {
        var picture = new ActorVisual { Name = name, PictureOffset = line.Offset };
        AddChild(picture);
        picture.Bind(samus.Room.Content);
        return picture;
    }

    /// <summary>Shows a picture of the line with its middle at a place in the room, in her colours.</summary>
    private static void Place(Samus samus, ActorVisual picture, string path, int x, int y)
    {
        picture.Show(path);
        picture.GlobalPosition = samus.GlobalPosition + new Vector2(x - samus.Position.X.Whole, y - samus.Position.Y.Whole);
        samus.Colours.ShowOn(picture);
    }
}
