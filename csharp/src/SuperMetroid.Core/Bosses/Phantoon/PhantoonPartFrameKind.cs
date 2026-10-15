namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>What a Phantoon part frame shows.</summary>
internal enum PhantoonPartFrameKind
{
    /// <summary>The invulnerable body.</summary>
    InvulnerableBody,
    /// <summary>The body selecting its full hitbox.</summary>
    FullHitboxBody,
    /// <summary>The body selecting only its eye hitbox.</summary>
    EyeHitboxBody,
    /// <summary>One of three eyelid poses, closed (0) to open (2).</summary>
    OpeningEye,
    /// <summary>The closed eye.</summary>
    ClosedEye,
    /// <summary>The centered eyeball.</summary>
    CenteredEye,
    /// <summary>The eyeball gazing in one of eight clockwise directions from up.</summary>
    Gaze,
    /// <summary>One of three tentacle poses.</summary>
    Tentacle,
    /// <summary>One of three mouth poses, closed (0) to open (2).</summary>
    Mouth,
}