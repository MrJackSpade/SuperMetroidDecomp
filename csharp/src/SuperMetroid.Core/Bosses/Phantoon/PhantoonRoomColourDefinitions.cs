using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The background colours Phantoon's death fades (#1405).</summary>
internal static class PhantoonRoomColourDefinitions
{
    /// <summary>The room colours that fade to the Wrecked Ship's powered-on colours: the first seven sets.</summary>
    internal static readonly BackgroundColourRun PowerOn = new(0, 7 * ColourSets.SetColours);
}
