using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Writes the Godot game's assets from an installation: its pictures cut from the installed sheets, and its data
/// files, whose values and timelines come from the old game's own definitions. The Godot project names every file
/// it wants; what is written here is written under those names.
/// </summary>
public sealed partial class EngineAssetWriter
{
    private readonly EngineAssetTree _tree;
    private readonly EngineSheets _sheets;
    private readonly GameInstallation _installation;
    private EngineRoomObjects? _roomObjects;

    /// <summary>The pictures of the things drawn with a room's own blocks; there once the layouts have said which sets of graphics there are.</summary>
    private EngineRoomObjects RoomObjects => _roomObjects ?? throw new InvalidOperationException("The room objects' pictures are asked for before the layouts are written.");

    private EngineAssetWriter(GameInstallation installation, string assets, Action<int>? written)
    {
        _tree = new EngineAssetTree(assets, written);
        _sheets = new EngineSheets(installation.ContentDirectory);
        _installation = installation;
    }

    /// <summary>
    /// The parts the assets are written in, in the order they are written, each with what a watcher is told is being
    /// written.
    /// </summary>
    private static readonly (string Writing, Action<EngineAssetWriter> Write)[] Parts =
    [
        ("what the rooms share", writer => writer.WriteShared()),
        ("the rooms' tiles", writer => writer.BuildLayouts()),
        ("the enemies", writer => writer.WriteEnemies()),
        ("the doors' caps", writer => writer.WriteDoorCaps()),
        ("the gates", writer => writer.WriteGates()),
        ("the stations", writer => writer.WriteStations()),
        ("the items", writer => writer.WriteItems()),
        ("the rooms' objects", writer => writer.WriteObjects()),
        ("the animals", writer => writer.WriteAnimals()),
        ("the glass tube", writer => writer.WriteGlassTube()),
        ("Spore Spawn", writer => writer.WriteSporeSpawn()),
        ("Kraid", writer => writer.WriteKraid()),
        ("Crocomire", writer => writer.WriteCrocomire()),
        ("Botwoon", writer => writer.WriteBotwoon()),
        ("the baby Metroid", writer => writer.WriteBabyMetroid()),
        ("Mother Brain", writer => writer.WriteMotherBrain()),
        ("the Yard", writer => writer.WriteYard()),
        ("the Magdollite", writer => writer.WriteMagdollite()),
        ("Shaktool", writer => writer.WriteShaktool()),
        ("the Nuclear Waffle", writer => writer.WriteNuclearWaffle()),
        ("Kago's bugs", writer => writer.WriteKagoBug()),
        ("the eye doors", writer => writer.WriteEyeDoorEye()),
        ("the Chozo statues", writer => writer.WriteChozoStatues()),
        ("the Torizos", writer => writer.WriteTorizos()),
        ("Phantoon", writer => writer.WritePhantoon()),
        ("Draygon", writer => writer.WriteDraygon()),
        ("Ridley", writer => writer.WriteRidley()),
        ("Samus", writer => writer.WriteSamus()),
        ("the rooms' patches", writer => writer.WriteRoomPatches()),
        ("the backdrops", writer => writer.WriteBackdrops()),
        ("the doors' lit colours", writer => writer.WriteDoorLitColours()),
        ("the statue hall", writer => writer.WriteStatueHall()),
        ("the liquids and overlays", writer => writer.WriteLiquidsAndOverlays()),
        ("Ceres's floors", writer => writer.WriteCeresFloors()),
        ("Ceres's debris", writer => writer.WriteCeresDebris()),
        ("the arrival at Ceres", writer => writer.WriteCeresArrival()),
        ("the Samus eaters", writer => writer.WriteSamusEaters()),
        ("the opening", writer => writer.WriteIntro()),
        ("Ceres's destruction", writer => writer.WriteCeresDestruction()),
        ("the ending", writer => writer.WriteEnding()),
        ("the status bar", writer => writer.WriteHud()),
        ("the menus", writer => writer.WriteMenus()),
        ("the pause screen", writer => writer.WritePause()),
        ("the choice of saved game", writer => writer.WriteFileSelect()),
        ("the options", writer => writer.WriteOptions()),
        ("the sounds and the music", writer => writer.WriteAudio()),
        ("the save map", writer => writer.WriteSaveMap()),
        ("the rooms' scrolling", writer => writer.WriteScrollLayouts()),
        ("the music sets", writer => writer.WriteMusicSets()),
        ("the words", writer => writer.WriteWords()),
        // The layouts after everything that gives them their patches.
        ("the rooms", writer => writer.Layouts.WriteAll()),
        // The rooms' colour effects read the rooms' pictures as they were cut, and give nothing to anything else.
        ("the rooms' colour effects", writer => writer.WriteRoomColourEffects()),
    ];

    /// <summary>How many parts the assets are written in: how many times <see cref="Write"/> tells its progress.</summary>
    public static int PartCount => Parts.Length;

    /// <summary>
    /// Writes the assets of the installation at <paramref name="installationRoot"/> into the directory
    /// <paramref name="assets"/>, which is the Godot project's <c>assets</c> folder.
    /// </summary>
    /// <param name="installationRoot">The installation the assets are written from.</param>
    /// <param name="assets">The folder they are written into.</param>
    /// <param name="progress">Told each part of the writing as it starts; null for a writing nobody watches.</param>
    /// <param name="written">Told how many files have been written, after each; null for a writing nobody watches.</param>
    public static void Write(string installationRoot, string assets, IProgress<string>? progress = null, Action<int>? written = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(assets);
        var writer = new EngineAssetWriter(new GameInstallation(installationRoot), assets, written);
        foreach ((string writing, Action<EngineAssetWriter> write) in Parts)
        {
            progress?.Report($"Writing {writing}…");
            write(writer);
        }
    }

    /// <summary>Reads the rooms' layouts, which say which sets of graphics there are, and readies the pictures of what is drawn with them.</summary>
    private void BuildLayouts()
    {
        var layouts = new EngineLayouts(_tree, _sheets, _installation.LoadRoomLevelStreams(), _installation.ContentDirectory);
        layouts.BuildAll();
        _layouts = layouts;
        _roomObjects = new EngineRoomObjects(_tree, _sheets, _installation.ContentDirectory, layouts.GraphicsSets);
    }

    /// <summary>
    /// Starts an enemy's data file: its pictures are cut from the installed sheet named for the old enemy, into a
    /// folder beside the file named as the file is.
    /// </summary>
    /// <param name="file">The data file, as the Godot project names it.</param>
    /// <param name="type">The Godot type that reads the file.</param>
    /// <param name="id">The old game's enemy.</param>
    /// <param name="sheets">
    /// The installed sheets its pictures are on, where they are not the one sheet named as the old enemy is: the
    /// sheets of each of its parts.
    /// </param>
    internal EngineEnemy Enemy(string file, string type, EnemyDefinitionId id, params ReadOnlySpan<string> sheets)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        string folder = file[..file.LastIndexOf('.')];
        var named = new List<string>();
        foreach (string sheet in sheets)
            named.Add(_sheets.EnemySheet(sheet));
        if (named.Count == 0)
            named.Add(_sheets.EnemySheet(EnemyDefinitionIdKeys.Installed.Of(id)));
        return new EngineEnemy(_tree, _sheets, file, type, id, _sheets.Cut(_tree, family, folder, [.. named]));
    }
}
