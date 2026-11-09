namespace SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Verification access to <see cref="MotherBrainFakeDeathDraws"/> members production does not use.</summary>
internal static class MotherBrainFakeDeathObjectDrawDefinitionsAccess
{
    /// <summary>Resolves a Mother Brain fake-death draw to the geometry the import pictures it by.</summary>
    extension(MotherBrainFakeDeathDraws)
    {
        /// <summary>The geometry of a mural row, boundary, fill row or tube clear; false for any other draw.</summary>
        internal static bool TryGet(RoomObjectDraw request, out RoomObjectFrameLayout layout)
        {
            if (request is RoomObjectFrameDraw<MotherBrainFakeDeathDraw> { Frame: var frame } &&
                RoomObjectMotherBrainFakeDeathVisualFiles.Family.Layouts.TryGetValue(frame, out layout))
                return true;
            layout = default;
            return false;
        }
    }
}
