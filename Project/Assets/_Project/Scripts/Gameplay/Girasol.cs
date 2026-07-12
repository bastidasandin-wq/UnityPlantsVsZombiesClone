using System.Collections;
using UnityEngine;

/// <summary>
/// Girasol: genera soles automaticamente cada cierto tiempo, que el jugador
/// puede recolectar haciendo click (ver GameManager.cs).
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Frecuencia De Soles: cada cuantos segundos genera un sol nuevo.
///   - Sol: el prefab del sol a instanciar (arrastra Sol.prefab).
///   - Tiempo De Vida Sol: cuanto dura el sol en pantalla si no lo recoges.
/// </summary>
public class Girasol : MonoBehaviour
{
    [Header("Generacion de soles")]
    [Tooltip("Segundos entre cada sol generado por este girasol.")]
    public float frecuenciaDeSoles = 10f;

    [Tooltip("Prefab del sol a instanciar (arrastra Sol.prefab).")]
    public GameObject Sol;

    [Tooltip("Tiempo que dura el sol en pantalla antes de desaparecer si no lo recoges.")]
    public float tiempoDeVidaSol = 10f;

    // --- Animacion (opcional) ---
    // Si tu Animator Controller tiene un Trigger llamado "Generar", se dispara cada
    // vez que sale un sol nuevo (util para un pequeño "salto" o brillo del girasol).
    const string PARAM_GENERAR = "Generar";

    Animator animator;
    bool tieneParamGenerar;

    void Awake()
    {
        animator = GetComponent<Animator>();
        tieneParamGenerar = TieneParametro(PARAM_GENERAR);
    }

    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(frecuenciaDeSoles);

            if (Sol == null)
            {
                Debug.LogWarning(name + ": el campo 'Sol' no esta asignado en el Inspector.");
                continue;
            }

            // Antes: Vector3.left * Random.Range(-1f, 1f) -> mismo resultado matematico,
            // pero "left * negativo" es confuso de leer. Sin cambio de comportamiento.
            Vector3 offset = Vector3.right * Random.Range(-1f, 1f) + Vector3.up * Random.Range(0f, 1f);
            GameObject go = Instantiate(Sol, transform.position + offset, Sol.transform.rotation);
            Destroy(go, tiempoDeVidaSol);

            if (tieneParamGenerar)
                animator.SetTrigger(PARAM_GENERAR);
        }
    }

    bool TieneParametro(string nombre)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == nombre) return true;
        return false;
    }
}