namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// One step of an eye door's authored sequence (#1554): the image it draws, how many updates the image stays, and what
/// the eye does as it draws.
/// </summary>
/// <param name="Image">The image the step draws.</param>
/// <param name="Updates">The updates the image stays before the next step, or before the sequence ends.</param>
/// <param name="Before">What the eye does as the image draws, before drawing it.</param>
internal readonly record struct EyeDoorStep(EyeDoorObjectFrame Image, int Updates, EyeDoorAction Before = EyeDoorAction.None);
