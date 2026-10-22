namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>A boss that shows its body on BG2 by writing tile words into the tilemap BG2 shows (#1392).</summary>
public enum BossBody
{
    /// <summary>Kraid, whose body pages and head frames fill his 64x64 map.</summary>
    Kraid,

    /// <summary>Phantoon, drawing its body frames into the enemy BG2 image.</summary>
    Phantoon,

    /// <summary>Draygon, drawing its body frames into the enemy BG2 image.</summary>
    Draygon,

    /// <summary>Crocomire, drawing its body frames and melting images into the enemy BG2 image.</summary>
    Crocomire,

    /// <summary>Mother Brain, drawing her body frames into the enemy BG2 image from phase two.</summary>
    MotherBrain,
}
