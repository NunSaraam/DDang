using System;
using Animal;

namespace UI
{
    // 동물 경고: 경고(27초) → 아이콘·"○○ 등장!" 표시, 등장(30초) → 닫기
    public class AnimalWarningPresenter : IDisposable
    {
        private readonly AnimalManager animals;
        private readonly AnimalWarningView view;

        public AnimalWarningPresenter(AnimalManager animals, AnimalWarningView view)
        {
            this.animals = animals;
            this.view = view;

            animals.Warned += OnWarned;
            animals.Spawned += OnSpawned;
        }

        public void Dispose()
        {
            animals.Warned -= OnWarned;
            animals.Spawned -= OnSpawned;
        }

        private void OnWarned(AnimalData data) => view.Show(data.icon, $"{data.displayName} 등장!");
        private void OnSpawned(AnimalBase animal) => view.Hide();
    }
}
