namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>
/// The side an Evir body faces, which its arms and spit copy. A body record that a kill has emptied reads as
/// facing left, which is what its arms and spit see on the final update they still receive.
/// </summary>
public enum EvirFacing
{
    /// <summary>Faces left: Samus is to the body's left, or the body's record has been emptied.</summary>
    Left,
    /// <summary>Faces right: Samus is level with or to the right of the body.</summary>
    Right,
}
