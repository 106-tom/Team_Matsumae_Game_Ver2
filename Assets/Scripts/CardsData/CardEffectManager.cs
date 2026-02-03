using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardEffectManager
{

    public static void ActivateEffects(CardData cardData, string trigger)
    {
        List<EffectEntry> effects = trigger switch
        {
            
            "onSummon" => cardData.triggerEffects.onSummon,
            "onField" => cardData.triggerEffects.onField,
            "onAttack" => cardData.triggerEffects.onAttack,
            "onDestroy" => cardData.triggerEffects.onDestroy,
            _ => null
        };
        
        if (effects == null) return;

        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                //ドロー
                case CardEffectType.DrawCard:
                    DrawCards(effect.timeValue);
                    break;
                //対象選択でモンスター破壊
                case CardEffectType.DestoryAnyMonster:
                    DestroySelectMonster(effect.timeValue, effect.mathValue);
                    break;
                //条件に達するモンスター全破壊
                case CardEffectType.DestoryAllMonster:
                    DestroyAllEnemyMonsters(effect.mathValue);
                    break;

                case CardEffectType.AddAP:
                    AddAP(cardData);
                    break;

                // 特性
                case CardEffectType.Dragon:
                    ApplyDragonBuff(cardData);
                    break;

                case CardEffectType.Hide:
                    ApplyHideStatus();
                    break;

                case CardEffectType.Instant:
                    EnableInstantCast();
                    break;

                case CardEffectType.Resurrection:
                    RegisterResurrection();
                    break;

                //特定のカード専用
                case CardEffectType.Princess:
                    ApplyPrincessAura();
                    break;
            }
        }
    }

    private static void DrawCards(int count)
    {
        Debug.Log($"プレイヤーはカードを {count} 枚ドロー！");
        // 実際のデッキからのドロー処理
        for(int i=0;i<count;i++)
        InGamePlayer.Instance.DrawCard();
    }

    private static void DestroySelectMonster(int count,int value)
    {
        
    }

    private static void DestroyAllEnemyMonsters(int value)
    {

    }

    private static void AddAP(CardData card)
    {
        card.ap += 1;

    }

    //カード特性
    private static void ApplyDragonBuff(CardData card)
    {
        card.bp = (int)Math.Ceiling(card.bp * 1.2);
    }

    private static void ApplyHideStatus()
    {
        // TODO: このカードを攻撃対象に選択できないフラグを付与する
    }

    private static void EnableInstantCast()
    {
        // TODO: 防御中に割り込みプレイ可能な状態を記録する
    }

    private static void RegisterResurrection()
    {
        // TODO: 破壊時に1度だけ自動再召喚可能な状態を記録する
    }

    //特定のカード
    private static void ApplyPrincessAura()
    {
        // 全プレイヤー or 自プレイヤーのフィールドを走査
        List<Card> fieldCards = InGamePlayer.Instance.field; // 自分の場を想定

        foreach (var card in fieldCards)
        {
            if (card.Data.effectType == CardEffectType.Dragon)
            {
                EffectEntry effectEntry = new EffectEntry
                {
                    effectType = CardEffectType.AddAP,
                    timeValue = 1,   // 発動回数や持続ターン
                    mathValue = 1    // AP +1 の値
                };
                card.Data.triggerEffects.onField.Add(effectEntry);
            }
        }
    }
}
