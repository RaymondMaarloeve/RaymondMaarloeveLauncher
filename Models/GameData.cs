using System.Collections.Generic;

namespace RaymondMaarloeveLauncher.Models;

public class GameData
{
    public int Revision { get; set; } = 1;
    public string LlmServerApi { get; set; } = "http://127.0.0.1:5000/";
    public bool Localhost { get; set; } = false; 
    public bool FullScreen { get; set; } = false;
    public int GameWindowWidth { get; set; } = 1920;
    public int GameWindowHeight { get; set; } = 1080;
    
    public List<ModelData>? Models { get; set; }
    public List<NpcConfig>? Npcs { get; set; }
}
