namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>How a resident SPC library's command handler admits a changed request for one sound effect.</summary>
internal enum SpcSoundRequestAdmission
{
    /// <summary>The request is refused while a high-priority sound plays, and in a low-health library's low-health mode.</summary>
    ByPriority,

    /// <summary>The request plays over a high-priority sound; only a low-health library's low-health mode refuses it.</summary>
    PriorityExempt,

    /// <summary>The library's clearing request, which nothing refuses.</summary>
    Clearing,
}
