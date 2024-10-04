// DirectionHelper.cs
public static class DirectionHelper
{
    /// <summary>
    /// Rotaciona uma direção em passos de 90 graus.
    /// </summary>
    /// <param name="direction">Direção original.</param>
    /// <param name="rotationSteps">Número de rotações de 90 graus (0-3).</param>
    /// <returns>Nova direção após rotação.</returns>
    public static DoorDirection RotateDirection(DoorDirection direction, int rotationSteps)
    {
        int dirValue = (int)direction;
        int rotatedValue = (dirValue + rotationSteps) % 4;
        return (DoorDirection)rotatedValue;
    }

    /// <summary>
    /// Obtém a direção oposta de uma direção dada.
    /// </summary>
    /// <param name="direction">Direção original.</param>
    /// <returns>Direção oposta.</returns>
    public static DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        return (DoorDirection)(((int)direction + 2) % 4);
    }
}
