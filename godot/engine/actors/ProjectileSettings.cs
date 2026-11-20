using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// How a projectile behaves, as settings: what it does on meeting terrain and a target, what the other side's shots
/// do to it, how it moves, how long it lasts and what it leaves. A projectile needs code of its own only for
/// behaviour no setting gives it.
/// </summary>
/// <remarks>
/// <code>
/// { "width": 8, "height": 8, "meeting_terrain": "ends", "meeting_target": "ends", "met_by_shot": "ignored",
///   "motion": "straight", "lasts_updates": 0, "view_margin": 64, "picture": "shots/beam.png" }
/// </code>
/// </remarks>
public sealed record ProjectileSettings
{
    /// <summary>What a projectile does on meeting terrain or a solid.</summary>
    public enum Terrain
    {
        /// <summary>It ends.</summary>
        Ends,

        /// <summary>It ends and delivers its hit to the terrain it met.</summary>
        EndsAndHits,

        /// <summary>It passes through.</summary>
        PassesThrough,

        /// <summary>It passes through and still delivers its hit to the terrain it crosses.</summary>
        PassesThroughAndHits,

        /// <summary>It bounces off.</summary>
        Bounces,
    }

    /// <summary>What a projectile does on hurting a target.</summary>
    public enum Target
    {
        /// <summary>It ends.</summary>
        Ends,

        /// <summary>It passes through and can hurt others, but not the same one again.</summary>
        PassesThrough,

        /// <summary>It passes through and can hurt the same one again once that one can be hit again.</summary>
        PassesThroughAndRehits,
    }

    /// <summary>What happens to a projectile met by the other side's shot.</summary>
    public enum MetBy
    {
        /// <summary>Nothing: the shot passes it.</summary>
        Ignored,

        /// <summary>It is destroyed.</summary>
        Destroyed,

        /// <summary>It stops the shot and is unharmed.</summary>
        StopsShot,
    }

    /// <summary>How a projectile moves.</summary>
    public enum Moves
    {
        /// <summary>In a straight line at its launch speed.</summary>
        Straight,

        /// <summary>In a straight line, gaining speed each update.</summary>
        Accelerating,

        /// <summary>Along its line while swinging from side to side.</summary>
        Wave,

        /// <summary>Steering toward its target.</summary>
        Homing,

        /// <summary>Not at all.</summary>
        Still,

        /// <summary>Under gravity.</summary>
        Falling,
    }

    /// <summary>Its width, in pixels.</summary>
    public int Width { get; init; } = 8;

    /// <summary>Its height, in pixels.</summary>
    public int Height { get; init; } = 8;

    /// <summary>What it does on meeting terrain or a solid.</summary>
    public Terrain MeetingTerrain { get; init; }

    /// <summary>What it does on hurting a target.</summary>
    public Target MeetingTarget { get; init; }

    /// <summary>What the other side's shots do to it.</summary>
    public MetBy MetByShot { get; init; }

    /// <summary>How it moves.</summary>
    public Moves Motion { get; init; }

    /// <summary>For one that accelerates, falls or homes: the speed it gains each update.</summary>
    public Pixels Acceleration { get; init; }

    /// <summary>For one that accelerates, falls or homes: the most speed it reaches along either axis; zero for no limit.</summary>
    public Pixels SpeedLimit { get; init; }

    /// <summary>For one that moves in a wave: how far it swings to each side.</summary>
    public Pixels WaveSize { get; init; }

    /// <summary>For one that moves in a wave: how many angle steps, of 256 to a full swing, it advances each update.</summary>
    public int WaveStep { get; init; } = 16;

    /// <summary>How many updates it lasts; zero for no limit.</summary>
    public int LastsUpdates { get; init; }

    /// <summary>How far outside the view, in pixels, it may travel before it is gone; negative for no limit.</summary>
    public int ViewMargin { get; init; } = 64;

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; init; } = "";

    /// <summary>The data file of the timeline of the effect it leaves on ending; empty for none.</summary>
    public string EndingEffect { get; init; } = "";

    /// <summary>Reads settings from a data file's group. Anything not given keeps its plain value.</summary>
    public static ProjectileSettings Read(DataNode data)
    {
        var plain = new ProjectileSettings();
        var settings = new ProjectileSettings
        {
            Width = data.Has("width") ? data.Int("width") : plain.Width,
            Height = data.Has("height") ? data.Int("height") : plain.Height,
            MeetingTerrain = data.Has("meeting_terrain") ? data.Get("meeting_terrain").AsEnum<Terrain>() : plain.MeetingTerrain,
            MeetingTarget = data.Has("meeting_target") ? data.Get("meeting_target").AsEnum<Target>() : plain.MeetingTarget,
            MetByShot = data.Has("met_by_shot") ? data.Get("met_by_shot").AsEnum<MetBy>() : plain.MetByShot,
            Motion = data.Has("motion") ? data.Get("motion").AsEnum<Moves>() : plain.Motion,
            Acceleration = data.Has("acceleration") ? data.Distance("acceleration") : Pixels.Zero,
            SpeedLimit = data.Has("speed_limit") ? data.Distance("speed_limit") : Pixels.Zero,
            WaveSize = data.Has("wave_size") ? data.Distance("wave_size") : Pixels.Zero,
            WaveStep = data.Has("wave_step") ? data.Int("wave_step") : plain.WaveStep,
            LastsUpdates = data.Has("lasts_updates") ? data.Int("lasts_updates") : 0,
            ViewMargin = data.Has("view_margin") ? data.Int("view_margin") : plain.ViewMargin,
            Picture = data.Has("picture") ? data.Text("picture") : "",
            EndingEffect = data.Has("ending_effect") ? data.Text("ending_effect") : "",
        };
        if (settings.Width <= 0 || settings.Height <= 0 || settings.LastsUpdates < 0)
            throw data.Problem("gives a size that is not positive or a negative lifetime");
        return settings;
    }
}
