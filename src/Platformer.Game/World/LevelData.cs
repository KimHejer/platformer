namespace Platformer.Game.World;

public class LevelData
{
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }

    public PositionData Spawn { get; set; } = new();

    public List<LevelObjectData> Objects { get; set; } = new();

    public string? TileMap { get; set; }
}

public class PositionData
{
    public float X { get; set; }
    public float Y { get; set; }
}

public class LevelObjectData
{
    public string Type { get; set; } = "";

    public int X { get; set; }
    public int Y { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    public string Direction { get; set; } = "up";
    public int Count { get; set; } = 1;

    public string EnemyType { get; set; } = "";
    public int Health { get; set; } = 100;
    public int Damage { get; set; } = 20;
    public bool RespawnAble { get; set; } = false;

    public float Radius { get; set; }
    public string TargetLevel { get; set; } = "";
    public string Sprite { get; set; } = "";
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
}