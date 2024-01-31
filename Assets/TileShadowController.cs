using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileShadowController : MonoBehaviour
{
    public Tilemap tilemap;
    public Color baseColor = Color.white;
    public Color shadowColor = Color.black;
    public int depthThreshold = 3; // Set the depth threshold here
    private bool shadowActive = false; // Flag to activate/deactivate shadow effect

    void UpdateTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);

                if (tile != null)
                {
                    int depth = CalculateDepthFromOuterEdge(pos);

                    // Calculate the normalized depth value
                    float normalizedDepth = Mathf.Clamp01((float)depth / depthThreshold);

                    // Interpolate between baseColor and shadowColor based on depth
                    Color finalColor = Color.Lerp(baseColor, shadowColor, normalizedDepth);

                    tilemap.SetTileFlags(pos, TileFlags.None);
                    tilemap.SetColor(pos, finalColor);
                }
            }
        }
    }

    public void ActivateShadowEffect()
    {
        shadowActive = true;
        UpdateTiles();
    }

    public void DeactivateShadowEffect()
    {
        shadowActive = false;
        UpdateTiles();
    }

    int CalculateDepthFromOuterEdge(Vector3Int position)
    {
        int depth = int.MaxValue; // Initialize depth to a high value

        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);

                // Check if it's an empty space or non-ground tile
                if (tile == null)
                {
                    int dist = Mathf.Abs(position.x - x) + Mathf.Abs(position.y - y);
                    depth = Mathf.Min(depth, dist);
                }
            }
        }

        return depth;
    }
}
