using Godot;
using System;
using System.Linq;

public partial class Chunk : StaticBody3D
{
	
	[Export] private MeshInstance3D _meshInstance;
	[Export] private CollisionShape3D _collisionShape;
	[Export] private FastNoiseLite _noise;
	
	public Vector2I ChunkPosition { get; private set; }
	
	public static int ChunkWidth = 16;
	public static int ChunkHeight = 32;
	
	private SurfaceTool _surfaceTool = new();
	
	private Block[,,] _blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];
	
	private readonly Vector3I[] _verticies = new Vector3I[]
	{
		new(0, 0, 0),
		new(1, 0, 0),
		new(0, 1, 0),
		new(1, 1, 0),
		new(0, 0, 1),
		new(1, 0, 1),
		new(0, 1, 1),
		new(1, 1, 1)
	};

	private readonly int[] _top = new[] { 2,3,7,6 };
	private readonly int[] _bottom = new[] { 0,4,5,1 };
	private readonly int[] _left = new[] { 6,4,0,2 };
	private readonly int[] _right = new[] { 3,1,5,7 };
	private readonly int[] _back = new[] { 7,5,4,6 };
	private readonly int[] _front = new[] { 2,0,1,3 };
	
	public void SetChunkPosition(Vector2I pos)
	{
		ChunkManager.Instance.UpdateChunkPosition(this, pos, ChunkPosition);
		ChunkPosition = pos;
		CallDeferred(Node3D.MethodName.SetGlobalPosition,
			new Vector3(ChunkPosition.X * ChunkWidth, 0, ChunkPosition.Y * ChunkWidth));
		
		Generate();
		UpdateChunk();
	}

	public void Generate()
	{
		for (int x = 0; x < ChunkWidth; x++)
		{
			for (int y = 0; y < ChunkHeight; y++)
			{
				for (int z = 0; z < ChunkWidth; z++)
				{
					//Block block = BlockRegistry.GetBlockByID("stone");
					//_blocks[x, y, z] = block;
					Block block;

					var globalBlockPos = ChunkPosition * new Vector2I(ChunkWidth, ChunkWidth) + new Vector2I(x,z);
					var groundHeight = (int)(ChunkHeight * (_noise.GetNoise2D(globalBlockPos.X, globalBlockPos.Y) + 1f) / 2f);
					//var groundHeight = _chunkHeight-1;

					if (y < groundHeight / 2)
					{
						block = BlockRegistry.GetBlockByID("stone");
					}
					else if (y < groundHeight)
					{
						block = BlockRegistry.GetBlockByID("dirt");
					}
					else if (y == groundHeight)
					{
						block = BlockRegistry.GetBlockByID("grass");
					}
					else
					{
						block = BlockRegistry.GetBlockByID("air");
					}
					
					_blocks[x, y, z] = block;
				}
			}
		}
	}

	public void UpdateChunk()
	{
		_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
		
		for (int x = 0; x < ChunkWidth; x++)
		{
			for (int y = 0; y < ChunkHeight; y++)
			{
				for (int z = 0; z < ChunkWidth; z++)
				{
					CreateBlockMesh(new(x,y,z));
				}
			}
		}
		
		_surfaceTool.SetMaterial(BlockRegistry.ChunkMaterial);
		var mesh = _surfaceTool.Commit();
		_meshInstance.Mesh = mesh;
		
		UpdateCollision();
	}

	public void SetBlock(Vector3I blockPosition, Block block)
	{
		_blocks[blockPosition.X, blockPosition.Y, blockPosition.Z] = block;
		UpdateChunk();
	}

	private void CreateBlockMesh(Vector3I blockPos)
	{
		
		//Create a block's mesh at blockPos using CreateFaceMesh and CheckTransparency for all sides of the block
		var block = _blocks[blockPos.X, blockPos.Y, blockPos.Z];

		if (block.Texture is null) return;
		
		if (CheckTransparency(blockPos + Vector3I.Up))
		{
			CreateFaceMesh(_top, blockPos, block.TopTexture ?? block.Texture);
		}
		if (CheckTransparency(blockPos + Vector3I.Down))
		{
			CreateFaceMesh(_bottom, blockPos, block.BottomTexture ?? block.Texture);
		}
		if (CheckTransparency(blockPos + Vector3I.Left))
		{
			CreateFaceMesh(_left, blockPos, block.LeftTexture ?? block.Texture);
		}
		if (CheckTransparency(blockPos + Vector3I.Right))
		{
			CreateFaceMesh(_right, blockPos, block.RightTexture ?? block.Texture);
		}
		if (CheckTransparency(blockPos + Vector3I.Back))
		{
			CreateFaceMesh(_back, blockPos, block.BackTexture ?? block.Texture);
		}
		if (CheckTransparency(blockPos + Vector3I.Forward))
		{
			CreateFaceMesh(_front, blockPos, block.FrontTexture ?? block.Texture);
		}
		
	}

	private void CreateFaceMesh(int[] face, Vector3I blockPos, Texture2D texture)
	{
		//Create block face, check for transparent blocks. Use the Texture property on _blocks[block.Position.X, block.Position.Y, block.Position.Z]
		var texturePos = BlockRegistry.GetTextureAtlasPosition(texture);
		var textureAtlasSize = BlockRegistry.TextureAtlasSize;

		var uvOffset = texturePos / textureAtlasSize;
		var uvWidth = 1f / textureAtlasSize.X;
		var uvHeight = 1f / textureAtlasSize.Y;

		var uvA = uvOffset + new Vector2(0, 0);
		var uvB = uvOffset + new Vector2(0, uvHeight);
		var uvC = uvOffset + new Vector2(uvWidth, uvHeight);
		var uvD = uvOffset + new Vector2(uvWidth, 0);
		
		var a = _verticies[face[0]] + blockPos;
		var b = _verticies[face[1]] + blockPos;
		var c = _verticies[face[2]] + blockPos;
		var d = _verticies[face[3]] + blockPos;

		var uvTri1 = new Vector2[] { uvA, uvB, uvC };
		var uvTri2 = new Vector2[] { uvA, uvC, uvD };

		var tri1 = new Vector3[] { a, b, c };
		var tri2 = new Vector3[] { a, c, d };

		var normal = ((Vector3)(c - a)).Cross((b - a)).Normalized();
		var normals = new Vector3[] { normal, normal, normal };
		
		_surfaceTool.AddTriangleFan(tri1, uvTri1, normals: normals);
		_surfaceTool.AddTriangleFan(tri2, uvTri2, normals: normals);
		
	}

	private bool CheckTransparency(Vector3I blockPosition)
	{
		if (blockPosition.X < 0 || blockPosition.X >= ChunkWidth) return true;
		if (blockPosition.Y < 0 || blockPosition.Y >= ChunkHeight) return true;
		if (blockPosition.Z < 0 || blockPosition.Z >= ChunkWidth) return true;
		
		return _blocks[blockPosition.X, blockPosition.Y, blockPosition.Z].IsTransparent;
	}

	private void UpdateCollision()
	{
		_collisionShape.Shape = _meshInstance.Mesh.CreateTrimeshShape();
		CollisionLayer = 1;
	}
}
