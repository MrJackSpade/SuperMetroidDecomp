using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Engine.World;

/// <summary>What every entity's node does the same way: where it starts, how it is shown, and how its colliders are found and sized.</summary>
public static class EntityNode
{
    /// <summary>The exact place a node placed in a scene starts at: its Godot position, on whole pixels.</summary>
    public static PixelPosition PlaceOf(Node2D node) =>
        PixelPosition.FromWhole((int)MathF.Round(node.Position.X), (int)MathF.Round(node.Position.Y));

    /// <summary>
    /// Where an entity starts: the exact place it was given in code, or, when it was given none, where its node was
    /// placed in its scene.
    /// </summary>
    public static PixelPosition StartingPlace(Node2D node, PixelPosition given) => given == default ? PlaceOf(node) : given;

    /// <summary>Puts a node where an exact place says, for drawing.</summary>
    public static void Show(Node2D node, PixelPosition place) => node.Position = place.ToDisplay();

    /// <summary>Every collider under <paramref name="node"/>, in tree order.</summary>
    public static IEnumerable<Collider> CollidersOf(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is Collider collider)
                yield return collider;
            if (child is ICollisionOwner)
                continue;
            foreach (Collider deeper in CollidersOf(child))
                yield return deeper;
        }
    }

    /// <summary>The first enabled-or-not collider of <paramref name="role"/> under <paramref name="node"/>, or null.</summary>
    public static Collider? ColliderOf(Node node, ColliderRole role) => CollidersOf(node).FirstOrDefault(collider => collider.Role == role);

    /// <summary>
    /// Sizes the colliders under <paramref name="node"/> from its data file, where the file lists them: each entry of
    /// "colliders" is named for a collider node and gives its rectangle from the entity's origin.
    /// </summary>
    /// <remarks><code>"colliders": { "Body": { "left": -4, "top": -16, "width": 8, "height": 16 } }</code></remarks>
    public static void ReadColliders(Node node, DataNode data)
    {
        if (!data.Has("colliders"))
            return;
        var byName = CollidersOf(node).ToDictionary(collider => collider.Name.ToString(), StringComparer.Ordinal);
        foreach ((string name, DataNode rect) in data.Get("colliders").Entries())
        {
            if (!byName.TryGetValue(name, out Collider? collider))
                throw rect.Problem($"names a collider the scene does not have; it has: {string.Join(", ", byName.Keys)}");
            int width = rect.Int("width"), height = rect.Int("height");
            if (width < 0 || height < 0)
                throw rect.Problem("has a negative size");
            collider.Rect = new PixelRect(rect.Int("left"), rect.Int("top"), width, height);
        }
    }
}
