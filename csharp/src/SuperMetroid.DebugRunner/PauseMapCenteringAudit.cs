using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks the native downloaded-map bounds independently of explored secret cells.</summary>
internal static class PauseMapCenteringAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        for (int area = 0; area < 6; area++)
        {
            var system = new Bank80SystemState();
            system.SetAreaMapAcquired(area);
            var samus = new SamusState { XPosition = 128, YPosition = 128 };
            var before = new PauseMenuState(bus, samus, system, (AreaId)area, 30, 16);
            // Expand the explored plane beyond the downloaded map in both directions.
            // Native DetermineMapScrollLimits selects the station plane exclusively.
            system.MarkExploredMapTile(area, 0, 0);
            system.MarkExploredMapTile(area, 63, 31);
            var after = new PauseMenuState(bus, samus, system, (AreaId)area, 30, 16);
            if (before.MapHorizontalScroll != after.MapHorizontalScroll ||
                before.MapVerticalScroll != after.MapVerticalScroll)
                throw new InvalidDataException($"Area {area}: secret exploration shifted downloaded-map centering " +
                    $"from {before.MapHorizontalScroll:X4},{before.MapVerticalScroll:X4} " +
                    $"to {after.MapHorizontalScroll:X4},{after.MapVerticalScroll:X4}.");
            before.Render();
            after.Render();
            if (before.LastIndicatorOriginX != after.LastIndicatorOriginX ||
                before.LastIndicatorOriginY != after.LastIndicatorOriginY)
                throw new InvalidDataException("Rendered map marker moved despite unchanged downloaded bounds.");
        }
        // The native vertical scans deliberately return defaults before examining the
        // last row in their respective directions. With one explored cell, horizontal
        // scroll centers that cell. The high player Y avoids the separate top clamp.
        foreach (var sample in new[] { (Row: 0, ExpectedY: -64), (Row: 31, ExpectedY: 16) })
        {
            var system = new Bank80SystemState();
            system.MarkExploredMapTile(0, 30, sample.Row);
            var pause = new PauseMenuState(bus, new SamusState(), system, AreaId.Crateria, 30, 24);
            if (pause.MapHorizontalScroll != 112 || pause.MapVerticalScroll != unchecked((ushort)sample.ExpectedY))
                throw new InvalidDataException($"Native edge-row centering mismatch: row {sample.Row}, scroll {pause.MapHorizontalScroll:X4},{pause.MapVerticalScroll:X4}.");
        }
        Console.WriteLine("Pause-map centering: all six downloaded area bounds remain independent of explored secret extremes.");
        return 0;
    }
}
