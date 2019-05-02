
public enum CellDirection
{
    NE, E, SE, SW, W, NW
}

public static class CellDirectionExtensions
{
    public static CellDirection Opposite (this CellDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }
}

