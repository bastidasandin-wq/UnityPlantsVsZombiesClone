using UnityEngine;

/// <summary>
/// El proyectil que disparan las plantas tipo LanzaGuizantes. Viaja en linea recta
/// hacia la derecha y se autodestruye si no impacta nada (evita fugas de memoria).
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Velocidad: que tan rapido viaja el guisante.
///   - Daño: cuanta vida le quita al zombie que golpee (Zombie.cs lo lee automaticamente).
///   - Tiempo De Vida: segundos antes de autodestruirse si no golpea nada.
/// </summary>
public class Guisante : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Unidades por segundo que avanza el guisante hacia la derecha.")]
    [Range(1, 30)]
    public int velocidad = 10;

    [Header("Combate")]
    [Tooltip("Vida que le resta al zombie al impactarlo. Zombie.cs lee este valor directamente.")]
    [Range(1, 10)]
    public int daño = 1;

    [Header("Limpieza")]
    [Tooltip("Si no impacta nada en este tiempo, se autodestruye. Evita guisantes 'fantasma' acumulandose fuera de camara.")]
    public float tiempoDeVida = 5f;

    void Start()
    {
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        transform.position += Vector3.right * velocidad * Time.deltaTime;
    }
}