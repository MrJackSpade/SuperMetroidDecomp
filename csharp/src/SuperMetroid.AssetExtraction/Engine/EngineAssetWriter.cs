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
        ("Ridley's wings at Ceres", writer => writer.WriteCeresGetawayWings()),
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
        // The looks of the rooms' enemies after everything that writes an enemy.
        ("the looks of the rooms' enemies", writer => writer.WriteRoomLooks()),
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
    /// <remarks>
    /// One drawn from the sheet of its own name that the old game draws in colours of the room's own, a look for some
    /// rooms and another for others, has a set of its pictures for each look, in a folder named as its own with an
    /// underscore and the look's word after it, which its timelines name by <see cref="LookPlaceholder"/>; the room
    /// places it with the look it is drawn in there, as <see cref="WriteRoomLooks"/> writes for the room generator.
    /// </remarks>
    /// <exception cref="InvalidDataException">Two of its looks differ in more than their colours.</exception>
    internal EngineEnemy Enemy(string file, string type, EnemyDefinitionId id, params ReadOnlySpan<string> sheets)
    {
        string folder = file[..file.LastIndexOf('.')];
        if (sheets.Length > 0)
        {
            var named = new List<string>();
            foreach (string sheet in sheets)
                named.Add(_sheets.EnemySheet(sheet));
            return Enemy(file, type, id, named, new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [folder] = named }, cutsPictures: true);
        }

        string enemy = EnemyDefinitionIdKeys.Installed.Of(id);
        IReadOnlyList<EngineEnemyLook> looks = _sheets.EnemyLooks(enemy);
        if (looks.Count == 1)
            return Enemy(file, type, id, [looks[0].Sheet], new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [folder] = [looks[0].Sheet] }, cutsPictures: true);

        // Its pictures are shown only from its looks' folders, so they are laid out under its own name and not cut there.
        var places = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (EngineEnemyLook look in looks)
            places[$"{folder}_{look.Word}"] = [look.Sheet];
        EngineEnemy drawn = Enemy(file, type, id, [looks[0].Sheet], places, cutsPictures: false);
        CutLooks(folder, enemy, looks, drawn.Pictures);
        return drawn.PictureSets(LookPlaceholder);
    }

    /// <summary>
    /// Cuts a set of the pictures of <paramref name="enemy"/>, an old enemy drawn in more than one look, for each of
    /// its looks, into the folder named <paramref name="folder"/> with an underscore and the look's word after it, and
    /// keeps which look each room's enemy population draws it in, for <see cref="WriteRoomLooks"/>.
    /// </summary>
    /// <param name="folder">The folder its pictures are named under where they name no look.</param>
    /// <param name="enemy">The old enemy, by the installation's name for it.</param>
    /// <param name="looks">Its looks, as the installation lists them.</param>
    /// <param name="laidOut">Its pictures as one of its looks lays them out, which every look's set must match.</param>
    /// <exception cref="InvalidDataException">
    /// Two of its looks differ in more than their colours, or the rooms of one population draw it in two looks.
    /// </exception>
    private void CutLooks(string folder, string enemy, IReadOnlyList<EngineEnemyLook> looks, EngineSheetPictures laidOut)
    {
        foreach (EngineEnemyLook look in looks)
        {
            EngineSheetPictures set = _sheets.Cut(_tree, GameInstallationLayout.EnemySheetDirectoryName, $"{folder}_{look.Word}", look.Sheet);
            if (!set.LaidOutAs(laidOut))
                throw new InvalidDataException($"The look {look.Word} of {enemy} ({look.Sheet}) differs from its look {looks[0].Word} ({looks[0].Sheet}) in more than its colours.");
            foreach (string population in look.Populations)
            {
                if (!_roomLooks.TryGetValue(population, out SortedDictionary<string, string>? drawnThere))
                    _roomLooks[population] = drawnThere = new SortedDictionary<string, string>(StringComparer.Ordinal);
                if (drawnThere.TryGetValue(enemy, out string? had) && had != look.Word)
                    throw new InvalidDataException($"The rooms of {population} draw {enemy} in the looks {had} and {look.Word}.");
                drawnThere[enemy] = look.Word;
            }
        }
    }

    /// <summary>
    /// Starts the data file of an enemy that only the rooms whose enemy populations are <paramref name="populations"/>
    /// place: its pictures are cut from the sheet those rooms draw it from, whatever other rooms draw it in.
    /// </summary>
    /// <exception cref="InvalidDataException">The rooms do not all draw it, or draw it in more than one look.</exception>
    internal EngineEnemy EnemyPlacedIn(string file, string type, EnemyDefinitionId id, IEnumerable<string> populations)
    {
        string sheet = _sheets.EnemySheetIn(EnemyDefinitionIdKeys.Installed.Of(id), populations);
        return Enemy(file, type, id, [sheet], new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal) { [file[..file.LastIndexOf('.')]] = [sheet] }, cutsPictures: true);
    }

    /// <summary>What a picture's name holds where the word of the look it is shown in goes, after the enemy's folder and an underscore.</summary>
    private const string LookPlaceholder = "_{look}";

    /// <summary>The data file the room generator reads the looks of the rooms' enemies from.</summary>
    private const string RoomLooksFile = "enemies/looks_by_room.json";

    /// <summary>For each room's enemy population, the word of the look each enemy drawn in a look of the room's own is drawn in there.</summary>
    private readonly SortedDictionary<string, SortedDictionary<string, string>> _roomLooks = new(StringComparer.Ordinal);

    /// <summary>
    /// Writes, for the room generator, which look each room's enemy population draws each enemy in that is drawn in a
    /// look of the room's own: by the population's name, each such enemy by its name in the old game's list of
    /// enemies, with the word of its look.
    /// </summary>
    private void WriteRoomLooks()
    {
        var written = new System.Text.Json.Nodes.JsonObject();
        foreach ((string population, SortedDictionary<string, string> drawn) in _roomLooks)
        {
            var enemies = new System.Text.Json.Nodes.JsonObject();
            foreach ((string enemy, string word) in drawn)
                enemies[enemy] = word;
            written[population] = enemies;
        }

        _tree.WriteData(RoomLooksFile, written);
    }

    /// <summary>
    /// Starts an enemy's data file whose pictures are cut from the sheets <paramref name="named"/> into a folder beside
    /// the file named as the file is, with the colours of its hurt flash and of its being frozen for the places
    /// <paramref name="places"/> its pictures are shown from. Where <paramref name="cutsPictures"/> is not asked, they
    /// are only laid out there, for one shown from other folders alike.
    /// </summary>
    private EngineEnemy Enemy(
        string file, string type, EnemyDefinitionId id, IReadOnlyList<string> named, IReadOnlyDictionary<string, IReadOnlyList<string>> places, bool cutsPictures)
    {
        const string family = GameInstallationLayout.EnemySheetDirectoryName;
        string folder = file[..file.LastIndexOf('.')];
        EngineSheetPictures pictures = cutsPictures ? _sheets.Cut(_tree, family, folder, [.. named]) : _sheets.LayOut(family, folder, [.. named]);
        var enemy = new EngineEnemy(_tree, _sheets, file, type, id, pictures, places);

        // The colours it is shown in on the updates its hurt flash shows: the cartridge's one flash shade over its
        // own pictures, each of their colours as a pale grey or white.
        System.Text.Json.Nodes.JsonObject flash = ShadeChange(SuperMetroid.Core.Enemies.Drawing.EnemyShades.HurtFlash.Key, family, places);

        // One whose sheets give the flash no colours of its own (a statue, a Torizo, which show a hurt their own way)
        // says nothing, and is shown brighter by the engine.
        if (flash.Any(place => place.Value is System.Text.Json.Nodes.JsonObject { Count: > 0 }))
            enemy.Group("hurt_colours", flash);

        // The colours it is shown in while it shows frozen: the cartridge's one frozen shade over its own pictures.
        // One whose sheets have no such shade, or give it no colours of its own, says nothing, and is tinted by the
        // engine if it is ever frozen.
        string frozenShade = SuperMetroid.Core.Enemies.Drawing.EnemyShades.Frozen.Key;
        if (places.Values.SelectMany(sheets => sheets).All(sheet => _sheets.HasShade(family, sheet, frozenShade)))
        {
            System.Text.Json.Nodes.JsonObject frozen = ShadeChange(frozenShade, family, places);
            if (frozen.Any(place => place.Value is System.Text.Json.Nodes.JsonObject { Count: > 0 }))
                enemy.Group("frozen_colours", frozen);
        }
        return enemy;
    }
}
