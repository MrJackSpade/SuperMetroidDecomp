using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The installed pre-coloured room artwork (#1392, #1466): the block sheet of each tileset a room can load with the
/// appearance inputs its blocks depend on, the image of each library background a room state loads, and the sheets of the
/// pictures written over the rooms' backgrounds, and the turning floor of each Ceres tileset (#1467).
/// </summary>
public sealed class InstalledRoomArtwork
{
    /// <summary>
    /// Keeps the sheets, each under its own tileset, the background images, each under its own name, and the written
    /// picture sheets, each under its own tileset.
    /// </summary>
    /// <exception cref="ArgumentException">A sheet is filed under another tileset, or an image under another name.</exception>
    public InstalledRoomArtwork(IReadOnlyDictionary<TilesetDefinition, RoomBlockSheet> sheets,
        IReadOnlyDictionary<string, RoomBackgroundImage> backgrounds, IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> written,
        IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> bg3Pictures, IReadOnlyDictionary<TilesetDefinition, MessageBoxSheet> messageBoxes,
        IReadOnlyDictionary<TilesetDefinition, RoomFloorSheet> floors)
    {
        Floors = Ensure.NotNull(floors);
        foreach ((TilesetDefinition tileset, RoomFloorSheet floor) in floors)
        {
            if (floor.Tileset != tileset)
                throw new ArgumentException($"{floor.Name} is filed under {tileset}.", nameof(floors));
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
        Bg3Pictures = Ensure.NotNull(bg3Pictures);
        foreach ((TilesetDefinition tileset, WrittenPictureSheet pictures) in written.Concat(bg3Pictures))
        {
            if (pictures.Tileset != tileset)
                throw new ArgumentException($"{pictures.Name} is filed under {tileset}.", nameof(written));
        }
        if (written.Values.Any(pictures => pictures.Layer != WrittenPictureLayer.Bg12) || bg3Pictures.Values.Any(pictures => pictures.Layer != WrittenPictureLayer.Bg3))
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

    /// <summary>The sheets of pictures written over BG1 and BG2, by the tileset whose rooms the pictures are written in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> Written { get; }

    /// <summary>The sheets of the HUD's and room effects' BG3 pictures, by the tileset whose colours they show in.</summary>
    public IReadOnlyDictionary<TilesetDefinition, WrittenPictureSheet> Bg3Pictures { get; }

    /// <summary>The turning floor of each Ceres tileset.</summary>
    public IReadOnlyDictionary<TilesetDefinition, RoomFloorSheet> Floors { get; }

    /// <summary>The turning floor of Ceres tileset <paramref name="tileset"/>.</summary>
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

    /// <summary>The BG3 picture sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public WrittenPictureSheet Bg3(TilesetDefinition tileset) =>
        Bg3Pictures.TryGetValue(tileset, out WrittenPictureSheet? pictures) ? pictures : throw new InvalidDataException($"No BG3 picture sheet is installed for {tileset}.");

    /// <summary>A content identity of every sheet, in tileset order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(InstalledRoomArtwork), content =>
    {
        foreach ((TilesetDefinition tileset, RoomBlockSheet sheet) in Sheets.OrderBy(entry => RoomBlockSheet.NameOf(entry.Key), StringComparer.Ordinal))
        {
            content.AppendName("tileset", RoomBlockSheet.NameOf(tileset));
            sheet.Sheet.AppendIdentity(content);
            sheet.Inputs.AppendIdentity(content);
        }
        foreach ((string name, RoomBackgroundImage background) in Backgrounds.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            content.AppendName("background", name);
            background.Sheet.AppendIdentity(content);
            foreach (RoomBackgroundCell cell in background.Cells)
            {
                content.Append("cell", cell.Cell);
                content.AppendName("picture", cell.Picture);
                AppendInputs(content, cell.Inputs);
            }
        }
        foreach (RoomFloorSheet floor in Floors.Values.OrderBy(floor => floor.Name, StringComparer.Ordinal))
        {
            content.AppendName("floor", floor.Name);
            floor.Sheet.AppendIdentity(content);
        }
        foreach (MessageBoxSheet messages in MessageBoxes.Values.OrderBy(messages => messages.Name, StringComparer.Ordinal))
        {
            content.AppendName("messages", messages.Name);
            messages.Sheet.AppendIdentity(content);
        }
        foreach (WrittenPictureSheet pictures in Written.Values.Concat(Bg3Pictures.Values).OrderBy(pictures => pictures.Name, StringComparer.Ordinal))
        {
            content.AppendName("written", pictures.Name);
            pictures.Sheet.AppendIdentity(content);
            foreach ((string picture, ImmutableArray<AppearanceInput> inputs) in pictures.Inputs.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                content.AppendName("picture", picture);
                AppendInputs(content, inputs);
            }
        }
    });

    /// <summary>Appends the tokens of <paramref name="inputs"/> to a content identity.</summary>
    private static void AppendInputs(SelectedPresentationHash content, ImmutableArray<AppearanceInput> inputs)
    {
        content.Append("inputs", inputs.Length);
        foreach (AppearanceInput input in inputs)
            content.AppendName("input", input.Token);
    }

    /// <summary>The library background image named <paramref name="name"/>.</summary>
    /// <exception cref="InvalidDataException">No such image is installed.</exception>
    public RoomBackgroundImage Background(string name) =>
        Backgrounds.TryGetValue(name, out RoomBackgroundImage? background) ? background
            : throw new InvalidDataException($"No room background image {name} is installed.");

    /// <summary>The block sheet of <paramref name="tileset"/>.</summary>
    /// <exception cref="InvalidDataException">No sheet is installed for the tileset.</exception>
    public RoomBlockSheet Get(TilesetDefinition tileset) =>
        Sheets.TryGetValue(tileset, out RoomBlockSheet? sheet) ? sheet : throw new InvalidDataException($"No block sheet is installed for {tileset}.");
}
