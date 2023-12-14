using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class StatuePart
{
    [SerializeField] public string name; // just for database visibility purposes

    [SerializeField] private StatuePartTypes statuePartType;
    public StatuePartTypes StatuePartType { get { return statuePartType; } }

    public Sprite chosenSprite = null;
    public StatuePartChild currentOption;
    public int optionsIndex;

    // the order in which the part will be rendered
    [SerializeField] private int zIndex;
    public int ZIndex { get { return zIndex; } }

    public bool shouldRender = false;


    // this is used for a first time generation
    [SerializeField] private bool isInFirstGeneration;
    public bool IsInFirstGeneration { get { return isInFirstGeneration; } }

    [SerializeField] private bool isInMenu;
    public bool IsInMenu { get { return isInMenu; } }


    // Constructor - used by DB upon creation
    public StatuePart(StatuePartTypes statuePartType)
    {
        // constructor
        this.statuePartType = statuePartType;
        name = this.statuePartType.ToString();
    }

    

    [SerializeField] private List<StatuePartChild> options;
    public List<StatuePartChild> Options { get { return options; } }

    // keep the reference for the children parts
    public void AssignParentPartToChild()
    {
        foreach (StatuePartChild child in options)
        {
            child.AssignParent(this);
        }
    }




    [Serializable]
    public class StatuePartChild
    {
        private StatuePart statuePart;
        public StatuePart StatuePartParent { get { return statuePart; } }

        public int optionChosenIndex = 0;

        [SerializeField] private List<Sprite> img;
        
        [SerializeField] private List<StatuePartTypes> dependencies;
        [SerializeField]
        public List<DependencyOptionPair> dependencyOptions;

        [SerializeField] private List<StatuePartTypes> incompatibilities;

        [SerializeField] private bool isMultipleParts;

        public void AssignParent(StatuePart statuePart)
        {
            this.statuePart = statuePart;
        }

        public List<Sprite> Images { get { return img; } }

        public bool isCompatible(StatuePartTypes statuePart)
        {
            return incompatibilities.Contains(statuePart);
        }

        public List<StatuePartTypes> GetDependencies()
        {
            return dependencies;
        }

        public List<StatuePartTypes> AreDependenciesRendered(Dictionary<StatuePartTypes, StatuePart> statueDict)
        {
            List<StatuePartTypes> dependenciesToBeRendered = new();

            foreach(StatuePartTypes dependency in dependencies)
            {
                if (!statueDict.ContainsKey(dependency))
                {
                    dependenciesToBeRendered.Add(dependency);
                }
            }

            return dependenciesToBeRendered;
        }

        public bool AreSpecificDependenciesRendered(Dictionary<StatuePartTypes, StatuePart> statueDict)
        {
            foreach (DependencyOptionPair depOption in dependencyOptions)
            {
                // Try to get the dependent part from the dictionary
                if (statueDict.TryGetValue(depOption.dependencyType, out StatuePart dependentPart))
                {
                    // Check if a specific option is required
                    if (depOption.requiresSpecificOption)
                    {
                        // If a specific option is required, check if the option matches
                        if (dependentPart.optionsIndex != depOption.requiredOptionIndex)
                        {
                            return false; // Specific required option is not matched
                        }
                    }
                    else if (!statueDict[depOption.dependencyType].shouldRender)
                    {
                        return false;
                    }
                    // If no specific option is required, the existence of the part is enough
                    // No action needed in this case
                }
                else
                {
                    return false; // Dependency part itself is missing
                }
            }

            return true; // All dependencies are satisfied
        }

        public bool AreSpecificDependenciesRendered(List<StatuePart> statueDict)
        {
            foreach (var depOption in dependencyOptions)
            {
                foreach(StatuePart sp in statueDict)
                {
                    if (sp.StatuePartType == depOption.dependencyType)
                    {
                        // Check if a specific option is required
                        if (depOption.requiresSpecificOption)
                        {
                            // If a specific option is required, check if the option matches
                            if (sp.optionsIndex != depOption.requiredOptionIndex)
                            {
                                return false; // Specific required option is not matched
                            }
                        }
                        // If no specific option is required, the existence of the part is enough
                        // No action needed in this case
                    }
                    else
                    {
                        return false; // Dependency part itself is missing
                    }
                }
                // Try to get the dependent part from the dictionary
                
            }

            return true; // All dependencies are satisfied
        }

        public List<StatuePartTypes> AreIncompatibilitiesRendered(Dictionary<StatuePartTypes, StatuePart> statueDict)
        {
            List<StatuePartTypes> incompatibilitiesToBeRendered = new();

            foreach (StatuePartTypes incompatibility in incompatibilities)
            {
                if (statueDict.ContainsKey(incompatibility))
                {
                    incompatibilitiesToBeRendered.Add(incompatibility);
                    //Debug.Log("INC");
                }
            }

            return incompatibilitiesToBeRendered;
        }

        public List<StatuePartTypes> AreIncompatibilitiesRendered(List<StatuePart> statueDict)
        {
            List<StatuePartTypes> incompatibilitiesToBeRendered = new();

            foreach (StatuePartTypes incompatibility in incompatibilities)
            {
                foreach(StatuePart sp in statueDict)
                {
                    if(sp.StatuePartType == incompatibility)
                    {
                        incompatibilitiesToBeRendered.Add(incompatibility);
                    }
                }   
            }
            return incompatibilitiesToBeRendered;
        }

        public void GetNextSprite(int addition = 1)
        {
            AssignIndex(optionChosenIndex + addition);

            statuePart.chosenSprite = Images[optionChosenIndex];
        }

        public Sprite GetRandomImage()
        {

            int randomIndex = UnityEngine.Random.Range(0, Images.Count);
            AssignIndex(randomIndex);

            statuePart.chosenSprite = Images[randomIndex];

            return statuePart.chosenSprite;
        }


        public void AssignIndex(int num)
        {
            if(num >= Images.Count)
            {
                optionChosenIndex = 0;
            }
            else if(num < 0)
            {
                optionChosenIndex = Images.Count - 1;
            }
            else
            {
                optionChosenIndex = num;
            }
        }

        public void GetSprite()
        {
            statuePart.chosenSprite = img[0];
        }
    }

    public StatuePartChild GetRandomPart()
    {
        optionsIndex = UnityEngine.Random.Range(0, Options.Count);
        options[optionsIndex].GetSprite();

        chosenSprite = options[optionsIndex].Images[0];
        currentOption = options[optionsIndex];

        return Options[optionsIndex];
    }

    public void ProcessNextOption(int num)
    {
        AssignOptionIndex(optionsIndex + num);

        //options[optionsIndex].GetNextSprite(1);

        chosenSprite = options[optionsIndex].Images[0];
        currentOption = options[optionsIndex];

        options[optionsIndex].GetSprite();
    }

    public void GetSprite()
    {
        currentOption = options[optionsIndex];
    }

    public void AssignOptionIndex(int num)
    {
        if (num >= options.Count)
        {
            optionsIndex = 0;
        }
        else if (num < 0)
        {
            optionsIndex = options.Count - 1;
        }
        else
        {
            optionsIndex = num;
        }
    }

    public void AssignSpecificOption(int num)
    {
        optionsIndex = num;
        currentOption = options[optionsIndex];
        chosenSprite = options[optionsIndex].Images[0];
        Debug.Log($"ITS ME {chosenSprite.name}");
    }

    [Serializable]
    public class DependencyOptionPair
    {
        public bool requiresSpecificOption;
        public StatuePartTypes dependencyType;
        public int requiredOptionIndex;
    }
}


