namespace SuperMetroid.Core.Frontend.Title;

/// <summary>Reads and writes a <see cref="TitleCard"/> in an installed document by its installed key.</summary>
internal sealed partial class TitleCardJsonConverter() : InstalledKeyJsonConverter<TitleCard>(TitleCardKeys.Installed);
