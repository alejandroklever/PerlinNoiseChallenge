using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float lookRadious = 10;
    public float stoppingDistance = 1f;
    public float speed = 6f;

    private Transform target;
    private CharacterController controller;
    private float gravity = -9.8f;
    private Vector3 velocity;
    float smoothRotationForce = .1f;
    float smoothRotationVelocity;

    void Start()
    {
        target = PlayerManager.instance.player.transform;
        controller = GetComponent<CharacterController>();
    }

    void Update() 
    {
        float distance = Vector3.Distance(target.position, transform.position);

        if (distance <= lookRadious) 
        {

            Vector3 direction = (target.position - transform.position).normalized;

            float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            float desiredAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothRotationVelocity, smoothRotationForce);
            transform.rotation = Quaternion.Euler(0, desiredAngle, 0);

            // var movDirection = Quaternion.Euler(0, desiredAngle, 0) * Vector3.forward;
            
            controller.Move(direction * speed* Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadious);
    }
}
