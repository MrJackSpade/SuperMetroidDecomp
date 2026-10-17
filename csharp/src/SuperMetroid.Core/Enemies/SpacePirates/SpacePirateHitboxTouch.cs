namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How one of the ordinary Pirates' collision rectangles reacts when Samus touches it.</summary>
internal enum SpacePirateHitboxTouch
{
    /// <summary>The empty frame's point rectangle: the common enemy contact reaction.</summary>
    Ordinary = 1,
    /// <summary>Every drawn Pirate rectangle: the Pirate family's contact reaction.</summary>
    Pirate,
}
