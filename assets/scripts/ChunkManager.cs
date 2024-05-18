using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Godot;

namespace SpudCraftGodot.assets.scripts;

public partial class ChunkManager : Node
{
    public bool IsRunning { get; set; } = true;
    
    private List<Chunk> _chunks = new();
    private Dictionary<Chunk, Vector2I> _chunkToPosition = new();
    private Dictionary<Vector2I, Chunk> _positionToChunk = new();
    
    
    private ConcurrentQueue<Chunk> _chunkQueue = new();
    private Thread _chunkUpdateThread;
    private Thread _chunkPositionUpdateThread;

    private Vector3 _playerPosition;
    private object _playerPositionLock = new();
    
    private int _renderDistance = 5;

    public override void _Ready()
    {
        PrepareChunks();
        
        _chunkUpdateThread = new Thread(UpdateChunksThreadProcess);
        _chunkUpdateThread.Start();
        
        _chunkPositionUpdateThread = new Thread(UpdateChunkPositionsThreadProcess);
        _chunkPositionUpdateThread.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        lock (_playerPositionLock)
        {
            _playerPosition = World.Instance.Player.GlobalPosition;
        }
    }

    public void UpdateChunkPosition(Chunk chunk, Vector2I currentPosition, Vector2I previousPosition)
    {
        if (_positionToChunk.TryGetValue(previousPosition, out var chunkAtPos) && chunkAtPos == chunk)
        {
            _positionToChunk.Remove(previousPosition);
        }
        
        _chunkToPosition[chunk] = currentPosition;
        _positionToChunk[currentPosition] = chunk;
    }
    
    public void SetBlock(Vector3I globalPosition, Block block)
    {
        var chunkTilePosition = new Vector2I(Mathf.FloorToInt(globalPosition.X / (float)ChunkData.ChunkWidth), 
            Mathf.FloorToInt(globalPosition.Z / (float)ChunkData.ChunkWidth));

        lock (_positionToChunk)
        {
            if (_positionToChunk.TryGetValue(chunkTilePosition, out var chunk))
            {
                chunk.SetBlock((Vector3I)(globalPosition - chunk.GlobalPosition), block);
            }
        }
    }

    private void PrepareChunks()
    {
        for (int i = _chunks.Count; i < _renderDistance * _renderDistance; i++)
        {
            var chunk = new Chunk();
            CallDeferred(Node.MethodName.AddChild, chunk);
            _chunks.Add(chunk);
        }

        for (int x = 0; x < _renderDistance; x++)
        {
            for (int y = 0; y < _renderDistance; y++)
            {
                var index = (y * _renderDistance) + x;
                var halfWidth = Mathf.FloorToInt(_renderDistance / 2f);
                _chunks[index].SetChunkPosition(new(x - halfWidth, y - halfWidth));
            }
        }
    }

    private void UpdateChunksThreadProcess()
    {
        while (IsRunning)
        {
            if (_chunkQueue.TryDequeue(out Chunk chunk))
            {
                chunk.Update();
            }
            else
            {
                Thread.Sleep(10);
            }
        }
    }

    private void UpdateChunkPositionsThreadProcess()
    {
        while (IsRunning)
        {
            int playerChunkX, playerChunkZ;

            lock (_playerPositionLock)
            {
                playerChunkX = Mathf.FloorToInt(_playerPosition.X / ChunkData.ChunkWidth);
                playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / ChunkData.ChunkWidth);
            }

            foreach (var chunk in _chunks)
            {
                var chunkPosition = _chunkToPosition[chunk];
                
                var chunkX = chunkPosition.X;
                var chunkZ = chunkPosition.Y;
                
                var newChunkX = Mathf.PosMod(chunkX - playerChunkX + _renderDistance / 2, _renderDistance) + playerChunkX - _renderDistance / 2;
                var newChunkZ = Mathf.PosMod(chunkZ - playerChunkZ + _renderDistance / 2, _renderDistance) + playerChunkZ - _renderDistance / 2;
                
                if (newChunkX != chunkX || newChunkZ != chunkZ)
                {
                    lock (_positionToChunk)
                    {
                        if (_positionToChunk.ContainsKey(chunkPosition))
                        {
                            _positionToChunk.Remove(chunkPosition);
                        }
                        
                        var newChunkPosition = new Vector2I(newChunkX, newChunkZ);
                        
                        _chunkToPosition[chunk] = newChunkPosition;
                        _positionToChunk[newChunkPosition] = chunk;

                        chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newChunkPosition);
                    }
                    
                    Thread.Sleep(10);
                }
            }
            Thread.Sleep(10);
        }
    }
}