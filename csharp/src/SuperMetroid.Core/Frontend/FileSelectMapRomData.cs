namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$81 definition tables shared by the saved-game area and room maps.</summary>
public static class FileSelectMapRomData
{
    /// <summary><c>$81:AA1C</c>, FileSelectMap_Labels_Positions: X/Y words per geographic area.</summary>
    public const int LabelPositions = 0x81aa1c;
    /// <summary><c>$81:AA34</c>, RoomSelectMap_ExpandingSquare_Velocities: four low/high pairs per area.</summary>
    public const int WindowVelocities = 0x81aa34;
    /// <summary><c>$81:AA94</c>, RoomSelectMap_ExpandingSquare_Timers: completion occurs on signed underflow.</summary>
    public const int WindowTimers = 0x81aa94;
    /// <summary><c>$81:AAA0</c>, FileSelectMapArea_IndexTable: display-order to geographic-area mapping.</summary>
    public const int DisplayAreaIndices = 0x81aaa0;
    /// <summary>Six Zebes areas participate; Ceres has no world-map selection entry.</summary>
    public const int AreaCount = 6;
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
}
