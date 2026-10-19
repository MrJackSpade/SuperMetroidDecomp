namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>How one of Mother Brain's extended-frame rectangles reacts when a Samus projectile hits it.</summary>
internal enum MotherBrainHitboxShot
{
    /// <summary>The head rectangle: the form-dependent head shot reaction that can damage the brain.</summary>
    Head = 1,

    /// <summary>Every body rectangle: the shot becomes a dud and the body is unaffected.</summary>
    Body,
}
