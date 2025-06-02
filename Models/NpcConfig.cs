namespace RaymondMaarloeveLauncher.Models;

/// <summary>
/// Represents the configuration for a single NPC, including its identifier and associated model ID.
/// </summary>
public class NpcConfig
{
    /// <summary>
    /// The unique identifier of the NPC.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The identifier of the model assigned to the NPC.
    /// </summary>
    public int ModelId { get; set; }
}
