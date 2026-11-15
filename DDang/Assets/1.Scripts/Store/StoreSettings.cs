using System.Collections.Generic;
using UnityEngine;

namespace Store
{
    [CreateAssetMenu(fileName = "StoreSettings", menuName = "DDang/Store Settings")]
    public class StoreSettings : ScriptableObject
    {
        [Header("증강 풀 (모든 증강이 세 등급 모두에 등장)")]
        public List<AugmentData> augments = new();

        [Header("등급 (실버 / 골드 / 프리즘)")]
        public float[] tierWeights = { 0.6f, 0.3f, 0.1f };
        public Color[] tierColors = { new(0.75f, 0.78f, 0.82f), new(1f, 0.8f, 0.25f), new(0.7f, 0.5f, 1f) };

        [Header("가격 (임시)")]
        public int[] tierPrices = { 8, 14, 20 };
        public int priceStepPerPurchase = 2;                    // 1개 살 때마다 내 가격만 상승
        public int refreshCost = 2;

        [Header("상점")]
        public float storeTime = 30f;
        public float readyCountdown = 3f;
        public int cardsPerOffer = 3;

        [Header("라운드 보상")]
        public int winnerReward = 8;
        public int loserReward = 10;
        [Range(0f, 1f)] public float tileRewardRatio = 0.1f;   // 칠한 칸의 10%

        public AugmentTier RollTier()
        {
            float total = 0f;
            foreach (float w in tierWeights) total += w;

            float r = Random.value * total;
            for (int i = 0; i < tierWeights.Length; i++)
            {
                if (r < tierWeights[i]) return (AugmentTier)i;
                r -= tierWeights[i];
            }
            return AugmentTier.Silver;
        }

        public static string TierName(AugmentTier tier) => tier switch
        {
            AugmentTier.Gold => "골드",
            AugmentTier.Prism => "프리즘",
            _ => "실버",
        };

        public static string TagName(AugmentTag tag) => tag switch
        {
            AugmentTag.Mobility => "이동",
            AugmentTag.Defense => "방어",
            AugmentTag.Combat => "전투",
            AugmentTag.Paint => "칠하기",
            _ => "경제",
        };
    }
}
