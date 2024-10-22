using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerClass", menuName = "Player Class")]
public class PlayerClass : ScriptableObject
{
    public string className;
    
    // Movement Stats
    public float walkSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float jumpHeight;
    public float slideSpeed;
    public float staminaUseRate;
    public float staminaRegenRate;

    public Skill[] skills; 
}
