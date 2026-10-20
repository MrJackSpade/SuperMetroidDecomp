namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>One of the Ceres destruction scene's graphics transfers, which its captured image replays in order (#1358).</summary>
internal enum CeresDestructionUpload
{
    /// <summary>
    /// <c>$8B:C11B</c>: the Mode 7 characters, the blank-filled map with the Ceres scene's map pair, the station OBJ
    /// characters, and the shared cinematic sheet over their first $1A00 bytes.
    /// </summary>
    CeresScene,

    /// <summary><c>$8B:C345</c>: the final explosion's two map halves, the front gunship (or a clear half without Samus) and a clear half.</summary>
    FinalExplosionMaps,

    /// <summary><c>$8B:C699</c>: the dormant rear-gunship map, the Zebes Mode 1 tilemap and the Zebes characters.</summary>
    ZebesReveal,
}
