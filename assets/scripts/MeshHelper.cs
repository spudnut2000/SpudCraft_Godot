using Godot;

namespace SpudCraftGodot.assets.scripts;

public static class MeshHelper
{
    public static readonly Vector3I[] CubeVertices = new Vector3I[]
    {
        new(0,0,0),
        new(1,0,0),
        new(0,1,0),
        new(1,1,0),
        new(0,0,1),
        new(1,0,1),
        new(0,1,1),
        new(1,1,1)
    };

    public static readonly int[] CubeTopFace = { 2, 3, 7, 6 };
    public static readonly int[] CubeBottomFace = { 0, 4, 5, 1 };
    public static readonly int[] CubeLeftFace = { 6, 4, 0, 2 };
    public static readonly int[] CubeRightFace = { 3, 1, 5, 7 };
    public static readonly int[] CubeBackFace = { 7, 5, 4, 6 };
    public static readonly int[] CubeFrontFace = { 2, 0, 1, 3 };
}