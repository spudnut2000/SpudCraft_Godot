using Godot;
using System;

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

    public Texture2D[] Textures => new Texture2D[]
        { Texture, TopTexture, BottomTexture, RightTexture, LeftTexture, FrontTexture, BackTexture };
    
    public string Name;
    
    public Block() { }
}
