using UI;
using UnityEngine;
using UnityEngine.Audio;

// 소리 관리자 (씬이 바뀌어도 남음): BGM·효과음 재생, 부팅할 때 저장된 설정 적용
// 볼륨은 AudioMixer(Master / BGM / SFX 그룹)가 담당
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource bgmSource;                                   // 출력 = Mixer의 BGM 그룹
    [SerializeField] private AudioSource sfxSource;                 // 출력 = Mixer의 SFX 그룹
    [SerializeField] private AudioMixer mixer;

    public AudioMixer Mixer => mixer;
    public SettingsModel Settings { get; private set; }             // 설정창(SettingsPresenter)과 같이 씀

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Settings = new SettingsModel();
            Settings.Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // AudioMixer 값은 Awake에서 바꾸면 적용되지 않아서 Start에서
    private void Start()
    {
        if (Instance == this) GameSettings.ApplyAll(Settings, mixer);   // 설정창을 안 열어도 지난번 설정 유지
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 효과음 (겹쳐서 재생 가능)
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip, volumeScale);
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }
}
