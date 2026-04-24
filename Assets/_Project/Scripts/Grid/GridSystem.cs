using UnityEngine;

namespace Prism
{
    // grid pozisyonu ile world pozisyonu arasindaki mantigi kur
    public class GridSystem : MonoBehaviour
    {
        [Header("Grid Boyutu")]
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 8;

        [Header("Kare Boyutu")]
        [Tooltip("Her karenin dunya birimindeki kenar uzunlugu.")]
        [SerializeField] private float cellSize = 1f;

        [Header("Debug Gorunum")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = new Color(0.4f, 0.4f, 0.6f, 0.5f);

        // disariya gerekli bilgileri acalim sadece okuma
        public int Columns => columns;
        public int Rows => rows;
        public float CellSize => cellSize;

        //  grid koordinatinin (x, y) , world pozisyonunu dondur
        public Vector3 GridToWorld(int x, int y)
        {
            float offsetX = -(columns - 1) * cellSize / 2f;
            float offsetY = -(rows - 1) * cellSize / 2f;

            float worldX = transform.position.x + offsetX + x * cellSize;
            float worldY = transform.position.y + offsetY + y * cellSize;

            return new Vector3(worldX, worldY, 0f);
        }

        // world pozisyonundan en yakin grid koordinatini hesapla
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            float offsetX = -(columns - 1) * cellSize / 2f;
            float offsetY = -(rows - 1) * cellSize / 2f;

            int x = Mathf.RoundToInt((worldPos.x - transform.position.x - offsetX) / cellSize);
            int y = Mathf.RoundToInt((worldPos.y - transform.position.y - offsetY) / cellSize);

            return new Vector2Int(x, y);
        }

        // verilen grid koordinati gecerli mi bak
        public bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < columns && y >= 0 && y < rows;
        }

        // debug icin
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = gizmoColor;

            
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 center = GridToWorld(x, y);
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f));
                }
            }
        }
    }
}
