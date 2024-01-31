using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public GameManager gameManager;
    public PauseManager pauseManager;
    public ItemManager itemManager;
    public GameObject shopCanvas;
    public GameObject[] canvasItemList;
    public List<bool> itemBought;
    private List<Item> shopItemList;
    private int difficulty;
    private List<Item> generatedItemList;

    void Start()
    {
        shopCanvas.SetActive(false);
        shopItemList = itemManager.GetTreasureItems();
        difficulty = gameManager.difficulty;
    }

    private List<Item> PickRandomTreasure(int difficulty)
    {
        List<Item> pickedTreasures = new();
        itemBought = new(){false, false, false};

        // Filter treasures based on rarity and difficulty level
        List<Item> availableTreasures = new();
        while (availableTreasures.Count < 3)
        {
            foreach (Item item in shopItemList)
            {
                // Adjust the probability of picking based on difficulty and rarity
                float probability = CalculateProbability(item, difficulty);
                if (Random.value <= probability && !availableTreasures.Contains(item))
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
            Item.Rarity.COMMON => 1.0f,// Adjust as needed
            Item.Rarity.RARE => 1.5f,// Adjust as needed
            Item.Rarity.LEGENDARY => 2.0f,// Adjust as needed
            _ => 1.0f,
        };
    }


    public void TriggerShopEvent()
    {
        shopCanvas.SetActive(true);
        pauseManager.PauseNoUI();
        generatedItemList ??= PickRandomTreasure(difficulty);
        List<Item> pickedItems = generatedItemList;

        int i = 0;
        foreach (var canvasItem in canvasItemList)
        {
            Image icon = canvasItem.transform.Find("Info").transform.Find("Icon").GetComponentInChildren<Image>();
            TMP_Text title = canvasItem.transform.Find("Info").transform.Find("Item Name").GetComponent<TMP_Text>();
            TMP_Text flavorText = canvasItem.transform.Find("Info").transform.Find("Flavor Text").GetComponent<TMP_Text>();
            Button button = canvasItem.transform.GetComponentInChildren<Button>();
            TMP_Text price = button.GetComponentInChildren<TMP_Text>();
            
            Item currentItem = pickedItems[i];

            icon.sprite = currentItem.icon;
            title.text = currentItem.title + " - " + currentItem.rarity.ToString();
            flavorText.text = currentItem.flavorText;

            price.text = currentItem.price.ToString();
            if (currentItem.price <= SessionManager.goldCarried) // if player can buy
            {
                price.color = Color.yellow;
            }
            else
            {
                price.color = Color.red;
                button.interactable = false;
            }
            if (itemBought[i])
            {
                button.interactable = false;
                price.color = Color.black;
                price.text = "Sold";
            }
            // Store the current item to be used inside the listener

            // Add a listener to the button's onClick event
            button.onClick.AddListener(() => TryToBuyOnClick(currentItem));

            i++;
        }
    }

    public void TryToBuyOnClick(Item item)
    {
        if (SessionManager.goldCarried >= item.price) // Check if money enough
        {
            // Invoke the UnityEvent associated with the item
            if (item != null && item.onUse != null)
            {
                SessionManager.goldCarried -= item.price;
                item.onUse.Invoke();
                
                itemBought[generatedItemList.IndexOf(item)] = true;
                ExitWindow();
            }
        }
    }

    public void ExitWindow() 
    {   
        pauseManager.Resume();
        shopCanvas.SetActive(false);
    }

}
