using UnityEngine;

namespace Prism
{
    // sinirli sekilde isinin gidecegi yonler
    // None = yansima yok , ozel durum icin
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    //  Direction enum yonlerini vector2 ye cevir
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
