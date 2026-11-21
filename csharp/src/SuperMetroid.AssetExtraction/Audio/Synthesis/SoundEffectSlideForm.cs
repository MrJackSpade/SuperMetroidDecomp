namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Which form the channel's last slide instruction took (#2560), kept until the next one.</summary>
internal enum SoundEffectSlideForm
{
    /// <summary>The plain slide, which keys each note on.</summary>
    Plain,
    /// <summary>The legato slide, which lets the sliding note continue without retriggering its voice.</summary>
    Legato,
}
