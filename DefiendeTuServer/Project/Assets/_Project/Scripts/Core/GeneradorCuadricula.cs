using UnityEngine;

public class GeneradorCuadricula : MonoBehaviour
{
    [Header("Configuración del Tablero")]
    public int columnas = 5;
    public int filas = 5;
    public float separacionX = 1.5f; // Controla el ancho de los cuadros
    public float separacionY = 1.2f; // Controla el alto de los cuadros

    [Header("Referencias")]
    public GameObject casillaPrefab;

    void Start()
    {
        ConstruirTablero();
    }

    private void ConstruirTablero()
    {
        for (int x = 0; x < columnas; x++)
        {
            for (int y = 0; y < filas; y++)
            {
                // 1. Calculamos la posición LOCAL (relativa al objeto Tablero)
                Vector3 posicionLocal = new Vector3(x * separacionX, y * separacionY, 0);

                // 2. Instanciamos la casilla asignándole el Tablero como padre desde el inicio
                GameObject nuevaCasilla = Instantiate(casillaPrefab, transform);

                // 3. Le asignamos su posición respecto al padre
                nuevaCasilla.transform.localPosition = posicionLocal;

                nuevaCasilla.name = $"Casilla [{x}, {y}]";
            }
        }
    }
}