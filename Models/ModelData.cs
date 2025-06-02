namespace RaymondMaarloeveLauncher.Models;

/// <summary>
/// Represents a model used in the game, including its identifier, name, and file path.
/// </summary>
public class ModelData
{
    /// <summary>
    /// The unique identifier of the model.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The name of the model.
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// The file path to the model.
    /// </summary>
    public string Path { get; set; } = "";
}