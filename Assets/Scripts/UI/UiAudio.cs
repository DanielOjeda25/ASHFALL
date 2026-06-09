using UnityEngine;

namespace ShooterDem
{
    /// <summary>
    /// Sonidos de la interfaz (2D): clic y hover de botones + abrir/cerrar pausa.
    /// Un solo punto con los clips; los controllers de UI llaman a los metodos estaticos
    /// (PlayClick/PlayHover) sin necesitar referencias. El sonido de pausa lo dispara solo
    /// (escucha GameManager.PauseChanged). Va en un GameObject de UI (p. ej. PauseMenu_UITK).
    /// </summary>
    public class UiAudio : MonoBehaviour
    {
        [Header("Clips")]
        public AudioClip clickClip;   // clic de boton
        public AudioClip hoverClip;   // pasar el mouse por un boton
        public AudioClip escClip;     // abrir/cerrar el menu de pausa
        [Range(0f, 1f)] public float volume = 1f;

        public static UiAudio Instance { get; private set; }

        private AudioSource source;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;             // 2D: feedback de interfaz
            source.ignoreListenerPause = true;    // suena aunque el juego este pausado
        }

        void OnEnable()  { GameManager.PauseChanged += OnPauseChanged; }
        void OnDisable() { GameManager.PauseChanged -= OnPauseChanged; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        void OnPauseChanged(bool paused) => Play(escClip);   // mismo sonido al abrir y cerrar

        public static void PlayClick() => Instance?.Play(Instance.clickClip);
        public static void PlayHover() => Instance?.Play(Instance.hoverClip);

        void Play(AudioClip clip)
        {
            if (clip != null && source != null) source.PlayOneShot(clip, volume);
        }
    }
}
