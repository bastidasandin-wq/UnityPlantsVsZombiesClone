using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controla el estado global de la partida: soles, mazo de cartas, plantado,
/// y deteccion de victoria/derrota. Es un singleton: cualquier script puede
/// acceder a el con "GameManager.Instance".
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Plantas A Usar: lista de plantas disponibles para el mazo de esta partida.
///   - Deck / Prefab Carta / Txt Soles: referencias de UI, ya deberian estar asignadas.
///   - Panel Derrota / Panel Victoria: opcional, arrastra un Panel de UI si quieres
///     mostrar algo al ganar/perder. Si los dejas vacios, el juego solo se pausa.
///   - Soles Iniciales / Soles Por Sol Recogido: economia basica.
///   - Generar Soles Pasivos: si lo activas, tambien caen soles solos cada rato
///     (ademas de los que sueltan los Girasoles), estilo PvZ clasico.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Plantas disponibles")]
    [Tooltip("Plantas que apareceran como cartas en el mazo, en este orden.")]
    public List<Plantas> plantasAUsar;

    [Header("UI - Mazo de cartas")]
    public GameObject Deck;
    public GameObject PrefabCarta;
    public Text TxtSoles;

    [Header("UI - Fin de partida (opcional, puedes dejarlo vacio)")]
    [Tooltip("Panel que se activa al perder. Puede quedar vacio: el juego se pausa igual.")]
    public GameObject PanelDerrota;
    [Tooltip("Panel que se activa al ganar. Puede quedar vacio: el juego se pausa igual.")]
    public GameObject PanelVictoria;

    [Header("Economia")]
    [SerializeField]
    [Tooltip("Soles con los que arranca el jugador.")]
    int solesIniciales = 200;
    [SerializeField]
    [Tooltip("Soles que da cada sol recogido con click.")]
    int solesPorSolRecogido = 50;

    [Header("Soles pasivos (opcional, estilo PvZ clasico)")]
    [SerializeField]
    [Tooltip("Si esta activo, ademas de los Girasoles, caen soles solos cada cierto tiempo.")]
    bool generarSolesPasivos = false;
    [SerializeField] int cantidadSolPasivo = 25;
    [SerializeField] float intervaloSolPasivo = 10f;

    int soles;
    int plantaSeleccionada = 0;

    readonly List<Zombie> zombiesActivos = new List<Zombie>();
    int totalInstanciadores;
    int instanciadoresTerminados;
    bool oleadasTerminadas = false;
    bool partidaTerminada = false;

    void Awake()
    {
        // Singleton simple: si por error hay dos GameManager en la escena, el segundo se elimina.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        soles = solesIniciales;
        ActualizarTextoSoles();
        GenerarCartas();

        totalInstanciadores = FindObjectsOfType<InstanciadorDeZombies>().Length;
        if (totalInstanciadores == 0)
            Debug.LogWarning("GameManager: no se encontro ningun InstanciadorDeZombies en la escena. La victoria nunca se va a disparar.");

        if (generarSolesPasivos)
            StartCoroutine(GenerarSolesPasivosRutina());
    }

    void Update()
    {
        if (partidaTerminada) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(r.origin, r.direction);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Cuadricula"))
                {
                    Transform t = hit.collider.transform;
                    CrearPlanta(plantaSeleccionada, t);
                }
                else if (hit.collider.CompareTag("Sol"))
                {
                    ActualizarSoles(solesPorSolRecogido);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }

    void GenerarCartas()
    {
        if (Deck == null || PrefabCarta == null)
        {
            Debug.LogWarning("GameManager: falta asignar 'Deck' o 'Prefab Carta' en el Inspector, no se generaron las cartas.");
            return;
        }

        for (int i = 0; i < plantasAUsar.Count; i++)
        {
            GameObject go = Instantiate(PrefabCarta) as GameObject;
            go.transform.SetParent(Deck.transform);
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one;

            Image img = go.GetComponent<Image>();
            img.sprite = plantasAUsar[i].cartaAsignada;

            Button bot = go.GetComponent<Button>();
            bot.onClick.RemoveAllListeners();
            int u = i;
            bot.onClick.AddListener(() => { plantaSeleccionada = u; });
        }
    }

    void CrearPlanta(int numero, Transform t)
    {
        if (numero < 0 || numero >= plantasAUsar.Count) return;
        if (plantasAUsar[numero].precioSoles > soles) return;
        if (t.childCount != 0) return;

        // Antes usaba gameObject.transform.rotation (la rotacion del GameManager, no la de la planta).
        GameObject g = Instantiate(plantasAUsar[numero].gameObject, t.position, Quaternion.identity);
        g.transform.SetParent(t);

        ActualizarSoles(-plantasAUsar[numero].precioSoles);
    }

    /// <summary>Suma (o resta, con numero negativo) soles y actualiza el texto en pantalla.</summary>
    public void ActualizarSoles(int cantidad)
    {
        soles += cantidad;
        ActualizarTextoSoles();
    }

    void ActualizarTextoSoles()
    {
        if (TxtSoles != null)
            TxtSoles.text = soles.ToString();
    }

    IEnumerator GenerarSolesPasivosRutina()
    {
        while (!partidaTerminada)
        {
            yield return new WaitForSeconds(intervaloSolPasivo);
            ActualizarSoles(cantidadSolPasivo);
        }
    }

    // --- Comunicacion con los zombies ---

    /// <summary>Zombie.cs llama esto en su OnEnable para anotarse como "vivo".</summary>
    public void RegistrarZombie(Zombie z)
    {
        if (!zombiesActivos.Contains(z))
            zombiesActivos.Add(z);
    }

    /// <summary>Zombie.cs llama esto en su OnDestroy para darse de baja.</summary>
    public void DesregistrarZombie(Zombie z)
    {
        zombiesActivos.Remove(z);
        VerificarVictoria();
    }

    /// <summary>InstanciadorDeZombies.cs llama esto cuando termina de generar su ultima oleada.</summary>
    public void NotificarFinDeOleadas()
    {
        instanciadoresTerminados++;
        if (instanciadoresTerminados >= totalInstanciadores)
        {
            oleadasTerminadas = true;
            VerificarVictoria();
        }
    }

    void VerificarVictoria()
    {
        if (partidaTerminada) return;
        if (oleadasTerminadas && zombiesActivos.Count == 0)
            Victoria();
    }

    // --- Estados de fin de partida ---

    /// <summary>Zombie.cs llama esto cuando un zombie toca el FailState.</summary>
    public void PerderJuego()
    {
        if (partidaTerminada) return;
        partidaTerminada = true;

        Debug.Log("Has perdido");
        if (PanelDerrota != null)
            PanelDerrota.SetActive(true);

        Time.timeScale = 0f;
    }

    void Victoria()
    {
        partidaTerminada = true;

        Debug.Log("Has ganado");
        if (PanelVictoria != null)
            PanelVictoria.SetActive(true);

        Time.timeScale = 0f;
    }

    /// <summary>
    /// Conecta esto al OnClick() de un boton "Reintentar" en tu Panel Derrota o
    /// Panel Victoria para volver a jugar el mismo nivel desde cero.
    /// </summary>
    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Conecta esto a un boton "Menu Principal" en Panel Derrota o Panel Victoria.
    /// Carga la escena llamada exactamente "MenuPrincipal" (debe estar en Build Settings).
    /// </summary>
    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    /// <summary>
    /// Conecta esto a un boton "Siguiente Nivel" en tu Panel Victoria.
    /// Carga la escena que sigue en el Build Settings. Si ya no hay mas
    /// niveles despues de este, regresa al menu principal en vez de dar error.
    /// </summary>
    public void SiguienteNivel()
    {
        Time.timeScale = 1f;
        int siguiente = SceneManager.GetActiveScene().buildIndex + 1;

        if (siguiente < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(siguiente);
        else
            SceneManager.LoadScene("MenuPrincipal");
    }
}