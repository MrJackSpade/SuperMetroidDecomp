using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Actors;

/// <summary>
/// A kind of shot something other than the player's weapons launches: the hit it delivers and how its projectile
/// behaves. An enemy's spit, a boss's fireball and the burst a bomb leaves on ending are all these.
/// </summary>
/// <remarks>
/// <code>
/// { "damage": 30, "kinds": ["bomb"], "speed": 2, "sound": "explosion", "scene": "",
///   "projectile": { "width": 32, "height": 32, "motion": "still", "lasts_updates": 8, "hits_each_update": true,
///                   "meeting_terrain": "passes_through_and_hits", "meeting_target": "passes_through", "view_margin": -1 } }
/// </code>
/// </remarks>
public sealed class ShotType
{
    /// <summary>The library the shot's scene is loaded from, which keeps it.</summary>
    private readonly ContentLibrary _content;

    private ShotType(int damage, IReadOnlyList<HitKind> kinds, Pixels speed, string sound, string scene, ProjectileSettings settings, ContentLibrary content)
    {
        _content = content;
        Damage = damage;
        Kinds = kinds;
        Speed = speed;
        Sound = sound;
        Scene = scene;
        Settings = settings;
    }

    /// <summary>The damage of the shot's hit.</summary>
    public int Damage { get; }

    /// <summary>The kinds the shot's hit carries.</summary>
    public IReadOnlyList<HitKind> Kinds { get; }

    /// <summary>How fast the shot is launched, for whoever launches it to use; zero when the launcher decides.</summary>
    public Pixels Speed { get; }

    /// <summary>The sound asked for as the shot is launched, by name; empty for none.</summary>
    public string Sound { get; }

    /// <summary>The scene of its projectile, when the projectile is a class of its own; empty for a plain projectile.</summary>
    public string Scene { get; }

    /// <summary>How the shot's projectile behaves.</summary>
    public ProjectileSettings Settings { get; }

    /// <summary>
    /// Reads a shot type from its data file, or from a group of one. Its projectile's scene, where it names one, is
    /// loaded from <paramref name="content"/> as the first of its shots is launched.
    /// </summary>
    public static ShotType Read(DataNode data, ContentLibrary content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var kinds = data.Has("kinds") ? data.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList() : [HitKind.Contact];
        int damage = data.Int("damage");
        if (damage < 0 || kinds.Count == 0)
            throw data.Problem("gives a negative damage, or a hit that carries no kind");
        return new ShotType(
            damage,
            kinds,
            data.Has("speed") ? data.Distance("speed") : Pixels.Zero,
            data.Has("sound") ? data.Text("sound") : "",
            data.Has("scene") ? data.Text("scene") : "",
            data.Has("projectile") ? ProjectileSettings.Read(data.Get("projectile")) : new ProjectileSettings(),
            content);
    }

    /// <summary>
    /// Makes the shot's projectile at <paramref name="at"/>, moving at <paramref name="velocity"/>, on
    /// <paramref name="side"/>, never hitting <paramref name="launcher"/>.
    /// </summary>
    public Projectile Launch(PixelPosition at, PixelVelocity velocity, HitSide side, ICollisionOwner? launcher, object? source) =>
        LaunchDoing(Damage, at, velocity, side, launcher, source);

    /// <summary>
    /// Makes the shot's projectile as <see cref="Launch"/> does, with a hit of <paramref name="damage"/> in place of
    /// the shot's own: for fire that does the harm of what it came of.
    /// </summary>
    public Projectile LaunchDoing(int damage, PixelPosition at, PixelVelocity velocity, HitSide side, ICollisionOwner? launcher, object? source)
    {
        if (damage < 0)
            throw new ArgumentOutOfRangeException(nameof(damage), damage, "A hit does no less than no damage.");
        var hit = new Hit(damage, side, [.. Kinds]) { Source = source, Direction = velocity.OverOneUpdate };
        Projectile projectile = Make();
        projectile.Launch(hit, at, velocity, Settings, launcher);
        return projectile;
    }

    /// <summary>Makes the shot's projectile: one of its scene, or a plain one.</summary>
    private Projectile Make()
    {
        if (Scene.Length == 0)
            return new Projectile { Name = "Projectile" };
        Node root = _content.LoadScene(Scene).Instantiate();
        return root as Projectile
            ?? throw new InvalidOperationException($"The projectile scene \"{Scene}\" has a {root.GetType().Name} at its root, not a Projectile.");
    }
}
