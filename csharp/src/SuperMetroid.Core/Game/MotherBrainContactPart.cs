namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Mother Brain component owning a Samus-contact list.</summary>
internal enum MotherBrainContactPart : byte
{
    /// <summary>Lower and upper body regions, tested in that native order.</summary>
    Body,
    /// <summary>Upper and lower brain regions meeting between local Y zero and one.</summary>
    Brain,
    /// <summary>Single centered contact region for a tested neck joint.</summary>
    Neck,
}
