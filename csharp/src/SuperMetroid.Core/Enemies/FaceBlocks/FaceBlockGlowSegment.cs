namespace SuperMetroid.Core.Enemies.FaceBlocks;

/// <summary>The runs of steps the Blue Brinstar face blocks' room-wide glow plays (#2477). Their beats are <see cref="FaceBlockGlowAnimations"/>.</summary>
internal enum FaceBlockGlowSegment
{
    /// <summary>The glow brightening from its dimmest to its brightest and dimming back, a level each step.</summary>
    Glowing,

    /// <summary>The glow resting on its last step's level, its dimmest, until its first step.</summary>
    Restarting,
}
