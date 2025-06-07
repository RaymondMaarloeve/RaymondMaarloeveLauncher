using System.Collections.Generic;

namespace RaymondMaarloeveLauncher.Models;

/// <summary>
/// Represents the main configuration data for the game, including server settings, display options, models, and NPCs.
/// </summary>
public class GameData
{
    /// <summary>
    /// The revision number of the configuration.
    /// </summary>
    public int Revision { get; set; } = 1;
    /// <summary>
    /// The API URL of the LLM server.
    /// </summary>
    public string LlmServerApi { get; set; } = "http://127.0.0.1:5000/";
    /// <summary>
    /// Indicates whether the application uses localhost for the LLM server.
    /// </summary>
    public bool Localhost { get; set; } = true; 
    /// <summary>
    /// Indicates whether the game should run in fullscreen mode.
    /// </summary>
    public bool FullScreen { get; set; } = false;
    /// <summary>
    /// The width of the game window.
    /// </summary>
    public int GameWindowWidth { get; set; } = 1920;
    /// <summary>
    /// The height of the game window.
    /// </summary>
    public int GameWindowHeight { get; set; } = 1080;
    
    /// <summary>
    /// The list of available models for the game.
    /// </summary>
    public List<ModelData>? Models { get; set; }
    /// <summary>
    /// The list of configured NPCs for the game.
    /// </summary>
    public List<NpcConfig>? Npcs { get; set; }

    /// <summary>
    /// The identifier of the currently selected narrator model.
    /// </summary>
    public int? NarratorModelId { get; set; }
}
