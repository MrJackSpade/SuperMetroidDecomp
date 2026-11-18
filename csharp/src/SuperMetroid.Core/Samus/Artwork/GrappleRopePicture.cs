namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// One picture of a grapple rope segment (#2285, #2388), named for how far along its cycle the rope's travelling pattern
/// stands. Values are arbitrary.
/// </summary>
public enum GrappleRopePicture
{
    /// <summary>The pattern at its start.</summary>
    Start,

    /// <summary>The pattern a quarter of the way on.</summary>
    Quarter,

    /// <summary>The pattern half of the way on.</summary>
    Half,

    /// <summary>The pattern three quarters of the way on.</summary>
    ThreeQuarters,
}
