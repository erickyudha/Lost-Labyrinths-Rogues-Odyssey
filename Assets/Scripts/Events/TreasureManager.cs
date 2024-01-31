using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureManager : MonoBehaviour
{
    public GameManager gameManager;
    public PauseManager pauseManager;
    public ItemManager itemManager;
    public GameObject treasureCanvas;
    public GameObject[] canvasItemList;
    private List<Item> treasureItemList;
    private int difficulty;

    void Start()
    {
        treasureCanvas.SetActive(false);
        treasureItemList = itemManager.GetTreasureItems();
        difficulty = gameManager.difficulty;
    }

    private List<Item> PickRandomTreasure(int difficulty)
    {
        List<Item> pickedTreasures = new();

        // Filter treasures based on rarity and difficulty level
        List<Item> availableTreasures = new();
        while (availableTreasures.Count < 3)
        {
            foreach (Item item in treasureItemList)
            {
                // Adjust the probability of picking based on difficulty and rarity
                float probability = CalculateProbability(item, difficulty);
                if (Random.value <= probability)
                {
                    availableTreasures.Add(item);
                }
            }
        }

        // Pick 3 random treasures from available treasures
        int treasuresToPick = Mathf.Min(3, availableTreasures.Count);
        for (int i = 0; i < treasuresToPick; i++)
        {
            int randomIndex = Random.Range(0, availableTreasures.Count);
            pickedTreasures.Add(availableTreasures[randomIndex]);
            availableTreasures.RemoveAt(randomIndex);
        }

        return pickedTreasures;
    }


    private float CalculateProbability(Item item, int difficulty)
    {
        // Adjust the probability based on the item's rarity and difficulty level
        float baseProbability = 0.5f; // Base probability
        float rarityMultiplier = GetRarityMultiplier(item); // Adjust based on rarity
        float difficultyFactor = difficulty / 100f; // Normalize difficulty to 0-1 range
        return baseProbability * rarityMultiplier * difficultyFactor;
    }

    private float GetRarityMultiplier(Item item)
    {
        // Adjust rarity multiplier based on the item's rarity
        return item.rarity switch
        {
            Item.Rarity.COMMON => 2.0f,// Adjust as needed
            Item.Rarity.RARE => 1.5f,// Adjust as needed
            Item.Rarity.LEGENDARY => 0.75f,// Adjust as needed
            _ => 1.0f,
        };
    }


    public void TriggerTreasureEvent()
    {
        treasureCanvas.SetActive(true);
        pauseManager.PauseNoUI();
        List<Item> pickedTreasure = PickRandomTreasure(difficulty);
        Debug.Log(pickedTreasure.Count);
        foreach (var treasure in pickedTreasure)
        {
            Debug.Log(treasure.title);
        }

        for (int i = 0; i < 3; i++)
        {

            Transform canvasItemInfo = canvasItemList[i].transform.Find("Info");
            Image icon = canvasItemInfo.Find("Icon").GetComponentInChildren<Image>();
            float currentAspect = icon.rectTransform.rect.width / icon.rectTransform.rect.height;
            TMP_Text title = canvasItemInfo.Find("Item Name").GetComponent<TMP_Text>();
            TMP_Text flavorText = canvasItemInfo.Find("Flavor Text").GetComponent<TMP_Text>();
            Button button = canvasItemList[i].transform.GetComponentInChildren<Button>();

            Item currentItem = pickedTreasure[i];

            icon.sprite = currentItem.icon;
            float newAspect = icon.sprite.rect.width / icon.sprite.rect.height;
            icon.rectTransform.sizeDelta = new Vector2(icon.rectTransform.sizeDelta.y * newAspect, icon.rectTransform.sizeDelta.y);

            title.text = currentItem.title + " - " + currentItem.rarity.ToString();
            flavorText.text = currentItem.flavorText;

            // Store the current item to be used inside the listener

            // Add a listener to the button's onClick event
            button.onClick.AddListener(() => UseItemOnClick(currentItem));
        }
    }

    void UseItemOnClick(Item item)
    {
        // Invoke the UnityEvent associated with the item
        if (item != null && item.onUse != null)
        {
            item.onUse.Invoke();
        }
        pauseManager.Resume();
        treasureCanvas.SetActive(false);
        Destroy(SessionManager.currentTreasure);
    }

}
