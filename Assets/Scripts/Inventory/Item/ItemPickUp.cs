using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory
{
    public class ItemPickUp : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            Item item = col.GetComponent<Item>();

            if (item != null)
            {
                if (item.itemDetails.canPickedUp)
                {
                    InventoryManager.Instance.AddItem(item,true);
                    
                    EventHandler.CallPlaySoundEvent(SoundName.Pickup);
                }
            }
        }
    }
}

