using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]

public class CardEntity : ScriptableObject
{
    public int Id;
    public string Name;
    public int ManaCosts;
    public int AnyCost;
    public CardColor Color;
    CardColor parsedColor;

    public int Ap;
    public int Bp;

    public string ImagePath;
    public string FrameImagePath;
    public string TextFrameImagePath;

    public string CardText;
    public string category;


    public void Init(CardData data)
    {
        Id = data.cardID;
        Name = data.cardName;
        ManaCosts = GetCollerCost(data);
        AnyCost = data.anyColorCost;
        if (!System.Enum.TryParse(data.color, out parsedColor))
        {
            Debug.LogError($"CardColor parse failed: {data.color}");
            Color = CardColor.Red;
        }
        else
        {
            Color = parsedColor;
        }

        Ap = data.ap;
        Bp = data.bp;
        ImagePath = data.imagePath;
        FrameImagePath = GetFrameImagePath(Color);
        TextFrameImagePath = GetTextFrameImagePath(Color);
        CardText = data.CardText;
        category = data.category;

    }
    int GetCollerCost(CardData data)
    {
        int total = 0;
        foreach (var mana in data.manaCosts)
        {
            total += mana.cost;
        }
        return total;
    }
    string GetFrameImagePath(CardColor color)
    {
        string path = null;
        switch(color)
        {
            case CardColor.Red:
                path = "FrameImage/Red";
                break;
            case CardColor.Blue:
                path = "FrameImage/Blue";
                break;
            case CardColor.Green:
                path = "FrameImage/Green";
                break;
            case CardColor.Yellow:
                path = "FrameImage/Yellow";
                break;
            case CardColor.Purple:
                path = "FrameImage/Purple";
                break;
            default:
                path = "FrameImage/White";
                break;
        }
        return path;
    }
    string GetTextFrameImagePath(CardColor color)
    {
        string path = null;
        switch(color)
        {
            case CardColor.Red:
                path = "FrameImage/Red_1";
                break;
            case CardColor.Blue:
                path = "FrameImage/Blue_1";
                break;
            case CardColor.Green:
                path = "FrameImage/Green_1";
                break;
            case CardColor.Yellow:
                path = "FrameImage/Yellow_1";
                break;
            case CardColor.Purple:
                path = "FrameImage/Purple_1";
                break;
            default:
                path = "FrameImage/White_1";
                break;
        }
        return path;
    }
}