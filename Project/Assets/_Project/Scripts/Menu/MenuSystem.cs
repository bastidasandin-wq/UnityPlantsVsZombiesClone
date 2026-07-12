using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Botones del menu principal. Conecta Jugar() y Salir() al OnClick() de tus
/// botones de UI desde el Inspector (arrastra este GameObject al campo del
/// Button, luego elige MenuSystem > Jugar o MenuSystem > Salir).
///
/// QUE PUEDES MODIFICAR:
///   - Jugar() carga la SIGUIENTE escena en el Build Settings (indice actual + 1).
///     Si tu escena de juego no es la siguiente en la lista, cambia esta linea
///     por SceneManager.LoadScene("NombreDeTuEscena").
/// </summary>
public class MenuSystem : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");

        // Application.Quit() no hace nada mientras pruebas en el Editor (es normal,
        // solo funciona en el juego ya compilado/exportado). El #if de abajo hace
        // que tambien funcione dentro del Editor, para que puedas probar el boton.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}