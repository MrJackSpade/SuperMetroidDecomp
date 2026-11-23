using Engine.Collision;
using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// What a projectile leaves on ending when it leaves another projectile: an explosion, with a hit and settings of its
/// own. It is launched where the first one ended, on the same side.
/// </summary>
/// <remarks>
/// <code>
/// { "damage": 30, "kinds": ["bomb"],
///   "projectile": { "width": 32, "height": 32, "motion": "still", "lasts_updates": 8, "hits_each_update": true,
///                   "meeting_terrain": "passes_through_and_hits", "meeting_target": "passes_through", "view_margin": -1 } }
/// </code>
/// </remarks>
public sealed class Burst
{
    private Burst(int damage, IReadOnlyList<HitKind> kinds, ProjectileSettings settings)
    {
        Damage = damage;
        Kinds = kinds;
        Settings = settings;
    }

    /// <summary>The damage of the burst's hit.</summary>
    public int Damage { get; }

    /// <summary>The kinds the burst's hit carries.</summary>
    public IReadOnlyList<HitKind> Kinds { get; }

    /// <summary>How the burst behaves.</summary>
    public ProjectileSettings Settings { get; }

    /// <summary>Reads a burst from its data file.</summary>
    public static Burst Read(DataNode data)
    {
        var kinds = data.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList();
        int damage = data.Int("damage");
        if (damage < 0 || kinds.Count == 0)
            throw data.Problem("gives a negative damage, or a hit that carries no kind");
        return new Burst(damage, kinds, ProjectileSettings.Read(data.Get("projectile")));
    }

    /// <summary>Makes the burst's projectile at <paramref name="at"/>, on <paramref name="side"/>, never hitting <paramref name="launcher"/>.</summary>
    public Projectile Launch(PixelPosition at, HitSide side, ICollisionOwner? launcher, object? source)
    {
        var hit = new Hit(Damage, side, [.. Kinds]) { Source = source };
        return Projectile.Of(hit, at, PixelVelocity.Zero, Settings, launcher);
    }
}
