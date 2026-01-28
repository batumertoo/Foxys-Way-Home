using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTile : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider;
    private Rigidbody2D rb;
    [SerializeField] private float breakDelay = 0.5f;  // Time before tile breaks
    [SerializeField] private float respawnDelay = 2f;  // Time before tile respawns
    [SerializeField] private TileBase breakableTileType; // Assign your breakable tile here
    private Dictionary<Vector3Int, TileBase> brokenTiles = new Dictionary<Vector3Int, TileBase>();
    private HashSet<Vector3Int> breakingTiles = new HashSet<Vector3Int>(); // Tiles currently breaking

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
        
        // Add static Rigidbody2D to prevent tilemap from moving
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void BreakTileAtPosition(Vector3 worldPosition)
    {
        // Offset slightly down to ensure we're inside the tile, not on the edge
        Vector3 adjustedPosition = worldPosition + new Vector3(0, -0.1f, 0);
        Vector3Int cellPos = tilemap.WorldToCell(adjustedPosition);
        TileBase tile = tilemap.GetTile(cellPos);

        // Only break if it's the designated breakable tile and not already broken/breaking
        if (tile != null && tile == breakableTileType && !brokenTiles.ContainsKey(cellPos) && !breakingTiles.Contains(cellPos))
        {
            StartCoroutine(BreakTileCoroutine(cellPos, tile));
        }
    }

    private IEnumerator BreakTileCoroutine(Vector3Int cellPos, TileBase tile)
    {
        breakingTiles.Add(cellPos);
        
        // Wait before breaking
        yield return new WaitForSeconds(breakDelay);
        
        breakingTiles.Remove(cellPos);
        brokenTiles[cellPos] = tile;
        tilemap.SetTile(cellPos, null);
        tilemapCollider.ProcessTilemapChanges();

        yield return new WaitForSeconds(respawnDelay);

        if (brokenTiles.ContainsKey(cellPos))
        {
            tilemap.SetTile(cellPos, brokenTiles[cellPos]);
            tilemapCollider.ProcessTilemapChanges();
            brokenTiles.Remove(cellPos);
        }
    }
}
