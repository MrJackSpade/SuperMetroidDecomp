using Engine.Actors;
using Engine.Actors.Movement;
using Engine.Content;
using Engine.Shared;
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

    /// <summary>Whether it goes round clockwise: rightward along a floor, down a wall on its left.</summary>
    [Export]
    public bool Clockwise { get; set; } = true;

    /// <summary>How far it crawls each update, as a number of pixels; zero to stay put until it has fallen and landed once.</summary>
    [Export, DataValue("crawl_speed")]
    public double CrawlSpeed { get; set; } = 0.5;

    /// <summary>The speed it takes on landing from a fall when it was placed with none.</summary>
    [Export, DataValue("landing_speed")]
    public double LandingSpeed { get; set; } = 0.5;

    /// <summary>Whether it turns toward Samus when it is free to go either way along its surface.</summary>
    [Export, DataValue("pursues")]
    public bool Pursues { get; set; }

    /// <summary>The strength of shake at and above which it lets go of its surface.</summary>
    [Export, DataValue("falls_at_shake")]
    public int FallsAtShake { get; set; } = 2;

    private readonly SurfaceCrawl _crawl = new();

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _crawl.Speed = Pixels.FromNumber(CrawlSpeed);
        _crawl.SpeedOnLanding = CrawlSpeed == 0 ? Pixels.FromNumber(LandingSpeed) : null;
        _crawl.Start(StartsOn, Clockwise);
        ShowSurface();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        if (Room.ShakeStrength >= FallsAtShake)
            _crawl.LetGo();
        MoveReport report = _crawl.Step(this, Pursues ? Target : null);
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
