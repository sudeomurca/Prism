using UnityEngine;

namespace Prism
{
    // Isinin yonu. 4 ana yon yeterli, capraz yok.
    // Sonra aynalar bu yonleri degistirecek.
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    // Yardimci: Direction enum'unu Vector2'ye cevirir.
    // Extension method kullandim, syntax daha temiz oluyor.
    public static class DirectionExtensions
    {
        public static Vector2 ToVector(this Direction dir) => dir switch
        {
            Direction.Up    => Vector2.up,
            Direction.Down  => Vector2.down,
            Direction.Left  => Vector2.left,
            Direction.Right => Vector2.right,
            _               => Vector2.zero
        };
    }
}
