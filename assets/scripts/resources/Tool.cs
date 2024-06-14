using Godot;

namespace SpudCraftGodot.assets.scripts.resources;

[GlobalClass]
public partial class Tool : Resource
{
    [Export] public ToolType ToolType;
    [Export] public float Efficiency;
    
    public string Name;
    
    public Tool(){}
}