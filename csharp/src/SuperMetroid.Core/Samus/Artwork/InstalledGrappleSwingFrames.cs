using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed swing-picture selection only, as installed (#1596); where Samus's body stands is the game's, not the pictures'.</summary>
public sealed partial class InstalledGrappleSwingFrames
{
    /// <summary>The installed picture each swing direction shows.</summary>
    private readonly Dictionary<GrappleSwingDirection, GrappleSwingPicture> pictures;

    /// <summary>Copies the picture of every swing direction the import pipeline validated.</summary>
    /// <param name="pictures">The picture each <see cref="GrappleSwingDirection"/> shows.</param>
    /// <exception cref="ArgumentException">A swing direction shows no picture.</exception>
    internal InstalledGrappleSwingFrames(IReadOnlyDictionary<GrappleSwingDirection, GrappleSwingPicture> pictures)
    {
        Ensure.NotNull(pictures);
        if (pictures.Count != GrappleSwingDirection.Count || GrappleSwingDirection.All.Any(direction => !pictures.ContainsKey(direction)))
            throw new ArgumentException($"Grapple swing frames need a picture for each of {GrappleSwingDirection.Count} swing directions.", nameof(pictures));
        this.pictures = pictures.ToDictionary();
    }

    /// <summary>Selects the editable displayed picture for one swing direction, independently of the body-placement offset and the runtime's separate straight-down animation handling.</summary>
    /// <param name="direction">The swing direction nearest the mirrored rope angle.</param>
    /// <returns>The swing picture the direction shows, within the current facing's swing artwork.</returns>
    public GrappleSwingPicture Resolve(GrappleSwingDirection direction) => pictures[direction];
}
