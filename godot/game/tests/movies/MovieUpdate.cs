using System.Text.Json;

namespace Game.Tests.Movies;

/// <summary>
/// One update of a recorded movie, as the converter that made the trace gives it: one update of the game, with
/// hardware lag left out.
/// </summary>
/// <param name="Number">The update's number in the movie, from one.</param>
/// <param name="Kind">
/// What kind of update the cartridge played: its main loop ("main-loop"), or one that went on inside something the
/// main loop had begun (a step of a door's scroll, a wait on a music upload, and the like).
/// </param>
/// <param name="Buttons">The cartridge's buttons held on the update, as the cartridge's controller word has them.</param>
/// <param name="Pressed">Those of them newly pressed on the update.</param>
/// <param name="SourceFrame">The movie's frame the update's buttons were read on.</param>
/// <param name="ExpectedRecord">The trace's record of the cartridge's memory as the update ended; none for an update that has no record of its own.</param>
/// <param name="MessageBoxStartFrame">The movie's frame on which a message box opened inside the update, when one did.</param>
/// <param name="MessageBoxEndFrame">The movie's frame on which that message box closed, when it closed inside the update.</param>
/// <param name="ConsoleReset">Whether the console was reset before the update: it is the first of the cartridge's updates after a reset.</param>
public sealed record MovieUpdate(
    int Number, string Kind, int Buttons, int Pressed, int SourceFrame, int? ExpectedRecord,
    int? MessageBoxStartFrame, int? MessageBoxEndFrame, bool ConsoleReset)
{
    /// <summary>The kind of an update of the cartridge's main loop.</summary>
    public const string MainLoop = "main-loop";

    /// <summary>Reads one update of the converter's list.</summary>
    internal static MovieUpdate Read(JsonElement update)
    {
        static int? Whole(JsonElement update, string name) =>
            update.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Number ? value.GetInt32() : null;

        return new MovieUpdate(
            update.GetProperty("update").GetInt32(),
            update.GetProperty("kind").GetString() ?? "",
            update.GetProperty("input").GetInt32(),
            update.GetProperty("pressed").GetInt32(),
            update.GetProperty("sourceFrame").GetInt32(),
            Whole(update, "expectedRecord"),
            Whole(update, "messageBoxStartFrame"),
            Whole(update, "messageBoxEndFrame"),
            update.TryGetProperty("consoleReset", out JsonElement reset) && reset.ValueKind == JsonValueKind.True);
    }
}
