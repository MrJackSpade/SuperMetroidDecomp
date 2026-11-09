namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One step of a door cap's authored sequence (#1554): the image of the cap it draws and how many updates the image
/// stays before the sequence's next step, with the sound the step requests as it draws.
/// </summary>
/// <param name="Image">The image of the cap the step draws.</param>
/// <param name="Updates">The updates the image stays before the next step, or before the sequence ends.</param>
/// <param name="SoundBefore">The sound requested as the image draws; null for none.</param>
internal readonly record struct DoorCapStep(DoorCapFrame Image, int Updates, SoundEffectId? SoundBefore = null);
