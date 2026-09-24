namespace SuperMetroid.Core.Assets;

/// <summary>Four mutually exclusive native explosion OBJ fragment uploads.</summary>
public enum EndingObjectFragmentId
{
    Segment70,
    Segment74,
    Segment78,
    Segment7C,
}

/// <summary>
/// Editable OBJ character sheets for the ending clouds and planet explosion.
/// Actor instructions, OAM composition, palette selection and timing remain code.
/// </summary>
public sealed class EndingObjectArtworkCatalog
{
    private readonly RoomCharacterAtlas[] fragments;

    public EndingObjectArtworkCatalog(RoomCharacterAtlas clouds,
        RoomCharacterAtlas explosion, IReadOnlyList<RoomCharacterAtlas> fragments)
    {
        Clouds = clouds ?? throw new ArgumentNullException(nameof(clouds));
        Explosion = explosion ?? throw new ArgumentNullException(nameof(explosion));
        ArgumentNullException.ThrowIfNull(fragments);
        if (Clouds.Transfer.Length != EndingObjectArtworkFormat.CloudByteCount ||
            Explosion.Transfer.Length != EndingObjectArtworkFormat.ExplosionByteCount ||
            fragments.Count != EndingObjectArtworkFormat.FragmentCount ||
            fragments.Any(fragment => fragment is null ||
                fragment.Transfer.Length != EndingObjectArtworkFormat.FragmentByteCount))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
        this.fragments = fragments.ToArray();
    }

    public RoomCharacterAtlas Clouds { get; }
    public RoomCharacterAtlas Explosion { get; }

    /// <summary>Four ordered $0800-byte OBJ fragments uploaded at VRAM $E000..$FFFF.</summary>
    public RoomCharacterAtlas Fragment(EndingObjectFragmentId id) =>
        (uint)id < fragments.Length ? fragments[(int)id] :
            throw new ArgumentOutOfRangeException(nameof(id));
}

/// <summary>File identities and exact native OBJ transfer dimensions.</summary>
public static class EndingObjectArtworkFormat
{
    public const int ManifestVersion = 1;
    public const string ManifestFileName = "ending-object-artwork.json";
    public const string CloudFileName = "ending-cloud-characters.png";
    public const string ExplosionFileName = "ending-explosion-objects.png";
    public const int CloudByteCount = 0x4000;
    public const int ExplosionByteCount = 0x6000;
    public const int FragmentByteCount = 0x0800;
    public const int FragmentCount = 4;
    public static string FragmentFileName(int index) => index switch
    {
        0 => "ending-explosion-fragment-70.png",
        1 => "ending-explosion-fragment-74.png",
        2 => "ending-explosion-fragment-78.png",
        3 => "ending-explosion-fragment-7c.png",
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };
}
