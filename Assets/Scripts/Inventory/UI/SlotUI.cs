using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Inventory
{
        
    public class SlotUI : MonoBehaviour,IPointerClickHandler, IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        [Header("组件")] 
        
        [SerializeField] private Image slotImage;
        [SerializeField] private TextMeshProUGUI amountText;
        public Image slotHighlight;
        [SerializeField] private Button button;
        
        public SlotType slotType;
        public bool isSelected;

        public ItemDetails itemDetails;
        public int itemAmount;
        public int slotIndex;
        
        public InventoryLocation Location
        {
            get
            {
                return slotType switch
                {
                    SlotType.Bag => InventoryLocation.Player,
                    SlotType.Box => InventoryLocation.Box,
                    _ => InventoryLocation.Player
                };
            }
        }

        public InventoryUI inventoryUI => GetComponentInParent<InventoryUI>();
        
        private void Start()
        {
            isSelected = false;
            if (itemDetails == null)
            {
                UpdateEmptySlot();
            }
        }

        /// <summary>
        /// 更新格子和信息
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public void UpdateSlot(ItemDetails item, int amount)
        {
            itemDetails = item;
            slotImage.sprite = item.itemIcon;
            itemAmount = amount;
            amountText.text = amount.ToString();
            button.interactable = true;
            slotImage.enabled = true;
        }


        /// <summary>
        /// 将格子置空
        /// </summary>
        public void UpdateEmptySlot()
        {
            if (isSelected)
            {
                isSelected = false;
                inventoryUI.UpdateSlotHighlight(-1);
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }

            itemDetails = null;
            slotImage.enabled = false;
            amountText.text = String.Empty;
            button.interactable = false;

        }

        
        public void OnPointerClick(PointerEventData eventData)
        {
            if(itemDetails == null) return;
            isSelected = !isSelected;
            
            inventoryUI.UpdateSlotHighlight(slotIndex);

            if (slotType == SlotType.Bag)
            {
                EventHandler.CallItemSelectedEvent(itemDetails, isSelected);
            }
        }

        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount == 0) return;
            inventoryUI.dragImage.enabled = true;
            inventoryUI.dragImage.sprite = slotImage.sprite;
            inventoryUI.dragImage.SetNativeSize();

            isSelected = true;
            inventoryUI.UpdateSlotHighlight(slotIndex);
        }

        
        public void OnDrag(PointerEventData eventData)
        {
            inventoryUI.dragImage.transform.position = Input.mousePosition;

        }

        
        public void OnEndDrag(PointerEventData eventData)
        {
            inventoryUI.dragImage.enabled = false;

            if (eventData.pointerCurrentRaycast.gameObject != null)
            {
                if(eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>() == null)
                    return;
                var targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<SlotUI>();
                int tagetIndex = targetSlot.slotIndex;

                if (targetSlot.slotType == SlotType.Bag && slotType == SlotType.Bag)
                {
                    InventoryManager.Instance.SwapItem(slotIndex,tagetIndex);
                }
                else if (slotType == SlotType.Shop && targetSlot.slotType == SlotType.Bag)  //买
                {
                    EventHandler.CallShowTradeUI(itemDetails, false);
                }
                else if (slotType == SlotType.Bag && targetSlot.slotType == SlotType.Shop)  //卖
                {
                    EventHandler.CallShowTradeUI(itemDetails, true);
                }
                else if (slotType != SlotType.Shop && targetSlot.slotType != SlotType.Shop && slotType != targetSlot.slotType)
                {
                    //跨背包数据交换物品
                    InventoryManager.Instance.SwapItem(Location, slotIndex, targetSlot.Location, targetSlot.slotIndex);
                }
                inventoryUI.UpdateSlotHighlight(-1);
            }
            
            
        }
    }
}
