namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>How one of Mother Brain's extended-frame rectangles reacts when Samus touches it.</summary>
internal enum MotherBrainHitboxTouch
{
    /// <summary>The head rectangle: a spin-jumping Samus toggles the head's flash cadence and nothing else happens.</summary>
    Head = 1,

    /// <summary>Every body rectangle: the body's own touch reaction.</summary>
    Body,
}
