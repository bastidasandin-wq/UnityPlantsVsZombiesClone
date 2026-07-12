using UnityEngine;

/// <summary>
/// Lanza guisantes hacia la derecha cuando detecta un zombie en su carril.
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Cadencia: segundos minimos entre cada disparo.
///   - Guisante: prefab del proyectil (arrastra guisante.prefab).
///   - Cañon: el Transform desde donde sale el guisante (normalmente un hijo
///     de este mismo GameObject, ya deberia estar asignado).
///   - Layer Zombie: que capa cuenta como "zombie detectado". Debe coincidir
///     con la Layer real de tus prefabs Zombie / ZombieCONO.
///   - Rango Deteccion: que tan lejos "ve" zombies en su carril.
/// </summary>
public class LanzaGuizantes : MonoBehaviour
{
    [Header("Disparo")]
    [Tooltip("Segundos minimos entre cada disparo.")]
    public float cadencia = 3f;

    [Tooltip("Prefab del proyectil a instanciar (arrastra guisante.prefab).")]
    public GameObject Guisante;

    [Tooltip("Transform desde donde sale el guisante (el hijo 'Cañon').")]
    public Transform cañon;

    [Header("Deteccion de zombies")]
    [Tooltip("Capa(s) que cuentan como zombie. Debe coincidir con la Layer de Zombie/ZombieCONO.")]
    public LayerMask layerZombie;

    [Tooltip("Distancia maxima a la que detecta zombies en su carril.")]
    public float rangoDeteccion = 12f;

    // --- Animacion (opcional) ---
    // Si tu Animator Controller tiene un Trigger llamado "Disparar", se dispara
    // cada vez que este script instancia un guisante.
    const string PARAM_DISPARAR = "Disparar";

    Animator animator;
    bool tieneParamDisparar;
    float cooldown = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        tieneParamDisparar = TieneParametro(PARAM_DISPARAR);
    }

    void Update()
    {
        if (cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
            return;
        }

        if (cañon == null || Guisante == null) return;

        // Antes: esperaba 'cadencia' segundos ANTES de revisar si habia un zombie,
        // lo que podia tardar hasta 'cadencia' segundos en reaccionar. Ahora revisa
        // cada frame y solo aplica el cooldown entre disparos reales.
        RaycastHit2D hit = Physics2D.Raycast(cañon.position, Vector3.right, rangoDeteccion, layerZombie);
        if (hit.collider != null)
        {
            Disparar();
            cooldown = cadencia;
        }
    }

    void Disparar()
    {
        // Guisante.cs ya se autodestruye solo (tiempoDeVida), no hace falta un Destroy() extra aqui.
        Instantiate(Guisante, cañon.position, Guisante.transform.rotation);

        if (tieneParamDisparar)
            animator.SetTrigger(PARAM_DISPARAR);
    }

    bool TieneParametro(string nombre)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == nombre) return true;
        return false;
    }
}