using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyJumpHandler : MonoBehaviour
{
    [Header("Configuración de Salto")]
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float jumpHeight = 3.0f;
    [SerializeField] private AnimationCurve speedCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 0.5f), new Keyframe(1, 1));
    [SerializeField] private float visualYOffset = 0.5f; // Levantar pies visualmente
    //PARA PRUEBAS
    [SerializeField] private float pivotCorrectionY = 1.0f;

    private NavMeshAgent agent;
    private Collider col;
    private bool canJump = false;
    private bool isJumping = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        col = GetComponent<Collider>();
        agent.autoTraverseOffMeshLink = false;
    }

    public void EnableJumping(bool enable) => canJump = enable;

    private void Update()
    {
        if (!canJump) return;
        if (agent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(PerformParabolicJump());
        }
    }

    private IEnumerator PerformParabolicJump()
    {
        isJumping = true;

        // 1. Desacoplar Agente de Transform
        agent.updatePosition = false;
        agent.updateRotation = false;
        if (col != null) col.enabled = false;

        OffMeshLinkData data = agent.currentOffMeshLinkData;

        // --- CORRECCIÓN DE ALTURA ---
        // Obtenemos las posiciones lógicas del NavMesh (Suelo puro)
        Vector3 logicalStart = data.startPos;
        Vector3 logicalEnd = data.endPos;

        // Calculamos las posiciones VISUALES (Donde debe estar el pivote del modelo)
        // Sumamos el baseOffset del agente para que no se entierre
        float agentOffset = agent.baseOffset;
        Vector3 visualStart = agent.transform.position;
        Vector3 visualEnd = logicalEnd + (Vector3.up * agentOffset);

        // Ajuste de seguridad: Raycast hacia abajo en el destino para encontrar el techo real
        // Esto evita que se hunda si el Link estaba mal puesto.
        if (Physics.Raycast(visualEnd + Vector3.up, Vector3.down, out RaycastHit hit, 5f, NavMesh.AllAreas))
        {
            visualEnd.y = hit.point.y + agentOffset;
            logicalEnd.y = hit.point.y; // Actualizamos también la lógica
        }
        // -----------------------------

        // Punto de control Bézier
        Vector3 controlPoint = visualStart + (visualEnd - visualStart) * 0.5f;
        controlPoint.y = Mathf.Max(visualStart.y, visualEnd.y) + jumpHeight;

        // Orientar
        Vector3 lookPos = visualEnd;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime / jumpDuration;
            float fluidTime = speedCurve.Evaluate(timer);

            // Interpolación Bézier
            Vector3 position =
                (1 - fluidTime) * (1 - fluidTime) * visualStart +
                2 * (1 - fluidTime) * fluidTime * controlPoint +
                fluidTime * fluidTime * visualEnd;

            // Offset visual extra (encoger piernas) que desaparece al final (Lerp a 0)
            float legLift = Mathf.Sin(fluidTime * Mathf.PI) * visualYOffset;
            position.y += legLift;

            transform.position = position;
            yield return null;
        }

        // --- ATERRIZAJE SIN POP ---
        // 1. Asegurar posición visual final EXACTA
        transform.position = visualEnd;

        // 2. Reactivar collider
        if (col != null) col.enabled = true;

        // 3. Sincronizar el cerebro lógico (NavMeshAgent)
        // Warp mueve el agente lógico al suelo real (logicalEnd)
        agent.Warp(logicalEnd);

        // 4. Devolver control
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.CompleteOffMeshLink();

        isJumping = false;
    }
}