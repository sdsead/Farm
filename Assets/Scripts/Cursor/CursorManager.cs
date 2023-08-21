using CropPlant;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using map;
using Inventory;

public class CursorManager : MonoBehaviour
{
    public Sprite normal, tool, seed, item;

    private Sprite currentSprite;
    private Image cursorImage;
    private RectTransform cursorCanvas;
    
    //建造图标跟随
    private Image buildImage;

    //鼠标检测
    private Camera mainCamera;
    private Grid currentGrid;

    private Vector3 mouseWorldPos;
    private Vector3Int mouseGridPos;

    private bool cursorEnable;
    private bool cursorPositionValid;

    private ItemDetails currentItem;
    private Transform player => FindObjectOfType<Player>().transform;
    
    
    private void OnDisable()
    {
        EventHandler.ItemSelectedEvent -= OnItemSelectedEvent;
        EventHandler.AfterSceneloadEvent -= OnAfterSceneLoadEvent;
        EventHandler.BeforeSceneUnloadEvent -= OnBeforeSceneUnloadEvent;
    }

    private void OnEnable()
    {
        EventHandler.ItemSelectedEvent += OnItemSelectedEvent;
        EventHandler.AfterSceneloadEvent += OnAfterSceneLoadEvent;
        EventHandler.BeforeSceneUnloadEvent += OnBeforeSceneUnloadEvent;
    }

    


    private void Start()
    {
        cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
        cursorImage = cursorCanvas.GetChild(0).GetComponent<Image>();
        //拿到建造图标
        buildImage = cursorCanvas.GetChild(1).GetComponent<Image>();
        buildImage.gameObject.SetActive(false);
        
        currentSprite = normal;
        
        SetCursorImage(normal);
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if(cursorCanvas == null) return;

        cursorImage.transform.position = Input.mousePosition;

        if (!InteractWithUI() && cursorEnable)
        {
            SetCursorImage(currentSprite);
            CheckCursorValid();
            CheckPlayerInput();
        }
        else
        {
            SetCursorImage(normal);
            buildImage.gameObject.SetActive(false);
        }
    }

    private void CheckPlayerInput()
    {
        if (Input.GetMouseButtonDown(0) && cursorPositionValid)
        {
            EventHandler.CallMouseClickedEvent(mouseWorldPos,currentItem);
        }
    }
    
    
    /// <summary>
    /// 物品选择事件函数
    /// </summary>
    /// <param name="itemDetails"></param>
    /// <param name="isSelected"></param>
    private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
    {
        if (!isSelected)
        {
            currentItem = null;
            cursorEnable = false;
            currentSprite = normal;
            buildImage.gameObject.SetActive(false);
        }
        else//物品选中切换图片
        {
            currentItem = itemDetails;
            
            //WORKFLOW :添加所有类型对应图片
            currentSprite = itemDetails.itemType switch
            {
                ItemType.Commodity => item,
                ItemType.Seed => seed,
                ItemType.Furniture => tool,
                ItemType.BreakTool => tool,
                ItemType.CollectTool =>tool,
                ItemType.ChopTool => tool,
                ItemType.HoeTool => tool,
                ItemType.ReapTool => tool,
                ItemType.WaterTool => tool,
                _ => normal,
            };   
            cursorEnable = true;
            
            //显示建造物品图片
            if (itemDetails.itemType == ItemType.Furniture)
            {
                buildImage.gameObject.SetActive(true);
                buildImage.sprite = itemDetails.itemOnWorldSprite;
                buildImage.SetNativeSize();
            }
        }
    }

    #region 设置鼠标样式
    /// <summary>
    /// 设置鼠标图片
    /// </summary>
    /// <param name="sprite"></param>
    private void SetCursorImage(Sprite sprite)
    {
        cursorImage.sprite = sprite;
        cursorImage.color = new Color(1, 1, 1, 1);
    }

    private void SetCursorValid()
    {
        cursorPositionValid = true;
        cursorImage.color = new Color(1, 1, 1, 1);
        buildImage.color = new Color(1, 1, 1, 0.5f);
    }

    private void SetCursorInvalid()
    {
        cursorPositionValid = false;
        cursorImage.color = new Color(1, 0, 0, 0.5f);
        buildImage.color = new Color(1, 0, 0, 0.5f);
    }
    #endregion
   
    
    private void OnAfterSceneLoadEvent()
    {
        currentGrid = FindObjectOfType<Grid>();
       
    }

    
    private void CheckCursorValid()
    {
        mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
            -mainCamera.transform.position.z));
        
        mouseGridPos = currentGrid.WorldToCell(mouseWorldPos);

        var PlayerGridPos = currentGrid.WorldToCell(player.position);
        
        //建造图片跟随移动
        buildImage.rectTransform.position = Input.mousePosition;
        
        //判断使用范围
        if (Mathf.Abs(mouseGridPos.x - PlayerGridPos.x) > currentItem.itemUseRadius || 
            Mathf.Abs(mouseGridPos.y - PlayerGridPos.y) > currentItem.itemUseRadius)
        {
            SetCursorInvalid();
            return;
        }
        
        TileDetails currentTile = GridMapManager.Instance.GetTileDetailsOnMousePosition(mouseGridPos);
        
        if (currentTile != null)
        {
            CropDetails currentCrop = CropManager.Instance.GetCropDetails(currentTile.seedItemID);
            Crop crop = GridMapManager.Instance.GetCropObject(mouseWorldPos);
            //WORKFLOW:补充所有类型的判断
            switch (currentItem.itemType)
            {
                case ItemType.Commodity:
                    if(currentTile.canDropItem && currentItem.canDropped) 
                        SetCursorValid();
                    else
                        SetCursorInvalid();
                    break;
                case ItemType.HoeTool:
                    if (currentTile.canDig)
                        SetCursorValid();
                    else
                        SetCursorInvalid();
                    break;
                case ItemType.WaterTool:
                    if(currentTile.daysSinceDug>-1 && currentTile.daysSinceWatered==-1)
                        SetCursorValid();
                    else
                    {
                        SetCursorInvalid();
                    }
                    break;
                case ItemType.Seed:
                    if (currentTile.daysSinceDug > -1 && currentTile.seedItemID == -1) SetCursorValid(); else SetCursorInvalid();
                    break;
                case ItemType.BreakTool:
                case ItemType.ChopTool:
                    if (crop != null)
                    {
                        if (crop.CanHarvest && crop.cropDetails.CheckToolAvailable(currentItem.itemID)) SetCursorValid(); else SetCursorInvalid();
                    }
                    else SetCursorInvalid();
                    break;
                case ItemType.CollectTool:
                    if (currentCrop != null)
                    {
                        if (currentCrop.CheckToolAvailable(currentItem.itemID))
                            if (currentTile.growthDays >= currentCrop.TotalGrowthDays) SetCursorValid(); else SetCursorInvalid();
                    }
                    else
                        SetCursorInvalid();
                    break;
                case ItemType.ReapTool:
                    if (GridMapManager.Instance.HaveReapableItemsInRadius(mouseWorldPos, currentItem)) SetCursorValid(); else SetCursorInvalid();
                    break;
                case ItemType.Furniture:
                    buildImage.gameObject.SetActive(true);
                    var bluePrintDetails = InventoryManager.Instance.bluePrintData.GetBluePrintDetails(currentItem.itemID);

                    if (currentTile.canPlaceFurniture && InventoryManager.Instance.CheckStock(currentItem.itemID) 
                                                      && !HaveFurnitureInRadius(bluePrintDetails))
                        SetCursorValid();
                    else
                        SetCursorInvalid();
                    break;
            }
            
        }
        else
        {
            SetCursorInvalid();
        }
    }
    
    private void OnBeforeSceneUnloadEvent()
    {
        cursorEnable = false;
    }
    
    
    private bool HaveFurnitureInRadius(BluePrintDetails bluePrintDetails)
    {
        var buildItem = bluePrintDetails.buildPrefab;
        Vector2 point = mouseWorldPos;
        var size = buildItem.GetComponent<BoxCollider2D>().size;

        var otherColl = Physics2D.OverlapBox(point, size, 0);
        if (otherColl != null)
            return otherColl.GetComponent<Furniture>();
            
        return false;
    }
    
    /// <summary>
    /// 判断是否与UI互动
    /// </summary>
    /// <returns></returns>
    private bool InteractWithUI()
    {
        if(EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
        return false;
    }
}
