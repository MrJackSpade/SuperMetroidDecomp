namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The runs of Mother Brain's rainbow beam backdrop (#2404): its first colour, then its hue cycle.</summary>
internal enum MotherBrainRainbowBeamSegment
{
    /// <summary>The colour the beam shows on its first update.</summary>
    FirstColour,
    /// <summary>The backdrop's hue cycle, a colour an update, which the beam loops while it fires.</summary>
    HueCycle,
}
