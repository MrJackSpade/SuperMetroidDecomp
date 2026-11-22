using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Verification access to <see cref="FileSelectMenuState"/> members production does not use.</summary>
internal static class FileSelectMenuStateAccess
{
    /// <summary>Reads file-select BG tiles and the selected helmet frame without mutation.</summary>
    extension(FileSelectMenuState self)
    {
        /// <summary>
        /// The page the menu shows, laid out as the cartridge's tilemap words from <paramref name="maps"/>' sources of the
        /// pictures it places (#1467), for cartridge-layout verification; <paramref name="maps"/> is the import's read of the
        /// presentation the menu was built with (#1616).
        /// </summary>
        internal ushort[] BackgroundTilemap(SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation maps) =>
            BackgroundTileWordBytes.Words(Ensure.NotNull(maps).FileSelect.LayOut(
                self.ShownPage));

        /// <summary>Selected helmet frame retained through the fade into loading/options.</summary>
        internal FileSelectHelmetFrame SelectedHelmetFrame => self.HelmetAnimationFrame;

        /// <summary>The menu screen file select draws, whose sprite entries a check reads.</summary>
        internal MenuScreen Screen => self.screen;

        /// <summary>The step the copy arrows' colour rotation shows.</summary>
        internal FileCopyArrowRotationStep CopyArrowStep => self.CopyArrowRotation;
    }

    /// <summary>Reads the world map's screen.</summary>
    extension(FileSelectAreaMapGraphics self)
    {
        /// <summary>The world map's menu screen, whose backdrop a check reads.</summary>
        internal MenuScreen Screen => self.screen;
    }

    /// <summary>Reads the map menu's room map and scroll.</summary>
    extension(FileSelectMapMenuState self)
    {
        /// <summary>The selected area's room map.</summary>
        internal FileSelectRoomMapGraphics RoomGraphics => self.roomGraphics;

        /// <summary>The room map's scroll, anchored to the selected save station.</summary>
        internal MapScroll RoomMapScroll => self.Scroll;
    }

    /// <summary>Reads the room map's icons and projected cells.</summary>
    extension(FileSelectRoomMapGraphics self)
    {
        /// <summary>The map landmarks, stations and boss icons, whose area a fixture reads.</summary>
        internal FileSelectMapIcons Icons => self.icons;

        /// <summary>How the room map shows the area's cells.</summary>
        internal ShownAreaMap ShownCells => self.AreaMap;
    }
}
