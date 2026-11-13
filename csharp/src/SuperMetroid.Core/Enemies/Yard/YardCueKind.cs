namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>What a cue of the Yard's animations does: show the previous pose again while hiding or by chance.</summary>
internal enum YardCueKind
{
    /// <summary>While hiding, or on a 50% chance, shows the previous pose again; otherwise runs on.</summary>
    RepeatPreviousPoseUnlessContinuing,
}
