using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;

using SuperMetroid.Verification.Harness;

/// <summary>
/// Confirms the frame displays gameplay draws by are the installed documents' own (#1463, #1513, #1603): every frame either
/// document composes is displayed, a frame composed of one spritemap is pictured unless it draws nothing, and a frame
/// composed of placed pieces is pictured exactly when it has a sprite piece.
/// </summary>
internal static class EnemyExtendedFrameDisplaysVerification
{
    /// <summary>
    /// For every enemy frame, the installed displays resolve exactly the frames the installed compositions resolve, and
    /// picture exactly those the compositions picture.
    /// </summary>
    internal static void VerifyEnemyExtendedFrameDisplays()
    {
        EnemyTileSourceArtwork stock = InstalledFixtures.EnemyTiles;
        EnemySpritemapCatalog spritemaps = stock.Spritemaps
            ?? throw new InvalidOperationException("Verification requires the installed enemy compositions.");
        EnemyExtendedFrameCatalog compositions = stock.ExtendedFrames
            ?? throw new InvalidOperationException("Verification requires the installed extended frames.");
        EnemyFrameDisplays displays = stock.FrameDisplays
            ?? throw new InvalidOperationException("Verification requires the installed enemy frame displays.");
        int installed = 0, pictured = 0;
        foreach (EnemyFrameId frame in Enum.GetValues<EnemyFrameId>())
        {
            bool ordinary = spritemaps.TryGetDisplay(frame, out _);
            bool composed = compositions.TryGetDisplay(frame, out ReadOnlyMemory<EnemyExtendedDrawComponent> pieces);
            bool displayed = displays.TryGetDisplay(frame, out bool hasPicture);
            AssertEqual(ordinary || composed, displayed, $"{frame} is displayed exactly when an installed document composes it");
            if (!displayed)
                continue;
            installed++;
            bool expected = ordinary ? !CommonEnemyEmptyFrames.DrawsNothing(frame) : !pieces.IsEmpty;
            AssertEqual(expected, hasPicture, $"{frame} has a picture exactly when its composition shows one");
            if (hasPicture)
                pictured++;
        }
        AssertTrue(installed > 0 && pictured > 0, "the installation holds pictured enemy frames");
        VerifyDisplayBindingsAreComplete();
        VerifyEditedBindingsLoadWhereverTheFrameWasStored();
        Console.WriteLine($"  Enemy frame displays: {installed} frames, {pictured} pictured, match their compositions; every binding displays an installed frame.");
    }

    /// <summary>
    /// A frame bound to another is displayed through that frame alone (#1627): a binding naming a frame the installation
    /// does not hold is rejected as the displays are built, rather than the bound frame displaying itself in its place.
    /// </summary>
    private static void VerifyDisplayBindingsAreComplete()
    {
        EnemyFrameId[] drawn = [.. Enum.GetValues<EnemyFrameId>().Where(frame => !CommonEnemyEmptyFrames.DrawsNothing(frame)).Take(3)];
        (EnemyFrameId shown, EnemyFrameId displayedFrame, EnemyFrameId missing) = (drawn[0], drawn[1], drawn[2]);
        var bound = new EnemyFrameDisplays([shown, displayedFrame], [displayedFrame], new() { [shown] = displayedFrame });
        AssertTrue(bound.TryGetDisplay(shown, out bool boundPicture) && boundPicture,
            "a bound frame is displayed through the installed frame its binding names");
        AssertThrows<InvalidDataException>(() => new EnemyFrameDisplays([shown], [shown], new() { [shown] = missing }),
            "a binding naming a frame the installation does not hold is rejected, even when the bound frame is installed");
    }

    /// <summary>
    /// The installed display bindings keep the rules of the enemies showing each frame, not where the original stored a frame
    /// (#1710): an edited binding selecting a frame every enemy showing the edited frame shows loads and displays it, though
    /// the original stores the two frames in different banks.
    /// </summary>
    private static void VerifyEditedBindingsLoadWhereverTheFrameWasStored()
    {
        ILookup<EnemyFrameId, EnemyDefinitionId> showing = EnemyDefinitionFrameDefinitions.Frames
            .SelectMany(entry => entry.Value, (entry, frame) => KeyValuePair.Create(frame, entry.Key))
            .ToLookup(shown => shown.Key, shown => shown.Value);
        EnemyFrameName[] frames = [.. EnemySpritemapDefinitions.Frames.Where(frame => !CommonEnemyEmptyFrames.DrawsNothing(frame.Frame))];
        // The frames fewest enemies show come first: any frame those enemies all show may stand in for them.
        (EnemyFrameName Edited, EnemyFrameName Selected)? pair = frames.OrderBy(frame => showing[frame.Frame].Count())
            .SelectMany(edited => frames, (edited, selected) => (Edited: edited, Selected: selected))
            .Where(candidate => candidate.Edited.Frame != candidate.Selected.Frame
                && EnemyFrameCartridgeLookup.Of(candidate.Edited.Frame).Bank != EnemyFrameCartridgeLookup.Of(candidate.Selected.Frame).Bank
                && !showing[candidate.Edited.Frame].Except(showing[candidate.Selected.Frame]).Any())
            .Select(candidate => ((EnemyFrameName, EnemyFrameName)?)candidate)
            .FirstOrDefault();
        AssertTrue(pair is not null, "an ordinary frame may select a frame of another bank by the enemies showing it");
        (EnemyFrameName edited, EnemyFrameName selected) = pair!.Value;
        JsonObject document = JsonNode.Parse(EnemyFrameDisplayFiles.Extract())?.AsObject()
            ?? throw new InvalidOperationException("Verification requires the stock enemy display bindings.");
        JsonObject bindings = document["displayFrames"]?.AsObject()
            ?? throw new InvalidOperationException("Verification requires the stock ordinary display bindings.");
        bindings[edited.Name] = selected.Name;
        InstalledEnemyFrameDisplays loaded = EnemyFrameDisplayFiles.Load(new MemoryStream(Encoding.UTF8.GetBytes(document.ToJsonString()), writable: false));
        AssertEqual(selected.Frame, loaded.Displays().GetDisplayFrame(edited.Frame),
            $"edited binding {edited.Name} displays {selected.Name}, which the original stores in another bank");
    }

    /// <summary>Runs the focused enemy frame display check.</summary>
    [VerificationCommand("--enemy-extended-frame-displays")]
    internal static int EnemyExtendedFrameDisplaysCommand()
    {
        Suite(nameof(VerifyEnemyExtendedFrameDisplays), () => VerifyEnemyExtendedFrameDisplays());
        return 0;
    }
}
