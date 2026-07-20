using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Aplica el volumen guardado (Musica y SFX) al Audio Mixer desde el primer
/// frame del juego, sin importar si el jugador abrio el panel de Ajustes o
/// no. Sobrevive el cambio de escena (Menu -> Nivel_01 -> Nivel_02...) para
/// que el volumen se sienta igual en todas las interfaces.
///
/// COMO USARLO:
///   1. Ponlo UNA SOLA VEZ, en un GameObject de tu escena MenuPrincipal
///      (puede ser el mismo "Menu" que ya tiene MenuSystem).
///   2. Arrastra tu Audio Mixer (el mismo que usa AjustesAudio) al campo "Mixer".
///   3. NO lo agregues en Nivel_01/02/03 -- este objeto ya viaja solo con
///      DontDestroyOnLoad, no hace falta duplicarlo.
/// </summary>
public class AudioBootstrap : MonoBehaviour
{
    [Tooltip("El mismo Audio Mixer que usa AjustesAudio.")]
    public AudioMixer mixer;

    [Header("Deben coincidir EXACTAMENTE con los nombres expuestos en el Mixer")]
    public string parametroMusica = "VolumenMusica";
    public string parametroSFX = "VolumenSFX";

    const string LLAVE_MUSICA = "VolumenMusicaGuardado";
    const string LLAVE_SFX = "VolumenSFXGuardado";

    static bool yaExiste = false;

    void Awake()
    {
        // Evita duplicados por si el jugador regresa a MenuPrincipal
        // (ej. boton "Menu Principal" desde Panel Derrota/Victoria) y esa
        // escena crea un AudioBootstrap nuevo encima del que ya sobrevivia.
        if (yaExiste)
        {
            Destroy(gameObject);
            return;
        }

        yaExiste = true;
        DontDestroyOnLoad(gameObject);

        AplicarVolumenGuardado();
    }

    void AplicarVolumenGuardado()
    {
        float musica = PlayerPrefs.GetFloat(LLAVE_MUSICA, 0.75f);
        float sfx = PlayerPrefs.GetFloat(LLAVE_SFX, 0.75f);

        AplicarVolumen(parametroMusica, musica);
        AplicarVolumen(parametroSFX, sfx);
    }

    void AplicarVolumen(string parametro, float valorLineal)
    {
        float db = Mathf.Log10(Mathf.Max(valorLineal, 0.0001f)) * 20f;

        if (mixer != null)
            mixer.SetFloat(parametro, db);
    }
}