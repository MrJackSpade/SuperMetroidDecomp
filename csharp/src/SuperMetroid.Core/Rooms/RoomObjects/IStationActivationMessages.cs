namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>What the gameplay frame opens for the station actions a room-object step publishes (#2383).</summary>
internal interface IStationActivationMessages
{
    /// <summary>Asks whether to save at <paramref name="save"/>.</summary>
    void RequestSaveConfirmation(SaveStationActivation save);

    /// <summary>Shows the saved-game message of <paramref name="save"/>.</summary>
    void ShowSaveCompleted(SaveStationActivation save);

    /// <summary>Shows a map or refill station's completion <paramref name="message"/>.</summary>
    void ShowStationMessage(GameplayMessageId message);
}
