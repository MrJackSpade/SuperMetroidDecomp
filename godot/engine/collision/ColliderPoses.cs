using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.Collision;

/// <summary>
/// Where a thing's parts are in each of its poses: the animation moves collider nodes and turns them on and off, and
/// this is what it moves them to. Taking a pose puts every collider the pose names where it has it and switches off
/// every other collider that any of the poses names; colliders no pose names are left alone.
/// </summary>
/// <remarks>
/// <code>
/// "poses": { "open": { "colliders": { "Mouth": { "left": 0, "top": -112, "width": 32, "height": 32 } } },
///            "closed": { "colliders": { } } }
/// </code>
/// A pose may hold more than its colliders; whoever owns the poses reads the rest.
/// </remarks>
public sealed class ColliderPoses
{
    private readonly Dictionary<string, IReadOnlyDictionary<string, PixelRect>> _poses = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Collider> _posed = new(StringComparer.Ordinal);
    private readonly string _owner;

    private ColliderPoses(string owner) => _owner = owner;

    /// <summary>The pose taken last; empty before any.</summary>
    public string Current { get; private set; } = "";

    /// <summary>Reads the poses in a data file's group, for the colliders under <paramref name="owner"/>.</summary>
    /// <exception cref="ContentException">A pose names a collider the owner does not have.</exception>
    public static ColliderPoses Read(DataNode group, Node owner)
    {
        ArgumentNullException.ThrowIfNull(group);
        ArgumentNullException.ThrowIfNull(owner);
        var byName = World.EntityNode.CollidersOf(owner).ToDictionary(collider => collider.Name.ToString(), StringComparer.Ordinal);
        var poses = new ColliderPoses(owner.Name);
        foreach ((string name, DataNode pose) in group.Entries())
        {
            var rects = new Dictionary<string, PixelRect>(StringComparer.Ordinal);
            foreach ((string part, DataNode rect) in pose.Get("colliders").Entries())
            {
                if (!byName.TryGetValue(part, out Collider? collider))
                    throw rect.Problem($"names a collider the scene does not have; it has: {string.Join(", ", byName.Keys)}");
                int width = rect.Int("width"), height = rect.Int("height");
                if (width < 0 || height < 0)
                    throw rect.Problem("has a negative size");
                rects[part] = new PixelRect(rect.Int("left"), rect.Int("top"), width, height);
                poses._posed[part] = collider;
            }

            poses._poses[name] = rects;
        }

        return poses;
    }

    /// <summary>Whether there is a pose named <paramref name="name"/>.</summary>
    public bool Has(string name) => _poses.ContainsKey(name);

    /// <summary>Switches off every collider any pose names: the thing has no pose yet, or is in none.</summary>
    public void TakeNone()
    {
        Current = "";
        foreach (Collider collider in _posed.Values)
            collider.Enabled = false;
    }

    /// <summary>
    /// Takes the pose named <paramref name="name"/>, with every collider it names moved by <paramref name="shift"/>
    /// from where the pose has it.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is no pose of that name.</exception>
    public void Take(string name, Vector2I shift = default)
    {
        if (!_poses.TryGetValue(name, out IReadOnlyDictionary<string, PixelRect>? rects))
            throw new InvalidOperationException($"The data of \"{_owner}\" gives it no pose named \"{name}\".");
        Current = name;
        foreach ((string part, Collider collider) in _posed)
        {
            collider.Enabled = rects.TryGetValue(part, out PixelRect rect);
            if (collider.Enabled)
                collider.Rect = new PixelRect(rect.Left + shift.X, rect.Top + shift.Y, rect.Width, rect.Height);
        }
    }
}
