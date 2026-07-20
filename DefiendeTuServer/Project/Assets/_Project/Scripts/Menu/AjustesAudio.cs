using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Controla el volumen de Musica y SFX a traves de un Audio Mixer, y lo
/// guarda/recupera con PlayerPrefs para que se recuerde la proxima vez que
/// abras el juego.
///
/// COMO CONECTARLO (ver guia paso a paso):
///   1. Pegalo en el objeto "PanelAjustes".
///   2. Arrastra tu Audio Mixer (MasterMixer) al campo "Mixer".
///   3. En el Slider de Musica: On Value Changed (Single) -> arrastra
///      PanelAjustes -> AjustesAudio -> CambiarVolumenMusica.
///   4. En el Slider de SFX: igual, pero -> CambiarVolumenSFX.
/// </summary>
public class AjustesAudio : MonoBehaviour
{
    [Tooltip("Arrastra aqui tu asset MasterMixer.mixer")]
    public AudioMixer mixer;

    [Header("Deben coincidir EXACTAMENTE con los nombres que expusiste en el Mixer")]
    public string parametroMusica = "VolumenMusica";
    public string parametroSFX = "VolumenSFX";

    const string LLAVE_MUSICA = "VolumenMusicaGuardado";
    const string LLAVE_SFX = "VolumenSFXGuardado";

    [Header("Opcional: arrastra tus Sliders aqui para que arranquen con el valor guardado")]
    public UnityEngine.UI.Slider sliderMusica;
    public UnityEngine.UI.Slider sliderSFX;

    void Start()
    {
        float musica = PlayerPrefs.GetFloat(LLAVE_MUSICA, 0.75f);
        float sfx = PlayerPrefs.GetFloat(LLAVE_SFX, 0.75f);

        if (sliderMusica != null) sliderMusica.value = musica;
        if (sliderSFX != null) sliderSFX.value = sfx;

        CambiarVolumenMusica(musica);
        CambiarVolumenSFX(sfx);
    }

    /// <summary>Conecta esto al OnValueChanged del Slider de Musica.</summary>
    public void CambiarVolumenMusica(float valor)
    {
        AplicarVolumen(parametroMusica, valor);
        PlayerPrefs.SetFloat(LLAVE_MUSICA, valor);
    }

    /// <summary>Conecta esto al OnValueChanged del Slider de SFX.</summary>
    public void CambiarVolumenSFX(float valor)
    {
        AplicarVolumen(parametroSFX, valor);
        PlayerPrefs.SetFloat(LLAVE_SFX, valor);
    }

    void AplicarVolumen(string parametro, float valorLineal)
    {
        // Los sliders van de 0 a 1 (lineal), pero el Mixer trabaja en
        // decibeles (escala logaritmica, como el oido humano). Por eso
        // convertimos. Mathf.Max evita log(0), que no existe.
        float db = Mathf.Log10(Mathf.Max(valorLineal, 0.0001f)) * 20f;

        if (mixer != null)
            mixer.SetFloat(parametro, db);
    }
}