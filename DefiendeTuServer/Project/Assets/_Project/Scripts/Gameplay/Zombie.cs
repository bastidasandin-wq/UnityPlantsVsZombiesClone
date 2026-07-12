using UnityEngine;

/// <summary>
/// Controla a un zombie individual: caminar hacia la izquierda, morder plantas que
/// encuentra en su camino, y morir por guisantes o por llegar al FailState.
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR (sin tocar código):
///   - Vida, Velocidad: estadisticas basicas del zombie.
///   - Layer Planta: que capa(s) puede "morder". Debe coincidir con la capa que
///     le pusiste a tus prefabs de plantas (Girasol, LanzaGuizantes, etc).
///   - Cadencia / Rango Mordida: que tan seguido y desde que distancia muerde.
///   - Soles Bonus Al Morir: si quieres recompensar al jugador cuando mata este zombie.
///   - Debug Raycast: activalo temporalmente para ver en la Consola que detecta el rayo.
/// </summary>
public class Zombie : MonoBehaviour
{
    [Header("Estadisticas")]
    [Tooltip("Puntos de vida. Cuando llega a 0 o menos, el zombie muere.")]
    public int vida = 4;

    [Tooltip("Unidades por segundo que avanza mientras no este mordiendo nada.")]
    public float velocidad;

    [Header("Ataque")]
    [Tooltip("Capa(s) que el zombie considera 'planta mordible'. Debe coincidir con la Layer de tus prefabs de plantas.")]
    public LayerMask layerPlanta;

    [Tooltip("Segundos entre cada mordida mientras haya una planta enfrente.")]
    public float cadencia = 1f;

    [Tooltip("Distancia del rayo que detecta plantas enfrente del zombie.")]
    public float rangoMordida = .5f;

    [Header("Recompensas (opcional)")]
    [Tooltip("Soles que se le dan al jugador si este zombie muere por un guisante. Dejalo en 0 para no dar recompensa.")]
    public int solesBonusAlMorir = 0;

    [Header("Debug")]
    [Tooltip("Actívalo temporalmente para ver en la Consola que objeto detecta el raycast cada vez que 'muerde'.")]
    public bool debugRaycast = false;

    // --- Animacion (opcional, no rompe nada si no la configuras) ---
    // Si tu Animator Controller tiene estos parametros, el script los usa automaticamente:
    //   - Bool "Mordiendo": true mientras esta mordiendo una planta, false mientras camina.
    //   - Trigger "Morir": se dispara justo antes de destruir al zombie.
    // Si NO tienes estos parametros creados en el Animator, el script simplemente los ignora
    // (no genera errores ni warnings en la Consola).
    const string PARAM_MORDIENDO = "Mordiendo";
    const string PARAM_MORIR = "Morir";

    Animator animator;
    bool tieneParamMordiendo;
    bool tieneParamMorir;

    float cadenciaAcumulada = 0f;
    bool muerto = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        tieneParamMordiendo = TieneParametro(PARAM_MORDIENDO);
        tieneParamMorir = TieneParametro(PARAM_MORIR);
    }

    void Start()
    {
        if (velocidad <= 0f)
            Debug.LogWarning(name + ": 'velocidad' esta en 0. Este zombie no se va a mover hasta que le pongas un valor en el Inspector.");
    }

    void OnEnable()
    {
        // Se registra para que el GameManager sepa cuantos zombies siguen vivos
        // (necesario para detectar la victoria cuando ya no quedan zombies en pantalla).
        if (GameManager.Instance != null)
            GameManager.Instance.RegistrarZombie(this);
    }

    void Update()
    {
        if (muerto) return;

        Debug.DrawRay(transform.position, Vector3.left * rangoMordida, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector3.left, rangoMordida, layerPlanta);

        bool mordiendoAhora = hit.collider != null;

        if (mordiendoAhora)
        {
            if (debugRaycast)
                Debug.Log(name + " -> raycast detecta: " + hit.collider.name + " (layer " + hit.collider.gameObject.layer + ")");

            cadenciaAcumulada += Time.deltaTime;
            if (cadenciaAcumulada >= cadencia)
            {
                cadenciaAcumulada = 0f;

                // Antes: hit.collider.SendMessage("Morder") -> usa reflection y falla
                // silenciosamente si algun dia renombras el metodo. Ahora es directo.
                Plantas planta = hit.collider.GetComponent<Plantas>();
                if (planta != null)
                    planta.Morder();
            }
        }
        else
        {
            cadenciaAcumulada = 0f;
            transform.position -= Vector3.right * velocidad * Time.deltaTime;
        }

        if (tieneParamMordiendo)
            animator.SetBool(PARAM_MORDIENDO, mordiendoAhora);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (muerto) return;

        if (col.CompareTag("Guisante"))
        {
            Guisante guisante = col.GetComponent<Guisante>();
            int daño = guisante != null ? guisante.daño : 1;

            vida -= daño;
            Destroy(col.gameObject);

            if (vida <= 0)
                Morir();
        }
        else if (col.CompareTag("FailState"))
        {
            muerto = true;
            if (GameManager.Instance != null)
                GameManager.Instance.PerderJuego();
            Destroy(gameObject);
        }
    }

    void Morir()
    {
        muerto = true;

        if (solesBonusAlMorir > 0 && GameManager.Instance != null)
            GameManager.Instance.ActualizarSoles(solesBonusAlMorir);

        if (tieneParamMorir)
        {
            animator.SetTrigger(PARAM_MORIR);
            // Si quieres que se vea la animacion de muerte antes de desaparecer,
            // cambia el Destroy de abajo por: Destroy(gameObject, 1f); (o el tiempo que dure el clip)
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.DesregistrarZombie(this);
    }

    bool TieneParametro(string nombre)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == nombre) return true;
        return false;
    }
}