using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 6f, -10f);

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        var desired = new Vector3(0f, target.position.y, 0f) + offset;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSpeed);
    }
}
