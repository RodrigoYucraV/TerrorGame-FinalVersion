using UnityEngine;

// Este componente define UN área rectangular donde puede aparecer el enemigo
[RequireComponent(typeof(BoxCollider))]
public class SpawnVolume : MonoBehaviour
{
    private BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        // Importante: Que sea Trigger para no chocar físicamente con cosas
        boxCollider.isTrigger = true;
        // Desactivamos el collider para que Raycasts de disparos no le peguen por error
        // (Usaremos sus datos matemáticos, no su física)
        boxCollider.enabled = false;
    }

    /// <summary>
    /// Devuelve un punto aleatorio dentro de la caja
    /// </summary>
    public Vector3 GetRandomPointInVolume()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider>();

        // Obtenemos los límites en espacio local
        Vector3 center = boxCollider.center;
        Vector3 size = boxCollider.size;

        // Calculamos una posición aleatoria local
        float randomX = Random.Range(-size.x / 2, size.x / 2);
        float randomY = Random.Range(-size.y / 2, size.y / 2);
        float randomZ = Random.Range(-size.z / 2, size.z / 2);

        Vector3 randomLocalPos = center + new Vector3(randomX, randomY, randomZ);

        // Convertimos esa posición local a posición mundial
        return transform.TransformPoint(randomLocalPos);
    }

    // Dibujar la caja en el editor para verla fácil (Gizmos)
    private void OnDrawGizmos()
    {
        // Color semitransparente verde para identificar zonas de spawn
        Gizmos.color = new Color(0, 1, 0, 0.3f);

        // Hack para dibujar el cubo rotado correctamente según el transform
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.matrix = rotationMatrix;

        if (GetComponent<BoxCollider>() != null)
        {
            Gizmos.DrawCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
        }
    }
}