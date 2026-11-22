namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The look each pose of an eye door's body's middle and bottom shows (#2481).</summary>
internal static partial class EyeDoorPartLookDefinitions
{
    /// <summary>Each part's look in each pose of its pulse.</summary>
    internal static IReadOnlyDictionary<EyeDoorBodyPart, IReadOnlyDictionary<EyeDoorPartPose, EyeDoorLook>> Looks { get; } =
        new Dictionary<EyeDoorBodyPart, IReadOnlyDictionary<EyeDoorPartPose, EyeDoorLook>>
        {
            [EyeDoorBodyPart.Middle] = new Dictionary<EyeDoorPartPose, EyeDoorLook>
            {
                [EyeDoorPartPose.Resting] = EyeDoorLook.MiddleResting,
                [EyeDoorPartPose.Swelling] = EyeDoorLook.MiddleSwelling,
                [EyeDoorPartPose.Swollen] = EyeDoorLook.MiddleSwollen,
            },
            [EyeDoorBodyPart.Bottom] = new Dictionary<EyeDoorPartPose, EyeDoorLook>
            {
                [EyeDoorPartPose.Resting] = EyeDoorLook.BottomResting,
                [EyeDoorPartPose.Swelling] = EyeDoorLook.BottomSwelling,
                [EyeDoorPartPose.Swollen] = EyeDoorLook.BottomSwollen,
            },
        };
}
