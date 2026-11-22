namespace SuperMetroid.Core.Gameplay;

/// <summary>A background showing nothing (#2544): the backdrop shows wherever nothing in front of it is drawn.</summary>
public sealed partial record NoRoomBackground : ShownRoomBackground
{
    /// <summary>The one background showing nothing, <see cref="ShownRoomBackground.Nothing"/>.</summary>
    internal NoRoomBackground()
    {
    }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).Nothing();
}
