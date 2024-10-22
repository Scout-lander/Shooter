using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public float cooldownTime;
    public GameObject skillEffectPrefab;  // Effect or action related to the skill
    public float manaCost;

    // You can add methods or properties related to skill activation here
}
