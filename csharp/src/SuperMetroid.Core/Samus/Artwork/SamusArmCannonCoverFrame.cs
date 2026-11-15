namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A picture of the arm cannon's cover part-way or fully open; a closed cover shows no picture.</summary>
public enum SamusArmCannonCoverFrame
{
    /// <summary>The cover barely open: an opening's first picture and a closing's last.</summary>
    SlightlyOpen,

    /// <summary>The cover half open, between the slightly and fully open pictures either way.</summary>
    HalfOpen,

    /// <summary>The cover fully open: where an opening ends and a closing starts.</summary>
    FullyOpen,
}
