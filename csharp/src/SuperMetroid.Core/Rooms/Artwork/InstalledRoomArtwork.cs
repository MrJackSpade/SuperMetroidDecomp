using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured room artwork (#1392, #1466): the block sheet of each tileset a room can load with the
/// appearance inputs its blocks depend on, the image of each library background a room state loads, the sheets of the
/// pictures encounters write over the rooms' layers, the sheets of the effect overlay's pictures (each room effect's liquid
/// or atmosphere), the sheets of the HUD's pictures (#1731), each tileset's message boxes, and the turning floor of each
/// tileset whose rooms show one (#1467, #1649).
/// </summary>
public sealed class InstalledRoomArtwork
{
    /// <summary>
    /// Keeps the sheets, each under its own tileset, the background images, each under its own name, and the written
    /// picture, effect overlay picture and HUD picture sheets, each under its own tileset.
    /// </summary>
    /// <exception cref="ArgumentException">A sheet is filed under another tileset, or an image under another name.</exception>
    public InstalledRoomArtwork(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets,
        IReadOnlyDictionary<string, RoomBackgroundImage> backgrounds, IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> written,
        IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> fxOverlayPictures, IReadOnlyDictionary<TilesetDefinition, HudPictureSheet> hud,
        IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> messageBoxes,
        IReadOnlyDictionary<TilesetDefinition, RoomFloorSheet> floors)
    {
        Floors = Ensure.NotNull(floors);
        foreach ((TilesetDefinition tileset, RoomFloorSheet floor) in floors)
        {
            if (floor.Tileset != tileset)
                throw new ArgumentException($"{floor.Name} is filed under {tileset}.", nameof(floors));
        }
        HudPictures = Ensure.NotNull(hud);
        foreach ((TilesetDefinition tileset, HudPictureSheet pictures) in hud)
        {
            if (pictures.Tileset != tileset)
                throw new ArgumentException($"{pictures.Name} is filed under {tileset}.", nameof(hud));
        }
        MessageBoxes = Ensure.NotNull(messageBoxes);
        foreach ((TilesetDefinition tileset, MessageBoxSheet messages) in messageBoxes)
        {
            if (messages.Tileset != tileset)
                throw new ArgumentException($"{messages.Name} is filed under {tileset}.", nameof(messageBoxes));
        }
        Sheets = Ensure.NotNull(sheets);
        Backgrounds = Ensure.NotNull(backgrounds);
        Written = Ensure.NotNull(written);
        FxOverlayPictures = Ensure.NotNull(fxOverlayPictures);
        foreach ((TilesetDefinition tileset, WrittenPictureSheet pictures) in written.Concat(fxOverlayPictures))
        {
            if (pictures.Tileset != tileset)
                throw new ArgumentException($"{pictures.Name} is filed under {tileset}.", nameof(written));
        }
        if (written.Values.Any(pictures => pictures.Layer != WrittenPictureLayer.WrittenOverRoom) || fxOverlayPictures.Values.Any(pictures => pictures.Layer != WrittenPictureLayer.FxOverlay))
            throw new ArgumentException("A written picture sheet is filed under another layer's sheets.", nameof(written));
        foreach ((string name, RoomBackgroundImage background) in backgrounds)
        {
            if (background.Name != name)
                throw new ArgumentException($"Room background {background.Name} is filed under {name}.", nameof(backgrounds));
        }
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in sheets)
        {
            if (sheet.Tileset != tileset)
                throw new ArgumentException($"The block sheet of {sheet.Tileset} is filed under {tileset}.", nameof(sheets));
        }
    }

    /// <summary>The sheets by tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> Sheets { get; }

    /// <summary>The library background images by name.</summary>
    public IReadOnlyDictionary<string, RoomBackgroundImage> Backgrounds { get; }

    /// <summary>The sheets of pictures written over the foreground and the background, by the tileset whose rooms the pictures are written in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> Written { get; }

    /// <summary>The sheets of the effect overlay's pictures (each room effect's liquid or atmosphere), by the tileset whose colours they show in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> FxOverlayPictures { get; }

    /// <summary>The sheets of the HUD's pictures, by the tileset whose colours they show in (#1731).</summary>
    public IReadOnlyDictionary<TilesetDefinition, HudPictureSheet> HudPictures { get; }

    /// <summary>The turning floor of each tileset whose rooms show one; a tileset whose rooms show none has none (#1649).</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomFloorSheet> Floors { get; }

    /// <summary>The turning floor of <paramref name="tileset"/>, whose rooms show one.</summary>
    /// <exception cref="InvalidDataException">No floor is installed for the tileset.</exception>
    public RoomFloorSheet FloorOf(TilesetDefinition tileset) =>
        Floors.TryGetValue(tileset, out RoomFloorSheet? floor) ? floor : throw new InvalidDataException($"No floor is installed for {tileset}.");

    /// <summary>The message box images of each tileset's rooms.</summary>
    public IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> MessageBoxes { get; }

    /// <summary>The message box images of <paramref name="tileset"/>'s rooms.</summary>
    /// <exception cref="InvalidDataException">No message box images are installed for the tileset.</exception>
    public MessageBoxSheet MessageBox(TilesetDefinition tileset) =>
        MessageBoxes.TryGetValue(tileset, out MessageBoxSheet? messages) ? messages
            : throw new InvalidDataException($"No message box images are installed for {tileset}.");

    /// <summary>The effect overlay picture sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public WrittenPictureSheet FxOverlay(TilesetDefinition tileset) =>
        FxOverlayPictures.TryGetValue(tileset, out WrittenPictureSheet? pictures) ? pictures : throw new InvalidDataException($"No effect overlay picture sheet is installed for {tileset}.");

    /// <summary>The HUD picture sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public HudPictureSheet Hud(TilesetDefinition tileset) =>
        HudPictures.TryGetValue(tileset, out HudPictureSheet? pictures) ? pictures : throw new InvalidDataException($"No HUD picture sheet is installed for {tileset}.");

    /// <summary>
    /// The image of library background <paramref name="background"/> in a room of <paramref name="tileset"/>: the picture
    /// <paramref name="door"/> shows when it is one of the background's picture doors, otherwise, for no door, the ordinary one.
    /// </summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    public RoomBackgroundImage Background(LibraryBackground background, DoorDefinition? door, TilesetDefinition tileset) =>
        Named(RoomBackgroundImage.NameOf(background, door, tileset));

    /// <summary>The image of scrolling sky <paramref name="variant"/>'s rows in a room of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    public RoomBackgroundImage Sky(ScrollingSkyChunkTable variant, TilesetDefinition tileset) =>
        Named(RoomBackgroundImage.SkyNameOf(variant, tileset));

    /// <summary>The background image installed as <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    private RoomBackgroundImage Named(string name) =>
        Backgrounds.TryGetValue(name, out RoomBackgroundImage? background) ? background
            : throw new InvalidDataException($"No room background image {name} is installed.");

    /// <summary>The block sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public RoomBlockSheet Get(TilesetDefinition tileset) =>
        Sheets.TryGetValue(tileset, out RoomBlockSheet? sheet) ? sheet : throw new InvalidDataException($"No block sheet is installed for {tileset}.");
}
