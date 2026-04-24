using UnityEngine;

namespace Prism
{
    // aynanin iki yerlesim tipi:
    // Slash    -> / seklinde, sag-yukari veya asagi-sol diagonal
    // BackSlash -> \ seklinde, sol-yukari veya asagi-sag diagonal
    public enum MirrorType
    {
        Slash,
        BackSlash
    }

    // grid uzerine yerlesen ayna
    // isin carptiginda yonu degistirir.
    // EdgeCollider2D otomatik olarak aynanin diagonal cizgisine yerlesir.
    [RequireComponent(typeof(EdgeCollider2D))]
    public class Mirror : MonoBehaviour
    {
        [Header("Ayna Tipi")]
        [SerializeField] private MirrorType type = MirrorType.Slash;

        [Tooltip("Aynanin cizgisinin uzunlugu (dunya birimi).")]
        [SerializeField] private float size = 0.8f;

        private EdgeCollider2D edgeCollider;

        public MirrorType Type => type;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            edgeCollider = GetComponent<EdgeCollider2D>();
            UpdateColliderPoints();
        }

        // collider noktalarini aynanin tipine gore ayarla
        // Edge collider 2 nokta arasi cizgi oluyor
        private void UpdateColliderPoints()
        {
            if (edgeCollider == null)
                edgeCollider = GetComponent<EdgeCollider2D>();

            float half = size / 2f;

            // yeni array her seferinde garbage uretir , class seviyesinde cache'leyebilirdik
            // ama bu sadece baslangicta calisir,bir defalik maliyet dert degil
            Vector2[] points = new Vector2[2];

            if (type == MirrorType.Slash)
            {
                // / seklinde, sol-alt'tan sag-ust'e
                points[0] = new Vector2(-half, -half);
                points[1] = new Vector2(half, half);
            }
            else
            {
                // \ seklinde, sol-ust'ten sag-alt'a
                points[0] = new Vector2(-half, half);
                points[1] = new Vector2(half, -half);
            }

            edgeCollider.points = points;
        }

        // eager update, editor'da degistirince hemen gozuksun
        // sadece editor'da calisir , runtime etkilemez
        private void OnValidate()
        {
            if (edgeCollider == null)
                edgeCollider = GetComponent<EdgeCollider2D>();
            if (edgeCollider != null)
                UpdateColliderPoints();
        }

        // gelen yon verilince yansima yonunu hesapla
        public Direction Reflect(Direction incoming)
        {
            // / ayna: (Right <-> Up) ve (Left <-> Down) takas eder
            // \ ayna: (Right <-> Down) ve (Left <-> Up) takas eder
            if (type == MirrorType.Slash)
            {
                return incoming switch
                {
                    Direction.Right => Direction.Up,
                    Direction.Up    => Direction.Right,
                    Direction.Left  => Direction.Down,
                    Direction.Down  => Direction.Left,
                    _               => incoming
                };
            }
            else // BackSlash
            {
                return incoming switch
                {
                    Direction.Right => Direction.Down,
                    Direction.Down  => Direction.Right,
                    Direction.Left  => Direction.Up,
                    Direction.Up    => Direction.Left,
                    _               => incoming
                };
            }
        }

        // Scene view'da aynayi gorsel olarak ciz
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            float half = size / 2f;
            Vector3 pos = transform.position;

            if (type == MirrorType.Slash)
            {
                Gizmos.DrawLine(pos + new Vector3(-half, -half, 0), pos + new Vector3(half, half, 0));
            }
            else
            {
                Gizmos.DrawLine(pos + new Vector3(-half, half, 0), pos + new Vector3(half, -half, 0));
            }
        }
    }
}
