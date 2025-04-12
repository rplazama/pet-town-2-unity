// CollectableNPC.cs - Para animales coleccionables
using UnityEngine;
using System.Collections.Generic;
using System;

public class VolunteerChief : BaseNPC
{
    [Header("Información del Animal")]
    [SerializeField] private string animalType;
    [SerializeField, TextArea(2, 4)] private string animalDescription;
    [SerializeField] private Sprite animalIcon;
    
    [Header("Diálogos")]
    [SerializeField, TextArea(3, 5)] private List<string> dialogueLines = new List<string>();
    [SerializeField, TextArea(3, 5)] private string collectionPrompt = "¿Quieres recoger a este animal?";
    [SerializeField, TextArea(3, 5)] private string postCollectionDialogue = "¡Has recogido al animal!";
    
    private bool hasBeenCollected = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Verificar si este animal ya ha sido coleccionado
        if (gameManager != null && gameManager.IsAnimalCollected(animalType))
        {
            hasBeenCollected = true;
            gameObject.SetActive(false); // Desactivar si ya fue coleccionado
        }
    }
    
    public override void OnInteract()
    {
        if (hasBeenCollected) return;
        
        base.OnInteract();
        
        if (dialogueLines.Count > 0 && dialogueManager != null)
        {
            // Mostrar diálogo inicial del animal
            dialogueManager.ShowDialogue(npcName, dialogueLines[0], dialogueAnchor.position, () => {
                // Reproducir diálogos adicionales si hay
                ShowRemainingDialogues(1, () => {
                    // Al finalizar todos los diálogos, mostrar prompt de colección
                    dialogueManager.ShowCollectionPrompt(
                        collectionPrompt, 
                        OnCollectionResponse,
                        dialogueAnchor.position
                    );
                });
            });
        }
    }
    
    private void ShowRemainingDialogues(int startIndex, Action onComplete)
    {
        if (startIndex >= dialogueLines.Count)
        {
            onComplete?.Invoke();
            return;
        }
        
        // Mostrar el siguiente diálogo y recursivamente continuar con el resto
        dialogueManager.ShowDialogue(npcName, dialogueLines[startIndex], dialogueAnchor.position, () => {
            ShowRemainingDialogues(startIndex + 1, onComplete);
        });
    }
    
    private void OnCollectionResponse(bool accepted)
    {
        if (accepted)
        {
            CollectAnimal();
        }
    }
    
    private void CollectAnimal()
    {
        if (hasBeenCollected) return;
        
        hasBeenCollected = true;
        
        // Registrar en el GameManager
        if (gameManager != null)
        {
            gameManager.CollectAnimal(animalType, animalDescription, animalIcon);
        }
        
        // Mostrar diálogo final
        if (dialogueManager != null)
        {
            dialogueManager.ShowDialogue(npcName, postCollectionDialogue, dialogueAnchor.position, () => {
                // Desactivar el objeto cuando termine el diálogo
                gameObject.SetActive(false);
            });
        }
        else
        {
            // Si no hay DialogueManager, desactivar inmediatamente
            gameObject.SetActive(false);
        }
    }
}
