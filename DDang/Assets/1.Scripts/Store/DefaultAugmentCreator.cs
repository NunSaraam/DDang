#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Store
{
    public static class DefaultAugmentCreator
    {
        private const string Folder = "Assets/4.Data/Augment";
        private const string SettingsPath = "Assets/4.Data/Store/StoreSettings.asset";

        [MenuItem("DDang/기본 증강 10종 만들기")]
        private static void Create()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/4.Data", "Augment");

            var settings = AssetDatabase.LoadAssetAtPath<StoreSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<StoreSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            settings.augments.Clear();
            settings.augments.Add(Make("MoveSpeed", "이속 증가", "이동속도 +{0}", AugmentTag.Mobility, AugmentStat.MoveSpeed, 0.5f, 1f, 1.5f));
            settings.augments.Add(Make("StunReduce", "기절 시간 감소", "내 기절 시간 -{0}초", AugmentTag.Defense, AugmentStat.StunTime, 0.15f, 0.3f, 0.45f));
            settings.augments.Add(Make("Invincible", "무적 시간 증가", "기절 후 무적 +{0}초", AugmentTag.Defense, AugmentStat.InvincibleTime, 0.2f, 0.4f, 0.6f));
            settings.augments.Add(Make("AttackCooldown", "공격 쿨타임 감소", "공격 쿨타임 -{0}초", AugmentTag.Combat, AugmentStat.AttackCooldown, 0.05f, 0.1f, 0.15f));
            settings.augments.Add(Make("AttackRange", "공격 범위 증가", "공격 범위 +{0}칸", AugmentTag.Combat, AugmentStat.AttackRange, 0.2f, 0.4f, 0.6f));
            settings.augments.Add(Make("StunBonus", "강타", "상대 기절 시간 +{0}초", AugmentTag.Combat, AugmentStat.StunBonus, 0.15f, 0.3f, 0.45f));
            settings.augments.Add(Make("WidePaint", "넓은 발자국", "{0} 확률로 옆 칸도 칠함", AugmentTag.Paint, AugmentStat.WidePaintChance, 0.1f, 0.2f, 0.3f, true));
            settings.augments.Add(Make("HardGround", "굳은 땅", "{0} 확률로 내 칸이 버팀", AugmentTag.Paint, AugmentStat.HardGroundChance, 0.1f, 0.2f, 0.3f, true));
            settings.augments.Add(Make("CarrotBonus", "당근 보너스", "라운드 보상 +{0} 당근", AugmentTag.Economy, AugmentStat.CarrotBonus, 3f, 4f, 5f));
            settings.augments.Add(Make("Harvest", "수확", "칠한 칸 보상 +{0}", AugmentTag.Economy, AugmentStat.HarvestRatio, 0.05f, 0.08f, 0.12f, true));

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"증강 {settings.augments.Count}종을 만들고 {SettingsPath}에 등록했습니다.");
        }

        private static AugmentData Make(string file, string name, string format, AugmentTag tag, AugmentStat stat,
                                        float silver, float gold, float prism, bool percent = false)
        {
            string path = $"{Folder}/{file}.asset";
            var data = AssetDatabase.LoadAssetAtPath<AugmentData>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<AugmentData>();
                AssetDatabase.CreateAsset(data, path);
            }

            data.displayName = name;
            data.effectFormat = format;
            data.tag = tag;
            data.stat = stat;
            data.tierValues = new[] { silver, gold, prism };
            data.showAsPercent = percent;
            EditorUtility.SetDirty(data);
            return data;
        }
    }
}
#endif
