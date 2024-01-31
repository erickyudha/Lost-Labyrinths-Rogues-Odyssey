using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;



public class LevelGenerator : MonoBehaviour
{
    public GameManager gameManager;
    public Grid spawnGrid;
    public LevelGraphGen levelGraphGen;
    // private List<string> mapLayers = new(){"Decoration", "Background", "Platforms", "Indicator Map", "Foreground Decoration", "Danger Foreground"};
    private readonly List<string> mapLayers = new(){"Platforms", "Indicator Map"};
    // Offset for spawning rooms
    public Vector2Int spawnOffset = new(0, 0); // Adjust as needed

    public Tilemap defaultTilemap;
    public TileBase defaultTile;

    public TileBase roomLimitTile;
    public TileBase spawnTile;
    public TileBase enemyTile;
    public TileBase dangerTile;
    public TileBase shopTile;
    public TileBase treasureTile;

    public float enemySpawnCoefficient = 10f;
    public EnemyManager enemyManager;

    public GameObject player;
    public GameObject playerAfterImagePool;

    public TileBase spikeRuleTile;
    public Tilemap spikeTilemap;
    public float spikeSpawnCoefficient = 20f;

    private Vector3 playerSpawnPos = Vector3.zero;

    private List<Vector3Int> enemyTilePos = new();
    private List<Vector3Int> dangerTilePos = new();
    private Vector2Int defaultRoomSize;

    public TileBase mapTile;
    public Tilemap mapTilemap;

    public GameObject treasurePrefab;
    public GameObject shopPrefab;
    private List<Vector3Int> treasurePosList = new();
    private List<Vector3Int> shopPosList = new();

    public TileShadowController tileShadowController;

    public TileBase bossTile;
    public TileBase exitTile;
    public GameObject exitPrefab;

    private GameObject boss;
    private Vector3Int bossTilePos;
    private Vector3Int exitTilePos;
    private bool bossSpawned;

    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false);
        playerAfterImagePool.SetActive(false);
        bossSpawned = false;

        bool isGraphValid = false;
        Dictionary<Vector2Int, Room> levelGrid = new();
        while (!isGraphValid)
        {
            levelGraphGen.GenerateLevelGraph();
            levelGrid = levelGraphGen.GetLastGeneratedLevelgrid();

            isGraphValid = CheckGraphValid(levelGrid);
        }
        SpawnLevel(levelGrid);
    }

    private bool CheckGraphValid(Dictionary<Vector2Int, Room> levelGrid)
    {
        int startRoomCount = 0;
        int treasureRoomCount = 0;
        int bossRoomCount = 0;
        int shopRoomCount = 0;

        foreach (var pos in levelGrid.Keys)
        {
            Room room = levelGrid[pos];

            if (room == null)
            {
                // Skip empty positions
                continue;
            }

            switch (room.roomType)
            {
                case RoomType.Start:
                    startRoomCount++;
                    break;
                case RoomType.Treasure:
                    treasureRoomCount++;
                    break;
                case RoomType.Boss:
                    bossRoomCount++;
                    break;
                case RoomType.Shop:
                    shopRoomCount++;
                    break;
                default:
                    break;
            }

            // Check if the room is connected in all entrance directions
            if (!AreAllDirectionsConnected(levelGrid, pos, room))
            {
                return false;
            }
        }

        return startRoomCount > 0 && treasureRoomCount > 0 && bossRoomCount > 0 && shopRoomCount > 0;
    }

    private bool AreAllDirectionsConnected(Dictionary<Vector2Int, Room> levelGrid, Vector2Int pos, Room room)
    {
        foreach (var entranceDir in room.entrances)
        {
            if (!IsDirectionConnected(levelGrid, pos, entranceDir))
            {
                return false;
            }
        }
        return true;
    }

    private bool IsDirectionConnected(Dictionary<Vector2Int, Room> levelGrid, Vector2Int pos, Room.EntranceDirection direction)
    {
        Vector2Int adjacentPos = GetAdjacentPosition(pos, direction);
        
        if (levelGrid.TryGetValue(adjacentPos, out Room connectedRoom))
        {
            if (connectedRoom != null)
            {
                // Check if the connected room has an entrance in the opposite direction
                Room.EntranceDirection oppositeDir = Room.GetOppositeDirection(direction);
                return connectedRoom.entrances.Contains(oppositeDir);
            }
        }
        
        return false;
    }

    private Vector2Int GetAdjacentPosition(Vector2Int pos, Room.EntranceDirection direction)
    {
        return direction switch
        {
            Room.EntranceDirection.North => new Vector2Int(pos.x, pos.y + 1),
            Room.EntranceDirection.South => new Vector2Int(pos.x, pos.y - 1),
            Room.EntranceDirection.East => new Vector2Int(pos.x + 1, pos.y),
            Room.EntranceDirection.West => new Vector2Int(pos.x - 1, pos.y),
            _ => pos,// Handle edge cases or errors
        };
    }

    
    void Update()
    {   
        if (bossSpawned && !boss.activeSelf)
        {
            SpawnExit();
            bossSpawned = false;
        }
    }

    private void SpawnLevel(Dictionary<Vector2Int, Room> levelGrid)
    {
        // Loop through the level graph and spawn rooms
        SpawnRoom(levelGrid, spawnOffset);
    }

    private void SpawnRoom(Dictionary<Vector2Int, Room> levelGrid, Vector2Int posOffset)
    {
        foreach (var posRoomPair in levelGrid)
        {
            List<Tilemap> roomTilemaps = GetRoomTilemaps(posRoomPair.Value);
            Vector2Int roomGridPos = posRoomPair.Key;

            foreach (var tilemap in roomTilemaps)
            {

                // Debug.Log(tilemap.name);
                Tilemap spawnTilemap = spawnGrid.transform.Find(tilemap.name).GetComponent<Tilemap>();
                // check if tilemap size correct
                TileBase[] roomTilebase = tilemap.GetTilesBlock(tilemap.cellBounds);
                TrimTilebase(roomTilebase, tilemap.size.x, tilemap.size.y, out TileBase[] trimmedTilebase, out Vector2Int trimmedRoomSize);
                if (defaultRoomSize == Vector2Int.zero && tilemap.name == defaultTilemap.name)
                {
                    defaultRoomSize = trimmedRoomSize;
                }

                for (int y = 0; y < trimmedRoomSize.y; y++)                                                                         
                {
                    for (int x = 0; x < trimmedRoomSize.x; x++)
                    {   
                        // Debug.Log(trimmedRoomSize.ToString() + "Accessing tile " + ((y * trimmedRoomSize.x) + x) + " out of " + trimmedTilebase.Length + " " + x + " " + y);
                        TileBase tile = trimmedTilebase[(y * trimmedRoomSize.x) + x];

                        int xAnchor = roomGridPos.x * trimmedRoomSize.x;
                        int yAnchor = roomGridPos.y * trimmedRoomSize.y;

                        Vector3Int tileSpawnPos = Vector3Int.zero;
                        tileSpawnPos.x = xAnchor + x + posOffset.x;
                        tileSpawnPos.y = yAnchor + y + posOffset.y;

                        if (tilemap.name == "Indicator Map")
                        {
                            if (tile == spawnTile)
                            {
                                playerSpawnPos = tileSpawnPos;    
                            }
                            else if (tile == enemyTile)
                            {
                                enemyTilePos.Add(tileSpawnPos);
                            }
                            else if (tile == dangerTile)
                            {
                                dangerTilePos.Add(tileSpawnPos);
                            }
                            else if (tile == treasureTile)
                            {
                                treasurePosList.Add(tileSpawnPos);
                            }
                            else if (tile == shopTile)
                            {
                                shopPosList.Add(tileSpawnPos);
                            }
                            else if (tile == bossTile)
                            {
                                bossTilePos = tileSpawnPos;
                            }
                            else if (tile == exitTile)
                            {
                                exitTilePos = tileSpawnPos;
                            }
                        }
                        else
                        {
                            if (tilemap.name == "Platforms" && tile == null)
                            {
                                mapTilemap.SetTile(tileSpawnPos, mapTile);
                            }
                            else if (tile != null)
                            {
                                spawnTilemap.SetTile(tileSpawnPos, tile);
                            }
                        }
                    }    
                }
            }
        }

        FillEmptyGrid(levelGrid, posOffset);
        SpawnPlayer();
        SpawnSpikes();
        SpawnTreasures();
        SpawnShop();
        SpawnEnemies();
        SpawnBoss();

        // tileShadowController.ActivateShadowEffect();       
        
    }

    private void SpawnTreasures()
    {
        foreach (var pos in treasurePosList)
        {
            Instantiate(treasurePrefab, pos, Quaternion.identity);
        }
    }

    private void SpawnShop()
    {
        foreach (var pos in shopPosList)
        {
            Instantiate(shopPrefab, pos, Quaternion.identity);
        }
    }

    public List<Vector2Int> FindAdjacentEmptyCells(Dictionary<Vector2Int, Room> levelGrid)
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, Room> entry in levelGrid)
        {
            Vector2Int currentPosition = entry.Key;

            if (levelGrid[currentPosition] != null)
            {
                // Check all eight neighboring cells
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        Vector2Int neighborPosition = new Vector2Int(currentPosition.x + xOffset, currentPosition.y + yOffset);

                        // Skip the current position
                        if (xOffset == 0 && yOffset == 0)
                            continue;

                        // Check if the neighboring cell is empty and within the grid
                        if (!levelGrid.ContainsKey(neighborPosition))
                        {
                            result.Add(neighborPosition);
                        }
                    }
                }
            }
        }

        return result;
    }


    private void FillEmptyGrid(Dictionary<Vector2Int, Room> levelGrid, Vector2Int posOffset)
    {
        List<Vector2Int> emptyGridPosList = FindAdjacentEmptyCells(levelGrid);
    
        foreach (var emptyGridPos in emptyGridPosList)
        {
            for (int y = 0; y < defaultRoomSize.y; y++)                                                                         
            {
                for (int x = 0; x < defaultRoomSize.x; x++)
                {   
                    TileBase tile = defaultTile;

                    int xAnchor = emptyGridPos.x * defaultRoomSize.x;
                    int yAnchor = emptyGridPos.y * defaultRoomSize.y;

                    Vector3Int tileSpawnPos = Vector3Int.zero;
                    tileSpawnPos.x = xAnchor + x + posOffset.x;
                    tileSpawnPos.y = yAnchor + y + posOffset.y;

                    defaultTilemap.SetTile(tileSpawnPos, tile);
                }    
            }
        }
    }

    private List<Tilemap> GetRoomTilemaps(Room room)
    {
        List<Tilemap> tilemapList = new();
        foreach (var tilemapName in mapLayers)
        {
            Transform tilemapObject = room.roomPrefab.transform.Find(tilemapName);
            if (tilemapObject != null)
            {
                tilemapList.Add(tilemapObject.GetComponent<Tilemap>());
            }
            else
            {
                Debug.LogWarning("Tilemap component" + tilemapName + " not found.");
            }
        }
        return tilemapList;
    }

    public static void TrimTilebase(TileBase[] originalTilebase, int originalSizeX, int originalSizeY, out TileBase[] trimmedMap, out Vector2Int newSize)
    {
        // Initialize the starting index and ending index for X and Y
        int startX = originalSizeX;
        int endX = 0;
        int startY = originalSizeY;
        int endY = 0;

        // Find the boundaries of the non-null elements
        for (int i = 0; i < originalSizeX * originalSizeY; i++)
        {
            if (originalTilebase[i] != null)
            {
                int x = i % originalSizeX;
                int y = i / originalSizeX;

                startX = Math.Min(startX, x);
                endX = Math.Max(endX, x);
                startY = Math.Min(startY, y);
                endY = Math.Max(endY, y);
            }
        }

        // Calculate the new size based on the boundaries found
        newSize = new Vector2Int(endX - startX + 1, endY - startY + 1);

        // Trim the original map based on the boundaries found
        List<TileBase> trimmedList = new();
        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                int index = x + y * originalSizeX;
                trimmedList.Add(originalTilebase[index]);
            }
        }

        // Convert the trimmed list to an array
        trimmedMap = trimmedList.ToArray();
    }
    private void SpawnPlayer()
    {
        player.transform.position = playerSpawnPos;
        player.SetActive(true);
        playerAfterImagePool.SetActive(true);
    }

    // Function to group adjacent enemy tiles
    public List<List<Vector3Int>> GroupAdjacentTiles(List<Vector3Int> tilePosList)
    {
        List<List<Vector3Int>> groupedTiles = new();

        HashSet<Vector3Int> visited = new();

        foreach (Vector3Int pos in tilePosList)
        {
            if (visited.Contains(pos))
            {
                continue; // Skip this position if it's already visited
            }

            List<Vector3Int> currentGroup = new();
            Queue<Vector3Int> queue = new();
            queue.Enqueue(pos);
            visited.Add(pos);

            while (queue.Count > 0)
            {
                Vector3Int currentPos = queue.Dequeue();
                currentGroup.Add(currentPos);

                // Check adjacent positions (modify as per your grid structure)
                Vector3Int[] adjacentPositions = new Vector3Int[]
                {
                    currentPos + new Vector3Int(1, 0, 0), // Right
                    currentPos + new Vector3Int(-1, 0, 0), // Left
                    currentPos + new Vector3Int(0, 1, 0), // Up
                    currentPos + new Vector3Int(0, -1, 0) // Down
                };

                foreach (Vector3Int adjPos in adjacentPositions)
                {
                    if (tilePosList.Contains(adjPos) && !visited.Contains(adjPos))
                    {
                        queue.Enqueue(adjPos);
                        visited.Add(adjPos);
                    }
                }
            }

            if (currentGroup.Count > 0)
            {
                groupedTiles.Add(currentGroup);
            }
        }

        return groupedTiles;
    }

    private void SpawnEnemies()
    {
        // Group Enemy Tiles Based On Adjacent Position
        List<List<Vector3Int>> groupedEnemyTiles = GroupAdjacentTiles(enemyTilePos);
        Debug.Log("Enemy Tiles Group: " + groupedEnemyTiles.Count);

        List<Enemy> normalEnemyList = enemyManager.GetEnemyByRarity(Enemy.Type.NORMAL);
        List<Enemy> eliteEnemyList = enemyManager.GetEnemyByRarity(Enemy.Type.ELITE);
        int enemyTileCount = 0;

        foreach (List<Vector3Int> group in groupedEnemyTiles)
        {
            int tileCount = group.Count;
            enemyTileCount += tileCount;
            
            int difficulty = gameManager.difficulty; 
            
            // Assuming gameManager.difficulty is an int between 0 - 100
            float spawnProbability = Mathf.Clamp01(tileCount * enemySpawnCoefficient * (difficulty / 100f)  / 100f) * 1.2f;
            bool spawnEnemy = UnityEngine.Random.value <= spawnProbability;
            bool spawnElite = spawnProbability >= 1;

            if (spawnEnemy)
            {   
                // Choose spawn pos based on 1 of the tiles in the group
                int randomIndex = UnityEngine.Random.Range(0, tileCount);
                Vector3Int spawnPos = group[randomIndex] + Vector3Int.up;

                // Choose enemy to spawn
                Enemy enemy;
                
                if (spawnElite && eliteEnemyList.Count > 0)
                {
                    randomIndex = UnityEngine.Random.Range(0, eliteEnemyList.Count);
                    enemy = eliteEnemyList[randomIndex];
                }
                else
                {
                    randomIndex = UnityEngine.Random.Range(0, normalEnemyList.Count);
                    enemy = normalEnemyList[randomIndex];
                }

                // Instantiate Enemy To World
                GameObject spawnedEnemy = Instantiate(enemy.prefab, spawnPos, Quaternion.identity);
                spawnedEnemy.transform.GetComponentInChildren<Stats>().goldValue = enemy.goldValue;
            }
        }
        Debug.Log("Average tile per group: " + (enemyTileCount / groupedEnemyTiles.Count));
    }

    private void SpawnSpikes()
    {
        // Group Enemy Tiles Based On Adjacent Position
        List<List<Vector3Int>> groupedSpikeTiles = GroupAdjacentTiles(dangerTilePos);
        // Debug.Log(groupedSpikeTiles.Count);

        foreach (List<Vector3Int> group in groupedSpikeTiles)
        {
            // Debug.Log(group.Count);
            int tileCount = group.Count;
            
            int difficulty = gameManager.difficulty; 
            
            // Assuming gameManager.difficulty is an int between 0 - 100
            float spawnProbability = Mathf.Clamp01(tileCount * spikeSpawnCoefficient * (difficulty / 100f)  / 100f);
            bool spawnSpike = UnityEngine.Random.value <= spawnProbability;

            if (spawnSpike)
            {
                foreach (var spawnPos in group)
                {
                    spikeTilemap.SetTile(spawnPos, spikeRuleTile);                    
                }
            }
        }
    }

    private void SpawnBoss()
    {
        List<Enemy> bossList = enemyManager.GetEnemyByRarity(Enemy.Type.BOSS);
        int randomIndex = UnityEngine.Random.Range(0, bossList.Count);
        Enemy bossSpawn = bossList[randomIndex];

        Debug.Log("Boss spawned at: " + bossTilePos);

        boss = Instantiate(bossSpawn.prefab, bossTilePos, Quaternion.identity);
        boss.transform.GetComponentInChildren<Stats>().goldValue = bossSpawn.goldValue;
        bossSpawned = true;
    }

    private void SpawnExit()
    {
        Instantiate(exitPrefab, exitTilePos, Quaternion.identity);
    }
}
