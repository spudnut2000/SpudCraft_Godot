using Godot;

namespace SpudCraftGodot.assets.scripts;

public class ChunkData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    
    public Block[,,] Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];

    public ChunkData()
    {
        CreateDefaultSuperflat();
        //FillWithBlock(BlockRegistry.GetBlockByID("dirt"));
    }
    
    public void FillWithBlock(Block block)
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    Blocks[x, y, z] = block;
                }
            }
        }
    }

    public void CreateDefaultSuperflat()
    {
        for (int x = 0; x < ChunkWidth; x++)
        {
            for (int y = 0; y < ChunkWidth; y++)
            {
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (y == 0)
                    {
                        Blocks[x, y, z] = BlockRegistry.GetBlockByID("bedrock");
                    }
                    else if (y <= 6)
                    {
                        Blocks[x, y, z] = BlockRegistry.GetBlockByID("stone");
                    }
                    else if (y <= 10 && y >= 7)
                    {
                        Blocks[x, y, z] = BlockRegistry.GetBlockByID("dirt");
                    }
                    else if (y == 11)
                    {
                        Blocks[x, y, z] = BlockRegistry.GetBlockByID("grass");
                    }
                    else
                    {
                        Blocks[x, y, z] = BlockRegistry.GetBlockByID("air");
                    }
                }
            }
        }
    }
}