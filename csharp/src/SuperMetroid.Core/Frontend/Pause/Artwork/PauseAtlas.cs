namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>One 256-character page of the loaded pause graphics, stored in documents by its name.</summary>
public enum PauseAtlas
{
    /// <summary>The first page, native tile numbers $000-$0FF.</summary>
    Map,
    /// <summary>The second page, native tile numbers $100-$1FF.</summary>
    Interface,
}
