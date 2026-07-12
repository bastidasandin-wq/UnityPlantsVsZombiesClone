using System.Collections;
using UnityEngine;

/// <summary>
/// Genera zombies en un carril, ya sea con tiempos fijos o de forma aleatoria.
/// Pon uno de estos por cada carril de tu pasto.
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Modo Aleatorio (desmarcado): usa el array "Tiempos" -> cada numero es un
///     segundo ABSOLUTO desde el inicio del nivel en el que sale un zombie.
///     Ej: {5, 10, 10, 15} = uno a los 5s, uno a los 10s, otro a los 10s, otro a los 15s.
///   - Modo Aleatorio (marcado): ignora "Tiempos" y genera "Cantidad Zombies" zombies,
///     esperando entre "Intervalo Minimo" e "Intervalo Maximo" segundos entre cada uno.
///   - Tipos De Zombie: si la llenas (arrastra Zombie, ZombieCONO, etc), elige uno al
///     azar en cada spawn. Si la dejas vacia, siempre usa el campo "Zombie" de abajo.
/// </summary>
public class InstanciadorDeZombies : MonoBehaviour
{
    [Header("Modo fijo (se ignora si 'Modo Aleatorio' esta activo)")]
    [Tooltip("Segundos ABSOLUTOS desde el inicio del nivel en los que debe aparecer cada zombie. Deben ir en orden ascendente.")]
    public int[] tiempos;

    [Tooltip("Prefab de zombie a instanciar cuando 'Tipos De Zombie' esta vacio.")]
    public GameObject Zombie;

    [Header("Modo aleatorio")]
    [Tooltip("Si esta activo, ignora 'tiempos' de arriba y genera zombies con intervalos al azar.")]
    public bool modoAleatorio = true;

    [Tooltip("Tiempo minimo entre zombies (segundos).")]
    public float intervaloMinimo = 3f;

    [Tooltip("Tiempo maximo entre zombies (segundos).")]
    public float intervaloMaximo = 8f;

    [Tooltip("Cuantos zombies genera esta linea en total, en modo aleatorio.")]
    public int cantidadZombies = 20;

    [Header("Variedad de zombies (opcional)")]
    [Tooltip("Si llenas esta lista, se elige un prefab al azar en cada spawn (sirve en ambos modos). Si la dejas vacia, siempre usa el campo 'Zombie' de arriba.")]
    public GameObject[] tiposDeZombie;

    void Start()
    {
        if (modoAleatorio)
            StartCoroutine(GenerarOleadasAleatorias());
        else
            StartCoroutine(GenerarOleadasFijas());
    }

    IEnumerator GenerarOleadasFijas()
    {
        float tiempoAnterior = 0f;

        foreach (int tiempoAbsoluto in tiempos)
        {
            float espera = Mathf.Max(0f, tiempoAbsoluto - tiempoAnterior);
            yield return new WaitForSeconds(espera);

            tiempoAnterior = tiempoAbsoluto;
            InstanciarZombie();
        }

        AvisarFinDeOleadas();
    }

    IEnumerator GenerarOleadasAleatorias()
    {
        for (int i = 0; i < cantidadZombies; i++)
        {
            float espera = Random.Range(intervaloMinimo, intervaloMaximo);
            yield return new WaitForSeconds(espera);

            InstanciarZombie();
        }

        AvisarFinDeOleadas();
    }

    void InstanciarZombie()
    {
        GameObject prefab = ElegirZombie();
        if (prefab == null) return;

        Instantiate(prefab, transform.position, prefab.transform.rotation);
    }

    GameObject ElegirZombie()
    {
        if (tiposDeZombie != null && tiposDeZombie.Length > 0)
        {
            // Evita elegir un hueco vacio si dejaste algun slot sin asignar en el Inspector.
            for (int intento = 0; intento < tiposDeZombie.Length; intento++)
            {
                GameObject candidato = tiposDeZombie[Random.Range(0, tiposDeZombie.Length)];
                if (candidato != null) return candidato;
            }
        }

        return Zombie;
    }

    void AvisarFinDeOleadas()
    {
        // Avisa al GameManager que esta linea de zombies ya no va a generar mas.
        // El GameManager espera a que TODAS las lineas (una por carril) terminen
        // para poder declarar la victoria.
        if (GameManager.Instance != null)
            GameManager.Instance.NotificarFinDeOleadas();
    }
}