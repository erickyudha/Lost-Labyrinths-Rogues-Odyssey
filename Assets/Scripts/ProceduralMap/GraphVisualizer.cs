using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphVisualizer : MonoBehaviour
{
    public RectTransform graphPanel; // Reference to the panel where UI elements will be placed
    public GameObject roomUIPrefab; // Prefab of the UI element representing a room

    private Dictionary<Room, GameObject> roomUIElements = new();
    private Dictionary<Room, Vector2> levelGrid = new();

    public LevelGraphGen levelGraphGen;

    public Vector2 startingRoomPosition = Vector2.zero;
    private Vector2 lastRoomPosition = Vector2.zero; // Track the position of the last created room

    public void VisualizeGraph(Room startRoom)
    {
        // Reset the dictionary before visualizing
        roomUIElements.Clear();
        lastRoomPosition = Vector2.zero; // Reset last room position

        CreateUIElement(startRoom, null, null);

        // Create UI elements for connected rooms recursively
        CreateConnectedRoomUIElements(startRoom);
    }
    
    private void CreateUIElement(Room room, Room parentRoom, Room.EntranceDirection? entranceDirection)
    {
        // Instantiate UI prefab for each room
        GameObject roomUI = Instantiate(roomUIPrefab, graphPanel);
        Vector2 roomPosition;
        Vector2 parentRoomGridPos = Vector2.zero;
        if (parentRoom != null)
        {
            parentRoomGridPos = levelGrid[parentRoom];
        }
        
        // Calculate the position based on the last created room's position and the prefab size
        switch (entranceDirection)
        {
            case Room.EntranceDirection.East:
                roomPosition = parentRoomGridPos + new Vector2(roomUIPrefab.GetComponent<RectTransform>().sizeDelta.x + 20f, 0f);
                break;
            case Room.EntranceDirection.West:
                roomPosition = parentRoomGridPos + new Vector2(-roomUIPrefab.GetComponent<RectTransform>().sizeDelta.x - 20f, 0f);
                break;
            case Room.EntranceDirection.North:
                roomPosition = parentRoomGridPos + new Vector2(0f, roomUIPrefab.GetComponent<RectTransform>().sizeDelta.y + 20f);
                break;
            case Room.EntranceDirection.South:
                roomPosition = parentRoomGridPos + new Vector2(0f, -roomUIPrefab.GetComponent<RectTransform>().sizeDelta.x - 20f);
                break;
            default:
                roomPosition = Vector2.zero;
                break;

        }

        roomUI.GetComponent<RectTransform>().anchoredPosition = startingRoomPosition + roomPosition; // Set position

        // Get the TextMeshProUGUI component in the Room UI prefab
        TextMeshProUGUI roomNameText = roomUI.GetComponentInChildren<TextMeshProUGUI>();

        if (roomNameText != null)
        {
            // Set the text of the TextMeshProUGUI component to the room prefab's name
            roomNameText.text = room.roomPrefab.name; // Set room name based on prefab name
        }
        else
        {
            Debug.LogWarning("TextMeshProUGUI component not found in the Room UI prefab.");
        }

        levelGrid[room] = roomPosition;
        // Add room and its UI element to the dictionary for reference
        roomUIElements.Add(room, roomUI);

        lastRoomPosition = roomPosition; // Update last room position
    }

    private void CreateConnectedRoomUIElements(Room room)
    {
        foreach (var pair in room.connectedRoomsByEntrance)
        {
            Room connectedRoom = pair.Value;

            if (connectedRoom != null && !roomUIElements.ContainsKey(connectedRoom))
            {
                CreateUIElement(connectedRoom, room, pair.Key);

                // Recursively create UI elements for connected rooms
                CreateConnectedRoomUIElements(connectedRoom);

                // Draw line between connected rooms
                DrawLineBetweenRooms(room, connectedRoom);
            }
        }
    }

    private void DrawLineBetweenRooms(Room roomA, Room roomB)
    {
        if (roomA.connectedRoomsByEntrance.ContainsValue(roomB))
        {
            GameObject line = new GameObject("Line");
            line.transform.SetParent(graphPanel.transform);

            RectTransform rectTransform = line.AddComponent<RectTransform>();
            Image image = line.AddComponent<Image>();
            image.color = Color.green;

            RectTransform roomAImage = roomUIElements[roomA].GetComponent<RectTransform>();
            RectTransform roomBImage = roomUIElements[roomB].GetComponent<RectTransform>();

            Vector2 roomAPosition = roomAImage.anchoredPosition;
            Vector2 roomBPosition = roomBImage.anchoredPosition;

            float roomAEdgeLeft = roomAPosition.x - roomAImage.sizeDelta.x / 2f; // Left edge of node A
            float roomAEdgeRight = roomAPosition.x + roomAImage.sizeDelta.x / 2f; // Right edge of node A
            float roomAEdgeUp = roomAPosition.y + roomAImage.sizeDelta.y / 2f; // Top edge of node A
            float roomAEdgeDown = roomAPosition.y - roomAImage.sizeDelta.y / 2f; // Bottom edge of node A

            float roomBEdgeLeft = roomBPosition.x - roomBImage.sizeDelta.x / 2f; // Left edge of node B
            float roomBEdgeRight = roomBPosition.x + roomBImage.sizeDelta.x / 2f; // Right edge of node B
            float roomBEdgeUp = roomBPosition.y + roomBImage.sizeDelta.y / 2f; // Top edge of node B
            float roomBEdgeDown = roomBPosition.y - roomBImage.sizeDelta.y / 2f; // Bottom edge of node B

            Vector2 startPoint = Vector2.zero;
            Vector2 endPoint = Vector2.zero;

            Room.EntranceDirection? direction = null;

            foreach (var kvp in roomA.connectedRoomsByEntrance)
            {
                if (kvp.Value == roomB)
                {
                    direction = kvp.Key;
                    break;
                }
            }

            if (direction.HasValue)
            {
                switch (direction)
                {
                    case Room.EntranceDirection.North:
                        startPoint = new Vector2(roomAPosition.x, roomAEdgeUp);
                        endPoint = new Vector2(roomBPosition.x, roomBEdgeDown);
                        break;
                    case Room.EntranceDirection.South:
                        startPoint = new Vector2(roomAPosition.x, roomAEdgeDown);
                        endPoint = new Vector2(roomBPosition.x, roomBEdgeUp);
                        break;
                    case Room.EntranceDirection.West:
                        startPoint = new Vector2(roomAEdgeLeft, roomAPosition.y);
                        endPoint = new Vector2(roomBEdgeRight, roomBPosition.y);
                        break;
                    case Room.EntranceDirection.East:
                        startPoint = new Vector2(roomAEdgeRight, roomAPosition.y);
                        endPoint = new Vector2(roomBEdgeLeft, roomBPosition.y);
                        break;
                }
            }

            Vector2 centerPosition = (startPoint + endPoint) / 2f;

            float distance = Vector2.Distance(startPoint, endPoint);
            Vector2 lineSize = new Vector2(distance, 3f); // Set line size

            rectTransform.sizeDelta = lineSize;
            rectTransform.anchoredPosition = centerPosition;
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg);

            // Create an arrowhead
            GameObject arrowhead = new GameObject("Arrowhead");
            arrowhead.transform.SetParent(line.transform);
            RectTransform arrowTransform = arrowhead.AddComponent<RectTransform>();
            Image arrowImage = arrowhead.AddComponent<Image>();
            arrowImage.color = Color.green;

            float arrowheadSize = 10f;
            arrowTransform.sizeDelta = new Vector2(arrowheadSize, arrowheadSize);
            arrowTransform.anchoredPosition = new Vector2(distance / 2f, 0f);
            arrowTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(endPoint.y - startPoint.y, endPoint.x - startPoint.x) * Mathf.Rad2Deg);
        }
    }


    // Example method to remove UI elements (if needed)
    public void ClearGraphVisualization()
    {
        foreach (var pair in roomUIElements)
        {
            Destroy(pair.Value);
        }

        roomUIElements.Clear();
    }

    // Use this for testing or invoking visualization from another script
    void Start()
    {
        // VisualizeGraph(levelGraphGen.GenerateLevelGraph());
    }
}
