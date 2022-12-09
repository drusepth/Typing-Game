using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatrixManager : MonoBehaviour
{
    // Game feel
    public float starting_spawn_rate = 1f;       // Number of seconds between words
    public float spawn_acceleration_rate = 0.1f; // How much to speed up spawn rate by each time a word is spawned
    public float level_time = 30f;               // How long each level lasts; no more words spawned after this time
    public int min_words_on_screen = 2;          // Minimum number of words on screen at any given time
    public int max_words_on_screen = 10;         // How many words can be on screen at once

    // Game setup
    public GameObject word_prefab;
    public KeyCode play_again_button = KeyCode.Space;

    // UI References
    public TextMeshProUGUI time_remaining_text;
    public TextMeshProUGUI words_typed_text;
    public TextMeshProUGUI words_missed_text;
    public TextMeshProUGUI words_per_minute_text;
    public TextMeshProUGUI personal_best_wpm_text;
    public GameObject end_of_level_screen_container;
    public CameraShake camera_shake;

    // Data references
    private string[] dictionary;

    // Game state
    private float current_spawn_rate;
    private float level_time_remaining;
    private int words_typed = 0;
    private int words_missed = 0;

    // Timers
    private float time_until_next_word_spawn = 1f;

    void Start()
    {
        // Initialize values
        current_spawn_rate = starting_spawn_rate;
        level_time_remaining = level_time;

        // Cache references
        camera_shake = Camera.main.GetComponent<CameraShake>();

        // Set up GUI
        time_remaining_text.text = level_time_remaining.ToString("0.0");
        end_of_level_screen_container.SetActive(false);

        LoadDictionaryFromFile();
    }

    // Update is called once per frame
    void Update()
    {
        ManageWordSpawning();
        UpdateCanvas();

        if (LevelIsOver())
        {
            ShowEndOfLevelCanvas();

            if (Input.GetKeyDown(play_again_button))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void UpdateCanvas()
    {
        if (level_time_remaining < 10f)
        {
            time_remaining_text.text = level_time_remaining.ToString("0.0");
        }
        else
        {
            time_remaining_text.text = level_time_remaining.ToString("0");
        }
    }

    void LoadDictionaryFromFile()
    {
        // Load the dictionary from a file
        TextAsset dictionary_file = Resources.Load("dictionary") as TextAsset;
        string dictionary_string = dictionary_file.text;

        // Split the dictionary into words
        dictionary = dictionary_string.Split("\r\n");
        Debug.Log("Dictionary loaded with " + dictionary.Length + " words");
    }

    void ShowEndOfLevelCanvas()
    {
        int words_per_minute = (int)(words_typed / level_time * 60f);
        words_per_minute_text.text = words_per_minute.ToString("0");

        int personal_best_wpm = PlayerPrefs.GetInt("matrix_best_wpm");
        if (words_per_minute > personal_best_wpm)
        {
            PlayerPrefs.SetInt("matrix_best_wpm", words_per_minute);
            personal_best_wpm_text.text = words_per_minute.ToString("0");
        } else
        {
            personal_best_wpm_text.text = personal_best_wpm.ToString("0");
        }

        end_of_level_screen_container.SetActive(true);
    }

    void ManageWordSpawning()
    {
        if (level_time_remaining >= 0)
            level_time_remaining -= Time.deltaTime;

        if (level_time_remaining > 0)
        {
            // Find objects with the Word tag, filtered by != null
            int word_count = GameObject.FindGameObjectsWithTag("Word").Where(w => w != null).Count();
            Debug.Log(word_count + " words on screen");
            if (word_count < max_words_on_screen)
            {
                if (time_until_next_word_spawn <= 0 || word_count < min_words_on_screen)
                {
                    SpawnWord();

                    current_spawn_rate -= spawn_acceleration_rate;
                    time_until_next_word_spawn = current_spawn_rate;
                }
                else
                {
                    time_until_next_word_spawn -= Time.deltaTime;
                }
            }
        }
    }
    
    void SpawnWord()
    {
        string random_word = RandomWordFromDictionary();
        Word new_word = new Word(random_word);

        // Add the word_prefab to the scene
        GameObject new_word_object = Instantiate(word_prefab, transform);
        new_word_object.GetComponent<TextMeshPro>().text = new_word.word;
        new_word_object.GetComponent<TypeWordToDestroy>().word_to_type = new_word.word;
        
        TypeWordToDestroy controller = new_word_object.GetComponent<TypeWordToDestroy>();
        controller.completed_typing_event.AddListener((data) => HandleCompletedWord());
        // controller.error_while_typing_event.AddListener((data) => HandleWordTypo());

        // And position it at a random location along the top edge of the scene
        float x = Random.Range(-7f, 7f);
        float y = 5.3f;

        // Apply a small amount of random downward velocity to randomize word speeds
        float y_velocity = Random.Range(-0.5f, -1f);
        new_word_object.GetComponent<Rigidbody2D>().velocity = new Vector2(0, y_velocity);

        new_word_object.transform.position = new Vector3(x, y, 0);
    }

    public void HandleCompletedWord()
    {
        // If the player types more words after the level is over, we don't want to count them
        if (!LevelIsOver())
        {
            words_typed++;
            words_typed_text.text = words_typed.ToString();
        }
    }

    public void HandleWordTypo()
    {
        // placeholder
    }

    public void HandleMissedWord()
    {
        camera_shake.TriggerShake(0.1f, 0.15f);

        // If words continue to fall after the level is over, we don't want to count them
        if (!LevelIsOver())
        {
            words_missed++;
            words_missed_text.text = words_missed.ToString();
        }
    }

    public void GameOver()
    {
        Debug.Log("Game over!");
    }

    public bool LevelIsOver()
    {
        //int words_on_screen = GameObject.FindGameObjectsWithTag("Word").Length;
        //Debug.Log("level time remaining: " + level_time_remaining + " / words remaining: " + words_on_screen);
        return level_time_remaining <= 0;// && words_on_screen == 0;
    }

    public string RandomWordFromDictionary()
    {
        int random_index = Random.Range(0, dictionary.Length);
        return dictionary[random_index];
    }
}
