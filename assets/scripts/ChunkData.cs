using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;

namespace SpudCraftGodot.assets.scripts;

public class ChunkData
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    
    public Block[,,] Blocks = new Block[ChunkWidth, ChunkHeight, ChunkWidth];

    public ChunkData()
    {
        //CreateDefaultSuperflat();
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
    
    RandomNumberGenerator numberGenerator = new();
    public void GenerateWorldData(Vector2I chunkPos)
    {
        ConcurrentDictionary<Vector2I, float> noiseCache = new ConcurrentDictionary<Vector2I, float>();

        Parallel.For(0, ChunkWidth, x =>
        {
            for (int z = 0; z < ChunkWidth; z++)
            {
                Vector2I noisePos = new Vector2I(chunkPos.X * ChunkWidth + x, chunkPos.Y * ChunkWidth + z);
                float noiseValue = noiseCache.GetOrAdd(noisePos, pos => World.Instance.Noise.GetNoise2D(pos.X * 0.05f, pos.Y * 0.05f));
                int height = (int)Mathf.Lerp(ChunkHeight / 2, ChunkHeight, noiseValue);

                // numberGenerator.Randomize();
                // var height = numberGenerator.RandiRange(0, ChunkHeight);

                //var height = 40;

                for (int y = 0; y < ChunkHeight; y++)
                {
                    if (y == 0)
                    {
                        Blocks[x,y,z] = BlockRegistry.GetBlockByID("bedrock");
                    }
                    else if (y < height / 2)
                    {
                        Blocks[x,y,z] = BlockRegistry.GetBlockByID("stone");
                    }
                    else if (y < height)
                    {
                        Blocks[x,y,z] = BlockRegistry.GetBlockByID("dirt");
                    }
                    else if (y == height)
                    {
                        Blocks[x,y,z] = BlockRegistry.GetBlockByID("grass");
                    }
                    else
                    {
                        Blocks[x,y,z] = BlockRegistry.GetBlockByID("air");
                    }
                }
            }
        });
    }
}