using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.EventTrigger;

public class TypeWordToDestroy : MonoBehaviour
{
    // Initialization
    public string word_to_type = "some really long default that will be overwritten";

    // Callbacks
    public TriggerEvent completed_typing_event;
    public TriggerEvent error_while_typing_event;

    // State
    private char[] letters_to_type;
    private int typed_letter_index = 0;

    // Lookup references
    private TextMeshPro text_mesh;
    public GameObject success_particle_system;

    void Start()
    {
        // Split word_to_type into letters for letters_remaining_to_type
        letters_to_type = word_to_type.ToCharArray();

        if (letters_to_type.Length > 0)
            typed_letter_index = 0;

        text_mesh = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        ListenForInput();
    }

    public void ListenForInput()
    {
        // Skip all this processing if the user isn't pressing anything :)
        if (Input.anyKeyDown)
        {
            // Skip over mouse "key" downs
            if (Input.GetMouseButtonDown(0)
                 || Input.GetMouseButtonDown(1)
                 || Input.GetMouseButtonDown(2))
                return; //Do Nothing

            // Get the key that was pressed
            char key_pressed = Input.inputString[0];

            // Debug.Log("key pressed: " + key_pressed);
            // Debug.Log("expecting... " + letters_to_type[typed_letter_index]);

            // Check if the key pressed matches the next letter in the word
            if (key_pressed == letters_to_type[typed_letter_index])
            {
                // If so, advance the typed_letter_index
                typed_letter_index++;

                // Change the letters typed so far to another color in TextMeshPro
                text_mesh.text = "<color=#1aeb47>" + word_to_type.Substring(0, typed_letter_index) + "</color>" + word_to_type.Substring(typed_letter_index);
            }
            else
            {
                // If the letter doesn't match our next letter, restart at the beginning!
                typed_letter_index = 0;

                // However, if the first letter is the letter we just typed, we should bump ahead one :)
                if (key_pressed == letters_to_type[typed_letter_index])
                    typed_letter_index++;

                // Change all letters back to their original color in TextMeshPro
                text_mesh.text = "<color=#1aeb47>" + word_to_type.Substring(0, typed_letter_index) + "</color>" + word_to_type.Substring(typed_letter_index);
            }
        }

        // If we've typed all the letters, we can finally destroy the word
        if (typed_letter_index >= letters_to_type.Length)
            TypedWordSuccessfully();
    }

    public void TypedWordSuccessfully()
    {
        // Instantiate a new transient object to play the particle system
        if (success_particle_system != null)
        {
            GameObject particle_system_object = Instantiate(success_particle_system, transform.position, Quaternion.identity);
            particle_system_object.GetComponent<ParticleSystem>().Play();
            Destroy(particle_system_object, 4f);
        }

        // Trigger completed_typing_event
        BaseEventData event_data = new BaseEventData(EventSystem.current);
        completed_typing_event.Invoke(event_data);

        Destroy(this.gameObject);
    }
}
