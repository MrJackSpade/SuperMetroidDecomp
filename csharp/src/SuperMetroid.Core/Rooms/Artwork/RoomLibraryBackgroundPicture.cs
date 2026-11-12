using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// What a room state's background draws from its library background (#1466, #1649): the background's installed picture,
/// or no picture, because the state loads no library background or because the player sees something else there (the
/// boss's body, the sky's rows, or nothing), which its own owner draws. Each kind is its own record.
/// </summary>
public abstract record RoomLibraryBackgroundPicture
{
    /// <summary>Only the nested kinds derive.</summary>
    private RoomLibraryBackgroundPicture()
    {
    }

    /// <summary>The state loads no library background, so its background draws none.</summary>
    public static RoomLibraryBackgroundPicture NoneLoaded { get; } = new NotLoaded();

    /// <summary>
    /// What a state loading <paramref name="background"/> draws from it: its picture, which <paramref name="picture"/>
    /// resolves from the installation, where the player sees it as such, and otherwise what <see cref="LibraryBackground.View"/>
    /// says the player sees instead, which no installed picture holds.
    /// </summary>
    /// <param name="background">The state's library background, or null where the state loads none.</param>
    /// <param name="picture">Resolves the installed picture of a background the player sees as such.</param>
    public static RoomLibraryBackgroundPicture Of(LibraryBackground? background, Func<LibraryBackground, RoomBackgroundImageResolver> picture)
    {
        Ensure.NotNull(picture);
        return background switch
        {
            null => NoneLoaded,
            { View: LibraryBackgroundView.Picture } pictured => new Drawn(picture(pictured)),
            { } shown => new ShownOtherwise(shown.View),
        };
    }

    /// <summary>The state loads no library background.</summary>
    public sealed record NotLoaded : RoomLibraryBackgroundPicture;

    /// <summary>The state's background draws its library background's installed picture.</summary>
    public sealed record Drawn : RoomLibraryBackgroundPicture
    {
        /// <summary>Holds the resolver of the background's picture.</summary>
        /// <param name="image">Resolves what the picture shows as the room's appearance inputs change.</param>
        public Drawn(RoomBackgroundImageResolver image) => Image = Ensure.NotNull(image);

        /// <summary>Resolves what the picture shows as the room's appearance inputs change.</summary>
        public RoomBackgroundImageResolver Image { get; }
    }

    /// <summary>
    /// The state's library background is not seen as its own picture, so the background draws none of it: the player sees
    /// what <see cref="View"/> names, which the boss's body or the sky draws, or nothing.
    /// </summary>
    public sealed record ShownOtherwise : RoomLibraryBackgroundPicture
    {
        /// <summary>Records what the player sees instead of the background's picture.</summary>
        /// <param name="view">What the player sees on the background instead: any view but the background's own picture.</param>
        /// <exception cref="ArgumentOutOfRangeException">The view is the background's own picture, or undefined.</exception>
        public ShownOtherwise(LibraryBackgroundView view) =>
            View = view != LibraryBackgroundView.Picture && Enum.IsDefined(view) ? view
                : throw new ArgumentOutOfRangeException(nameof(view), view, "A background seen as its own picture draws that picture.");

        /// <summary>What the player sees on the background instead.</summary>
        public LibraryBackgroundView View { get; }
    }
}
