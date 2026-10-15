namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The three parallel item presentations encoded by bank-$84 header ranges.</summary>
public enum CollectiblePresentation : byte
{
    /// <summary>The item begins visible and can be acquired by Samus contact.</summary>
    Exposed,
    /// <summary>The item begins inside a projectile-triggered Chozo orb; breaking the shell does not persist between room visits.</summary>
    ChozoOrb,
    /// <summary>The item begins in a concealed shot block that temporarily reveals the item and later restores its saved block.</summary>
    ShotBlock,
}
