using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>App-owned immutable cartridge/audio content, separate from persistent player data.</summary>
/// <param name="Root">Installation root containing replaceable <c>game</c> content and persistent <c>overrides</c>; constructing this record does not create or validate it.</param>
/// <remarks>
/// Directory properties compute paths without accessing the filesystem. Stock loaders validate
/// the installed formats and source manifests before applying compatible replacements from
/// the override tree. Missing files, unreadable paths, and malformed stock or selected override
/// content propagate the corresponding I/O or invalid-data exception to the caller.
/// </remarks>
public sealed record GameInstallation(string Root)
{
    /// <summary>Replaceable stock-content tree published by the installer beneath the installation root.</summary>
    public string ContentDirectory => Path.Combine(Root, GameInstallationLayout.ContentDirectoryName);
    /// <summary>Private validated cartridge copy used for extraction and repair; gameplay can open complete extracted content without it.</summary>
    public string RomPath => Path.Combine(ContentDirectory, GameInstallationLayout.RomFileName);
    /// <summary>
    /// Opens mutable game memory only after validating the extracted installation.
    /// Gameplay never maps a cartridge image, even when the private import copy exists.
    /// </summary>
    public SuperMetroidAddressSpace OpenRuntimeAddressSpace()
    {
        if (GameAssetInstaller.TryOpenExtractedContent(Root) is null)
            throw new InvalidDataException(
                "Extracted game content is incomplete or invalid.");
        return SuperMetroidAddressSpace.CreateWithoutCartridge();
    }
    /// <summary>Stock audio files and manifests under the replaceable content tree.</summary>
    public string AudioDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.AudioDirectoryName);
    /// <summary>Persistent editable audio content, outside the replaceable stock installation.</summary>
    public string AudioOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.AudioDirectoryName);
    /// <summary>Validates stock content, then selects a complete compatible user override when present.</summary>
    public ExtractedAudioAssetCatalog LoadAudio() =>
        ExtractedAudioAssetCatalog.Load(AudioDirectory, AudioOverrideDirectory);
    /// <summary>Stock area-map artwork and exploration masks under the replaceable content tree.</summary>
    public string MapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.MapDirectoryName);
    /// <summary>Stock starting CGRAM and room-entry sprite palettes under the replaceable content tree.</summary>
    public string GameplayBasePaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.GameplayBasePaletteDirectoryName);
    /// <summary>Editable starting CGRAM and room-entry sprite colors survive stock rebuilds.</summary>
    public string GameplayBasePaletteOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.GameplayBasePaletteDirectoryName);
    /// <summary>Validates stock starting palettes and loads any compatible user replacements for initial CGRAM and room-entry sprite colors.</summary>
    public GameplayBasePaletteCatalog LoadGameplayBasePalettes() =>
        GameplayBasePaletteFiles.Load(GameplayBasePaletteDirectory, GameplayBasePaletteOverrideDirectory);
    /// <summary>Stock indexed common gameplay OBJ sheet and its source manifest.</summary>
    public string StandardObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.StandardObjectDirectoryName);
    /// <summary>Editable common gameplay OBJ sheet, preserved across stock rebuilds.</summary>
    public string StandardObjectOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.StandardObjectDirectoryName);
    /// <summary>Validates the stock common OBJ sheet and selects a compatible replacement PNG for the shared character transfer.</summary>
    public RoomCharacterAtlas LoadStandardObjects() =>
        StandardObjectArtworkFiles.Load(StandardObjectDirectory, StandardObjectOverrideDirectory);
    /// <summary>Stock projectile artwork and presentation selectors under the replaceable content tree.</summary>
    public string ProjectileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.ProjectileDirectoryName);
    /// <summary>Stock room character-tile atlases and their source manifests.</summary>
    public string RoomCharacterDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomCharacterDirectoryName);
    /// <summary>Stock Samus body PNGs and visual selectors under the replaceable content tree.</summary>
    public string SamusBodyDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.SamusBodyDirectoryName);
    /// <summary>Replacement Samus body PNGs and visual selectors survive stock rebuilds.</summary>
    public string SamusBodyOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.SamusBodyDirectoryName);
    /// <summary>Loads validated Samus body artwork and compatible replacement PNGs and selectors from the persistent override tree.</summary>
    public SamusBodyArtworkCatalog LoadSamusBodyArt() =>
        SamusBodyArtworkFiles.Load(SamusBodyDirectory, SamusBodyOverrideDirectory);
    /// <summary>Stock opening-cinematic artwork under the replaceable content tree.</summary>
    public string IntroCinematicDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.IntroCinematicDirectoryName);
    /// <summary>Opening-cinematic PNG edits survive stock content replacement.</summary>
    public string IntroCinematicOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.IntroCinematicDirectoryName);
    /// <summary>Loads validated opening-cinematic artwork, selecting compatible user PNG replacements where supplied.</summary>
    public IntroCinematicArtworkCatalog LoadIntroCinematicArt() =>
        IntroCinematicArtworkFiles.Load(IntroCinematicDirectory, IntroCinematicOverrideDirectory);
    /// <summary>Stock ending Mode 7 scene artwork under the replaceable content tree.</summary>
    public string EndingMode7Directory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingMode7DirectoryName);
    /// <summary>Ending scene art edits remain outside replaceable stock content.</summary>
    public string EndingMode7OverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingMode7DirectoryName);
    /// <summary>Loads validated ending Mode 7 artwork and compatible scene replacements from the persistent override tree.</summary>
    public EndingMode7ArtworkCatalog LoadEndingMode7Art() =>
        EndingMode7ArtworkFiles.Load(EndingMode7Directory, EndingMode7OverrideDirectory);
    /// <summary>Stock ending sprite artwork under the replaceable content tree.</summary>
    public string EndingObjectDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingObjectDirectoryName);
    /// <summary>Ending OBJ edits remain outside replaceable stock content.</summary>
    public string EndingObjectOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingObjectDirectoryName);
    /// <summary>Loads validated ending OBJ artwork and any compatible user replacements.</summary>
    public EndingObjectArtworkCatalog LoadEndingObjectArt() =>
        EndingObjectArtworkFiles.Load(EndingObjectDirectory, EndingObjectOverrideDirectory);
    /// <summary>Stock ending-scene color palettes under the replaceable content tree.</summary>
    public string EndingPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EndingPaletteDirectoryName);
    /// <summary>Ending color edits survive replacement of the stock installation.</summary>
    public string EndingPaletteOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EndingPaletteDirectoryName);
    /// <summary>Loads validated ending color palettes and compatible color replacements from the persistent override tree.</summary>
    public EndingPaletteCatalog LoadEndingPalettes() =>
        EndingPaletteArtworkFiles.Load(EndingPaletteDirectory, EndingPaletteOverrideDirectory);
    /// <summary>Editable room character art stays outside the replaceable stock game directory.</summary>
    public string RoomCharacterOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomCharacterDirectoryName);
    /// <summary>Loads validated room character atlases, selecting compatible user tile artwork for each supplied replacement.</summary>
    public RoomCharacterAtlasCatalog LoadRoomCharacters() =>
        RoomCharacterArtworkFiles.Load(RoomCharacterDirectory, RoomCharacterOverrideDirectory);
    /// <summary>Stock RGB5 room palettes and their source-hash manifest.</summary>
    public string RoomPaletteDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPaletteDirectoryName);
    /// <summary>Persistent RGB5 room palette replacements, preserved when stock content is rebuilt.</summary>
    public string RoomPaletteOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPaletteDirectoryName);
    /// <summary>Validates stock room palette hashes and loads each existing replacement palette keyed by its source identity.</summary>
    public RoomStaticPaletteCatalog LoadRoomPalettes() =>
        RoomStaticPaletteArtworkFiles.Load(RoomPaletteDirectory, RoomPaletteOverrideDirectory);
    /// <summary>Stock visual room-block compositions under the replaceable content tree.</summary>
    public string RoomMetatileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomMetatileDirectoryName);
    /// <summary>Editable visual block compositions survive stock content replacement.</summary>
    public string RoomMetatileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomMetatileDirectoryName);
    /// <summary>Loads validated visual metatile compositions and compatible user edits independently of collision and BTS data.</summary>
    public RoomMetatileCatalog LoadRoomMetatiles() =>
        RoomMetatileArtworkFiles.Load(RoomMetatileDirectory, RoomMetatileOverrideDirectory);
    /// <summary>Stock background tilemaps, including sky tilemaps, under the replaceable content tree.</summary>
    public string RoomBackgroundTilemapDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
    /// <summary>Stock visual room-block references under the replaceable content tree.</summary>
    public string RoomVisualLayoutDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomVisualLayoutDirectoryName);
    /// <summary>Persistent replacements for visual room-block references, preserved during stock rebuilds.</summary>
    public string RoomVisualLayoutOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomVisualLayoutDirectoryName);
    /// <summary>Visual room-block references independent of native collision and BTS.</summary>
    public RoomVisualLayoutCatalog LoadRoomVisualLayouts() =>
        RoomVisualLayoutFiles.Load(RoomVisualLayoutDirectory, RoomVisualLayoutOverrideDirectory);
    /// <summary>Stock definitions for shootable block appearances under the replaceable content tree.</summary>
    public string RoomPlmShotBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName);
    /// <summary>Persistent replacements for shootable block appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmShotBlockVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmShotBlockVisualDirectoryName);
    /// <summary>Editable shot-block appearances; PLM timing, placement, and collision remain compiled.</summary>
    public RoomPlmShotBlockVisualCatalog LoadRoomPlmShotBlockVisuals() =>
        RoomPlmShotBlockVisualFiles.Load(RoomPlmShotBlockVisualDirectory, RoomPlmShotBlockVisualOverrideDirectory);
    /// <summary>Stock definitions for Grapple-reactive block appearances under the replaceable content tree.</summary>
    public string RoomPlmGrappleBlockVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName);
    /// <summary>Persistent replacements for Grapple-reactive block appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmGrappleBlockVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmGrappleBlockVisualDirectoryName);
    /// <summary>Editable Grapple-block appearances; timing, placement, and collision remain compiled.</summary>
    public RoomPlmGrappleBlockVisualCatalog LoadRoomPlmGrappleBlockVisuals() =>
        RoomPlmGrappleBlockVisualFiles.Load(RoomPlmGrappleBlockVisualDirectory, RoomPlmGrappleBlockVisualOverrideDirectory);
    /// <summary>Stock definitions for save, refill, and map station appearances under the replaceable content tree.</summary>
    public string RoomPlmStationVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmStationVisualDirectoryName);
    /// <summary>Persistent replacements for save, refill, and map station appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmStationVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmStationVisualDirectoryName);
    /// <summary>Editable station appearances; activation, rewards, and collision remain compiled.</summary>
    public RoomPlmStationVisualCatalog LoadRoomPlmStationVisuals() =>
        RoomPlmStationVisualFiles.Load(RoomPlmStationVisualDirectory, RoomPlmStationVisualOverrideDirectory);
    /// <summary>Stock definitions for blue-door cap frames under the replaceable content tree.</summary>
    public string RoomPlmBlueDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName);
    /// <summary>Persistent replacements for blue-door cap frames, preserved during stock rebuilds.</summary>
    public string RoomPlmBlueDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBlueDoorVisualDirectoryName);
    /// <summary>Editable blue-door cap art; opening timing and physical tiles remain compiled.</summary>
    public RoomPlmBlueDoorVisualCatalog LoadRoomPlmBlueDoorVisuals() =>
        RoomPlmBlueDoorVisualFiles.Load(RoomPlmBlueDoorVisualDirectory,
            RoomPlmBlueDoorVisualOverrideDirectory);
    /// <summary>Stock definitions for colored-door cap frames under the replaceable content tree.</summary>
    public string RoomPlmColoredDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName);
    /// <summary>Persistent replacements for colored-door cap frames, preserved during stock rebuilds.</summary>
    public string RoomPlmColoredDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmColoredDoorVisualDirectoryName);
    /// <summary>Editable colored-door cap art; hit rules and physical tiles remain compiled.</summary>
    public RoomPlmColoredDoorVisualCatalog LoadRoomPlmColoredDoorVisuals() =>
        RoomPlmColoredDoorVisualFiles.Load(RoomPlmColoredDoorVisualDirectory,
            RoomPlmColoredDoorVisualOverrideDirectory);
    /// <summary>Stock definitions for grey-door caps and shared clear frames under the replaceable content tree.</summary>
    public string RoomPlmGreyDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName);
    /// <summary>Persistent replacements for grey-door caps and shared clear frames, preserved during stock rebuilds.</summary>
    public string RoomPlmGreyDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmGreyDoorVisualDirectoryName);
    /// <summary>Editable grey caps and shared clear frames; physical door rules stay compiled.</summary>
    public RoomPlmGreyDoorVisualCatalog LoadRoomPlmGreyDoorVisuals() =>
        RoomPlmGreyDoorVisualFiles.Load(RoomPlmGreyDoorVisualDirectory,
            RoomPlmGreyDoorVisualOverrideDirectory);
    /// <summary>Stock definitions for the three eye-door components' appearances under the replaceable content tree.</summary>
    public string RoomPlmEyeDoorVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName);
    /// <summary>Persistent replacements for the three eye-door components' appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmEyeDoorVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmEyeDoorVisualDirectoryName);
    /// <summary>Editable eye-door blocks; the three PLM components retain compiled behavior.</summary>
    public RoomPlmEyeDoorVisualCatalog LoadRoomPlmEyeDoorVisuals() =>
        RoomPlmEyeDoorVisualFiles.Load(RoomPlmEyeDoorVisualDirectory,
            RoomPlmEyeDoorVisualOverrideDirectory);
    /// <summary>Stock definitions for Mother Brain glass appearances under the replaceable content tree.</summary>
    public string RoomPlmMotherBrainGlassVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName);
    /// <summary>Persistent replacements for Mother Brain glass appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmMotherBrainGlassVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMotherBrainGlassVisualDirectoryName);
    /// <summary>Editable glass appearance; shatter physics and room-object logic stay compiled.</summary>
    public RoomPlmMotherBrainGlassVisualCatalog LoadRoomPlmMotherBrainGlassVisuals() =>
        RoomPlmMotherBrainGlassVisualFiles.Load(RoomPlmMotherBrainGlassVisualDirectory,
            RoomPlmMotherBrainGlassVisualOverrideDirectory);
    /// <summary>Stock definitions for intact and broken n00b-tube appearances under the replaceable content tree.</summary>
    public string RoomPlmNoobTubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName);
    /// <summary>Persistent replacements for intact and broken n00b-tube appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmNoobTubeVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmNoobTubeVisualDirectoryName);
    /// <summary>Editable n00b-tube appearance; break logic and physical blocks stay compiled.</summary>
    public RoomPlmNoobTubeVisualCatalog LoadRoomPlmNoobTubeVisuals() =>
        RoomPlmNoobTubeVisualFiles.Load(RoomPlmNoobTubeVisualDirectory,
            RoomPlmNoobTubeVisualOverrideDirectory);
    /// <summary>Stock definitions for downward gate-block appearances under the replaceable content tree.</summary>
    public string RoomPlmDownwardGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName);
    /// <summary>Persistent replacements for downward gate-block appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmDownwardGateVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmDownwardGateVisualDirectoryName);
    /// <summary>Editable gate-block art; activation, animation, and collision remain compiled.</summary>
    public RoomPlmDownwardGateVisualCatalog LoadRoomPlmDownwardGateVisuals() =>
        RoomPlmDownwardGateVisualFiles.Load(RoomPlmDownwardGateVisualDirectory,
            RoomPlmDownwardGateVisualOverrideDirectory);
    /// <summary>Stock definitions for elevator-platform frames under the replaceable content tree.</summary>
    public string RoomPlmElevatorPlatformVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName);
    /// <summary>Persistent replacements for elevator-platform frames, preserved during stock rebuilds.</summary>
    public string RoomPlmElevatorPlatformVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmElevatorPlatformVisualDirectoryName);
    /// <summary>Editable elevator frames; physical blocks and the instruction loop stay compiled.</summary>
    public RoomPlmElevatorPlatformVisualCatalog LoadRoomPlmElevatorPlatformVisuals() =>
        RoomPlmElevatorPlatformVisualFiles.Load(RoomPlmElevatorPlatformVisualDirectory,
            RoomPlmElevatorPlatformVisualOverrideDirectory);
    /// <summary>Stock definitions for closing escape-gate appearances under the replaceable content tree.</summary>
    public string RoomPlmEscapeGateVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName);
    /// <summary>Persistent replacements for closing escape-gate appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmEscapeGateVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmEscapeGateVisualDirectoryName);
    /// <summary>Editable escape-gate appearance; closing and collision stay compiled.</summary>
    public RoomPlmEscapeGateVisualCatalog LoadRoomPlmEscapeGateVisuals() =>
        RoomPlmEscapeGateVisualFiles.Load(RoomPlmEscapeGateVisualDirectory,
            RoomPlmEscapeGateVisualOverrideDirectory);
    /// <summary>Stock definitions for Bomb Torizo hand appearances under the replaceable content tree.</summary>
    public string RoomPlmBombTorizoHandVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName);
    /// <summary>Persistent replacements for Bomb Torizo hand appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmBombTorizoHandVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBombTorizoHandVisualDirectoryName);
    /// <summary>Editable Bomb Torizo hand art; its physical block words stay compiled.</summary>
    public RoomPlmBombTorizoHandVisualCatalog LoadRoomPlmBombTorizoHandVisuals() =>
        RoomPlmBombTorizoHandVisualFiles.Load(RoomPlmBombTorizoHandVisualDirectory,
            RoomPlmBombTorizoHandVisualOverrideDirectory);
    /// <summary>Stock definitions for reachable Draygon cannon appearances under the replaceable content tree.</summary>
    public string RoomPlmDraygonCannonVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName);
    /// <summary>Persistent replacements for reachable Draygon cannon appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmDraygonCannonVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmDraygonCannonVisualDirectoryName);
    /// <summary>Editable reachable cannon art; physical blocks remain compiled.</summary>
    public RoomPlmDraygonCannonVisualCatalog LoadRoomPlmDraygonCannonVisuals() =>
        RoomPlmDraygonCannonVisualFiles.Load(RoomPlmDraygonCannonVisualDirectory,
            RoomPlmDraygonCannonVisualOverrideDirectory);
    /// <summary>Stock definitions for Chozo hand and slope-access appearances under the replaceable content tree.</summary>
    public string RoomPlmChozoStatueVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName);
    /// <summary>Persistent replacements for Chozo hand and slope-access appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmChozoStatueVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmChozoStatueVisualDirectoryName);
    /// <summary>Editable Chozo hand and slope-access art; physical blocks remain compiled.</summary>
    public RoomPlmChozoStatueVisualCatalog LoadRoomPlmChozoStatueVisuals() =>
        RoomPlmChozoStatueVisualFiles.Load(RoomPlmChozoStatueVisualDirectory,
            RoomPlmChozoStatueVisualOverrideDirectory);
    /// <summary>Stock definitions for linked-block restoration appearances under the replaceable content tree.</summary>
    public string RoomPlmLinkedRestoreVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName);
    /// <summary>Persistent replacements for linked-block restoration appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmLinkedRestoreVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmLinkedRestoreVisualDirectoryName);
    /// <summary>Editable linked-block restoration art; collision stays compiled.</summary>
    public RoomPlmLinkedRestoreVisualCatalog LoadRoomPlmLinkedRestoreVisuals() =>
        RoomPlmLinkedRestoreVisualFiles.Load(RoomPlmLinkedRestoreVisualDirectory,
            RoomPlmLinkedRestoreVisualOverrideDirectory);
    /// <summary>Stock definitions for Tourian access-floor appearances under the replaceable content tree.</summary>
    public string RoomPlmTourianAccessVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName);
    /// <summary>Persistent replacements for Tourian access-floor appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmTourianAccessVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmTourianAccessVisualDirectoryName);
    /// <summary>Editable Tourian access-floor art; physical rows and timing stay compiled.</summary>
    public RoomPlmTourianAccessVisualCatalog LoadRoomPlmTourianAccessVisuals() =>
        RoomPlmTourianAccessVisualFiles.Load(RoomPlmTourianAccessVisualDirectory,
            RoomPlmTourianAccessVisualOverrideDirectory);
    /// <summary>Stock definitions for the bomb-revealed Speed Booster tile under the replaceable content tree.</summary>
    public string RoomPlmSpeedBoosterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName);
    /// <summary>Persistent replacements for the bomb-revealed Speed Booster tile, preserved during stock rebuilds.</summary>
    public string RoomPlmSpeedBoosterVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmSpeedBoosterVisualDirectoryName);
    /// <summary>Editable bomb-revealed Speed Booster tile; collision stays compiled.</summary>
    public RoomPlmSpeedBoosterVisualCatalog LoadRoomPlmSpeedBoosterVisuals() =>
        RoomPlmSpeedBoosterVisualFiles.Load(RoomPlmSpeedBoosterVisualDirectory,
            RoomPlmSpeedBoosterVisualOverrideDirectory);
    /// <summary>Stock definitions for the Maridia elevatube PLM tile under the replaceable content tree.</summary>
    public string RoomPlmMaridiaElevatubeVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName);
    /// <summary>Persistent replacements for the Maridia elevatube PLM tile, preserved during stock rebuilds.</summary>
    public string RoomPlmMaridiaElevatubeVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMaridiaElevatubeVisualDirectoryName);
    /// <summary>Editable Maridia elevatube PLM tile; its physical block stays compiled.</summary>
    public RoomPlmMaridiaElevatubeVisualCatalog LoadRoomPlmMaridiaElevatubeVisuals() =>
        RoomPlmMaridiaElevatubeVisualFiles.Load(RoomPlmMaridiaElevatubeVisualDirectory,
            RoomPlmMaridiaElevatubeVisualOverrideDirectory);
    /// <summary>Stock definitions for Spore Spawn ceiling tiles under the replaceable content tree.</summary>
    public string RoomPlmSporeSpawnCeilingVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName);
    /// <summary>Persistent replacements for Spore Spawn ceiling tiles, preserved during stock rebuilds.</summary>
    public string RoomPlmSporeSpawnCeilingVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmSporeSpawnCeilingVisualDirectoryName);
    /// <summary>Editable Spore Spawn ceiling tiles; physical draw words stay compiled.</summary>
    public RoomPlmSporeSpawnCeilingVisualCatalog LoadRoomPlmSporeSpawnCeilingVisuals() =>
        RoomPlmSporeSpawnCeilingVisualFiles.Load(RoomPlmSporeSpawnCeilingVisualDirectory,
            RoomPlmSporeSpawnCeilingVisualOverrideDirectory);
    /// <summary>Stock definitions for floor and ceiling plant tiles under the replaceable content tree.</summary>
    public string RoomPlmSamusEaterVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName);
    /// <summary>Persistent replacements for floor and ceiling plant tiles, preserved during stock rebuilds.</summary>
    public string RoomPlmSamusEaterVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmSamusEaterVisualDirectoryName);
    /// <summary>Editable floor/ceiling plant tiles; physical draw geometry stays compiled.</summary>
    public RoomPlmSamusEaterVisualCatalog LoadRoomPlmSamusEaterVisuals() =>
        RoomPlmSamusEaterVisualFiles.Load(RoomPlmSamusEaterVisualDirectory,
            RoomPlmSamusEaterVisualOverrideDirectory);
    /// <summary>Stock definitions for Botwoon wall-clear tiles under the replaceable content tree.</summary>
    public string RoomPlmBotwoonWallVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName);
    /// <summary>Persistent replacements for Botwoon wall-clear tiles, preserved during stock rebuilds.</summary>
    public string RoomPlmBotwoonWallVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmBotwoonWallVisualDirectoryName);
    /// <summary>Editable Botwoon wall-clear tiles; collision and crumble timing stay compiled.</summary>
    public RoomPlmBotwoonWallVisualCatalog LoadRoomPlmBotwoonWallVisuals() =>
        RoomPlmBotwoonWallVisualFiles.Load(RoomPlmBotwoonWallVisualDirectory,
            RoomPlmBotwoonWallVisualOverrideDirectory);
    /// <summary>Stock definitions for Kraid ceiling and spike appearances under the replaceable content tree.</summary>
    public string RoomPlmKraidVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmKraidVisualDirectoryName);
    /// <summary>Persistent replacements for Kraid ceiling and spike appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmKraidVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmKraidVisualDirectoryName);
    /// <summary>Editable Kraid ceiling/spike art; physical mutations stay compiled.</summary>
    public RoomPlmKraidVisualCatalog LoadRoomPlmKraidVisuals() =>
        RoomPlmKraidVisualFiles.Load(RoomPlmKraidVisualDirectory,
            RoomPlmKraidVisualOverrideDirectory);
    /// <summary>Stock definitions for Crocomire bridge and wall appearances under the replaceable content tree.</summary>
    public string RoomPlmCrocomireVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName);
    /// <summary>Persistent replacements for Crocomire bridge and wall appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmCrocomireVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmCrocomireVisualDirectoryName);
    /// <summary>Editable Crocomire bridge/wall art; collision and timing stay compiled.</summary>
    public RoomPlmCrocomireVisualCatalog LoadRoomPlmCrocomireVisuals() =>
        RoomPlmCrocomireVisualFiles.Load(RoomPlmCrocomireVisualDirectory,
            RoomPlmCrocomireVisualOverrideDirectory);
    /// <summary>Stock definitions for Mother Brain fake-death terrain appearances under the replaceable content tree.</summary>
    public string RoomPlmMotherBrainFakeDeathVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName);
    /// <summary>Persistent replacements for Mother Brain fake-death terrain appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmMotherBrainFakeDeathVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmMotherBrainFakeDeathVisualDirectoryName);
    /// <summary>Editable fake-death terrain art; physical room mutations remain compiled.</summary>
    public RoomPlmMotherBrainFakeDeathVisualCatalog LoadRoomPlmMotherBrainFakeDeathVisuals() =>
        RoomPlmMotherBrainFakeDeathVisualFiles.Load(
            RoomPlmMotherBrainFakeDeathVisualDirectory,
            RoomPlmMotherBrainFakeDeathVisualOverrideDirectory);
    /// <summary>Stock definitions for item, orb, and reveal appearances under the replaceable content tree.</summary>
    public string RoomPlmCollectibleVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName);
    /// <summary>Persistent replacements for item, orb, and reveal appearances, preserved during stock rebuilds.</summary>
    public string RoomPlmCollectibleVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomPlmCollectibleVisualDirectoryName);
    /// <summary>Editable item/orb/reveal appearances; pickup and collision remain compiled.</summary>
    public RoomPlmCollectibleVisualCatalog LoadRoomPlmCollectibleVisuals() =>
        RoomPlmCollectibleVisualFiles.Load(RoomPlmCollectibleVisualDirectory,
            RoomPlmCollectibleVisualOverrideDirectory);
    /// <summary>Stock collectible character PNGs and tile-palette selectors under the replaceable content tree.</summary>
    public string RoomPlmDynamicCollectibleArtDirectory => Path.Combine(ContentDirectory,
        GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName);
    /// <summary>Persistent collectible character PNG and tile-palette selector replacements, preserved during stock rebuilds.</summary>
    public string RoomPlmDynamicCollectibleArtOverrideDirectory => Path.Combine(Root,
        "overrides", GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName);
    /// <summary>Editable item character PNGs and tile-palette selectors.</summary>
    public RoomPlmDynamicCollectibleArtCatalog LoadRoomPlmDynamicCollectibleArt() =>
        RoomPlmDynamicCollectibleArtFiles.Load(RoomPlmDynamicCollectibleArtDirectory,
            RoomPlmDynamicCollectibleArtOverrideDirectory);
    /// <summary>Stock definitions for X-ray reveal metatile selections under the replaceable content tree.</summary>
    public string XrayRevealVisualDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.XrayRevealVisualDirectoryName);
    /// <summary>Persistent replacements for X-ray reveal metatile selections, preserved during stock rebuilds.</summary>
    public string XrayRevealVisualOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.XrayRevealVisualDirectoryName);
    /// <summary>Editable X-ray metatile choices; reveal commands and collision rules remain compiled.</summary>
    public XrayRevealVisualCatalog LoadXrayRevealVisuals() =>
        XrayRevealVisualFiles.Load(XrayRevealVisualDirectory, XrayRevealVisualOverrideDirectory);
    /// <summary>Editable BG tilemaps survive replacement of stock game content.</summary>
    public string RoomBackgroundTilemapOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.RoomBackgroundTilemapDirectoryName);
    /// <summary>Loads validated room background tilemaps and compatible replacements from the persistent override tree.</summary>
    public RoomBackgroundTilemapCatalog LoadRoomBackgroundTilemaps() =>
        RoomBackgroundTilemapArtworkFiles.Load(RoomBackgroundTilemapDirectory, RoomBackgroundTilemapOverrideDirectory);
    /// <summary>Loads validated sky tilemaps from the shared room-background directory and its compatible user replacements.</summary>
    public RoomSkyTilemapCatalog LoadRoomSkyTilemaps() =>
        RoomSkyTilemapArtworkFiles.Load(RoomBackgroundTilemapDirectory, RoomBackgroundTilemapOverrideDirectory);
    /// <summary>Persistent projectile artwork and presentation replacements, preserved during stock rebuilds.</summary>
    public string ProjectileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.ProjectileDirectoryName);
    /// <summary>Loads validated projectile presentation with compatible user artwork and visual-selector replacements.</summary>
    public InstalledProjectilePresentation LoadProjectiles() => ProjectilePresentationFiles.Load(ProjectileDirectory, ProjectileOverrideDirectory);
    /// <summary>Ordinary enemy tile sheets; replacement PNGs survive stock-content rebuilds.</summary>
    public string EnemyTileDirectory => Path.Combine(ContentDirectory, GameInstallationLayout.EnemyTileDirectoryName);
    /// <summary>Persistent ordinary-enemy PNG replacements, preserved during stock rebuilds.</summary>
    public string EnemyTileOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.EnemyTileDirectoryName);
    /// <summary>Loads validated ordinary-enemy tile sheets and compatible replacement PNGs from the persistent override tree.</summary>
    public EnemyTileArtworkCatalog LoadEnemyTiles() =>
        EnemyTileArtworkFiles.Load(EnemyTileDirectory, EnemyTileOverrideDirectory);
    /// <summary>Outside the replaceable game directory: reinstall and stock repair preserve these edits.</summary>
    public string MapOverrideDirectory => Path.Combine(Root, "overrides", GameInstallationLayout.MapDirectoryName);

    /// <summary>Loads installed maps and stock exploration masks without a cartridge address space.</summary>
    public AreaMapPresentationCatalog LoadMaps() => AreaMapPresentationCatalog.Load(MapDirectory, MapOverrideDirectory);
}

/// <summary>Shared on-disk layout used by the desktop host, Android host, and installation CLI.</summary>
public static class GameInstallationLayout
{
    /// <summary>Replaceable stock-content child directory, atomically published and repaired by the installer.</summary>
    public const string ContentDirectoryName = "game";
    /// <summary>Private validated, unheadered ROM filename retained for asset extraction and repair.</summary>
    public const string RomFileName = "SuperMetroid.smc";
    /// <summary>Content child directory for stock audio and its compatible override counterpart.</summary>
    public const string AudioDirectoryName = "audio";
    /// <summary>Content child directory for area-map artwork and stock exploration masks.</summary>
    public const string MapDirectoryName = "maps";
    /// <summary>Content child directory for initial CGRAM and room-entry sprite colors.</summary>
    public const string GameplayBasePaletteDirectoryName = "gameplay-palettes";
    /// <summary>Content child directory for the indexed common gameplay OBJ sheet.</summary>
    public const string StandardObjectDirectoryName = "standard-objects";
    /// <summary>Content child directory for projectile artwork and presentation selectors.</summary>
    public const string ProjectileDirectoryName = "projectiles";
    /// <summary>Content child directory for ordinary-enemy tile artwork.</summary>
    public const string EnemyTileDirectoryName = "enemy-tiles";
    /// <summary>Content child directory for room character-tile atlases.</summary>
    public const string RoomCharacterDirectoryName = "room-characters";
    /// <summary>Content child directory for Samus body PNGs and visual selectors.</summary>
    public const string SamusBodyDirectoryName = "samus-body";
    /// <summary>Content child directory for opening-cinematic artwork.</summary>
    public const string IntroCinematicDirectoryName = "intro-cinematic";
    /// <summary>Content child directory for ending Mode 7 scene artwork.</summary>
    public const string EndingMode7DirectoryName = "ending-mode7";
    /// <summary>Content child directory for ending OBJ artwork.</summary>
    public const string EndingObjectDirectoryName = "ending-objects";
    /// <summary>Content child directory for ending-scene color palettes.</summary>
    public const string EndingPaletteDirectoryName = "ending-palettes";
    /// <summary>Content child directory for stock RGB5 room palettes.</summary>
    public const string RoomPaletteDirectoryName = "room-palettes";
    /// <summary>Content child directory for visual block compositions independent of collision.</summary>
    public const string RoomMetatileDirectoryName = "room-blocks";
    /// <summary>Content child directory shared by background tilemaps and sky tilemaps.</summary>
    public const string RoomBackgroundTilemapDirectoryName = "room-backgrounds";
    /// <summary>Content child directory for visual room-block references independent of native collision and BTS.</summary>
    public const string RoomVisualLayoutDirectoryName = "room-layouts";
    /// <summary>Content child directory for shootable block appearances.</summary>
    public const string RoomPlmShotBlockVisualDirectoryName = "room-plm-shot-blocks";
    /// <summary>Content child directory for Grapple-reactive block appearances.</summary>
    public const string RoomPlmGrappleBlockVisualDirectoryName = "room-plm-grapple-blocks";
    /// <summary>Content child directory for station appearances; activation and rewards remain compiled.</summary>
    public const string RoomPlmStationVisualDirectoryName = "room-plm-stations";
    /// <summary>Content child directory for blue-door cap artwork.</summary>
    public const string RoomPlmBlueDoorVisualDirectoryName = "room-plm-blue-doors";
    /// <summary>Content child directory for colored-door cap artwork.</summary>
    public const string RoomPlmColoredDoorVisualDirectoryName = "room-plm-colored-doors";
    /// <summary>Content child directory for grey-door caps and shared clear frames.</summary>
    public const string RoomPlmGreyDoorVisualDirectoryName = "room-plm-grey-doors";
    /// <summary>Content child directory for the three eye-door components' artwork.</summary>
    public const string RoomPlmEyeDoorVisualDirectoryName = "room-plm-eye-doors";
    /// <summary>Content child directory for Mother Brain glass appearances.</summary>
    public const string RoomPlmMotherBrainGlassVisualDirectoryName = "room-plm-mother-brain-glass";
    /// <summary>Content child directory for n00b-tube appearances.</summary>
    public const string RoomPlmNoobTubeVisualDirectoryName = "room-plm-noob-tube";
    /// <summary>Content child directory for downward gate-block artwork.</summary>
    public const string RoomPlmDownwardGateVisualDirectoryName = "room-plm-downward-gates";
    /// <summary>Content child directory for elevator-platform frames.</summary>
    public const string RoomPlmElevatorPlatformVisualDirectoryName = "room-plm-elevator-platforms";
    /// <summary>Content child directory for closing escape-gate appearances.</summary>
    public const string RoomPlmEscapeGateVisualDirectoryName = "room-plm-escape-gate";
    /// <summary>Content child directory for Bomb Torizo hand artwork.</summary>
    public const string RoomPlmBombTorizoHandVisualDirectoryName = "room-plm-bomb-torizo-hand";
    /// <summary>Content child directory for reachable Draygon cannon artwork.</summary>
    public const string RoomPlmDraygonCannonVisualDirectoryName = "room-plm-draygon-cannons";
    /// <summary>Content child directory for Chozo hand and slope-access artwork.</summary>
    public const string RoomPlmChozoStatueVisualDirectoryName = "room-plm-chozo-statues";
    /// <summary>Content child directory for linked-block restoration appearances.</summary>
    public const string RoomPlmLinkedRestoreVisualDirectoryName = "room-plm-linked-restores";
    /// <summary>Content child directory for Tourian access-floor artwork.</summary>
    public const string RoomPlmTourianAccessVisualDirectoryName = "room-plm-tourian-access";
    /// <summary>Content child directory for the bomb-revealed Speed Booster tile.</summary>
    public const string RoomPlmSpeedBoosterVisualDirectoryName = "room-plm-speed-booster";
    /// <summary>Content child directory for the Maridia elevatube PLM tile.</summary>
    public const string RoomPlmMaridiaElevatubeVisualDirectoryName = "room-plm-maridia-elevatube";
    /// <summary>Content child directory for Spore Spawn ceiling tiles.</summary>
    public const string RoomPlmSporeSpawnCeilingVisualDirectoryName = "room-plm-spore-spawn-ceiling";
    /// <summary>Content child directory for floor and ceiling plant tiles.</summary>
    public const string RoomPlmSamusEaterVisualDirectoryName = "room-plm-samus-eater";
    /// <summary>Content child directory for Botwoon wall-clear tiles.</summary>
    public const string RoomPlmBotwoonWallVisualDirectoryName = "room-plm-botwoon-wall";
    /// <summary>Content child directory for Kraid ceiling and spike artwork.</summary>
    public const string RoomPlmKraidVisualDirectoryName = "room-plm-kraid";
    /// <summary>Content child directory for Crocomire bridge and wall artwork.</summary>
    public const string RoomPlmCrocomireVisualDirectoryName = "room-plm-crocomire";
    /// <summary>Content child directory for Mother Brain fake-death terrain artwork.</summary>
    public const string RoomPlmMotherBrainFakeDeathVisualDirectoryName = "room-plm-mother-brain-fake-death";
    /// <summary>Content child directory for item, orb, and reveal appearances.</summary>
    public const string RoomPlmCollectibleVisualDirectoryName = "room-plm-collectibles";
    /// <summary>Content child directory for collectible character PNGs and tile-palette selectors.</summary>
    public const string RoomPlmDynamicCollectibleArtDirectoryName = "room-plm-collectible-tiles";
    /// <summary>Content child directory for X-ray reveal metatile selections.</summary>
    public const string XrayRevealVisualDirectoryName = "xray-reveals";
    /// <summary>Receipt filename within stock content, recording the installation format and source cartridge digest.</summary>
    public const string ReceiptFileName = "installation.json";
    /// <summary>Current installation-receipt version; incompatible stock content requires validation and repair before opening.</summary>
    public const int FormatVersion = 86;
    internal const string PreviousDirectoryName = ".game.previous";
    internal const string StagingPrefix = ".game.install-";
    internal const string LockFileName = ".game-install.lock";
}
