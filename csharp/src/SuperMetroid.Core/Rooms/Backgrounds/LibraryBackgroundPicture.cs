using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Which picture of a library background a room shows (#2558): the background's ordinary picture, or the picture of the
/// entry door that shows one of its own (<see cref="LibraryBackground.PictureDoors"/>).
/// </summary>
public abstract record LibraryBackgroundPicture
{
    /// <summary>Limits the pictures to the ordinary one and a door's.</summary>
    private LibraryBackgroundPicture()
    {
    }

    /// <summary>The ordinary picture, which a room entered through a door that shows no picture of its own shows.</summary>
    public static LibraryBackgroundPicture Ordinary { get; } = new OrdinaryPicture();

    /// <summary>The picture of <paramref name="door"/>, an entry door that shows its own picture of the background.</summary>
    public static LibraryBackgroundPicture Of(DoorDefinition door) => new DoorPicture(Ensure.NotNull(door));

    /// <summary>The background's ordinary picture.</summary>
    public sealed record OrdinaryPicture : LibraryBackgroundPicture;

    /// <summary>The picture a room entered through <paramref name="Door"/> shows.</summary>
    /// <param name="Door">The entry door that shows its own picture.</param>
    public sealed record DoorPicture(DoorDefinition Door) : LibraryBackgroundPicture;
}
