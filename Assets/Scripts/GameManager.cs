// GameManager.cs - Gestiona el estado del juego y los coleccionables
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AnimalData
{
    public string type;
    public string description;
    public Sprite icon;
}

public class GameManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int totalAnimalsRequired = 9;
    [SerializeField] private string saveDataKey = "PetTownSaveData";
    
    [Header("Referencias UI")]
    [SerializeField] private CounterUI counterUI;
    //[SerializeField] private InventoryUI inventoryUI;
    
    // Datos de juego (guardados)
    private List<AnimalData> collectedAnimals = new List<AnimalData>();
    private bool hasMetVolunteerChief = false;
    private bool gameCompleted = false;
    
    // Singleton
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Configuración Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        UpdateCounterUI();
    }
    
    public void CollectAnimal(string type, string description, Sprite icon)
    {
        // Verificar si ya está coleccionado
        if (IsAnimalCollected(type)) return;
        
        // Crear y añadir los datos
        AnimalData newAnimal = new AnimalData
        {
            type = type,
            description = description,
            icon = icon
        };
        
        collectedAnimals.Add(newAnimal);
        
        // Actualizar UI
        UpdateCounterUI();
        //if (inventoryUI != null)
            //inventoryUI.UpdateInventory(collectedAnimals);
            
        // Guardar datos
        SaveGameData();
    }
    
    public bool IsAnimalCollected(string type)
    {
        return collectedAnimals.Any(a => a.type == type);
    }
    
    public int GetCollectedAnimalsCount()
    {
        return collectedAnimals.Count;
    }
    
    public bool HasMetVolunteerChief()
    {
        return hasMetVolunteerChief;
    }
    
    public void SetHasMetVolunteerChief(bool value)
    {
        hasMetVolunteerChief = value;
        SaveGameData();
    }
    
    public bool IsGameCompleted()
    {
        return gameCompleted;
    }
    
    public void CompleteGame()
    {
        gameCompleted = true;
        SaveGameData();
    }
    
    private void UpdateCounterUI()
    {
        if (counterUI != null)
            counterUI.UpdateCounter(collectedAnimals.Count, totalAnimalsRequired);
    }
    
    public void ResetGame()
    {
        collectedAnimals.Clear();
        hasMetVolunteerChief = false;
        gameCompleted = false;
        
        // Actualizar UI
        UpdateCounterUI();
        
        // Limpiar datos guardados
        PlayerPrefs.DeleteKey(saveDataKey);
        PlayerPrefs.Save();
        
        // Opcional: Recargar la escena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Sistema simple de guardado con PlayerPrefs
    private void SaveGameData()
    {
        // Crear objeto de datos
        SaveData data = new SaveData
        {
            collectedAnimalTypes = collectedAnimals.Select(a => a.type).ToArray(),
            hasMetVolunteerChief = hasMetVolunteerChief,
            gameCompleted = gameCompleted
        };
        
        // Convertir a JSON y guardar
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveDataKey, json);
        PlayerPrefs.Save();
    }
    
    private void LoadGameData()
    {
        if (PlayerPrefs.HasKey(saveDataKey))
        {
            string json = PlayerPrefs.GetString(saveDataKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            // Cargar tipos de animales
            foreach (string type in data.collectedAnimalTypes)
            {
                // Crear un objeto básico para cada tipo
                AnimalData animal = new AnimalData
                {
                    type = type,
                    description = "Animal recuperado de datos guardados",
                    icon = null // Se cargará más tarde si es necesario
                };
                
                collectedAnimals.Add(animal);
            }
            
            // Cargar otros datos
            hasMetVolunteerChief = data.hasMetVolunteerChief;
            gameCompleted = data.gameCompleted;
        }
    }
    
    [System.Serializable]
    private class SaveData
    {
        public string[] collectedAnimalTypes;
        public bool hasMetVolunteerChief;
        public bool gameCompleted;
    }
}