using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public float cooldownTime;
    public GameObject skillEffectPrefab;
    public float manaCost;

    private float lastActivatedTime = -Mathf.Infinity; // Tracks when the skill was last used

    // Checks if the skill is ready based on cooldown
    public bool IsReady()
    {
        return Time.time >= lastActivatedTime + cooldownTime;
    }

    // Activates the skill and starts the cooldown
    public void Activate()
    {
        lastActivatedTime = Time.time;
        // Add other activation logic here if needed (e.g., mana cost reduction, skill effects)
    }
}
