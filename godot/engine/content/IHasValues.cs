namespace Engine.Content;

/// <summary>
/// A node or resource that holds values. Its simple values are its properties marked <see cref="DataValueAttribute"/>;
/// values with more structure, such as a list of reactions, it reads itself in <see cref="ReadValues"/>.
/// </summary>
public interface IHasValues
{
    /// <summary>
    /// The data file this thing's values come from, as a path within the assets tree. Empty when its values are edited
    /// and saved in the editor.
    /// </summary>
    string DataFile { get; }

    /// <summary>Reads the values that are not simple properties from the thing's data file. Called after the simple ones are set.</summary>
    void ReadValues(DataNode data);
}
