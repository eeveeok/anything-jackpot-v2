using UnityEngine;
using UnityEngine.Tilemaps;

public class CannonEffect : MonoBehaviour
{
    [Header("이펙트 설정")]
    public float duration = 0.6f;

    [Header("타일 파괴 설정")]
    public float tileBreakRadius = 2.5f;
    public LayerMask breakableLayers;

    private float timer = 0f;
    private bool hasTriggered = false;

    void Start()
    {
        // 일정 시간 후 자동 파괴
        Destroy(gameObject, duration);
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    // 트리거 충돌 감지
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        // Breakable 태그가 있는 오브젝트와 충돌 시
        if (other.CompareTag("Breakable"))
        {
            hasTriggered = true;
            BreakObject(other.gameObject);
        }
    }

    // 오브젝트 파괴 처리
    private void BreakObject(GameObject breakableObject)
    {
        Debug.Log($"CannonEffect hit: {breakableObject.name}");

        // 1. 타일맵인지 확인
        Tilemap tilemap = breakableObject.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            // 타일맵 파괴
            BreakTilesInRadius(tilemap, transform.position);
        }
        else
        {
            // 일반 오브젝트 파괴
            Destroy(breakableObject);
        }

        // 이펙트 즉시 파괴 (선택사항)
        // Destroy(gameObject);
    }

    // 반경 내 타일 파괴
    private void BreakTilesInRadius(Tilemap tilemap, Vector2 center)
    {
        // 월드 좌표를 셀 좌표로 변환
        Vector3Int centerCell = tilemap.WorldToCell(center);
        int radius = Mathf.CeilToInt(tileBreakRadius / tilemap.cellSize.x);

        Debug.Log($"Breaking tiles around cell: {centerCell} with radius: {radius}");

        // 반경 내 모든 셀 확인
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int cell = centerCell + new Vector3Int(x, y, 0);

                // 거리 계산
                Vector3 cellWorldPos = tilemap.CellToWorld(cell) + tilemap.cellSize * 0.5f;
                float distance = Vector2.Distance(center, cellWorldPos);

                if (distance <= tileBreakRadius)
                {
                    // 타일이 있으면 제거
                    if (tilemap.HasTile(cell))
                    {
                        tilemap.SetTile(cell, null);
                        Debug.Log($"Tile destroyed at cell: {cell}");
                    }
                }
            }
        }
    }

    // 디버그 시각화
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tileBreakRadius);
    }
}