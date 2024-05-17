using Godot;
using System;

public partial class World : Node3D
{

	[Export] public FastNoiseLite Noise;
	
	public static World Instance;
	
	public override void _Ready()
	{
		if (Instance is not null) return;
		Instance = this;
		
		

		// for (int x = 0; x < _worldSize; x++)
		// {
		// 	for (int z = 0; z < _worldSize; z++)
		// 	{
		// 		Chunk chunk = new Chunk();
		// 		chunk.Position = new(x*ChunkWidth,0,z*ChunkWidth);
		// 		chunk.Name = $"Chunk_{x}-{z}";
		// 		AddChild(chunk);
		// 	}
		// }
	}
}
