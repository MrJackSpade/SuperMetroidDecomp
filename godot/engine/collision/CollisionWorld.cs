using Engine.Shared;

namespace Engine.Collision;

/// <summary>
/// One room's collision. It knows the room's terrain and every collider registered with it, and answers what a
/// rectangle overlaps, always in the order the colliders were registered. It finds overlaps; it never decides what one
/// means.
/// </summary>
public sealed class CollisionWorld
{
    /// <summary>The registered colliders, in the order they were registered.</summary>
    private readonly List<Collider> _colliders = [];

    /// <summary>A collision world over <paramref name="terrain"/>.</summary>
    public CollisionWorld(ITerrainShape terrain)
    {
        ArgumentNullException.ThrowIfNull(terrain);
        Terrain = terrain;
    }

    /// <summary>The room's terrain.</summary>
    public ITerrainShape Terrain { get; }

    /// <summary>
    /// The regions where the room's ways out lie, which the room gives as it is set up. A way out is open to whoever
    /// leaves by it, and a wall, as terrain is, to a move that says so (<see cref="MoveOptions.StoppedByWaysOut"/>).
    /// </summary>
    public IReadOnlyList<PixelRect> WaysOut { get; set; } = [];

    /// <summary>
    /// The blocks things of the room hold while they lie in them, as an item holds the block it lies in: open to the
    /// player, who takes what is there, and a wall, as a way out is, to a move a way out stops.
    /// </summary>
    private readonly List<PixelRect> _heldBlocks = [];

    /// <summary>Holds <paramref name="block"/> for something that lies in it (<see cref="OpenToPlayerAloneOver"/>).</summary>
    public void HoldBlock(PixelRect block) => _heldBlocks.Add(block);

    /// <summary>Gives up a block held, as what lay in it goes.</summary>
    /// <exception cref="InvalidOperationException">The block is not held.</exception>
    public void LetBlockGo(PixelRect block)
    {
        if (!_heldBlocks.Remove(block))
            throw new InvalidOperationException($"The block at {block.Left},{block.Top} is given up, and nothing holds it.");
    }

    /// <summary>
    /// The first of the room's ways out, or failing one of the blocks held by what lies in them, that lies over any
    /// of <paramref name="area"/>: what is open to the player alone there. Null when nothing of the kind is.
    /// </summary>
    public PixelRect? OpenToPlayerAloneOver(PixelRect area)
    {
        foreach (PixelRect wayOut in WaysOut)
        {
            if (wayOut.Overlaps(area))
                return wayOut;
        }

        foreach (PixelRect block in _heldBlocks)
        {
            if (block.Overlaps(area))
                return block;
        }

        return null;
    }

    /// <summary>Adds a collider. Registering one already registered changes nothing.</summary>
    public void Register(Collider collider)
    {
        ArgumentNullException.ThrowIfNull(collider);
        if (!_colliders.Contains(collider))
            _colliders.Add(collider);
    }

    /// <summary>Removes a collider.</summary>
    public void Unregister(Collider collider) => _colliders.Remove(collider);

    /// <summary>
    /// The enabled colliders of <paramref name="role"/> that <paramref name="rect"/> overlaps, in registration order.
    /// </summary>
    /// <param name="rect">The rectangle, in room pixels.</param>
    /// <param name="role">The role of collider looked for.</param>
    /// <param name="touchingCounts">Whether a collider lying edge to edge with the rectangle counts.</param>
    /// <param name="except">An owner whose own colliders are left out.</param>
    public IReadOnlyList<Collider> Overlapping(
        PixelRect rect, ColliderRole role, bool touchingCounts = false, ICollisionOwner? except = null)
    {
        var found = new List<Collider>();
        foreach (Collider collider in _colliders)
        {
            if (!collider.Enabled || collider.Role != role)
                continue;
            if (except is not null && ReferenceEquals(collider.CollisionOwner, except))
                continue;
            PixelRect other = collider.RoomRect;
            if (touchingCounts ? rect.OverlapsOrTouches(other) : rect.Overlaps(other))
                found.Add(collider);
        }

        return found;
    }

    /// <summary>The enabled colliders of <paramref name="role"/> belonging to <paramref name="owner"/>, in registration order.</summary>
    public IReadOnlyList<Collider> Of(ICollisionOwner owner, ColliderRole role) =>
        _colliders.Where(collider => collider.Enabled && collider.Role == role && ReferenceEquals(collider.CollisionOwner, owner)).ToList();
}
