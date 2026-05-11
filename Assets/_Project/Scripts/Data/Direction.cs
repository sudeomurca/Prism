using UnityEngine;

namespace Prism
{
    // isinin gidebilecegi yonler
    public enum Direction
    {
        None, //yansima yok
        Up,
        Down,
        Left,
        Right
    }

    // enum yonlerini vector2 ye cevir (raycast atarken,isin cizdirirken gerekli)
    //extension metodla enuma bagli metod yaz (DRY)
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
