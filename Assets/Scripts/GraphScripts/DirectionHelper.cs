public static class DirectionHelper
{
    public static DoorDirection RotateDirection(DoorDirection originalDirection, int rotationIndex)
    {
        if (originalDirection == DoorDirection.None)
            return DoorDirection.None;

        int directionValue = (int)originalDirection;

        int newDirectionValue = (directionValue + rotationIndex) % 4;
        if (newDirectionValue < 0)
            newDirectionValue += 4;

        return (DoorDirection)newDirectionValue;
    }

    public static DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        if (direction == DoorDirection.None)
            return DoorDirection.None;

        int oppositeDirectionValue = ((int)direction + 2) % 4;

        return (DoorDirection)oppositeDirectionValue;
    }
}
