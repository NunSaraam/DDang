using UnityEngine;

namespace Store
{
    public enum AugmentTier
    {
        Silver,
        Gold,
        Prism
    }

    // TODO: 같은 태그 3개 시너지 (보류 - 증강 목록을 늘린 뒤 제작)
    public enum AugmentTag 
    { 
        Mobility,
        Defense,
        Combat,
        Paint,
        Economy
    }

    public enum AugmentStat
    {
        MoveSpeed,          // 이동속도 +
        StunTime,           // 내 기절 시간 -
        InvincibleTime,     // 기절 후 무적 시간 +
        AttackCooldown,     // 공격 쿨타임 -
        AttackRange,        // 공격 범위 +
        StunBonus,          // 내가 때린 상대의 기절 시간 +
        WidePaintChance,    // 옆 칸도 칠할 확률 +
        HardGroundChance,   // 상대가 내 칸을 덮을 때 버틸 확률 +
        CarrotBonus,        // 라운드 보상 당근 +
        HarvestRatio,       // 칠한 칸 보상 비율 +
    }

    [CreateAssetMenu(fileName = "Augment", menuName = "DDang/Augment")]
    public class AugmentData : ScriptableObject
    {
        public string displayName;
        [Tooltip("{0} 자리에 등급 수치가 들어감. 예: 이동속도 +{0}")]
        public string effectFormat = "+{0}";
        public Sprite icon;
        public AugmentTag tag;
        public AugmentStat stat;

        [Tooltip("실버 / 골드 / 프리즘 수치. 감소 효과도 양수로 입력")]
        public float[] tierValues = new float[3];
        [Tooltip("확률·비율이면 체크 (0.1 → 10%로 표시)")]
        public bool showAsPercent;

        public float GetValue(AugmentTier tier) => tierValues[(int)tier];

        public string Describe(AugmentTier tier)
        {
            float v = GetValue(tier);
            string value = showAsPercent ? $"{v * 100f:0}%" : $"{v:0.##}";
            return string.Format(effectFormat, value);
        }
    }
}
