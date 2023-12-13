using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StatueData", menuName = "Data/CreateStatueData", order = 1)]
public class StatueRulesDB : ScriptableObject
{
    public List<StatuePart> statueParts = new List<StatuePart>();

    public StatuePart GetPart(StatuePartTypes partType)
    {
        return statueParts.Find(x => x.StatuePartType == partType);
    }

    // BEWARE!: Only used upon initilization
    public void InitializeStatueGroups()
    {
        statueParts.Clear();
        foreach (StatuePartTypes type in Enum.GetValues(typeof(StatuePartTypes)))
        {
            StatuePart newStatueGroup = new StatuePart(type);
            statueParts.Add(newStatueGroup);
        }
    }

    public void UpdateImagesZIndex()
    {
        foreach(StatuePart statuePart in statueParts)
        {
            statuePart.AssignParentPartToChild();
        }
    }
}
