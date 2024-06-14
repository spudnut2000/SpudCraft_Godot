using Godot;
using System;
using SpudCraftGodot.assets.scripts;

[GlobalClass]
public partial class Block : Resource
{
    [ExportCategory("Textures")]
    [Export] public Texture2D Texture;
    [ExportGroup("Side Overrides")]
    [Export] public Texture2D TopTexture;
    [Export] public Texture2D BottomTexture;
    [Export] public Texture2D RightTexture;
    [Export] public Texture2D LeftTexture;
    [Export] public Texture2D FrontTexture;
    [Export] public Texture2D BackTexture;

    [ExportCategory("Block Properties")]
    [Export] public bool IsTransparent;
    [Export] public bool IsUnbreakable;
    
    [Export] public float Hardness = 1;
    [Export] public ToolType RequiredToolType = ToolType.None;
    [Export] public ToolType PreferredToolType = ToolType.None;

    [Export] public string MetaData = "";

    public Texture2D[] Textures => new[] { Texture, TopTexture, BottomTexture, RightTexture, LeftTexture, FrontTexture, BackTexture };

    public string Name;
    
    public Block() { }
}
