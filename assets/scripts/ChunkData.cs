using Godot;

namespace SpudCraftGodot.assets.scripts;

public class ChunkData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 32;
    
    public Block[,,] Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];

    public ChunkData()
    {
        FillWithBlock(BlockRegistry.GetBlockByID("dirt"));
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
}