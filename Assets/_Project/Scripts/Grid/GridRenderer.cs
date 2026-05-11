using UnityEngine;

namespace Prism
{
    // grid'i oyuncuya gorunur sekilde ciz
    // her hucre icin tek bir sprite kullanir, pixel art uyumlu
    // eski sistemde runtime kare + border uretiyorduk, simdi tek sprite daha temiz ve performanslı
    [RequireComponent(typeof(GridSystem))]
    public class GridRenderer : MonoBehaviour
    {
        [Header("Hucre Gorunumu")]
        [Tooltip("Her hucre icin kullanilacak sprite. Aseprite'ta cizilmis pixel art.")]
        [SerializeField] private Sprite cellSprite;

        [Tooltip("Sprite'a uygulanacak renk tonu. Beyaz seersen sprite kendi renginde gozukur.")]
        [SerializeField] private Color cellTint = Color.white;

        [Tooltip("Hucrelerin render sirasi. Diger parcalarin altinda kalsin diye dusuk deger.")]
        [SerializeField] private int sortingOrder = -1;

        private GridSystem gridSystem;

        private void Awake()
        {
            gridSystem = GetComponent<GridSystem>();
        }

        private void Start()
        {
            BuildGrid();
        }

        private void BuildGrid()
        {
            if (cellSprite == null)
            {
                Debug.LogWarning("[GridRenderer] Cell sprite atanmamis. Inspector'dan sprite ver.");
                return;
            }

            float cellSize = gridSystem.CellSize;

            for (int x = 0; x < gridSystem.Columns; x++)
            {
                for (int y = 0; y < gridSystem.Rows; y++)
                {
                    Vector3 worldPos = gridSystem.GridToWorld(x, y);
                    CreateCell(worldPos, cellSize, x, y);
                }
            }
        }

        // her hucre icin tek bir SpriteRenderer, minimal GameObject
        private void CreateCell(Vector3 position, float size, int gridX, int gridY)
        {
            GameObject cell = new GameObject($"Cell_{gridX}_{gridY}");
            cell.transform.SetParent(transform);
            cell.transform.position = position;

            SpriteRenderer sr = cell.AddComponent<SpriteRenderer>();
            sr.sprite = cellSprite;
            sr.color = cellTint;
            sr.sortingOrder = sortingOrder;

            // sprite pixel boyutunu PPU'ya bol,dunya birimindeki gercek boyutunu bul
            // sonra hucre boyutuna getir , sprite ne olursa olsun tam oturur
            float spriteWorldWidth  = cellSprite.rect.width  / cellSprite.pixelsPerUnit;
            float spriteWorldHeight = cellSprite.rect.height / cellSprite.pixelsPerUnit;

            float scaleX = size / spriteWorldWidth;
            float scaleY = size / spriteWorldHeight;
            cell.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}
