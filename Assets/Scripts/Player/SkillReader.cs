using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Data
{
    public Data() { }
    public string skillName = "";
    public float cooldownTime = 0;
    public float castTime = 0;
    public Vector3 ornamentLocalScale = Vector3.zero;
    public Vector3 ornamentLocalPosition = new Vector3(0.5f, 1.5f, 0f);
    public Vector3 usingPosition = Vector3.zero;
    public Vector3 usingScale = Vector3.zero;
    //Jump
    public bool jumpHigh = false;
    public float jumperClip = 1;
    //dance invincible
    public bool invincible = false;
    public float dancingAngle = 0;
}
public class SkillReader : Data
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public string[] GetSkillNames()
    {
        return new string[] { "JumpHigh", "DanceInvincible", "Shoot", "Magnetic", "Hook", "Tack" };
    }
    public Data GetSkill(string skillName)
    {
        string[] skillNames = GetSkillNames();
        switch (skillName) {
            case "JumpHigh":return GetJumpHighData(skillName);
            case "DanceInvincible": return GetDanceInvincibleData(skillName);
            case "Shoot": return GetShootData(skillName);
            case "Magnetic": return GetMagneticData(skillName);
            case "Hook": return GetHookData(skillName);
            case "Tack": return GetTackData(skillName);
        }
        return GetJumpHighData("DanceInvincible");
        //return GetSkill(skillNames[(int) Random.Range(0, skillNames.Length)]);
    }

    private Data GetJumpHighData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 0;
        skillData.usingPosition = new Vector3(0, 0.3f, 0.8f);
        skillData.usingScale = new Vector3(0.3f, 0.3f, 0.3f);
        return skillData;
    }
    private Data GetDanceInvincibleData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 0;
        return skillData;
    }
    private Data GetShootData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 10;
        return skillData;
    }
    private Data GetMagneticData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 60;
        return skillData;
    }
    private Data GetHookData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 30;
        return skillData;
    }
    private Data GetTackData(string skillName)
    {
        Data skillData = new Data();
        skillData.skillName = skillName;
        skillData.cooldownTime = 10;
        return skillData;
    }
}
