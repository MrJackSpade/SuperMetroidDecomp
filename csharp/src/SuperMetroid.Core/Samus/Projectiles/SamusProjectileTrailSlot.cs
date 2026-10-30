namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One of the projectile-trail slots. The left timer marks the slot free even though both sides otherwise animate
/// independently.
/// </summary>
public sealed class SamusProjectileTrailSlot
{
    /// <summary>Creates independent left and right trail streams for one trail slot.</summary>
    internal SamusProjectileTrailSlot(int slotIndex)
    {
        SlotIndex = slotIndex;
        Left = new SamusProjectileTrailSide();
        Right = new SamusProjectileTrailSide();
    }

    /// <summary>The slot's ordinal among the trail slots, zero first.</summary>
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
