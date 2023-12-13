using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatueManager : MonoBehaviour
{
    [SerializeField] private StatueRulesDB statueData;

    // dictionary to hold the parts
    private Dictionary<StatuePartTypes, StatuePart> statueDict;

    private List<Sprite> spritesToBeCreated;

    [SerializeField] private Image godUIImage;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    [SerializeField] private GameObject leftButton;
    [SerializeField] private GameObject rightButton;

    public StatuePartTypes currentChosenPart;
    private int currentChosenPartIndex;

    // Image dimensions
    // All assets are in this size
    private const int width = 164;
    private const int height = 80;

    // Start is called before the first frame update
    void Start()
    {
        spritesToBeCreated = new List<Sprite>();
        statueDict = new();

        GenerateMonolith();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            GenerateBasicRandomMonolith();
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            UpdateNewPart();
        }
    }

    public void UpdateNewPart(int addition = 1)
    {
        //statueDict[currentChosenPart].Options[0].GetNextSprite(addition);
        statueDict[currentChosenPart].ProcessNextOption(addition);

        UpdatePortrait();
    }

    private void ActivateUIObjects()
    {
        currentChosenPart = statueDict.First().Value.StatuePartType;
        currentChosenPartIndex = 0;

        textMeshProUGUI.text = currentChosenPart.ToString();
        textMeshProUGUI.gameObject.SetActive(true);

        leftButton.SetActive(true);
        rightButton.SetActive(true);
    }

    public void GetNextPartType(int index = 1)
    {
       if (currentChosenPartIndex + index >= statueDict.Count)
       {
            currentChosenPartIndex = 0;
       }
       else if (currentChosenPartIndex + index < 0)
       {
            currentChosenPartIndex = statueDict.Count - 1;
       }
       else
       {
            currentChosenPartIndex += index;
       }

        var test = statueDict.ToList();

        currentChosenPart = test[currentChosenPartIndex].Key;
        textMeshProUGUI.text = currentChosenPart.ToString();
    }

    public void UpdatePortrait()
    {
        spritesToBeCreated.Clear();

        List<StatuePart> childrenToBeRendered = new();
        List<StatuePartTypes> requiredDependencies = new();
        List<StatuePartTypes> incompatibilities = new();


        foreach (StatuePartTypes statuePartType in statueDict.Keys.ToList())
        {
            StatuePart newPart = statueDict[statuePartType];



            childrenToBeRendered.Add(newPart);

            requiredDependencies = newPart.currentOption.AreDependenciesRendered(statueDict);

            if (requiredDependencies.Count > 0)
            {
                foreach (StatuePartTypes part in requiredDependencies)
                {
                    //Debug.Log(part);
                    statueDict.Add(part, statueData.GetPart(part));
                    childrenToBeRendered.Add(statueData.GetPart(part));
                }
            }

            //incompatibilities = newPart.currentOption.AreIncompatibilitiesRendered(statueDict);

            //if (incompatibilities.Count > 0)
            //{
            //    foreach (StatuePartTypes part in incompatibilities)
            //    {
            //        //statueDict.Remove(part);
            //        Debug.Log("HLLO");
            //        childrenToBeRendered.Remove(statueData.GetPart(part));
            //    }
            //}
        }

        foreach (StatuePartTypes statuePartType in statueDict.Keys.ToList())
        {
            StatuePart newPart = statueDict[statuePartType];

            incompatibilities = newPart.currentOption.AreIncompatibilitiesRendered(statueDict);

            if (incompatibilities.Count > 0)
            {
                foreach (StatuePartTypes part in incompatibilities)
                {
                    //statueDict.Remove(part);
                    //Debug.Log("HLLO");
                    childrenToBeRendered.Remove(statueData.GetPart(part));
                }
            }
        }

        // sort the parts by index
        childrenToBeRendered.Sort((part1, part2) => part1.ZIndex.CompareTo(part2.ZIndex));

        foreach (StatuePart statuePartChild in childrenToBeRendered)
        {
            //Debug.Log(statuePartChild.name);
            spritesToBeCreated.Add(statuePartChild.chosenSprite);
            //Debug.Log(statuePartChild.chosenSprite.name);
            //statueDict.Add(statuePartChild, statueData.GetPart(part));
        }

        Texture2D combinedTexture = CreateCombinedTexture();

        UpdateGodPortraitUI(combinedTexture);
    }

    // initially this method is used to generate the monolith before
    // deity choice
    public void GenerateMonolith()
    {
        spritesToBeCreated.Clear();

        Sprite sprite = statueData.GetPart(StatuePartTypes.Monolith).Options[0].Images[0];
        spritesToBeCreated.Add(sprite);

        Texture2D combinedTexture = CreateCombinedTexture();

        UpdateGodPortraitUI(combinedTexture);
    }

    // Upon deity selection, a randomly generated portrait 
    // is generated only with the required parts (face and body and their dependencies)
    public void GenerateBasicRandomMonolith()
    {
        spritesToBeCreated.Clear();
        UpdateDictionary();

        List<StatuePart> childrenToBeRendered = new();
        List<StatuePartTypes> requiredDependencies = new();
        List<StatuePartTypes> incompatibilities = new();


        List<StatuePart> initialRenderParts = statueData.statueParts.FindAll(x =>  x.IsInFirstGeneration);

        foreach(StatuePart statuePart in initialRenderParts)
        {
            //Debug.Log(statuePart.StatuePartType);
            StatuePart newPart = statuePart;

            childrenToBeRendered.Add(statuePart);

            statueDict.Add(statuePart.StatuePartType, statueData.GetPart(statuePart.StatuePartType));

            requiredDependencies = newPart.currentOption.AreDependenciesRendered(statueDict);

            if (requiredDependencies.Count > 0)
            {
                foreach (StatuePartTypes part in requiredDependencies)
                {
                    //Debug.Log(part);
                    statueDict.Add(part, statueData.GetPart(part));
                    childrenToBeRendered.Add(statueData.GetPart(part));
                }
            }

            incompatibilities = newPart.currentOption.AreIncompatibilitiesRendered(statueDict);

            if (incompatibilities.Count > 0)
            {
                foreach (StatuePartTypes part in incompatibilities)
                {
                    //statueDict.Remove(part);
                    Debug.Log("HLLO");
                    childrenToBeRendered.Remove(statueData.GetPart(part));
                }
            }
        }

        // sort the parts by index
        childrenToBeRendered.Sort((part1, part2) => part1.ZIndex.CompareTo(part2.ZIndex));

        foreach (StatuePart statuePartChild in childrenToBeRendered)
        {
            //spritesToBeCreated.Add(statuePartChild.GetRandomImage());
            spritesToBeCreated.Add(statuePartChild.chosenSprite);
            //statueDict.Add(statuePartChild, statueData.GetPart(part));
        }

        Texture2D combinedTexture = CreateCombinedTexture();

        UpdateGodPortraitUI(combinedTexture);

        ActivateUIObjects();
    }

    #region Utilities

    // Called when generated texture
    public void UpdateGodPortraitUI(Texture2D combinedTexture)
    {
        godUIImage.sprite = null;
        godUIImage.sprite = Sprite.Create(combinedTexture, new Rect(0, 0,
            combinedTexture.width,
            combinedTexture.height),
            new Vector2(0.5f, 0.5f));
    }

    private void UpdateDictionary()
    {
        statueDict.Clear();
    }

    private Texture2D CreateCombinedTexture()
    {
        Texture2D combinedTexture = new Texture2D(width, height);
        combinedTexture.filterMode = FilterMode.Point;

        SetBackground(combinedTexture);

        foreach (Sprite sprite in spritesToBeCreated)
        {
            Texture2D spriteTexture = sprite.texture;

            //Rect spriteRect = sprite.textureRect;
            //Color[] pixels = spriteTexture.GetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height);

            // Set background color

            for (int x = 0; x < spriteTexture.width; x++)
            {
                for (int y = 0; y < spriteTexture.height; y++)
                {
                    Color col = spriteTexture.GetPixel(x, y);

                    // Only set the pixel if it isn't transparent
                    if (col.a > 0)
                        combinedTexture.SetPixel(x, y, col);
                }
            }

        }
        combinedTexture.Apply();

        return combinedTexture;
    }

    // Sets a background for the final god portrait ui
    // currently only color #AEDDF4
    private void SetBackground(Texture2D combinedTexture)
    {
        Color bgColor = new Color32(0xAE, 0xDD, 0xF4, 0xFF); // RGBA of #AEDDF4

        for (int x = 0; x < combinedTexture.width; x++)
        {
            for (int y = 0; y < combinedTexture.height; y++)
            {
                combinedTexture.SetPixel(x, y, bgColor);
            }
        }
    }
    #endregion
}