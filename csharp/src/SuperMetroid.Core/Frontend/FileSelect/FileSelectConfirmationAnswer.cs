namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Where a copy's or clear's Yes or No confirmation stands after an update (#2553).</summary>
internal enum FileSelectConfirmationAnswer
{
    /// <summary>No answer yet: the selection moved, or nothing was pressed.</summary>
    Undecided,

    /// <summary>B, or A or Start on No: the menu goes back to the choice before the confirmation.</summary>
    Declined,

    /// <summary>A or Start on Yes: the copy or clear happens.</summary>
    Accepted,
}
