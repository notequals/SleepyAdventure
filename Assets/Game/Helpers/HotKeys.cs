﻿using UnityEngine;

class HotKeys : MonoBehaviour
{
    bool inventoryKeyPress = false;

    public GameObject test;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && Input.GetButton("Fire1"))
        {
            var obj = Instantiate(test, Input.mousePosition, Quaternion.identity);
            Destroy(obj, 5);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Inventory.instance.Save();
            GetComponent<SceneChanger>().OnLoadButtonPressed("IntroScreen");
        }

        if (GameManager.instance.player && !GameManager.instance.player.isDead)
        {
            if (Input.GetKeyDown(KeyCode.I) && !inventoryKeyPress)
            {
                Inventory.instance.OpenInventory();
                inventoryKeyPress = true;
            }

            if (Input.GetKeyUp(KeyCode.I))
            {
                inventoryKeyPress = false;
            }
        }
    }
}
