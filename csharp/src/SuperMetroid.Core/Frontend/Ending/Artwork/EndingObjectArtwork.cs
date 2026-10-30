namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The ending and credits artwork the game binds (#1463, #1467): the ending's actors and backgrounds are drawn from the
/// installed sheets, whose characters, maps and compositions belong to the import pipeline that reads the installed
/// artwork and composes those sheets; the game keeps only the content's identity.
/// </summary>
public abstract class EndingObjectArtwork
{
    /// <summary>Identity of every selected ending/credits character, map and the compositions its sheets are composed from.</summary>
    public abstract string ContentIdentity { get; }
}
