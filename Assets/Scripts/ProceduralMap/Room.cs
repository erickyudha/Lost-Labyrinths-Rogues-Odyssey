using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Room
{
    public enum EntranceDirection
    {
        North,
        South,
        East,
        West
    }
    public GameObject roomPrefab; // Reference to the Room prefab
    public RoomType roomType; // Type of the room (Normal, Start, Boss, etc.)

    public List<EntranceDirection> entrances; // List of entrance directions for the room
    // public Tilemap tilemap; // Reference to the Tilemap of the room
    public Dictionary<EntranceDirection, Room> connectedRoomsByEntrance; // Map entrance direction to connected room
    
    
    public Room (RoomTemplate template)
    {
        roomPrefab = template.roomPrefab;
        roomType = template.roomType;
        entrances = template.entrances;

        connectedRoomsByEntrance = new Dictionary<EntranceDirection, Room>();
        
    }

    // Method to connect a room in a specific direction
    public void ConnectRoom(Room room, EntranceDirection direction)
    {
        if (!connectedRoomsByEntrance.ContainsKey(direction))
        {
            connectedRoomsByEntrance.Add(direction, room);
            room.connectedRoomsByEntrance[GetOppositeDirection(direction)] = this;
        }
        else
        {
            // Room in this direction is already connected, handle accordingly
            Debug.LogWarning("Room in this direction already connected.");
        }
    }

    // Method to get the count of open entrances in the room
    public int GetOpenEntranceCount()
    {
        return GetOpenEntranceList().Count;
    }

    public List<EntranceDirection> GetOpenEntranceList()
    {
        List<EntranceDirection> openEntrancesList = new();
        foreach (var direction in entrances)
        {
            if (!HasEntrance(direction))
            {
                openEntrancesList.Add(direction);
            }
        }
        return openEntrancesList;
    }

    // Method to get the count of total open entrances in the room and connected rooms recursively
    public int GetTotalOpenEntranceCountRecursive()
    {
        HashSet<Room> visitedRooms = new();
        return RecursiveTotalOpenEntrancesCheck(this, visitedRooms);
    }

    // Recursive method to count total open entrances in the room and its connected rooms
    private int RecursiveTotalOpenEntrancesCheck(Room room, HashSet<Room> visitedRooms)
    {
        visitedRooms.Add(room);
        int totalOpenEntrances = room.GetOpenEntranceCount();

        foreach (var kvp in room.connectedRoomsByEntrance)
        {
            Room connectedRoom = kvp.Value;
            if (connectedRoom != null && !visitedRooms.Contains(connectedRoom))
            {
                totalOpenEntrances += RecursiveTotalOpenEntrancesCheck(connectedRoom, visitedRooms);
            }
        }

        return totalOpenEntrances;
    }

    public List<Room> GetRoomsWithOpenEntrances()
    {
        List<Room> roomsWithOpenEntrances = new();
        HashSet<Room> visitedRooms = new();

        foreach (var kvp in connectedRoomsByEntrance)
        {
            if (kvp.Value == null && !visitedRooms.Contains(this))
            {
                roomsWithOpenEntrances.Add(this);
                RecursiveRoomsOpenEntrancesCheck(this, roomsWithOpenEntrances, visitedRooms);
                break;
            }
        }

        return roomsWithOpenEntrances;
    }

    private void RecursiveRoomsOpenEntrancesCheck(Room room, List<Room> roomsWithOpenEntrances, HashSet<Room> visitedRooms)
    {
        visitedRooms.Add(room);

        foreach (var kvp in room.connectedRoomsByEntrance)
        {
            if (kvp.Value == null && !visitedRooms.Contains(room))
            {
                roomsWithOpenEntrances.Add(room);
            }
            else if (kvp.Value != null && !visitedRooms.Contains(kvp.Value))
            {
                RecursiveRoomsOpenEntrancesCheck(kvp.Value, roomsWithOpenEntrances, visitedRooms);
            }
        }
    }


    // Method to disconnect a room in a specific direction
    public void DisconnectRoom(EntranceDirection direction)
    {
        if (connectedRoomsByEntrance.ContainsKey(direction))
        {
            connectedRoomsByEntrance.Remove(direction);
        }
        else
        {
            // No room connected in this direction, handle accordingly
            Debug.LogWarning("No room connected in this direction.");
        }
    }

    // Example method to get connected room in a specific direction
    public Room GetConnectedRoomInDirection(EntranceDirection direction)
    {
        if (connectedRoomsByEntrance.ContainsKey(direction))
        {
            return connectedRoomsByEntrance[direction];
        }
        return null; // No room connected in this direction
    }

    // Method to check if the room has a specific entrance direction
    public bool HasEntrance(EntranceDirection direction)
    {
        return connectedRoomsByEntrance.ContainsKey(direction);
    }

    // Method to recursively get all open entrances from this room and its connected rooms
    public List<KeyValuePair<Room, List<EntranceDirection>>> GetAllOpenEntrancesRecursive()
    {
        List<KeyValuePair<Room, List<EntranceDirection>>> openEntrancesList = new();
        HashSet<Room> visitedRooms = new();

        RecursiveOpenEntrancesCheck(this, openEntrancesList, visitedRooms);

        return openEntrancesList;
    }

    // Recursive method to check open entrances from a room and its connected rooms
    private void RecursiveOpenEntrancesCheck(Room room, List<KeyValuePair<Room, List<EntranceDirection>>> openEntrancesList, HashSet<Room> visitedRooms)
    {
        visitedRooms.Add(room);
        List<EntranceDirection> openEntrances = new();

        foreach (var kvp in room.connectedRoomsByEntrance)
        {
            if (kvp.Value == null && !openEntrances.Contains(kvp.Key))
            {
                openEntrances.Add(kvp.Key);
            }
            else if (kvp.Value != null && !visitedRooms.Contains(kvp.Value))
            {
                RecursiveOpenEntrancesCheck(kvp.Value, openEntrancesList, visitedRooms);
            }
        }

        openEntrancesList.Add(new KeyValuePair<Room, List<EntranceDirection>>(room, openEntrances));
    }

    // Helper method to get opposite direction
    public static EntranceDirection GetOppositeDirection(EntranceDirection direction)
    {
        switch (direction)
        {
            case EntranceDirection.North:
                return EntranceDirection.South;
            case EntranceDirection.South:
                return EntranceDirection.North;
            case EntranceDirection.East:
                return EntranceDirection.West;
            case EntranceDirection.West:
                return EntranceDirection.East;
            default:
                return direction; // Handle edge cases or errors
        }
    }
    
    // Method to disconnect all connected rooms
    public void DisconnectAllRooms()
    {
        foreach (var kvp in connectedRoomsByEntrance)
        {
            EntranceDirection direction = kvp.Key;
            Room connectedRoom = kvp.Value;

            connectedRoom?.DisconnectRoom(GetOppositeDirection(direction));
        }

        connectedRoomsByEntrance.Clear();
    }

}