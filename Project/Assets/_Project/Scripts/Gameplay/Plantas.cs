using UnityEngine;

/// <summary>
/// Componente base para todas las plantas (Girasol, LanzaGuizantes, etc). Maneja
/// vida, la carta del mazo, y que pasa cuando un zombie la muerde.
///
/// QUE PUEDES MODIFICAR EN EL INSPECTOR:
///   - Carta Asignada: el sprite que se muestra en la carta del mazo (Deck).
///   - Precio Soles: cuanto cuesta plantarla.
///   - Vida: cuantas mordidas aguanta antes de morir.
///
/// PARA CREAR UNA PLANTA ESPECIAL (ej. una que explota al morir):
///   Crea una clase que herede de esta: "public class PlantaExplosiva : Plantas"
///   y sobreescribe AlMorir() con "protected override void AlMorir() { ... }".
/// </summary>
public class Plantas : MonoBehaviour
{
    [Header("Configuracion de la carta")]
    [Tooltip("Sprite que se muestra en la carta del mazo (Deck).")]
    public Sprite cartaAsignada;

    [Tooltip("Costo en soles para plantarla.")]
    public int precioSoles;

    [Header("Estadisticas")]
    [Tooltip("Cuantas mordidas aguanta antes de morir.")]
    public int vida;

    // --- Animacion (opcional) ---
    // Si tu Animator Controller tiene un Trigger llamado "Morder", se dispara en cada
    // mordida. Si no existe ese parametro, el script lo ignora sin generar errores.
    const string PARAM_MORDER = "Morder";

    Animator animator;
    bool tieneParamMorder;
    bool viva = true;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        tieneParamMorder = TieneParametro(PARAM_MORDER);
    }

    /// <summary>
    /// Llamado por Zombie.cs cada vez que un zombie la muerde. Publico porque
    /// necesita ser accesible desde otro script (Zombie).
    /// </summary>
    public void Morder()
    {
        if (!viva) return;

        vida--;

        if (tieneParamMorder)
            animator.SetTrigger(PARAM_MORDER);

        if (vida <= 0)
        {
            viva = false;
            AlMorir();
        }
    }

    /// <summary>
    /// Se ejecuta una sola vez, cuando la vida llega a 0. Sobreescribe este metodo
    /// en una subclase para plantas especiales (ej. dejar una explosion, soltar un
    /// bonus, etc). Si no la sobreescribes, simplemente se destruye el GameObject.
    /// </summary>
    protected virtual void AlMorir()
    {
        Destroy(gameObject);
    } 

    bool TieneParametro(string nombre)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.name == nombre) return true;
        return false;
    }
}