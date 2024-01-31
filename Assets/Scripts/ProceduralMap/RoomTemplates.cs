using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// Enum to define different room types
public enum RoomType
{
    Normal,
    Start,
    Boss,
    Shop,
    Treasure
}

// Class to hold room template data
[System.Serializable]
public class RoomTemplate
{
    public GameObject roomPrefab; // Reference to the Room prefab
    public RoomType roomType; // Type of the room (Normal, Start, Boss, etc.)
    public List<Room.EntranceDirection> entrances; // List of entrance directions for the room

    // Constructor to initialize values
    public RoomTemplate(GameObject prefab, RoomType type, List<Room.EntranceDirection> entranceDirs)
    {
        roomPrefab = prefab;
        roomType = type;
        entrances = entranceDirs;
    }
}

public class RoomTemplates : MonoBehaviour
{
    public List<RoomTemplate> roomTemplatesList = new(); // List to hold room templates

    // Method to add a room template to the list
    public void AddRoomTemplate(GameObject prefab, RoomType type, List<Room.EntranceDirection> entranceDirs)
    {
        RoomTemplate newTemplate = new(prefab, type, entranceDirs);
        roomTemplatesList.Add(newTemplate);
    }

    // Example method to get a room template by type
    public RoomTemplate GetRoomTemplateByType(RoomType type)
    {
        return roomTemplatesList.Find(template => template.roomType == type);
    }

    // Method to get a room template by type and entrance direction
    public RoomTemplate GetRoomTemplateByTypeAndEntrance(RoomType type, Room.EntranceDirection entranceDir)
    {
        return roomTemplatesList.Find(template => template.roomType == type && template.entrances.Contains(entranceDir));
    }

    // Method to get a room template by type
    public RoomTemplate GetRandomRoomTemplateByType(RoomType type)
    {
        List<RoomTemplate> matchingTemplates = roomTemplatesList.FindAll(template => template.roomType == type);
        if (matchingTemplates.Count > 0)
        {
            int randomIndex = Random.Range(0, matchingTemplates.Count);
            return matchingTemplates[randomIndex];
        }
        else
        {
            return null;
        }
    }

    // Method to get a room template by type and entrance direction
    public RoomTemplate GetRandomRoomTemplateByTypeAndEntrance(RoomType type, Room.EntranceDirection entranceDir)
    {
        List<RoomTemplate> matchingTemplates = roomTemplatesList.FindAll(template => template.roomType == type && template.entrances.Contains(entranceDir));
        if (matchingTemplates.Count > 0)
        {
            int randomIndex = Random.Range(0, matchingTemplates.Count);
            return matchingTemplates[randomIndex];
        }
        else
        {
            return null;
        }
    }

    public RoomTemplate GetRandomRoomTemplateByCriteria(RoomType targetType, List<Room.EntranceDirection> requiredEntrances, List<Room.EntranceDirection> blockedEntrances, int minEntranceCount, int maxEntranceCount)
    {
        List<RoomTemplate> candidateTemplates = new();

        foreach (RoomTemplate template in roomTemplatesList)
        {
            if (template.roomType == targetType)
            {
                int matchingEntrances = 0;

                foreach (Room.EntranceDirection entrance in requiredEntrances)
                {
                    if (template.entrances.Contains(entrance))
                    {
                        matchingEntrances++;
                    }
                }

                if 
                (
                    matchingEntrances == requiredEntrances.Count &&
                    template.entrances.Count <= maxEntranceCount &&
                    template.entrances.Count >= minEntranceCount
                )
                {
                    candidateTemplates.Add(template);
                }
            }
        }

        // Filter result by blocked entrance
        if (blockedEntrances.Count > 0)
        {
            candidateTemplates = FilterObjects(candidateTemplates, blockedEntrances);
        } 

        if (candidateTemplates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidateTemplates.Count);
            return candidateTemplates[randomIndex];
        }

        throw new System.Exception("No Matchin Room Found");
    }

    // Function to filter objects based on the blacklist in their lists
    static List<RoomTemplate> FilterObjects(List<RoomTemplate> roomTemplates, List<Room.EntranceDirection> blacklists)
    {
        return roomTemplates.Where(template => !template.entrances.Any(entrance => blacklists.Contains(entrance))).ToList();
    }
    
}
