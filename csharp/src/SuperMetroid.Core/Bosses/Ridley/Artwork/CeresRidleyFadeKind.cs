namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Selects the eye fade or body fade, which use opposite shade-phase directions.</summary>
internal enum CeresRidleyFadeKind
{
    /// <summary>The eye fade starts at its bright endpoint and descends toward black.</summary>
    Eyes,
    /// <summary>The body fade starts black and rises toward its selected endpoint.</summary>
    Body,
}
