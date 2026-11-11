using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animal
{
    // 고릴라: 전체 타일 주인을 섞음. 5초 간격 3회, 장애물·얼린 칸 제외
    public class Gorilla : AnimalBase
    {
        public float shuffleInterval = 5f;
        public int shuffleCount = 3;

        protected override void OnSpawn() => StartCoroutine(ShuffleRoutine());

        private IEnumerator ShuffleRoutine()
        {
            for (int i = 0; i < shuffleCount; i++)
            {
                ShuffleTiles();
                yield return new WaitForSeconds(shuffleInterval);
            }
            Finish();
        }

        private void ShuffleTiles()
        {
            var cells = new List<Vector2Int>();
            foreach (Vector2Int c in Grid.FloorCells)
                if (!Grid.IsLocked(c)) cells.Add(c);

            var owners = cells.ConvertAll(c => Grid.GetBlock(c).owner);
            for (int i = owners.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (owners[i], owners[j]) = (owners[j], owners[i]);
            }
            for (int i = 0; i < cells.Count; i++) PaintCell(cells[i], owners[i]);
        }
    }
}
