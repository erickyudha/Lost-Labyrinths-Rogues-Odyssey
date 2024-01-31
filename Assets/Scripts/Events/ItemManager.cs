using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Item
{
    public enum Rarity
    {
        COMMON,
        RARE,
        LEGENDARY
    }
    
    public Sprite icon; // Icon for the item
    public string title; // Title/name of the item
    public string flavorText; // Description or flavor text of the item
    public bool inShop;
    public bool inTreasure;
    public UnityEvent onUse; // UnityEvent to handle the item's functionality
    public Rarity rarity;
    public int price;

    public Item(Sprite icon, string title, string flavorText)
    {
        this.icon = icon;
        this.title = title;
        this.flavorText = flavorText;
        onUse = new UnityEvent();
    }

    public void UseItem()
    {
        // Invoke the UnityEvent to execute the item's functionality
        onUse?.Invoke();
    }
}

public class ItemManager : MonoBehaviour
{
    public List<Item> itemList = new();

    public List<Item> GetTreasureItems()
    {
        List<Item> treasureItems = new();
        foreach (Item item in itemList)
        {
            if (item.inTreasure)
            {
                treasureItems.Add(item);
            }
        }
        return treasureItems;
    }

    public List<Item> GetShopItems()
    {
        List<Item> shopItems = new();
        foreach (Item item in itemList)
        {
            if (item.inShop)
            {
                shopItems.Add(item);
            }
        }
        return shopItems;
    }
}
