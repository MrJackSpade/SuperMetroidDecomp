using Engine.Shared;

namespace Game.Enemies;

/// <summary>
/// Ridley's tail: a chain of links, each swinging about the one before it. A swing starts at the base and passes
/// down the chain, each link setting off a little after the last, so the tail curls like a whip. Left to itself it
/// sways between its limits; given a target it lashes out to it; for the pogo it points straight down and stabs.
/// Angles are in 256ths of a turn from straight down, growing toward the right.
/// </summary>
public sealed class RidleyTail
{
    /// <summary>One link of the tail.</summary>
    public sealed class Link
    {
        internal Link(Pixels rest, Pixels longest, int angle, Link? previous)
        {
            (Rest, Longest, Angle, Previous, Distance) = (rest, longest, angle, previous, rest);
            if (previous is not null)
                previous.Next = this;
        }

        /// <summary>How far it sits from the link before it at rest, and at its farthest.</summary>
        public Pixels Rest { get; }

        /// <summary>How far it sits from the link before it at its farthest.</summary>
        public Pixels Longest { get; }

        /// <summary>The link before it, and after it.</summary>
        public Link? Previous { get; }

        /// <summary>The link after it.</summary>
        public Link? Next { get; private set; }

        /// <summary>Its angle from straight down.</summary>
        public int Angle { get; set; }

        /// <summary>How far it sits from the link before it now.</summary>
        public Pixels Distance { get; set; }

        /// <summary>A distance it is stretching out to; none when it is not stretching.</summary>
        public Pixels? TargetDistance { get; set; }

        /// <summary>Where it is, in the room.</summary>
        public PixelPosition Position { get; set; }

        /// <summary>Where it sits from the link before it.</summary>
        public PixelOffset Offset { get; private set; }

        /// <summary>Whether it is in motion: waiting its turn to swing, or swinging.</summary>
        public bool Active => State != Swinging.Resting;

        internal Swinging State { get; set; }

        internal int Stagger { get; set; } = 17;

        /// <summary>The way it swings: 1 toward the right, -1 toward the left.</summary>
        internal int Direction { get; set; } = -1;

        /// <summary>The way it points: its own angle added to that of the link before it.</summary>
        public int Pointing => Turns.Wrap(Previous is null ? Angle : Angle + Previous.Angle);

        internal void Activate()
        {
            if (State == Swinging.Resting)
                State = Swinging.Staggering;
        }

        internal void Place()
        {
            Pixels reach = Pixels.FromWhole(Distance.Whole);
            Offset = PixelOffset.FromWhole(Turns.Sine(Pointing, reach).Whole, Turns.Cosine(Pointing, reach).Whole);
        }
    }

    internal enum Swinging
    {
        Resting,
        Staggering,
        Swinging,
    }

    /// <summary>What the tail is doing.</summary>
    public enum Doing
    {
        /// <summary>Nothing: it hangs as it is.</summary>
        None,

        /// <summary>Swaying, and lashing out when it has cause.</summary>
        Neutral,

        /// <summary>Swinging round to point down, for the pogo.</summary>
        PogoSetup,

        /// <summary>Pointing down.</summary>
        PointingDown,

        /// <summary>Bouncing on its tip.</summary>
        Bouncing,

        /// <summary>Drawn up, ready to stab down.</summary>
        ReadyingStab,

        /// <summary>Stabbing down.</summary>
        Stabbing,
    }

    /// <summary>The swing each update of a slow sway, a fast sway, a lash and a stab.</summary>
    public const int SlowSwing = 1, FastSwing = 2, WhipSwing = 8, StabSwing = 3;

    private readonly List<Link> _links = [];

    /// <summary>A tail of the usual seven links.</summary>
    public RidleyTail()
    {
        Link previous = Add(Pixels.FromWhole(2), Pixels.FromWhole(2), 0, null);
        (int Longest, int Angle)[] shaft = [(24, 16), (24, 32), (22, 48), (22, 64), (18, 80)];
        foreach ((int longest, int angle) in shaft)
            previous = Add(Pixels.FromWhole(8), Pixels.FromWhole(longest), angle, previous);
        Add(Pixels.FromWhole(5), Pixels.FromWhole(5), 96, previous);
    }

    /// <summary>The links, from the base to the tip.</summary>
    public IReadOnlyList<Link> Links => _links;

    /// <summary>The link at its base.</summary>
    public Link Base => _links[0];

    /// <summary>The link at its tip.</summary>
    public Link Tip => _links[^1];

    /// <summary>The last link before the tip.</summary>
    public Link InnerJoint => _links[^2];

    /// <summary>What it is doing.</summary>
    public Doing Action { get; set; }

    /// <summary>How far each link swings in an update.</summary>
    public int SwingStep { get; set; } = SlowSwing;

    /// <summary>How far a link swings before the next sets off: less makes a curlier tail.</summary>
    public int LinkSpacing { get; set; } = 16;

    /// <summary>How fast a link stretches out, in pixels an update.</summary>
    public Pixels ExtensionSpeed { get; set; } = Pixels.FromNumber(0.9375);

    /// <summary>Whether it lashes out of its own accord when swaying.</summary>
    public bool WhipsWhenIdle { get; set; }

    /// <summary>Whether it has been told to lash out when next it can.</summary>
    public bool WhipOrdered { get; set; }

    /// <summary>The angle a swing to the left is lashing out to; none when it is not.</summary>
    public int? LeftwardTarget { get; set; }

    /// <summary>The angle a swing to the right is lashing out to.</summary>
    public int? RightwardTarget { get; set; }

    /// <summary>How far left it may swing, and how far right.</summary>
    public (int Leftward, int Rightward) Limits { get; set; } = (-16, 64);

    /// <summary>Whether none of its links is in motion.</summary>
    public bool IsAtRest => _links.All(link => !link.Active);

    /// <summary>Whether all of its links are in motion.</summary>
    public bool IsAllMoving => _links.All(link => link.Active);

    /// <summary>Sets every link in motion at once, as when Ridley takes off.</summary>
    public void ActivateAll()
    {
        foreach (Link link in _links)
            link.Activate();
    }

    /// <summary>Sets its limits for the way Ridley faces; turning counts as facing left.</summary>
    public void SetLimits(bool facingRight) => Limits = facingRight ? (-64, 16) : (-16, 64);

    /// <summary>Turns the tail over as Ridley turns about.</summary>
    public void Mirror()
    {
        foreach (Link link in _links)
        {
            link.Distance = link.Rest;
            link.Angle = -link.Angle;
            link.Direction = -1;
        }
    }

    /// <summary>Swings every link for one update.</summary>
    public void SwingLinks()
    {
        foreach (Link link in _links)
            Swing(link);
    }

    /// <summary>Chains the links together from where the base is, and stretches and draws in those that are stretching.</summary>
    public void Place(PixelPosition root, bool turning, int bodyX)
    {
        Base.Position = turning ? new PixelPosition(Pixels.FromWhole(bodyX), (root + Base.Offset).Y) : root + Base.Offset;
        for (Link previous = Base; previous.Next is { } link; previous = link)
        {
            PixelPosition chained = previous.Position + link.Offset;
            link.Position = turning ? new PixelPosition(Pixels.FromWhole(bodyX), chained.Y) : chained;
        }

        foreach (Link link in _links.Skip(1))
        {
            if (link.TargetDistance is { } target)
            {
                if (target < link.Distance)
                    link.TargetDistance = null;
                Pixels longer = link.Distance + ExtensionSpeed;
                link.Distance = longer > link.Longest ? link.Longest : longer;
            }
            else if (link.Distance > link.Rest)
            {
                link.Distance -= Pixels.FromNumber(0.5);
            }
        }
    }

    /// <summary>
    /// Aims a lash at a place. The angle is that of the place from the tail's base, in 256ths of a turn from straight
    /// down toward the right; it is kept out of Ridley's own body. A lash that would not carry the tail the way Ridley
    /// faces is not made.
    /// </summary>
    public void AimWhip(int towardTarget, bool facingRight, int turnsBeyond)
    {
        bool throughBody = towardTarget is >= 24 and < 232;
        if (!facingRight)
        {
            int target = -Turns.PerTurn - turnsBeyond * Turns.PerTurn + (throughBody ? 232 : towardTarget);
            if (target >= Base.Angle)
                return;
            LeftwardTarget = target;
        }
        else
        {
            int target = turnsBeyond * Turns.PerTurn + (throughBody ? 24 : towardTarget);
            if (target < Base.Angle)
                return;
            RightwardTarget = target;
        }

        SwingStep = WhipSwing;
    }

    private Link Add(Pixels rest, Pixels longest, int angle, Link? previous)
    {
        var link = new Link(rest, longest, angle, previous);
        _links.Add(link);
        return link;
    }

    private void Swing(Link link)
    {
        if (link.Active)
        {
            if (link.State != Swinging.Swinging)
            {
                // It waits its turn; when that comes the next link begins to wait for its own.
                if (link.Stagger < LinkSpacing)
                {
                    link.Stagger += SwingStep;
                    return;
                }

                link.State = Swinging.Swinging;
                if (link.Next is { } next)
                {
                    next.Activate();
                    next.Direction = link.Direction;
                }
            }

            int direction = link.Direction;
            int swung = link.Angle + direction * SwingStep;
            int limit = direction < 0 ? Limits.Leftward : Limits.Rightward;
            int? target = direction < 0 ? LeftwardTarget : RightwardTarget;
            if (target is { } aimed)
            {
                link.TargetDistance = Pixels.FromWhole(12);
                if (Reaches(swung, aimed, direction))
                {
                    if (link.Previous is not { Active: true })
                    {
                        // The lash ends link by link, each coming to rest once the one before it has.
                        link.Angle = aimed;
                        link.State = Swinging.Resting;
                        link.Stagger = 0;
                        link.Direction = -direction;
                        return;
                    }

                    link.Direction = -direction;
                    link.Angle = limit;
                }
                else
                {
                    link.Angle = swung;
                }
            }
            else if (Reaches(swung, limit, direction))
            {
                link.Direction = -direction;
                link.Angle = limit;
            }
            else
            {
                link.Angle = swung;
            }
        }

        link.Place();
    }

    private static bool Reaches(int angle, int bound, int direction) => direction < 0 ? angle <= bound : angle >= bound;
}
