using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class ChunkManager : Node
{
	[Export] private PackedScene _chunkScene;
	
	public static ChunkManager Instance;

	private Dictionary<Chunk, Vector2I> _chunkToPosition = new();
	private Dictionary<Vector2I, Chunk> _positionToChunk = new();

	private List<Chunk> _chunks;

	private int _viewDistance = 25;

	private Vector3 _playerPosition;
	private object _playerPositionLock = new();

	public override void _Ready()
	{
		Instance = this;
		BlockRegistry.Initialize();
		_chunks = GetParent().GetChildren().OfType<Chunk>().ToList();

		for (int i = _chunks.Count; i < _viewDistance * _viewDistance; i++)
		{
			var chunk = _chunkScene.Instantiate<Chunk>();
			GetParent().CallDeferred(Node.MethodName.AddChild, chunk);
			_chunks.Add(chunk);
		}

		for (int x = 0; x < _viewDistance; x++)
		{
			for (int y = 0; y < _viewDistance; y++)
			{
				var index = (y * _viewDistance) + x;
				var halfWidth = Mathf.FloorToInt(_viewDistance / 2f);
				_chunks[index].SetChunkPosition(new Vector2I(x - halfWidth, y - halfWidth));
			}
		}

		new Thread(new ThreadStart(ThreadProcess)).Start();
	}

	public void UpdateChunkPosition(Chunk chunk, Vector2I currentPos, Vector2I previousPos)
	{
		if (_positionToChunk.TryGetValue(previousPos, out var chunkAtPos) && chunkAtPos == chunk)
		{
			_positionToChunk.Remove(previousPos);
		}

		_chunkToPosition[chunk] = currentPos;
		_positionToChunk[currentPos] = chunk;
	}

	public void SetBlock(Vector3I globalPos, Block block)
	{
		var chunkTilePos = new Vector2I(Mathf.FloorToInt(globalPos.X / (float)Chunk.ChunkWidth),
			Mathf.FloorToInt(globalPos.Z / (float)Chunk.ChunkWidth));

		lock (_positionToChunk)
		{
			if (_positionToChunk.TryGetValue(chunkTilePos, out var chunk))
			{
				chunk.SetBlock((Vector3I)(globalPos - chunk.GlobalPosition), block);
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		lock (_playerPositionLock)
		{
			_playerPosition = Player.Instance.GlobalPosition;
		}
	}

	private void ThreadProcess()
	{
		while (IsInstanceValid(this))
		{
			int playerChunkX, playerChunkZ;
			lock (_playerPositionLock)
			{
				playerChunkX = Mathf.FloorToInt(_playerPosition.X / Chunk.ChunkWidth);
				playerChunkZ = Mathf.FloorToInt(_playerPosition.Z / Chunk.ChunkWidth);
			}

			foreach (var chunk in _chunks)
			{
				var chunkPosition = _chunkToPosition[chunk];

				var chunkX = chunkPosition.X;
				var chunkZ = chunkPosition.Y;
				
				var newChunkX = Mathf.PosMod(chunkX - playerChunkX + _viewDistance / 2, _viewDistance) + playerChunkX - _viewDistance / 2;
				var newChunkZ = Mathf.PosMod(chunkZ - playerChunkZ + _viewDistance / 2, _viewDistance) + playerChunkZ - _viewDistance / 2;

				if (newChunkX != chunkX || newChunkZ != chunkZ)
				{
					lock (_positionToChunk)
					{
						if (_positionToChunk.ContainsKey(chunkPosition))
						{
							_positionToChunk.Remove(chunkPosition);
						}

						var newPos = new Vector2I(newChunkX, newChunkZ);

						_chunkToPosition[chunk] = newPos;
						_positionToChunk[newPos] = chunk;

						chunk.CallDeferred(nameof(Chunk.SetChunkPosition), newPos);
					}
					
					Thread.Sleep(100);
				}
			}
			
			Thread.Sleep(100);
		}
	}
}
