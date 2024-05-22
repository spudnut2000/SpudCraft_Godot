using Godot;
using System.Collections.Generic;
using System.Linq;

public static class BlockRegistry
{
	public static string BlockResourcePath = "res://assets/blocks";
	
	public static Dictionary<string, Block> Blocks => _blocks;

	private static Dictionary<string, Block> _blocks = new();

	public static Vector2I BlockTextureSize { get; } = new(16,16);
	public static Vector2 TextureAtlasSize { get; private set; }
	public static StandardMaterial3D ChunkMaterial { get; private set; }

	private static readonly Dictionary<Texture2D, Vector2I> _atlasLookup = new();
	private static int _gridHeight;
	private static int _gridWidth = 4;

	public static void Initialize()
	{
		PopulateRegistry();
		CreateTextureAtlas();
	}

	public static Block GetBlockByID(string id)
	{
		if (_blocks.TryGetValue(id, out var byId))
		{
			return byId;
		}
		
		GD.PrintRich($"[color=yellow]BlockManager: Could not find Block id: {id}. Returning first block in registry.[/color]");
		return _blocks.Values.First();
	}
	
	public static Vector2I GetTextureAtlasPosition(Texture2D texture)
	{
		if (texture == null)
		{
			return Vector2I.Zero;
		}

		return _atlasLookup[texture];
	}
	
	private static void PopulateRegistry()
	{
		var dir = DirAccess.Open(BlockResourcePath);

		if (dir != null)
		{
			dir.ListDirBegin();
			string fileName = dir.GetNext();
			while (!string.IsNullOrEmpty(fileName))
			{
				if (!dir.CurrentIsDir())
				{
					fileName = fileName.Replace(".remap", "");
					Block block = ResourceLoader.Load<Block>($"{BlockResourcePath}/{fileName}");
					block.Name = fileName.Replace(".tres", "");

					if (!_blocks.TryAdd(block.Name, block))
					{
						GD.PrintErr($"BlockManager: Trying to add block {block.Name} but a block with that name already exists.");
						break;
					}
					
					GD.Print($"BlockRegistry: Registering block `{block.Name}`");
				}

				fileName = dir.GetNext();
			}
		}
		else
		{
			GD.PrintErr($"BlockManager: Could not open path {BlockResourcePath}");
		}
	}
	
	private static void CreateTextureAtlas()
	{
		List<Texture2D> blockTextures = new List<Texture2D>();

		int index = 0;
		foreach (var block in _blocks.Values)
		{
			foreach (var texture in block.Textures)
			{
				if (texture != null && !_atlasLookup.ContainsKey(texture))
				{
					blockTextures.Add(texture);
					_atlasLookup.Add(texture, new Vector2I(index % _gridWidth, index / _gridWidth));
					index++;
				}
			}
		}
		

		_gridHeight = Mathf.CeilToInt(index / (float)_gridWidth);

		var image = Image.Create(_gridWidth * BlockTextureSize.X, _gridHeight * BlockTextureSize.Y, false, Image.Format.Rgba8);

		foreach (var kvp in _atlasLookup)
		{
			var texture = kvp.Key;
			var position = kvp.Value;

			var currentImage = texture.GetImage();
			currentImage.Convert(Image.Format.Rgba8);

			image.BlitRect(currentImage, new Rect2I(Vector2I.Zero, BlockTextureSize), position * BlockTextureSize);
		}

		var textureAtlas = ImageTexture.CreateFromImage(image);

		var success = ResourceSaver.Save(textureAtlas, "res://assets/textures/blocks_atlas.res");
		GD.Print(success);

		ChunkMaterial = new StandardMaterial3D()
		{
			AlbedoTexture = textureAtlas,
			TextureFilter = BaseMaterial3D.TextureFilterEnum.Nearest,
			ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel
		};

		TextureAtlasSize = new Vector2(_gridWidth, _gridHeight);

		GD.Print($"Done loading {blockTextures.Count} images to make {_gridWidth} x {_gridHeight} atlas");
	}

	
}
