using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A crawler: it follows the surface it is on, round every corner, and does nothing else. The Zoomers, the Zeela, the
/// Sova, the Sciser, the Zero and the Viola are all this scene with their own data files. One that pursues turns
/// toward Samus when it is free to; a hard enough shake of the room makes any of them let go and fall.
/// </summary>
/// <remarks>Its timelines are named for the surface it is on: "floor", "ceiling", "left_wall" and "right_wall".</remarks>
public partial class Crawler : Enemy
{
    /// <summary>The side the surface it starts on is on.</summary>
    [Export]
    public SurfaceCrawl.Clings StartsOn { get; set; } = SurfaceCrawl.Clings.Below;

    /// <summary>
    /// Whether it starts pressing left, not right: the way it crawls along a floor or ceiling, or the wall it presses
    /// into.
    /// </summary>
    [Export]
    public bool PressesLeft { get; set; }

    /// <summary>
    /// Whether it starts pressing up, not down: the way it crawls along a wall, or the ceiling it presses into. One
    /// placed pressing away from its surface turns about until it finds something to cling to, as on the cartridge.
    /// </summary>
    [Export]
    public bool PressesUp { get; set; }

    /// <summary>How far it crawls each update, as a number of pixels; zero to stay put until it has fallen and landed once.</summary>
    [Export]
    public double CrawlSpeed { get; set; } = 0.5;

    /// <summary>The speed it takes on landing from a fall when it was placed with none.</summary>
    [Export, DataValue("landing_speed")]
    public double LandingSpeed { get; set; } = 0.5;

    /// <summary>Whether it turns toward Samus when it is free to go either way along its surface.</summary>
    [Export, DataValue("pursues")]
    public bool Pursues { get; set; }

    /// <summary>
    /// The shake that makes it let go of its surface, as its data gives it ("falls_with_shake": a super missile
    /// striking a wall); none where its data gives none. Only that shake does, and only as it begins.
    /// </summary>
    private RoomShake? _fallsWith;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _fallsWith = data.Has("falls_with_shake") ? RoomShake.Read(data.Get("falls_with_shake")) : null;
    }

    private readonly SurfaceCrawl _crawl = new();

    private bool _settled;
    private bool _fallsForGood;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _settled = false;
        _fallsForGood = false;
        _crawl.Speed = Pixels.FromNumber(CrawlSpeed);
        _crawl.SpeedOnLanding = CrawlSpeed == 0 ? Pixels.FromNumber(LandingSpeed) : null;
        _crawl.Start(StartsOn, PressesLeft, PressesUp);
        ShowSurface();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // Its first update only settles it on its surface: it moves from the next.
        if (!_settled)
        {
            _settled = true;
            return;
        }

        bool wasFalling = _crawl.IsFalling;
        MoveReport report = _crawl.Step(this, Pursues ? Target : null);

        // The shake it answers lets go of a crawler on its surface as it begins, as the cartridge sets it falling
        // before it crawls that update: it still crawls, and falls from the next. A corner turned in that move takes it
        // onto its new surface instead. Losing its surface too often in that move has it fall for good: on the
        // cartridge falling itself is then what it goes back to on landing, so it lets go again each time it lands.
        if (!wasFalling && _fallsWith is { } shake && Room.HasJustBegunShaking(shake))
        {
            if (_crawl.IsFalling)
                _fallsForGood = true;
            else if (!report.ReachedEdge)
                _crawl.LetGo();
        }

        if (_fallsForGood && report.Finished)
            _crawl.LetGo();
        if (report.ReachedEdge || report.Finished)
            ShowSurface();
    }

    /// <summary>Shows the timeline of the surface it is on.</summary>
    private void ShowSurface() => Play(_crawl.Side switch
    {
        SurfaceCrawl.Clings.Below => "floor",
        SurfaceCrawl.Clings.Above => "ceiling",
        SurfaceCrawl.Clings.Left => "left_wall",
        SurfaceCrawl.Clings.Right => "right_wall",
        _ => throw new InvalidOperationException($"Undefined side {_crawl.Side}."),
    });
}
