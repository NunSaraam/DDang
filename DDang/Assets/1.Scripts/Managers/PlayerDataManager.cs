using System;
using System.Collections.Generic;
using Rules;
using Store;
using UnityEngine;

namespace Stats
{
    public enum DebuffType { MoveSpeedDown, StunTimeUp, AttackCooldownUp }

    public struct PlayerStats
    {
        public float MoveSpeed;
        public float StunTime;
        public float InvincibleTime;
        public float AttackCooldown;
        public float AttackRange;
        public float StunBonus;
        public float WidePaintChance;
        public float HardGroundChance;
        public int CarrotBonus;
        public float HarvestRatio;
    }

    public readonly struct OwnedAugment
    {
        public readonly AugmentData Data;
        public readonly AugmentTier Tier;

        public OwnedAugment(AugmentData data, AugmentTier tier)
        {
            Data = data;
            Tier = tier;
        }
    }

    public class PlayerLoadout
    {
        public readonly List<OwnedAugment> Augments = new();
        public int PurchaseCount;
        public DebuffType? PendingDebuff;       // 상점에서 받음 → 상점이 끝나면 적용
        public DebuffType? ActiveDebuff;        // 이번 라운드에 적용 중

        public void Clear()
        {
            Augments.Clear();
            PurchaseCount = 0;
            PendingDebuff = null;
            ActiveDebuff = null;
        }
    }

    // 플레이어별 증강·디버프를 모아 최종 능력치를 계산. 한 판이 끝나면 초기화
    public class PlayerDataManager : SingletonMonoBehaviour<PlayerDataManager>
    {
        [Header("기본 능력치")]
        public float baseMoveSpeed = 5f;
        public float baseStunTime = 1.5f;
        public float baseInvincibleTime = 0.5f;
        public float baseAttackCooldown = 0.5f;
        public float baseAttackRange = 1f;

        [Header("능력치 하한")]
        public float minStunTime = 0.3f;
        [Range(0f, 1f)] public float minMoveSpeedRatio = 0.5f;     // 기본 이동속도 대비
        public float minAttackCooldown = 0.1f;

        [Header("능력치 상한 (증강을 여러 개 사도 이 이상은 안 오름)")]
        public float maxMoveSpeed = 8f;
        [Range(0f, 1f)] public float maxWidePaintChance = 0.6f;
        [Range(0f, 1f)] public float maxHardGroundChance = 0.5f;   // 100%면 상대가 내 칸을 못 덮음

        [Header("패자 디버프")]
        public float debuffMultiplier = 1.2f;

        public event Action<PlayerType> LoadoutChanged;

        private readonly PlayerLoadout p1 = new();
        private readonly PlayerLoadout p2 = new();
        private RoundManager round;
        private RoundPhase lastPhase;

        private void Start()
        {
            if (Instance != this) return;
            round = RoundManager.Instance;
            if (round != null) round.PhaseChanged += OnPhaseChanged;
        }

        protected override void OnDestroy()
        {
            if (round != null) round.PhaseChanged -= OnPhaseChanged;
            base.OnDestroy();
        }

        public PlayerLoadout GetLoadout(PlayerType player) => player == PlayerType.Player1 ? p1 : p2;

        public void AddAugment(PlayerType player, AugmentData data, AugmentTier tier)
        {
            PlayerLoadout loadout = GetLoadout(player);
            loadout.Augments.Add(new OwnedAugment(data, tier));
            loadout.PurchaseCount++;
            LoadoutChanged?.Invoke(player);
        }

        // 패자가 고른 디버프를 상대(target)에게. 다음 1라운드만 적용
        public void GiveDebuff(PlayerType target, DebuffType type)
        {
            GetLoadout(target).PendingDebuff = type;
            LoadoutChanged?.Invoke(target);
        }

        public void ResetMatch()
        {
            p1.Clear();
            p2.Clear();
        }

        public PlayerStats GetStats(PlayerType player)
        {
            var s = new PlayerStats
            {
                MoveSpeed = baseMoveSpeed,
                StunTime = baseStunTime,
                InvincibleTime = baseInvincibleTime,
                AttackCooldown = baseAttackCooldown,
                AttackRange = baseAttackRange,
            };
            if (RoundManager.HasInstance && RoundManager.Instance.IsDuelRound) return s;    // 파이널 라운드는 기본 능력치

            PlayerLoadout loadout = GetLoadout(player);
            foreach (OwnedAugment a in loadout.Augments) Apply(ref s, a.Data.stat, a.Data.GetValue(a.Tier));
            if (loadout.ActiveDebuff.HasValue) ApplyDebuff(ref s, loadout.ActiveDebuff.Value);

            s.MoveSpeed = Mathf.Clamp(s.MoveSpeed, baseMoveSpeed * minMoveSpeedRatio, maxMoveSpeed);
            s.StunTime = Mathf.Max(s.StunTime, minStunTime);
            s.AttackCooldown = Mathf.Max(s.AttackCooldown, minAttackCooldown);
            s.WidePaintChance = Mathf.Clamp(s.WidePaintChance, 0f, maxWidePaintChance);
            s.HardGroundChance = Mathf.Clamp(s.HardGroundChance, 0f, maxHardGroundChance);
            return s;
        }

        private static void Apply(ref PlayerStats s, AugmentStat stat, float v)
        {
            switch (stat)
            {
                case AugmentStat.MoveSpeed: s.MoveSpeed += v; break;
                case AugmentStat.StunTime: s.StunTime -= v; break;
                case AugmentStat.InvincibleTime: s.InvincibleTime += v; break;
                case AugmentStat.AttackCooldown: s.AttackCooldown -= v; break;
                case AugmentStat.AttackRange: s.AttackRange += v; break;
                case AugmentStat.StunBonus: s.StunBonus += v; break;
                case AugmentStat.WidePaintChance: s.WidePaintChance += v; break;
                case AugmentStat.HardGroundChance: s.HardGroundChance += v; break;
                case AugmentStat.CarrotBonus: s.CarrotBonus += Mathf.RoundToInt(v); break;
                case AugmentStat.HarvestRatio: s.HarvestRatio += v; break;
            }
        }

        private void ApplyDebuff(ref PlayerStats s, DebuffType type)
        {
            switch (type)
            {
                case DebuffType.MoveSpeedDown: s.MoveSpeed /= debuffMultiplier; break;
                case DebuffType.StunTimeUp: s.StunTime *= debuffMultiplier; break;
                case DebuffType.AttackCooldownUp: s.AttackCooldown *= debuffMultiplier; break;
            }
        }

        // 상점이 끝나면 받은 디버프를 적용하고, 라운드가 끝나면 해제
        private void OnPhaseChanged(RoundPhase phase)
        {
            if (lastPhase == RoundPhase.Store && phase != RoundPhase.Store)
            {
                foreach (PlayerLoadout l in new[] { p1, p2 })
                {
                    l.ActiveDebuff = l.PendingDebuff;
                    l.PendingDebuff = null;
                }
            }
            if (phase == RoundPhase.RoundEnd)
            {
                p1.ActiveDebuff = null;
                p2.ActiveDebuff = null;
            }
            lastPhase = phase;
        }
    }
}
