using UnityEngine;

namespace DDang.Rules
{
    [CreateAssetMenu(fileName = "RoundRuleSettings", menuName = "DDang/Round Rule Settings")]
    public class RoundRuleSettings : ScriptableObject
    {
        [Header("승리 조건")]
        public int winsToWin = 3;                                   // 5판 3선승

        [Header("일반 라운드")]
        public float countdownTime = 3f;
        public float roundTime = 60f;
        public float extraTime = 15f;
        [Range(0f, 1f)] public float extraTimeMarginRatio = 0.03f;  // 칠할 수 있는 칸의 3%

        [Header("라운드 이벤트 시점 (시작 후 경과 초)")]
        public float questTime = 10f;
        public float animalWarningTime = 27f;
        public float animalSpawnTime = 30f;

        [Header("결투 라운드")]
        public float duelTime = 20f;

        [Header("연출")]
        public float resultShowTime = 3f;
    }
}
