using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class WordCollisionArea : MonoBehaviour
{
    public TriggerEvent collision_event;
    public LayerMask layers_to_collide_with;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure other.gameObject.layer is in layers_to_collide with
        if ((layers_to_collide_with.value & (1 << other.gameObject.layer)) > 0)
        {
            Debug.Log("Collision with " + other.gameObject.name);

            // Trigger the collision callback
            BaseEventData event_data = new BaseEventData(EventSystem.current);
            collision_event.Invoke(event_data);

            // Finally, destroy the word
            Destroy(other.gameObject);
        }
    }
}
