namespace SuperMetroid.Core.Frontend;

/// <summary>The primary and two secondary options pages selected by menu navigation.</summary>
internal enum GameOptionsPage
{
    /// <summary>Start, language, controller settings, and special settings.</summary>
    Primary,
    /// <summary>Editable action-button bindings plus reset and exit rows.</summary>
    Controller,
    /// <summary>Automatic item cancel and moonwalk toggles.</summary>
    Special,
}
