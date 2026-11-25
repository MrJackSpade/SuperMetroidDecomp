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

    private EngineAssetWriter(GameInstallation installation, string assets)
    {
        _tree = new EngineAssetTree(assets);
        _sheets = new EngineSheets(installation.ContentDirectory);
        _installation = installation;
    }

    /// <summary>
    /// Writes the assets of the installation at <paramref name="installationRoot"/> into the directory
    /// <paramref name="assets"/>, which is the Godot project's <c>assets</c> folder.
    /// </summary>
    public static void Write(string installationRoot, string assets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(assets);
        var installation = new GameInstallation(installationRoot);
        var writer = new EngineAssetWriter(installation, assets);
        writer.WriteShared();
        var layouts = new EngineLayouts(writer._tree, writer._sheets, installation.LoadRoomLevelStreams(), installation.ContentDirectory);
        layouts.BuildAll();
        writer._layouts = layouts;
        writer._roomObjects = new EngineRoomObjects(writer._tree, writer._sheets, installation.ContentDirectory, layouts.GraphicsSets);
        writer.WriteEnemies();
        writer.WriteDoorCaps();
        writer.WriteGates();
        writer.WriteStations();
        writer.WriteItems();
        writer.WriteObjects();
        writer.WriteAnimals();
        writer.WriteGlassTube();
        writer.WriteSporeSpawn();
        writer.WriteKraid();
        writer.WriteCrocomire();
        writer.WriteBotwoon();
        writer.WriteBabyMetroid();
        writer.WriteMotherBrain();
        writer.WriteYard();
        writer.WriteMagdollite();
        writer.WriteShaktool();
        writer.WriteNuclearWaffle();
        writer.WriteKagoBug();
        writer.WriteEyeDoorEye();
        writer.WriteChozoStatues();
        writer.WriteTorizos();
        writer.WritePhantoon();
        writer.WriteDraygon();
        writer.WriteRidley();
        writer.WriteSamus();
        writer.WriteRoomPatches();
        writer.WriteBackdrops();
        writer.WriteDoorLitColours();
        writer.WriteStatueHall();
        writer.WriteLiquidsAndOverlays();
        writer.WriteCeresFloors();
        writer.WriteCeresDebris();
        writer.WriteCeresArrival();
        writer.WriteSamusEaters();
        writer.WriteIntro();
        writer.WriteCeresDestruction();
        writer.WriteEnding();
        writer.WriteHud();
        writer.WriteMenus();
        writer.WritePause();
        writer.WriteFileSelect();
        writer.WriteOptions();
        writer.WriteAudio();
        writer.WriteSaveMap();
        writer.WriteScrollLayouts();
        writer.WriteMusicSets();
        writer.WriteWords();
        // The layouts after everything that gives them their patches.
        layouts.WriteAll();

        // The rooms' colour effects read the rooms' pictures as they were cut, and give nothing to anything else.
        writer.WriteRoomColourEffects();
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
