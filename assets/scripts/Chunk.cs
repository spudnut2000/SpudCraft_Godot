using Godot;

namespace SpudCraftGodot.assets.scripts;

public partial class Chunk : StaticBody3D
{
    public Vector2I ChunkPosition { get; private set; }
    public ChunkData Data { get; set; } = new ChunkData();
    
    private MeshInstance3D _meshInstance = new();
    private CollisionShape3D _collisionShape = new();
    
    private SurfaceTool _surfaceTool = new();
    
    public override void _Ready()
    {
        CollisionLayer = 1;
        
        AddChild(_meshInstance);
        AddChild(_collisionShape);
        
        UpdateCollisionShape();
    }

    public void Update()
    {
        LoadChunkData();
        UpdateMesh();
        UpdateCollisionShape();
        
        ChunkManager.HasDeferredCall = false;
    }
    
    public void SetChunkPosition(Vector2I position, bool performUpdate)
    {
        World.Instance.ChunkManager.UpdateChunkPosition(this, position, ChunkPosition);
        ChunkPosition = position;
        CallDeferred(Node3D.MethodName.SetGlobalPosition, new Vector3(position.X * ChunkData.ChunkWidth, 0, position.Y * ChunkData.ChunkWidth));
        if (performUpdate)
        {
            Update();
        }
    }
    
    public void SetBlock(Vector3I position, Block block)
    {
        Data.Blocks[position.X, position.Y, position.Z] = block;
        ChunkManager.QueueChunk(this);
    }
    
    public Block GetBlock(Vector3I position)
    {
        return Data.Blocks[position.X, position.Y, position.Z];
    }

    public void LoadChunkData(ChunkData data = null)
    {
        if (data is not null) Data = data;
        
        for (int x = 0; x < ChunkData.ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.ChunkWidth; z++)
                {
                    Block block = Data.Blocks[x, y, z];
                    if (block is null) Data.Blocks[x, y, z] = BlockRegistry.GetBlockByID("air");
                }
            }
        }
    }

    private void UpdateMesh()
    {
        _surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

        for (int x = 0; x < ChunkData.ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkData.ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkData.ChunkWidth; z++)
                {
                    CreateBlock(new(x,y,z));
                }
            }
        }
        
        _surfaceTool.SetMaterial(BlockRegistry.ChunkMaterial);
        _meshInstance.Mesh = _surfaceTool.Commit();
    }

    private void CreateBlock(Vector3I blockPosition)
    {
        var block = Data.Blocks[blockPosition.X, blockPosition.Y, blockPosition.Z];
        
        if (block.Texture is null) return;

        if (CheckIfTransparentBlock(blockPosition + Vector3I.Up))
        {
            CreateBlockFace(MeshHelper.CubeTopFace, blockPosition, block.TopTexture ?? block.Texture);
        }
        
        if (CheckIfTransparentBlock(blockPosition + Vector3I.Down))
        {
            CreateBlockFace(MeshHelper.CubeBottomFace, blockPosition, block.BottomTexture ?? block.Texture);
        }
        
        if (CheckIfTransparentBlock(blockPosition + Vector3I.Left))
        {
            CreateBlockFace(MeshHelper.CubeLeftFace, blockPosition, block.LeftTexture ?? block.Texture);
        }
        
        if (CheckIfTransparentBlock(blockPosition + Vector3I.Right))
        {
            CreateBlockFace(MeshHelper.CubeRightFace, blockPosition, block.RightTexture ?? block.Texture);
        }
        
        if (CheckIfTransparentBlock(blockPosition + Vector3I.Back))
        {
            CreateBlockFace(MeshHelper.CubeBackFace, blockPosition, block.BackTexture ?? block.Texture);
        }
        
        if (CheckIfTransparentBlock(blockPosition + Vector3I.Forward))
        {
            CreateBlockFace(MeshHelper.CubeFrontFace, blockPosition, block.FrontTexture ?? block.Texture);
        }
    }

    private Vector2[] uvs = new Vector2[4];
    private Vector3[] verts = new Vector3[4];
    private Vector3[] normals = new Vector3[4];


    private static Vector2 textureAtlasSize = BlockRegistry.TextureAtlasSize;
    private static float uvWidth = 1f / textureAtlasSize.X;
    private static float uvHeight = 1f / textureAtlasSize.Y;

    private Vector2 uv1 = new Vector2(0, uvHeight); 
    private Vector2 uv2 = new Vector2(uvWidth, uvHeight); 
    private Vector2 uv3 = new Vector2(uvWidth, 0); 
    
    private void CreateBlockFace(int[] face, Vector3I blockPosition, Texture2D texture)
    {
        //Create block face, check for transparent blocks. Use the Texture property on _blocks[block.Position.X, block.Position.Y, block.Position.Z]
        var texturePos = BlockRegistry.GetTextureAtlasPosition(texture);

        var uvOffset = texturePos / textureAtlasSize;
        
        uvs[0] = uvOffset;
        uvs[1] = uvOffset + uv1;
        uvs[2] = uvOffset + uv2;
        uvs[3] = uvOffset + uv3;
		
        verts[0] = MeshHelper.CubeVertices[face[0]] + blockPosition;
        verts[1] = MeshHelper.CubeVertices[face[1]] + blockPosition;
        verts[2] = MeshHelper.CubeVertices[face[2]] + blockPosition;
        verts[3] = MeshHelper.CubeVertices[face[3]] + blockPosition;

        // var uvTri1 = new Vector2[] { uvA, uvB, uvC, uvD };
        // var uvTri2 = new Vector2[] { uvA, uvC, uvD };

        // var tri1 = new Vector3[] { a, b, c, d };
        // var tri2 = new Vector3[] { a, c, d };

        var normal = (verts[2] - verts[0]).Cross(verts[1] - verts[0]).Normalized();
        normals[0] = normals[1] = normals[2] = normals[3] = normal;

        _surfaceTool.AddTriangleFan(verts, uvs, normals: normals);
        // _surfaceTool.AddTriangleFan(tri2, uvTri2, normals: normals);      
    }
    
    private bool CheckIfTransparentBlock(Vector3I blockPosition)
    {
        if (blockPosition.X < 0 || blockPosition.X >= ChunkData.ChunkWidth ||
            blockPosition.Y < 0 || blockPosition.Y >= ChunkData.ChunkHeight ||
            blockPosition.Z < 0 || blockPosition.Z >= ChunkData.ChunkWidth) return true;
        
        return Data.Blocks[blockPosition.X, blockPosition.Y, blockPosition.Z].IsTransparent;
    }
    
    private void UpdateCollisionShape()
    {
        _collisionShape.Shape = _meshInstance.Mesh.CreateTrimeshShape();
    }
}