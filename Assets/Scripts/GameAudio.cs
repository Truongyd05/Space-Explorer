using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Persistent 2D music and effects; automatically available in every scene.</summary>
public class GameAudio : MonoBehaviour
{
    static GameAudio instance;
    AudioSource music;
    AudioSource effects;
    AudioClip laser, collect, explosion, click, gameOver;
    bool muted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init() { Ensure(); }

    static void Ensure()
    {
        if (instance != null) return;
        var go = new GameObject("GameAudio");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<GameAudio>();
    }

    void Awake()
    {
        instance = this;
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        music.spatialBlend = 0f;
        music.volume = 0.22f;
        music.clip = Resources.Load<AudioClip>("Audio/SpaceMusic");
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
        effects.spatialBlend = 0f;
        effects.volume = 0.7f;
        laser = Resources.Load<AudioClip>("Audio/Laser");
        collect = Resources.Load<AudioClip>("Audio/Collect");
        explosion = Resources.Load<AudioClip>("Audio/Explosion");
        click = Resources.Load<AudioClip>("Audio/Click");
        gameOver = Resources.Load<AudioClip>("Audio/GameOver");
        muted = PlayerPrefs.GetInt("AudioMuted", 0) != 0;
        ApplyMute();
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureScene(SceneManager.GetActiveScene());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { ConfigureScene(scene); }

    void ConfigureScene(Scene scene)
    {
        // Existing project cameras have no listener. Keep one on the active camera.

#if UNITY_2022_2_OR_NEWER
        bool hasListener = FindAnyObjectByType<AudioListener>() != null;
#else
        bool hasListener = FindObjectOfType<AudioListener>() != null;
#endif
        if (!hasListener && Camera.main != null)
            Camera.main.gameObject.AddComponent<AudioListener>();
        music.volume = scene.name == "Gameplay" ? 0.22f : 0.15f;
        if (scene.name == "EndGame")
        {
            music.Stop();
            Play(gameOver);
        }
        else if (music.clip != null && !music.isPlaying) music.Play();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.M)) return;
        muted = !muted;
        ApplyMute();
        PlayerPrefs.SetInt("AudioMuted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    void ApplyMute() { music.mute = effects.mute = muted; }
    void Play(AudioClip clip) { if (clip != null) effects.PlayOneShot(clip); }
    public static void Shoot() { Ensure(); instance.Play(instance.laser); }
    public static void Collect() { Ensure(); instance.Play(instance.collect); }
    public static void Explode() { Ensure(); instance.Play(instance.explosion); }
    public static void Click() { Ensure(); instance.Play(instance.click); }
}
