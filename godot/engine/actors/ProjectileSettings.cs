using Engine.Content;
using Engine.Shared;
using Engine.World;

namespace Engine.Actors;

/// <summary>
/// How a projectile behaves, as settings: what it does on meeting terrain and a target, what the other side's shots
/// do to it, how it moves, how long it lasts and what it leaves. A projectile needs code of its own only for
/// behaviour no setting gives it.
/// </summary>
/// <remarks>
/// <code>
/// { "width": 8, "height": 8, "meeting_terrain": "ends", "terrain_by": "body", "ends_at_front": false, "meeting_target": "ends", "met_by_shot": "ignored",
///   "motion": "straight", "lasts_updates": 0, "view_margin": 64, "view_margin_below": 96, "picture": "shots/beam.png",
///   "terrain_shake": { "strength": 1, "updates": 30, "way": "both" } }
/// </code>
/// A step of its timeline may give the cue "size:&lt;width&gt;:&lt;height&gt;", which makes it that size from then on.
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

    /// <summary>What of a projectile meets terrain.</summary>
    public enum TerrainBy
    {
        /// <summary>
        /// All it covers, tried at every pixel of its way: anything solid in it stops it, and so does the room's edge.
        /// </summary>
        Body,

        /// <summary>
        /// Its middle point alone, tried at every pixel of its way: its width and height are then only what targets
        /// and solids meet. Outside the room nothing stops it, and a way out of the room does.
        /// </summary>
        Middle,

        /// <summary>
        /// The edge it leads with, tried once for each way it moves, where the move ended: it moves the whole of its
        /// speed across and its leading side is tried, then, if that did not stop it, the whole of its speed down and
        /// that side is tried. It is stopped only when every tile the edge lies in stops it
        /// (<see cref="Engine.World.Terrain.MeetShotEdge"/>), its hit lands on every tile the edge reached whether it is
        /// stopped or not, and it is tried once more where it was launched, before it first moves: going right and
        /// down whichever way it will go, but for one launched straight left. Outside the room nothing stops it, and a
        /// way out of the room does. It cannot bounce.
        /// </summary>
        FrontEdge,
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

        /// <summary>In a straight line, gaining speed each update, once it has moved that update.</summary>
        Accelerating,

        /// <summary>Along its line while swinging from side to side.</summary>
        Wave,

        /// <summary>Steering toward its target.</summary>
        Homing,

        /// <summary>Not at all.</summary>
        Still,

        /// <summary>Under gravity, which changes its speed each update once it has moved that update.</summary>
        Falling,
    }

    /// <summary>Its width, in pixels.</summary>
    public int Width { get; init; } = 8;

    /// <summary>Its height, in pixels.</summary>
    public int Height { get; init; } = 8;

    /// <summary>What it does on meeting terrain or a solid.</summary>
    public Terrain MeetingTerrain { get; init; }

    /// <summary>What of it meets terrain, and lands its hit on the terrain there.</summary>
    public TerrainBy MeetsTerrainBy { get; init; }

    /// <summary>
    /// Whether what it leaves on ending is left at its front and not at its middle: half its width and half its
    /// height on from its middle along each way it was launched, as a shot that bursts against what it struck does.
    /// </summary>
    public bool EndsAtFront { get; init; }

    /// <summary>What it does on hurting a target.</summary>
    public Target MeetingTarget { get; init; }

    /// <summary>What the other side's shots do to it.</summary>
    public MetBy MetByShot { get; init; }

    /// <summary>How it moves.</summary>
    public Moves Motion { get; init; }

    /// <summary>For one that accelerates, falls or homes: the speed it gains each update.</summary>
    public Pixels Acceleration { get; init; }

    /// <summary>
    /// For one that accelerates or homes: the most speed it reaches along either axis. For one that falls: the fastest
    /// it falls, which does not slow a throw upward. Zero for no limit.
    /// </summary>
    public Pixels SpeedLimit { get; init; }

    /// <summary>
    /// For one that accelerates, launched diagonally: the speed it gains each update along each axis, where that is
    /// not <see cref="Acceleration"/>; null for the same.
    /// </summary>
    public Pixels? DiagonalAcceleration { get; init; }
    /// <summary>The shake of the room it starts as terrain or something solid ends it; null for none.</summary>
    public RoomShake? TerrainShake { get; init; }
    /// <summary>The shake of the room it starts the first time its hit lands on anything; null for none.</summary>
    public RoomShake? TargetShake { get; init; }
    /// <summary>For one that moves in a wave: how far it swings to each side.</summary>
    public Pixels WaveSize { get; init; }

    /// <summary>For one that moves in a wave: how many angle steps, of 256 to a full swing, it advances each update.</summary>
    public int WaveStep { get; init; } = 16;

    /// <summary>For one that moves in a wave: the angle step its swing starts from; half a swing on mirrors it.</summary>
    public int WaveStart { get; init; }

    /// <summary>Whether it delivers its hit to what it meets. One that does not only carries its ending, as a bomb does.</summary>
    public bool DeliversHit { get; init; } = true;

    /// <summary>
    /// Whether its hit lands on everything it overlaps in every update, as a blast does, instead of once on meeting a
    /// target.
    /// </summary>
    public bool HitsEachUpdate { get; init; }

    /// <summary>How many updates it lasts; zero for no limit.</summary>
    public int LastsUpdates { get; init; }

    /// <summary>How far outside the view, in pixels, it may travel before it is gone; negative for no limit.</summary>
    public int ViewMargin { get; init; } = 64;

    /// <summary>
    /// How many updates it waits where it was launched before it sets off, as a shot that gathers at a muzzle does.
    /// Its timeline plays and it can hit all the while.
    /// </summary>
    public int LaunchDelayUpdates { get; init; }

    /// <summary>
    /// How far below the view it may go before it is taken away, where that is not <see cref="ViewMargin"/>; below
    /// zero for the same. With a <see cref="ViewMargin"/> below zero, it is taken away only for falling this far below.
    /// </summary>
    public int ViewMarginBelow { get; init; } = -1;

    /// <summary>Its picture, as a path within the assets tree; empty for none.</summary>
    public string Picture { get; init; } = "";

    /// <summary>
    /// Where its picture's top-left corner lies from its centre, across and down, in pixels. Every picture of one
    /// projectile shares it. When not given, the picture's corner is the corner of the projectile's own rectangle.
    /// </summary>
    public (int X, int Y)? PictureOffset { get; init; }

    /// <summary>
    /// The data file of the timeline it is shown by, when it is animated; empty for one shown by its picture alone.
    /// "{direction}" in it, or in the picture, stands for the way the projectile was launched: "right", "up_right",
    /// "up", "up_left", "left", "down_left", "down" or "down_right"; "{side}" stands for "left" or "right", the way
    /// across it is going.
    /// </summary>
    public string Timeline { get; init; } = "";

    /// <summary>
    /// For a falling projectile launched upward, the data file of the timeline it changes to at the top of its arc;
    /// empty for one that keeps the timeline it left with. It takes the same placeholders.
    /// </summary>
    public string FallingTimeline { get; init; } = "";

    /// <summary>The data file of the timeline of the effect it leaves on ending; empty for none.</summary>
    public string EndingEffect { get; init; } = "";

    /// <summary>
    /// The data file of the burst it leaves on ending: another projectile with a hit and settings of its own, as an
    /// explosion is; empty for none.
    /// </summary>
    public string EndingBurst { get; init; } = "";

    /// <summary>The sound asked for as it ends, by name; empty for none.</summary>
    public string EndingSound { get; init; } = "";

    /// <summary>
    /// The data file of the timeline it plays itself when a shot destroys it, where it is; empty for one that is
    /// gone at once. While it plays the projectile holds still, stops shots no longer, harms what it touches and ends
    /// on it; what it drops is left as the timeline ends, in the place it held.
    /// </summary>
    public string ShotTimeline { get; init; } = "";

    /// <summary>What it may leave when a shot destroys it; null for one that leaves nothing.</summary>
    public DropTable? Drops { get; init; }

    /// <summary>Reads settings from a data file's group. Anything not given keeps its plain value.</summary>
    public static ProjectileSettings Read(DataNode data)
    {
        var plain = new ProjectileSettings();
        var settings = new ProjectileSettings
        {
            Width = data.Has("width") ? data.Int("width") : plain.Width,
            Height = data.Has("height") ? data.Int("height") : plain.Height,
            MeetingTerrain = data.Has("meeting_terrain") ? data.Get("meeting_terrain").AsEnum<Terrain>() : plain.MeetingTerrain,
            MeetsTerrainBy = data.Has("terrain_by") ? data.Get("terrain_by").AsEnum<TerrainBy>() : plain.MeetsTerrainBy,
            EndsAtFront = data.Has("ends_at_front") && data.Flag("ends_at_front"),
            MeetingTarget = data.Has("meeting_target") ? data.Get("meeting_target").AsEnum<Target>() : plain.MeetingTarget,
            MetByShot = data.Has("met_by_shot") ? data.Get("met_by_shot").AsEnum<MetBy>() : plain.MetByShot,
            Motion = data.Has("motion") ? data.Get("motion").AsEnum<Moves>() : plain.Motion,
            Acceleration = data.Has("acceleration") ? data.Distance("acceleration") : Pixels.Zero,
            SpeedLimit = data.Has("speed_limit") ? data.Distance("speed_limit") : Pixels.Zero,
            DiagonalAcceleration = data.Has("diagonal_acceleration") ? data.Distance("diagonal_acceleration") : null,
            TerrainShake = data.Has("terrain_shake") ? RoomShake.Read(data.Get("terrain_shake")) : null,
            TargetShake = data.Has("target_shake") ? RoomShake.Read(data.Get("target_shake")) : null,
            WaveSize = data.Has("wave_size") ? data.Distance("wave_size") : Pixels.Zero,
            WaveStep = data.Has("wave_step") ? data.Int("wave_step") : plain.WaveStep,
            WaveStart = data.Has("wave_start") ? data.Int("wave_start") : 0,
            DeliversHit = !data.Has("delivers_hit") || data.Flag("delivers_hit"),
            HitsEachUpdate = data.Has("hits_each_update") && data.Flag("hits_each_update"),
            LastsUpdates = data.Has("lasts_updates") ? data.Int("lasts_updates") : 0,
            ViewMargin = data.Has("view_margin") ? data.Int("view_margin") : plain.ViewMargin,
            LaunchDelayUpdates = data.Has("launch_delay_updates") ? data.Int("launch_delay_updates") : 0,
            ViewMarginBelow = data.Has("view_margin_below") ? data.Int("view_margin_below") : plain.ViewMarginBelow,
            Picture = data.Has("picture") ? data.Text("picture") : "",
            PictureOffset = data.Has("picture_offset") ? (data.Get("picture_offset").Int("x"), data.Get("picture_offset").Int("y")) : null,
            Timeline = data.Has("timeline") ? data.Text("timeline") : "",
            FallingTimeline = data.Has("falling_timeline") ? data.Text("falling_timeline") : "",
            EndingEffect = data.Has("ending_effect") ? data.Text("ending_effect") : "",
            ShotTimeline = data.Has("shot_timeline") ? data.Text("shot_timeline") : "",
            EndingBurst = data.Has("ending_burst") ? data.Text("ending_burst") : "",
            EndingSound = data.Has("ending_sound") ? data.Text("ending_sound") : "",
            Drops = data.Has("drops") ? DropTable.Read(data.Get("drops")) : null,
        };
        if (settings.Width <= 0 || settings.Height <= 0 || settings.LastsUpdates < 0)
            throw data.Problem("gives a size that is not positive or a negative lifetime");
        if (settings.MeetsTerrainBy == TerrainBy.FrontEdge && settings.MeetingTerrain == Terrain.Bounces)
            throw data.Problem("meets terrain by its front edge and bounces, which no projectile does");
        return settings;
    }
}
