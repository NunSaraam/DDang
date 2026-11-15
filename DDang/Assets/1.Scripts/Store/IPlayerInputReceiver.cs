using UnityEngine;

namespace Store
{
    public interface IPlayerInputReceiver
    {
        void OnNavigate(Vector2 input);
        void OnConfirm();
    }
}
