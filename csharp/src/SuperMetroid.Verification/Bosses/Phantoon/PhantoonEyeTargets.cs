using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Verification.Bosses.Phantoon;

/// <summary>Phantoon's eight eye directions, a Samus placement that selects each, and the cartridge's gaze table as their oracle.</summary>
internal static class PhantoonEyeTargets
{
    /// <summary>The cartridge's table of the eye's gaze entries, two bytes each, in selector order.</summary>
    private const int CartridgeGazeTable = 0xa7d40d;

    /// <summary>The cartridge selector of the downward gaze, and the one after it, which no octant test produces.</summary>
    private const int DownSelector = 4;

    /// <summary>Samus placed in each of the eight directions from Phantoon, with the cartridge selector that direction reads.</summary>
    internal static EyeTarget[] All =>
    [
        new(0, -100, CompassDirection.Up, 0), new(100, -100, CompassDirection.UpRight, 1),
        new(100, 0, CompassDirection.Right, 2), new(100, 100, CompassDirection.DownRight, 3),
        new(0, 100, CompassDirection.Down, DownSelector), new(-100, 100, CompassDirection.DownLeft, 6),
        new(-100, 0, CompassDirection.Left, 7), new(-100, -100, CompassDirection.UpLeft, 8),
    ];

    /// <summary>
    /// Checks every direction's gaze program against the cartridge entry its selector reads, that the targets cover all
    /// eight directions, and that the one entry no selector reaches repeats the downward gaze, so the typed table loses
    /// nothing the cartridge could select.
    /// </summary>
    internal static void VerifyGazeProgramsAgainstCartridge(IImportCartridgeSource cartridge)
    {
        EyeTarget[] targets = All;
        AssertEqual(Enum.GetValues<CompassDirection>().Length, targets.Select(target => target.Direction).Distinct().Count(),
            "the eye targets cover all eight directions");
        foreach (EyeTarget target in targets)
        {
            AssertEqual(cartridge.ReadCartridgeWord(CartridgeGazeTable + target.CartridgeSelector * 2),
                PhantoonNativeListAddresses.PartEntry(PhantoonEyeGaze.EyeAnimation(target.Direction)),
                $"native Phantoon eye gaze {target.Direction}");
        }
        AssertEqual(cartridge.ReadCartridgeWord(CartridgeGazeTable + DownSelector * 2),
            cartridge.ReadCartridgeWord(CartridgeGazeTable + (DownSelector + 1) * 2),
            "the gaze entry no octant test selects repeats the downward gaze");
    }
}
