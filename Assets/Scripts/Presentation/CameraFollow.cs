using UnityEngine;

/// <summary>平滑跟随玩家，并将摄像机保持在 2D 平面前方。</summary>
public sealed class CameraFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 velocity;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = new Vector3(target.position.x, target.position.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.16f);
    }
}
