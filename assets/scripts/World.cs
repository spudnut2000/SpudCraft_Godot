using Godot;
using System;
using System.Collections.Concurrent;
using InGameConsole;
using SpudCraftGodot.assets.scripts;

public partial class World : Node3D
{

	[Export] public FastNoiseLite Noise;
	
	public static World Instance;
	public ChunkManager ChunkManager { get; private set; } = new();
	public Player Player { get; set; }
	
	[Export] private PackedScene _playerScene;
	
	public override void _Ready()
	{
		if (Instance is not null) return;
		Instance = this;
		
		this.Player = _playerScene.Instantiate() as Player;
		
		CallDeferred(Node.MethodName.AddChild, this.Player);
		CallDeferred(Node.MethodName.AddChild, this.ChunkManager);

		this.Player.CallDeferred(Node3D.MethodName.SetGlobalPosition, new Vector3(0, 128, 0));
		
		BlockRegistry.Initialize();
		
	}
	
	[Command(CommandName = "SetBlock", Description = "Set a block at a specific position")]
	public static void SetBlockCommand(Vector3I position, Block block)
	{
		Instance.ChunkManager.SetBlock(position, block);
	}
	
}
