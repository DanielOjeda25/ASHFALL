using UnityEngine;
using UnityEngine.UIElements;

namespace ShooterDem
{
// Pantalla de fin de partida en UI Toolkit. Se muestra al recibir GameManager.GameOverShown
// (con el mensaje GANASTE/PERDISTE) y cablea Reiniciar/Salir. Va en un GameObject con
// UIDocument (GameOver_UITK), sortOrder por encima del HUD y del menu de pausa.
[RequireComponent(typeof(UIDocument))]
public class GameOverController : MonoBehaviour
{
    private VisualElement root;
    private Label resultText;

    void OnEnable()
    {
        GameManager.GameOverShown += OnGameOver;
    }

    void OnDisable()
    {
        GameManager.GameOverShown -= OnGameOver;
    }

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        resultText = root.Q<Label>("result-text");

        Bind("btn-restart", () => GameManager.Instance?.RestartGame());
        Bind("btn-quit", () => GameManager.Instance?.QuitGame());

        // Sonidos de UI: clic + hover en los botones de esta pantalla.
        root.Query<Button>().ForEach(b =>
        {
            b.clicked += UiAudio.PlayClick;
            b.RegisterCallback<MouseEnterEvent>(_ => UiAudio.PlayHover());
        });

        Hide();   // oculto al empezar; aparece al terminar la partida
    }

    void Bind(string name, System.Action action)
    {
        var btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += action;
    }

    void OnGameOver(string message)
    {
        if (root == null) return;
        if (resultText != null) resultText.text = message;
        Show();
    }

    void Show() { root.style.display = DisplayStyle.Flex; }
    void Hide() { root.style.display = DisplayStyle.None; }
}
}
