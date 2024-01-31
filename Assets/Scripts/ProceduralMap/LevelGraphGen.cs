using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGraphGen : MonoBehaviour
{
    public RoomTemplates roomTemplates;
    public int minRoomStopNum = 7;
    public int maxRoomStopNum = 15;
    private int stoppingRoomNum;
    private int currentRoomNum = 0;
    public int treasureSpawnCoefficient = 5;
    private Dictionary<Vector2Int, Room> levelGrid = new();
    private int treasureToSpawn;

    private bool IsGridFilled(Vector2Int pos)
    {
        return levelGrid.ContainsKey(pos);
    }
    

    
    public void GenerateLevelGraph()
    {
        int difficulty = SessionManager.difficulty;
        stoppingRoomNum = Mathf.Clamp(difficulty / 5, minRoomStopNum, maxRoomStopNum);
        if (stoppingRoomNum < minRoomStopNum)
        {
            stoppingRoomNum = minRoomStopNum;
        }
        if (stoppingRoomNum > maxRoomStopNum)
        {
            stoppingRoomNum = maxRoomStopNum;
        }

        treasureToSpawn = Mathf.Clamp(stoppingRoomNum / treasureSpawnCoefficient, 1, 5);

        levelGrid = new();
        currentRoomNum = 0;
        Room rootRoom = new(roomTemplates.GetRandomRoomTemplateByType(RoomType.Start));
        levelGrid[Vector2Int.zero] = rootRoom;
        currentRoomNum++;

        Queue<Room> roomsToProcess = new();
        roomsToProcess.Enqueue(rootRoom);

        while (roomsToProcess.Count > 0)
        {
            Room currentRoom = roomsToProcess.Dequeue();

            foreach (var entranceDirection in currentRoom.GetOpenEntranceList())
            {
                List<Room.EntranceDirection> requiredEntrances = new();
                Vector2Int nextRoomPos;
                if (levelGrid.ContainsValue(currentRoom))
                {
                    nextRoomPos = GetKeyFromValue(levelGrid, currentRoom);
                }
                else
                {
                    break;
                }


                switch (entranceDirection)
                {
                    case Room.EntranceDirection.North:
                        nextRoomPos += Vector2Int.up;
                        break;
                    case Room.EntranceDirection.South:
                        nextRoomPos += Vector2Int.down;
                        break;
                    case Room.EntranceDirection.East:
                        nextRoomPos += Vector2Int.right;
                        break;
                    case Room.EntranceDirection.West:
                        nextRoomPos += Vector2Int.left;
                        break;
                    default:
                        break;
                }

                // Connect the entrance to the opposite direction in the new room
                Room.EntranceDirection oppositeDirection = Room.GetOppositeDirection(entranceDirection);
                requiredEntrances.Add(oppositeDirection);

                List<Room.EntranceDirection> blockedEntrances = GetFilledSpaceDirectionList(nextRoomPos);
                blockedEntrances.RemoveAll(entrance => requiredEntrances.Contains(entrance));

                if (IsGridFilled(nextRoomPos))
                {
                    if (!currentRoom.HasEntrance(entranceDirection))
                    {
                        requiredEntrances = new();
                        blockedEntrances = new();
                        Dictionary<Room.EntranceDirection, Room> connectDict = new();

                        // check all room in 4 directions to see if they have an entrance to this room
                        if (levelGrid.ContainsKey(nextRoomPos + Vector2Int.up))
                        {
                            Room upRoom = levelGrid[nextRoomPos + Vector2Int.up];
                            if (upRoom.entrances.Contains(Room.EntranceDirection.South))
                            {
                                requiredEntrances.Add(Room.EntranceDirection.North);
                                upRoom.DisconnectRoom(Room.EntranceDirection.South);
                                connectDict[Room.EntranceDirection.North] = upRoom;
                            }
                            else
                            {
                                blockedEntrances.Add(Room.EntranceDirection.North);
                            }
                        }
                        else if (levelGrid.ContainsKey(nextRoomPos + Vector2Int.down))
                        {
                            Room downRoom = levelGrid[nextRoomPos + Vector2Int.down];
                            if (downRoom.entrances.Contains(Room.EntranceDirection.North))
                            {
                                requiredEntrances.Add(Room.EntranceDirection.South);
                                downRoom.DisconnectRoom(Room.EntranceDirection.North);
                                connectDict[Room.EntranceDirection.South] = downRoom;
                            }
                            else
                            {
                                blockedEntrances.Add(Room.EntranceDirection.South);
                            }
                        }
                        else if (levelGrid.ContainsKey(nextRoomPos + Vector2Int.left))
                        {
                            Room leftRoom = levelGrid[nextRoomPos + Vector2Int.left];
                            if (leftRoom.entrances.Contains(Room.EntranceDirection.East))
                            {
                                requiredEntrances.Add(Room.EntranceDirection.West);
                                leftRoom.DisconnectRoom(Room.EntranceDirection.East);
                                connectDict[Room.EntranceDirection.West] = leftRoom;
                            }
                            else
                            {
                                blockedEntrances.Add(Room.EntranceDirection.West);
                            }
                        }
                        else if (levelGrid.ContainsKey(nextRoomPos + Vector2Int.right))
                        {
                            Room rightRoom = levelGrid[nextRoomPos + Vector2Int.right];
                            if (rightRoom.entrances.Contains(Room.EntranceDirection.West))
                            {
                                requiredEntrances.Add(Room.EntranceDirection.East);
                                rightRoom.DisconnectRoom(Room.EntranceDirection.West);
                                connectDict[Room.EntranceDirection.East] = rightRoom;
                            }
                            else
                            {
                                blockedEntrances.Add(Room.EntranceDirection.East);
                            }
                        }

                        RoomTemplate randomNormalRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Normal, requiredEntrances, blockedEntrances, requiredEntrances.Count, 4 - blockedEntrances.Count);
                        Room newRoom = new(randomNormalRoomTemplate);

                        levelGrid[nextRoomPos] = newRoom;
                        // newRoom.ConnectRoom(currentRoom, oppositeDirection);

                        currentRoom.ConnectRoom(newRoom, entranceDirection);
                        connectDict.Remove(oppositeDirection);
                        foreach (var pair in connectDict)
                        {
                            newRoom.ConnectRoom(pair.Value, pair.Key);
                        }

                        currentRoomNum++;

                        roomsToProcess.Enqueue(newRoom);
                    }
                    else
                    {
                        Debug.Log("Skipped");
                    }
                }
                else if (currentRoomNum < stoppingRoomNum)
                {
                    if (!currentRoom.HasEntrance(entranceDirection))
                    {
                        RoomTemplate randomNormalRoomTemplate = roomTemplates.GetRoomTemplateByType(RoomType.Normal);
                        if (GetFilledSpaceDirectionList(nextRoomPos).Count == 4)
                        {
                            randomNormalRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Normal, requiredEntrances, blockedEntrances, 1, 1);
                        }
                        else
                        {
                            randomNormalRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Normal, requiredEntrances, blockedEntrances, 2, 4);
                        }
                        Room newRoom = new(randomNormalRoomTemplate);

                        levelGrid[nextRoomPos] = newRoom;
                        // newRoom.ConnectRoom(currentRoom, oppositeDirection);
                        currentRoom.ConnectRoom(newRoom, entranceDirection);
                        currentRoomNum++;

                        roomsToProcess.Enqueue(newRoom);
                    }
                    else
                    {
                        Debug.Log("Skipped");
                    }
                }
                else if (rootRoom.GetTotalOpenEntranceCountRecursive() >= 0)
                {
                    if (!currentRoom.HasEntrance(entranceDirection))
                    {
                        RoomTemplate randomNormalRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Normal, requiredEntrances, blockedEntrances, 1, 1);
                        Room newRoom = new(randomNormalRoomTemplate);

                        levelGrid[nextRoomPos] = newRoom;
                        // newRoom.ConnectRoom(currentRoom, oppositeDirection);
                        currentRoom.ConnectRoom(newRoom, entranceDirection);
                        currentRoomNum++;

                        roomsToProcess.Enqueue(newRoom);
                    }
                    else
                    {
                        Debug.Log("Skipped");
                    }
                }
                /*
                else if (currentRoom.GetOpenEntranceCount() == 1)
                {
                    if (!currentRoom.HasEntrance(entranceDirection))
                    {
                        RoomTemplate bossRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Boss, requiredEntrances, blockedEntrances, 0, 4);
                        Room bossRoom = new(bossRoomTemplate);

                        levelGrid[nextRoomPos] = bossRoom;
                        // bossRoom.ConnectRoom(currentRoom, oppositeDirection);
                        currentRoom.ConnectRoom(bossRoom, entranceDirection);
                        currentRoomNum++;
                    }
                    else
                    {
                        Debug.Log("Skipped");
                    }
                }
                */
            }
        }

        // Collect Edge Room positions
        List<Vector2Int> edgeRoomPositions = new List<Vector2Int>();
        foreach (var roomPos in levelGrid.Keys)
        {
            if (IsEdgeRoom(levelGrid[roomPos]))
            {
                edgeRoomPositions.Add(roomPos);
            }
        }

        // Shuffle the list of Edge Room positions to randomize the selection
        edgeRoomPositions = ShuffleList(edgeRoomPositions);
        
        foreach (var roomPos in edgeRoomPositions)
        {
            Room currentRoom = levelGrid[roomPos];

            Room.EntranceDirection currentRoomEntranceDir = currentRoom.entrances[0];
            Room.EntranceDirection currentRoomOppositeDir = Room.GetOppositeDirection(currentRoomEntranceDir);

            Vector2Int adjacentPos = GetAdjacentPosition(roomPos, currentRoomOppositeDir); 

            if (levelGrid.ContainsKey(adjacentPos))
            {
                Room connectedRoom = levelGrid[adjacentPos];
                Room baseRoom = levelGrid[GetAdjacentPosition(roomPos, currentRoomEntranceDir)];

                currentRoom.DisconnectAllRooms();
                edgeRoomPositions.Remove(roomPos);

                // Attach Shop
                Room shop = new(roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Shop, currentRoom.entrances, new(), 2, 2));
                baseRoom.ConnectRoom(shop, currentRoomOppositeDir);
                levelGrid[roomPos] = shop;

                // Attach Boss Room to adjacent
                Room bossRoom = new(roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Boss, currentRoom.entrances, new(), 1, 1));
                shop.ConnectRoom(bossRoom, currentRoomOppositeDir);
                levelGrid[adjacentPos] = bossRoom;

                break;
            }
        }
        
        // Replace Edge Rooms with Edge Treasure Rooms
        foreach (var roomPos in edgeRoomPositions)
        {
            if (treasureToSpawn > 0)
            {
                Room normalRoom = levelGrid[roomPos];
                normalRoom.DisconnectAllRooms();

                // Get the entrance list of the replaced normal Edge Room
                List<Room.EntranceDirection> entranceList = normalRoom.entrances;

                // Use GetRandomRoomByCriteria to get the Edge Treasure Room with the same entrance list
                RoomTemplate edgeTreasureRoomTemplate = roomTemplates.GetRandomRoomTemplateByCriteria(RoomType.Treasure, entranceList, new(), 0, 4);
                Room edgeTreasureRoom = new(edgeTreasureRoomTemplate);

                levelGrid[roomPos] = edgeTreasureRoom;
                normalRoom.ConnectRoom(edgeTreasureRoom, Room.EntranceDirection.North);  // Connect the normal room to the edge treasure room

                treasureToSpawn--;
            }
        }
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

    public Dictionary<Vector2Int, Room> GetLastGeneratedLevelgrid()
    {
        return levelGrid;
    }

    private List<Room.EntranceDirection> GetFreeSpaceDirectionList(Vector2Int roomPos)
    {
        List<Room.EntranceDirection> freeDirections = new();

        if (!levelGrid.ContainsKey(roomPos + Vector2Int.up)) freeDirections.Add(Room.EntranceDirection.North);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.down)) freeDirections.Add(Room.EntranceDirection.South);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.left)) freeDirections.Add(Room.EntranceDirection.West);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.right)) freeDirections.Add(Room.EntranceDirection.East);

        return freeDirections;
    }

    private List<Room.EntranceDirection> GetFilledSpaceDirectionList(Vector2Int roomPos)
    {
        List<Room.EntranceDirection> filledDirections = new();

        if (levelGrid.ContainsKey(roomPos + Vector2Int.up)) filledDirections.Add(Room.EntranceDirection.North);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.down)) filledDirections.Add(Room.EntranceDirection.South);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.left)) filledDirections.Add(Room.EntranceDirection.West);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.right)) filledDirections.Add(Room.EntranceDirection.East);

        return filledDirections;
    }

    private static TKey GetKeyFromValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TValue value)
    {
        foreach (var pair in dictionary)
        {
            if (EqualityComparer<TValue>.Default.Equals(pair.Value, value))
            {
                return pair.Key;
            }
        }
        return default;
        // throw new ArgumentException("Value not found in the dictionary", nameof(value));
    }

    // Helper method to check if a room is on the edge
    private bool IsEdgeRoom(Room room)
    {
        return room.entrances.Count == 1;
    }

    // Helper method to shuffle a list
    private List<T> ShuffleList<T>(List<T> list)
    {
        System.Random rng = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }
}
