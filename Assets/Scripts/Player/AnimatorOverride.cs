using System.Collections;
using System.Collections.Generic;
using Inventory;
using UnityEngine;

public class AnimatorOverride : MonoBehaviour
{
    private Animator[] animators;
    public SpriteRenderer holdItem;

    public List<AnimatorType> animatorTypes;
    public Dictionary<string, Animator> animatorNameDict = new Dictionary<string, Animator>();
    private void Awake()
    {
        animators = GetComponentsInChildren<Animator>();
        foreach (var anim in animators)
        {
            animatorNameDict.Add(anim.name, anim);
        }
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition += OnHarvestPlayerPosition;
    }


    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.HarvestAtPlayerPosition -= OnHarvestPlayerPosition;
    }

    private void OnHarvestPlayerPosition(int ID)
    {
        Sprite itemSprite = InventoryManager.Instance.GetItemDetails(ID).itemOnWorldSprite;
        if (holdItem.enabled==false)
        {
            StartCoroutine(ShowItem(itemSprite));
        }
        

    }

    private IEnumerator ShowItem(Sprite itemSprite)
    {
        holdItem.sprite = itemSprite;
        holdItem.enabled = true;
        yield return new WaitForSeconds(1f);
        
        holdItem.enabled = false;
    }

    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSeleted)
    {
        PartType currentType = itemDetails.itemType switch
        {
            ItemType.Commodity => PartType.Carry,
            ItemType.Seed => PartType.Carry,
            ItemType.HoeTool => PartType.Hoe,
            ItemType.WaterTool => PartType.Water,
            ItemType.ChopTool => PartType.Chop,
            ItemType.CollectTool => PartType.Collect,
            ItemType.ReapTool => PartType.Reap,
            ItemType.BreakTool => PartType.Break,
            ItemType.Furniture => PartType.Carry,
            _ => PartType.None
        };

        if (!isSeleted)
        {
            currentType = PartType.None;
            holdItem.enabled = false;
        }
        else
        {
            if (currentType == PartType.Carry)
            {
                holdItem.sprite = itemDetails.itemOnWorldSprite;
                holdItem.enabled = true;
            }
            else
            {
                holdItem.enabled = false;
            }
        }
        SwitchAnimator(currentType);
    }


    private void SwitchAnimator(PartType partType)
    {
        foreach (var item in animatorTypes)
        {
            if (item.partType == partType)
            {
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.animatorOverrideController;
            }
            else if (item.partType == PartType.None)
            {
                animatorNameDict[item.partName.ToString()].runtimeAnimatorController = item.animatorOverrideController;
            }
        }
    }
    
    
    
    private void OnBeforeSceneUnloadEvent()
    {
        holdItem.enabled = false;
        SwitchAnimator(PartType.None);
    }
}
