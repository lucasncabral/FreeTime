using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {
    float speed = 10;
    float damage = 1;
    public LayerMask collisionMask;

    public void SetSpeed(float newSpeed) {
        this.speed = newSpeed;
    }

    void Update () {
        float moveDistance = speed * Time.deltaTime;

        CheckCollisions(moveDistance);

        transform.Translate(Vector3.forward * moveDistance);
	}

    void CheckCollisions(float moveDistance) {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide)) {
            OnHitObject(hit);
        }
    }

    void OnHitObject(RaycastHit hit){
        IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();
        if(damageableObject != null) {
            damageableObject.TakeHit(damage, hit);
        }

        GameObject.Destroy(gameObject);
    }
}
