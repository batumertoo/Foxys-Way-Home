using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelGenerator : MonoBehaviour
{
    [Header("Tilemap Settings")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private TileBase grassTile;
    [SerializeField] private TileBase dirtTile;
    
    [Header("Level Settings")]
    [SerializeField] private int levelWidth = 50;
    [SerializeField] private int groundLevel = 5; // Y pozisyonu ground için
    [SerializeField] private float platformChance = 0.3f; // Platform oluşturma şansı
    [SerializeField] private int minPlatformLength = 3;
    [SerializeField] private int maxPlatformLength = 8;
    
    [Header("Prefab Settings")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject frogPrefab;
    [SerializeField] private GameObject eaglePrefab;
    [SerializeField] private Transform prefabParent; // Prefab'ları buraya yerleştir
    
    [Header("Spawn Rates")]
    [SerializeField] private float coinSpawnRate = 0.15f; // Her platform için coin şansı
    [SerializeField] private float enemySpawnRate = 0.1f; // Her platform için düşman şansı
    [SerializeField] private float minDistanceBetweenEnemies = 5f;
    
    private List<Vector3> platformPositions = new List<Vector3>();
    private List<Vector3> enemyPositions = new List<Vector3>();
    
    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("Ground Tilemap is not assigned!");
            return;
        }
        
        // Önce mevcut level'ı temizle
        ClearLevel();
        
        // Ground oluştur
        GenerateGround();
        
        // Platformlar oluştur
        GeneratePlatforms();
        
        // Prefab'ları yerleştir
        SpawnPrefabs();
        
        Debug.Log("Level generated successfully!");
    }
    
    private void ClearLevel()
    {
        // Tilemap'i temizle
        if (groundTilemap != null)
        {
            groundTilemap.ClearAllTiles();
        }
        
        // Prefab'ları temizle
        if (prefabParent != null)
        {
            for (int i = prefabParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(prefabParent.GetChild(i).gameObject);
            }
        }
        
        platformPositions.Clear();
        enemyPositions.Clear();
    }
    
    private void GenerateGround()
    {
        // Ana ground platformu oluştur
        for (int x = 0; x < levelWidth; x++)
        {
            // Üst katman (çimen)
            Vector3Int pos = new Vector3Int(x, groundLevel, 0);
            groundTilemap.SetTile(pos, grassTile);
            
            // Alt katmanlar (toprak)
            for (int y = groundLevel - 1; y >= groundLevel - 3; y--)
            {
                pos = new Vector3Int(x, y, 0);
                groundTilemap.SetTile(pos, dirtTile);
            }
        }
    }
    
    private void GeneratePlatforms()
    {
        platformPositions.Clear();
        
        for (int x = 5; x < levelWidth - 5; x += Random.Range(3, 8))
        {
            if (Random.value < platformChance)
            {
                int platformLength = Random.Range(minPlatformLength, maxPlatformLength + 1);
                int platformHeight = Random.Range(groundLevel + 2, groundLevel + 8);
                
                // Platform oluştur
                for (int px = 0; px < platformLength; px++)
                {
                    if (x + px < levelWidth)
                    {
                        Vector3Int pos = new Vector3Int(x + px, platformHeight, 0);
                        groundTilemap.SetTile(pos, grassTile);
                        
                        // Platform pozisyonunu kaydet (prefab yerleştirme için)
                        if (px == platformLength / 2) // Platformun ortası
                        {
                            platformPositions.Add(groundTilemap.CellToWorld(pos) + new Vector3(0.5f, 1f, 0));
                        }
                    }
                }
            }
        }
    }
    
    private void SpawnPrefabs()
    {
        if (prefabParent == null)
        {
            GameObject parent = new GameObject("GeneratedPrefabs");
            parent.transform.SetParent(transform);
            prefabParent = parent.transform;
        }
        
        foreach (Vector3 platformPos in platformPositions)
        {
            // Coin yerleştir
            if (coinPrefab != null && Random.value < coinSpawnRate)
            {
                Vector3 coinPos = platformPos + new Vector3(Random.Range(-1f, 1f), 0.5f, 0);
                InstantiatePrefab(coinPrefab, coinPos);
            }
            
            // Düşman yerleştir
            if (Random.value < enemySpawnRate)
            {
                // Düşmanlar arası minimum mesafe kontrolü
                bool tooClose = false;
                foreach (Vector3 enemyPos in enemyPositions)
                {
                    if (Vector3.Distance(platformPos, enemyPos) < minDistanceBetweenEnemies)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    Vector3 enemyPos = platformPos + new Vector3(0, 0, 0);
                    GameObject enemyPrefab = Random.value < 0.7f ? frogPrefab : eaglePrefab;
                    
                    if (enemyPrefab != null)
                    {
                        InstantiatePrefab(enemyPrefab, enemyPos);
                        enemyPositions.Add(enemyPos);
                    }
                }
            }
        }
    }
    
    private void InstantiatePrefab(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;
        
#if UNITY_EDITOR
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance != null)
        {
            instance.transform.position = position;
            instance.transform.SetParent(prefabParent);
        }
#else
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.SetParent(prefabParent);
#endif
    }
    
    [ContextMenu("Clear Level")]
    public void ClearLevelManual()
    {
        ClearLevel();
        Debug.Log("Level cleared!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        LevelGenerator generator = (LevelGenerator)target;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Level", GUILayout.Height(30)))
        {
            generator.GenerateLevel();
        }
        
        if (GUILayout.Button("Clear Level", GUILayout.Height(30)))
        {
            generator.ClearLevelManual();
        }
    }
}
#endif

