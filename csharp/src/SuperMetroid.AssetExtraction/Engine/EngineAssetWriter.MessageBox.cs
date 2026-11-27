using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The game's messages: the picture of each, for every set of graphics a room is drawn with, and how a message is
/// shown (<c>messages/box.json</c>): the window that opens and closes about it, when a question reads its answer,
/// and which picture of a message names the button the player has bound.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The file of how a message is shown.</summary>
    private const string MessageBoxFile = "messages/box.json";

    /// <summary>
    /// The name the Godot game keeps one of the old controller's buttons under: the control an action of play is on,
    /// which a message's picture names.
    /// </summary>
    private static string ControlName(ControllerButton button) => button switch
    {
        ControllerButton.A => "a",
        ControllerButton.B => "b",
        ControllerButton.X => "x",
        ControllerButton.Y => "y",
        ControllerButton.Select => "select",
        ControllerButton.L => "l",
        ControllerButton.R => "r",
        _ => throw new InvalidDataException($"The Godot game keeps no control for the old button {button}."),
    };

    /// <summary>The ending of the name of a message's picture that names <paramref name="button"/>.</summary>
    private static string ButtonPictureName(ControllerButton button) => EngineSheets.FileName(ControllerButtonKeys.Installed.Of(button));

    /// <summary>The Godot game's action whose button a message's picture names for <paramref name="binding"/>.</summary>
    private static string ActionOf(GameplayMessagePanelButtonBinding binding) => binding switch
    {
        GameplayMessagePanelButtonBinding.Shoot => "attack",
        GameplayMessagePanelButtonBinding.Run => "run",
        _ => throw new InvalidDataException($"A message names the button of {binding}, which the Godot game has no action for."),
    };

    /// <summary>How many updates <paramref name="message"/> stays up before it can be dismissed.</summary>
    /// <exception cref="InvalidDataException">The message is one that is not held.</exception>
    private static int MessageHold(GameplayMessageId message) => GameplayMessageBoxDefinitions.Message(message).MinimumDisplayFrames
        ?? throw new InvalidDataException($"The message {message} has no time it stays up.");

    /// <summary>
    /// How many updates one of the old message box's pauses lasts: the pause is begun on a box's pauses of its own
    /// and counted until it is over.
    /// </summary>
    private static int UpdatesOf(Action<MessageBoxPauses> begin)
    {
        var pauses = new MessageBoxPauses();
        begin(pauses);
        int updates = 0;
        while (!pauses.IsOver)
        {
            pauses.Advance();
            updates++;
        }

        return updates;
    }

    /// <summary>
    /// What each update of one of the old message box's pauses does while the box covers play: whether the music's
    /// queue gets its turn, how many turns the sounds' queues get, and whether the room's liquid, statues and a power
    /// bomb's blast move.
    /// </summary>
    private static JsonArray PauseOf(Action<MessageBoxPauses> begin)
    {
        var pauses = new MessageBoxPauses();
        begin(pauses);
        var updates = new JsonArray();
        while (!pauses.IsOver)
        {
            MessageBoxFrameAudio update = pauses.Advance();
            if (update.MusicQueuePasses > 1)
                throw new InvalidDataException("An update of a message box's pause gives the music more than one turn.");
            updates.Add(new JsonObject
            {
                ["music"] = update.MusicQueuePasses != 0,
                ["sound_turns"] = update.SoundQueuePasses,
                ["opens_room"] = update.AdvancesLiquidWaveAndStatueDescent,
            });
        }

        return updates;
    }

    /// <summary>
    /// When the old save question looks at the controller: how many updates it waits after its window has opened
    /// before its first look, and how many between one look and the next, counted on the old cadence itself.
    /// </summary>
    private static (int First, int Between) QuestionReads()
    {
        var cadence = new SaveChoiceInputCadence();
        cadence.Begin();
        int WaitsBeforeRead()
        {
            int waits = 0;
            while (true)
            {
                SaveChoiceInputCadence.Update update = cadence.Advance();
                if (update == SaveChoiceInputCadence.Update.ReadsController)
                    return waits;
                if (update != SaveChoiceInputCadence.Update.Waits)
                    throw new InvalidDataException($"The save question's cadence gave {update} with no choice moved.");
                waits++;
            }
        }

        int first = WaitsBeforeRead();
        return (first, WaitsBeforeRead());
    }

    /// <summary>
    /// Writes the pictures of the game's messages, once for every set of graphics a room is drawn with: a message is
    /// drawn with the room's own colours. One that asks yes or no has a picture of each answer chosen. One that names
    /// the button that uses what was found has a picture for each button the old game can name and one for none.
    /// Then writes how a message is shown.
    /// </summary>
    private void WriteMessages()
    {
        const string family = GameInstallationLayout.RoomBlockSheetDirectoryName, folder = "messages";
        var namesAction = new JsonObject();
        foreach (GameplayMessageId id in Enum.GetValues<GameplayMessageId>())
        {
            if (GameplayMessageBoxDefinitions.Message(id).PanelBinding is { } binding)
                namesAction[MessageName(id)] = ActionOf(binding);
        }

        foreach (string graphicsSet in Layouts.GraphicsSets)
        {
            string sheet = "Messages." + graphicsSet;
            void Write(ShownMessage form, string name)
            {
                (RgbaPixel[] pixels, int width, int height) = _sheets.PictureOf(family, sheet, form.FrameName);
                if (height > 2 * GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels)
                    throw new InvalidDataException($"The picture {form.FrameName} of {sheet} is taller than the message's open window.");
                _tree.WritePicture($"{folder}/{EngineRoomObjects.GraphicsName(graphicsSet)}/{name}.png", width, height, pixels);
            }

            foreach (GameplayMessageId id in Enum.GetValues<GameplayMessageId>())
            {
                GameplayMessageDefinition message = GameplayMessageBoxDefinitions.Message(id);
                if (message.Confirmation != GameplayMessageConfirmation.None)
                {
                    if (message.PanelBinding is not null)
                        throw new InvalidDataException($"The message {id} both asks and names a button, which the Godot game has no pictures for.");
                    Write(new ShownMessage(id, null, SaveChoice.Yes), MessageName(id) + "_yes");
                    Write(new ShownMessage(id, null, SaveChoice.No), MessageName(id) + "_no");
                }
                else if (message.PanelBinding is null)
                    Write(new ShownMessage(id, null, null), MessageName(id));
                else
                {
                    foreach (ControllerButton button in GameplayMessageBoxDefinitions.Buttons.GlyphOrder)
                        Write(new ShownMessage(id, button, null), $"{MessageName(id)}_{ButtonPictureName(button)}");
                }
            }
        }

        var controls = new JsonArray();
        foreach (ControllerButton button in GameplayMessageBoxDefinitions.Buttons.GlyphOrder)
            controls.Add(new JsonObject { ["control"] = ControlName(button), ["picture"] = ButtonPictureName(button) });

        (int firstRead, int betweenReads) = QuestionReads();
        _tree.WriteData(MessageBoxFile, new JsonObject
        {
            ["pictures"] = folder,
            ["centre_y"] = GameplayMessageBoxDefinitions.Layout.WindowCenterY,
            ["full_radius"] = GameplayMessageBoxDefinitions.Timing.MaximumRadiusPixels,
            ["radius_step"] = GameplayMessageBoxDefinitions.Timing.RadiusStepPixels,
            // The old box's first update of nothing is the update of play that asked for it, which the Godot game
            // stops part way through; and the rest of that update is played on the last update of nothing once
            // the box has closed.
            ["before_opening"] = PauseOf(pauses => pauses.BeginBeforeOpening()),
            ["after_closing"] = PauseOf(pauses => pauses.BeginAfterClosing()),
            // The old game asks every library of sounds to stop as a box opens over play.
            ["silences_sounds"] = true,
            ["first_read_updates"] = firstRead,
            ["updates_between_reads"] = betweenReads,
            // The old question's choice is moved by left, right and select: select is the button the old game gives
            // the choosing of an item to.
            ["move_actions"] = new JsonArray("left", "right", "item_select"),
            ["move_sound"] = EngineNames.Sound(SamusSound.MenuCursor),
            ["names_action"] = namesAction,
            ["controls"] = controls,
        });
    }
}
