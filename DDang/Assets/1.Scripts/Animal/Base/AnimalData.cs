using UnityEngine;

namespace Animal
{
    public enum AnimalType { TileControl, Obstacle, PlayerInterference, ScreenInterference }
    public enum AnimalSpawnArea { RandomFloor, Center }

    [CreateAssetMenu(fileName = "AnimalData", menuName = "DDang/Animal Data")]
    public class AnimalData : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;                                     // 경고 UI에 표시
        public AnimalType category;
        public AnimalBase prefab;

        [Header("규칙")]
        [Range(1f, 30f)] public float duration = 15f;           // 지나면 자동 퇴장
        public AnimalSpawnArea spawnArea = AnimalSpawnArea.RandomFloor;
        public bool inRotation = true;                          // 끄면 랜덤 등장 목록에서 빠짐
    }
}
