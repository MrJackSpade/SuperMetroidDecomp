using Engine.Collision;
using Engine.Shared;
using Engine.Tests.Content;
using Engine.World;

namespace Engine.Tests.Collision;

/// <summary>
/// A small hand-written room for the engine's own checks: terrain from a grid of letters, a collision world over it
/// and a mover. Tiles are 16 pixels. "#" is solid, "." empty, "/" rises to the right, "\" falls to the right, "S" is
/// a block a "shot" breaks, "C" one that gives way when stood on, "H" one hidden until a "bomb" reveals it, and "R"
/// one a "shot" breaks that regrows.
/// </summary>
public sealed class StandInRoom : IDisposable
{
    /// <summary>The room's assets.</summary>
    private readonly TempAssets _assets = new();

    /// <summary>Everything made for the room, to be freed with it.</summary>
    private readonly List<Godot.Node> _nodes = [];

    /// <summary>A room whose terrain is the given rows of letters.</summary>
    public StandInRoom(params string[] rows)
    {
        string risingRight = string.Join(", ", Enumerable.Range(1, 16));
        string fallingRight = string.Join(", ", Enumerable.Range(1, 16).Reverse());
        _assets
            .With("blocks/shot.json", """{ "solid": true, "broken_by": ["shot"] }""")
            .With("blocks/crumble.json", """{ "solid": true, "breaks_when_stood_on": true, "stood_on_updates": 2 }""")
            .With("blocks/hidden.json", """{ "solid": true, "broken_by": ["shot"], "hidden_until": ["bomb"] }""")
            .With("blocks/regrow.json", """{ "solid": true, "broken_by": ["shot"], "regrows_after": 3 }""")
            .With("room.json", $$"""
                {
                  "tile_size": 16,
                  "tiles": {
                    ".": { "kind": "empty" },
                    "#": { "kind": "solid" },
                    "/": { "kind": "slope", "solid": [{{risingRight}}] },
                    "\\": { "kind": "slope", "solid": [{{fallingRight}}] },
                    "S": { "kind": "block", "block": "blocks/shot.json" },
                    "C": { "kind": "block", "block": "blocks/crumble.json" },
                    "H": { "kind": "block", "block": "blocks/hidden.json" },
                    "R": { "kind": "block", "block": "blocks/regrow.json" }
                  },
                  "rows": [{{string.Join(", ", rows.Select(row => $"\"{row.Replace("\\", "\\\\")}\""))}}],
                  "patches": { "top left opened": { "column": 0, "row": 0, "rows": ["."] } }
                }
                """);
        Terrain = new Terrain { LayoutFile = "room.json" };
        Terrain.Load(_assets.Library);
        _nodes.Add(Terrain);
        World = new CollisionWorld(Terrain);
        Mover = new Mover(World);
    }

    /// <summary>The room's terrain.</summary>
    public Terrain Terrain { get; }

    /// <summary>The room's collision.</summary>
    public CollisionWorld World { get; }

    /// <summary>The room's mover.</summary>
    public Mover Mover { get; }

    /// <summary>
    /// Adds a body whose collider reaches <paramref name="halfWidth"/> each side of its origin and <paramref name="height"/>
    /// up from it, standing with its origin, the middle of its feet, at the given pixel.
    /// </summary>
    public (StandInBody Owner, Collider Body) AddBody(int x, int feetY, int halfWidth = 4, int height = 16)
    {
        var owner = new StandInBody { Position = PixelPosition.FromWhole(x, feetY) };
        Collider body = owner.AddCollider(ColliderRole.Body, new PixelRect(-halfWidth, -height, halfWidth * 2, height));
        World.Register(body);
        _nodes.Add(owner);
        return (owner, body);
    }

    /// <summary>Adds a solid covering <paramref name="rect"/> of the room.</summary>
    public (StandInBody Owner, Collider Solid) AddSolid(PixelRect rect)
    {
        var owner = new StandInBody { Position = PixelPosition.FromWhole(rect.Left, rect.Top) };
        Collider solid = owner.AddCollider(ColliderRole.Solid, new PixelRect(0, 0, rect.Width, rect.Height));
        World.Register(solid);
        _nodes.Add(owner);
        return (owner, solid);
    }

    /// <summary>Frees the room's nodes and deletes its assets.</summary>
    public void Dispose()
    {
        foreach (Godot.Node node in _nodes)
            node.Free();
        _assets.Dispose();
    }
}
