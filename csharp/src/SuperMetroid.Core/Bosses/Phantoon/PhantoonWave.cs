using SuperMetroid.Core.Rendering;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's wave on BG2 as an effect model (#1353): the wave channel's mode and the sine phase, amplitude and base
/// scroll its last pass built from. A display builds the per-line BG2 horizontal scrolls from it; nothing keeps a table.
/// </summary>
/// <param name="Mode">The channel's nonzero mode from its setup; bit zero selects the 128-line cycle.</param>
/// <param name="Phase">The sine phase the last pass built from.</param>
/// <param name="Amplitude">The amplitude the last pass built from; zero is the flat base scroll death fills.</param>
/// <param name="BaseScroll">The BG2 horizontal scroll the wave displaces.</param>
public sealed record PhantoonWave(int Mode, int Phase, int Amplitude, int BaseScroll)
{
    /// <summary>One whole cycle of horizontal scrolls, one per line, as the wave's HDMA data repeats it down the screen.</summary>
    public ushort[] Cycle()
    {
        int half = (Mode & PhantoonWaveDefinitions.LongWaveModeBit) != 0
            ? PhantoonWaveDefinitions.LongHalfCycle : PhantoonWaveDefinitions.ShortHalfCycle;
        var cycle = new ushort[half * 2];
        PhantoonWaveTable.Build(Mode, Phase, Amplitude, BaseScroll, cycle);
        return cycle;
    }

    /// <summary>The BG2 horizontal scroll of each gameplay line below the HUD.</summary>
    public ImmutableArray<ushort> GameplayScrolls()
    {
        ushort[] cycle = Cycle();
        int lines = ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;
        var scrolls = new ushort[lines];
        for (int line = 0; line < lines; line++)
            scrolls[line] = cycle[(line + ScreenGeometry.GameplayHudHeightPixels) % cycle.Length];
        return ImmutableArray.Create(scrolls);
    }
}
