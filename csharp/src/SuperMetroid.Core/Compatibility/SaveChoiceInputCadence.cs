namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When an open save confirmation reads the controller (#1477). Once the box has opened, the confirmation waits two
/// updates and then reads the controller; after each read it waits one update and reads again, so it reads on every
/// second update. A read that moves the choice spends the next update showing the moved choice, which delays the
/// following read by one update. Presses are new relative to the previous read, so a button pressed and released
/// between reads is never seen, and which update a press lands on decides whether the save is taken and when gameplay
/// resumes. The count is gameplay-observable.
/// </summary>
/// <remarks>
/// Advanced once per update while a save or gunship-save confirmation awaits its answer, before the box acts on the
/// controller; restarted when such a box finishes opening. Created once per session at its composition root and
/// handed to every gameplay runtime's message box. Evidence: the original confirmation loop (<c>$85:84BA</c>) waits
/// for two displayed frames before each controller read, the read's own frame counting as the next loop's first wait,
/// and the choice toggle (<c>$85:8532</c>) waits one more displayed frame before redrawing; the save-station and gunship
/// confirmations in the SMV recordings consume their inputs on exactly these updates.
/// </remarks>
[CompatibilityService("the save confirmation reads the controller every second update, and a moved choice delays the next read by one update")]
public sealed class SaveChoiceInputCadence
{
    /// <summary>The updates an opened confirmation waits before its first read.</summary>
    public const int UpdatesBeforeFirstRead = 2;

    /// <summary>The updates the confirmation waits between one read and the next.</summary>
    private const int UpdatesBetweenReads = 1;

    /// <summary>The waiting updates left before the next read.</summary>
    private int _updatesBeforeRead;

    /// <summary>Whether the last read moved the choice and the next update shows it.</summary>
    private bool _movedChoicePending;

    /// <summary>What one update of an open save confirmation does with the controller.</summary>
    public enum Update
    {
        /// <summary>The confirmation waits; the controller is not read.</summary>
        Waits,

        /// <summary>The confirmation shows the choice the previous read moved to and plays its move sound; the controller is not read.</summary>
        ShowsMovedChoice,

        /// <summary>The confirmation reads the controller: a new press confirms, cancels or moves the choice.</summary>
        ReadsController,
    }

    /// <summary>Starts the cadence for a confirmation box that has just finished opening.</summary>
    public void Begin()
    {
        _updatesBeforeRead = UpdatesBeforeFirstRead;
        _movedChoicePending = false;
    }

    /// <summary>Advances one update and reports what that update does with the controller.</summary>
    public Update Advance()
    {
        if (_movedChoicePending)
        {
            _movedChoicePending = false;
            return Update.ShowsMovedChoice;
        }
        if (_updatesBeforeRead > 0)
        {
            _updatesBeforeRead--;
            return Update.Waits;
        }
        _updatesBeforeRead = UpdatesBetweenReads;
        return Update.ReadsController;
    }

    /// <summary>Records that this update's read moved the choice; the next update shows it before the waits resume.</summary>
    public void ChoiceMoved() => _movedChoicePending = true;
}
