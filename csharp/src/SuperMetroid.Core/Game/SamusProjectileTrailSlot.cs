namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the eighteen independent projectile-trail allocations. The left timer is the
/// native free-slot sentinel even though both sides otherwise animate independently.
/// </summary>
public sealed class SamusProjectileTrailSlot
{
    /// <summary>Creates independent left and right trail streams for one native allocation slot.</summary>
    internal SamusProjectileTrailSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    /// <summary>Zero-based host trail-slot index.</summary>
    public int SlotIndex { get; }
    /// <summary>Left-side trail animation state and free-slot timer.</summary>
    public SamusProjectileTrailSide Left { get; }
    /// <summary>Right-side trail animation state.</summary>
    public SamusProjectileTrailSide Right { get; }

    /// <summary>Clears both independent trail streams when the allocation slot is reset.</summary>
    internal void ClearFields()
    {
        Left.ClearFields();
        Right.ClearFields();
    }
}
