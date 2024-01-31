using System;
using System.Collections.Generic;
using UnityEngine;


public class LevelGraphGenCopy : MonoBehaviour
{
    public RoomTemplates roomTemplates;
    public int stoppingRoomNum;
    private int currentRoomNum = 0;
    private Dictionary<Vector2Int, Room> levelGrid = new();
    private Room lastGenLevel = null;

    private bool IsGridFilled(Vector2Int pos)
    {
        return levelGrid.ContainsKey(pos);
    }

    public Room GenerateLevelGraph()
    {
        levelGrid = new();
        currentRoomNum = 0;
        Room rootRoom = new(roomTemplates.GetRandomRoomTemplateByType(RoomType.Start));
        levelGrid[Vector2Int.zero] = rootRoom;
        currentRoomNum++;

        // Queue to process rooms with open entrances
        Queue<Room> roomsToProcess = new();
        roomsToProcess.Enqueue(rootRoom);

        while (roomsToProcess.Count > 0)
        {
            Room currentRoom = roomsToProcess.Dequeue();
            
            // Debug.Log(currentRoom.roomPrefab.name + ": " + currentRoom.GetOpenEntranceCount());
            

            foreach (var entranceDirection in currentRoom.GetOpenEntranceList())
            {
                List<Room.EntranceDirection> requiredEntrances= new();
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
                else if (rootRoom.GetTotalOpenEntranceCountRecursive() > 1)
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
            }
        }

        lastGenLevel = rootRoom;
        return rootRoom;
    }

    public Room GetLastGeneratedLevel()
    {
        return lastGenLevel;
    }
    public Dictionary<Vector2Int, Room> GetLastGeneratedLevelgrid()
    {
        return levelGrid;
    }
    private List<Room.EntranceDirection> GetFreeSpaceDirectionList(Vector2Int roomPos)
    {
        List<Room.EntranceDirection> freeDirections = new();

        // Check free spaces around room
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.up)) freeDirections.Add(Room.EntranceDirection.North);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.down)) freeDirections.Add(Room.EntranceDirection.South);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.left)) freeDirections.Add(Room.EntranceDirection.West);
        if (!levelGrid.ContainsKey(roomPos + Vector2Int.right)) freeDirections.Add(Room.EntranceDirection.East);

        return freeDirections;
    }

    private List<Room.EntranceDirection> GetFilledSpaceDirectionList(Vector2Int roomPos)
    {
        List<Room.EntranceDirection> filledDirections = new();

        // Check free spaces around room
        if (levelGrid.ContainsKey(roomPos + Vector2Int.up)) filledDirections.Add(Room.EntranceDirection.North);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.down)) filledDirections.Add(Room.EntranceDirection.South);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.left)) filledDirections.Add(Room.EntranceDirection.West);
        if (levelGrid.ContainsKey(roomPos + Vector2Int.right)) filledDirections.Add(Room.EntranceDirection.East);

        return filledDirections;
    }

    public Room GetTestLevel()
    {
        Room testLevel = new(roomTemplates.GetRandomRoomTemplateByType(RoomType.Start));
        testLevel.ConnectRoom(new Room(roomTemplates.GetRandomRoomTemplateByType(RoomType.Boss)), Room.EntranceDirection.North);

        return testLevel;
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
        
        throw new ArgumentException("Value not found in the dictionary", nameof(value));
    }
}
