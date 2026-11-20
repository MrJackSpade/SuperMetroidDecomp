namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Which way an arm-cannon cover picture is drawn (#2556): the cartridge holds one opening of three pictures for each
/// orientation, and every aim shows one orientation's opening, mirrored as the aim's cover appearance says.
/// </summary>
public enum SamusArmCannonCoverOrientation
{
    /// <summary>For an aim straight up or straight down.</summary>
    Vertical,

    /// <summary>For an aim to the left or the right.</summary>
    Horizontal,

    /// <summary>For an aim diagonally downward.</summary>
    DownwardDiagonal,

    /// <summary>For an aim diagonally upward.</summary>
    UpwardDiagonal,
}
