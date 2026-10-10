using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The opening's Japanese subtitles: bank $8B's three "mode 7 objects" that load each page's
/// subtitle, and the subtitle timer <c>$1BA3</c> that holds every page's input wait after a
/// second subtitle appears. English text spawns none of them and leaves the timer at zero.
/// </summary>
/// <remarks>
/// Game state $1E runs <c>Handle_Mode7Objects</c> ($8B:951D) after the cinematic sprite
/// objects and before the BG objects whose page instructions spawn these subtitles, so an
/// object first runs on the frame after its spawn. Slots are searched and processed from
/// index two down to zero.
/// </remarks>
internal sealed class IntroJapaneseSubtitles
{
    /// <summary>$8B:B5B1 and its siblings: frames the second subtitle holds a page's input wait.</summary>
    public const ushort SecondSubtitleHoldFrames = 0x3c;

    private const int SlotCount = 3;
    private readonly SubtitleObject?[] slots = new SubtitleObject?[SlotCount];

    public IntroJapaneseSubtitles(bool enabled) => Enabled = enabled;

    /// <summary>Native <c>AltText</c> ($09E2) at the start of the intro.</summary>
    public bool Enabled { get; }

    /// <summary><c>CinematicSpriteObject_IntroSubtitleTimer</c> ($1BA3), cleared by intro setup at $8B:A56B.</summary>
    public ushort Timer { get; private set; }

    /// <summary>
    /// <c>Instruction_HandleCreatingSubtitle_PageN</c> ($8B:AE43 and siblings), the first
    /// instruction of each English page's text list: Japanese text loads the page's first
    /// subtitle.
    /// </summary>
    public void BeginPage(IntroNarrationPageId page)
    {
        if (Enabled)
            Spawn(new SubtitleObject(page, SubtitleStage.LoadFirstSubtitle));
    }

    /// <summary>
    /// <c>Instruction_SpawnBlinkingMarkers_WaitForInput_PageN</c> ($8B:AE5B and siblings).
    /// Returns true when the page's input wait starts now; on pages two to five Japanese text
    /// instead waits for a press that shows the second subtitle, and that object starts the wait.
    /// </summary>
    public bool FinishPage(IntroNarrationPageId page)
    {
        if (!Enabled || page is IntroNarrationPageId.Page1 or IntroNarrationPageId.Page6)
            return true;
        Spawn(new SubtitleObject(page, SubtitleStage.AwaitSecondSubtitleInput));
        return false;
    }

    /// <summary>
    /// The timer check at the start of every intro input wait ($8B:AEB8, $AF6C, $B0F2, $B123,
    /// $B1DA). True while the wait is held: the call consumed one frame of the timer.
    /// </summary>
    public bool HoldInputWait()
    {
        if (Timer == 0)
            return false;
        Timer--;
        return true;
    }

    /// <summary>Runs <c>Handle_Mode7Objects</c> ($8B:951D) for one frame.</summary>
    /// <param name="newlyPressed">This frame's controller rising edges ($8F), read by the second subtitle's pre-instruction.</param>
    /// <param name="startInputWait">Receives each page whose second subtitle has started its input wait.</param>
    public void Step(ushort newlyPressed, Action<IntroNarrationPageId> startInputWait)
    {
        for (int index = SlotCount - 1; index >= 0; index--)
            if (slots[index] is { } subtitle && subtitle.Process(this, newlyPressed, startInputWait))
                slots[index] = null;
    }

    /// <summary>
    /// <c>Spawn_Mode7Objects</c> ($8B:94E4) takes the highest free slot. The intro never has
    /// more than one subtitle alive, so a full table would be a modelling error.
    /// </summary>
    private void Spawn(SubtitleObject subtitle)
    {
        for (int index = SlotCount - 1; index >= 0; index--)
        {
            if (slots[index] is not null) continue;
            slots[index] = subtitle;
            return;
        }
        throw new InvalidOperationException("All three intro subtitle object slots are occupied.");
    }

    private enum SubtitleStage
    {
        /// <summary>First record of a page's first-subtitle list: load the text, then a one-frame transfer record.</summary>
        LoadFirstSubtitle,
        /// <summary>After the transfer record: enable cinematic BG tilemap updates and delete.</summary>
        FinishFirstSubtitle,
        /// <summary>The second-subtitle list's one-frame transfer loop, whose pre-instruction watches for a press.</summary>
        AwaitSecondSubtitleInput,
        /// <summary>The page's "done input" list: enable tilemap updates, select its input wait and delete.</summary>
        StartInputWait,
    }

    /// <summary>One mode 7 object: its instruction timer and position in its subtitle list.</summary>
    private sealed class SubtitleObject(IntroNarrationPageId page, SubtitleStage stage)
    {
        private SubtitleStage stage = stage;
        private ushort instructionTimer = 1;

        /// <summary>
        /// <c>Process_Mode7Objects_InstList</c> ($8B:9537): the pre-instruction, then the
        /// instruction list once the timer reaches zero. Returns true when the object deleted itself.
        /// </summary>
        public bool Process(IntroJapaneseSubtitles owner, ushort newlyPressed, Action<IntroNarrationPageId> startInputWait)
        {
            if (stage == SubtitleStage.AwaitSecondSubtitleInput && newlyPressed != 0)
            {
                // $8B:B585 and siblings: show the second subtitle, hold the input wait it
                // will start, and switch to the done-input list on this same frame.
                stage = SubtitleStage.StartInputWait;
                instructionTimer = 1;
                owner.Timer = SecondSubtitleHoldFrames;
            }
            if (--instructionTimer != 0)
                return false;
            switch (stage)
            {
                case SubtitleStage.LoadFirstSubtitle:
                    stage = SubtitleStage.FinishFirstSubtitle;
                    instructionTimer = 1;
                    return false;
                case SubtitleStage.AwaitSecondSubtitleInput:
                    // $8B:D39D: one-frame transfer record, then goto itself.
                    instructionTimer = 1;
                    return false;
                case SubtitleStage.FinishFirstSubtitle:
                    return true;
                case SubtitleStage.StartInputWait:
                    startInputWait(page);
                    return true;
                default:
                    throw new InvalidOperationException($"Unknown intro subtitle stage {stage}.");
            }
        }
    }
}
