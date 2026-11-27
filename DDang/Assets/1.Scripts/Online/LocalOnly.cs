using UnityEngine;

namespace Online
{
    // 로컬 2인 전용 화면 요소(가운데 분할선 등)에 붙임. 온라인이면 자동으로 꺼짐
    public class LocalOnly : MonoBehaviour
    {
        private void Start()
        {
            if (OnlineSession.IsOnline) gameObject.SetActive(false);
        }
    }
}
