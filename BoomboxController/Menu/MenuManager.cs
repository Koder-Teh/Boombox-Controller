using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BoomboxController.Menu
{
    public class MenuManager : BoomboxController
    {
        public static void CreateMenu()
        {
            GameObject canvas = GameObject.Find("Canvas");
            GameObject panel = new GameObject("BoomboxMenu");
            panel.AddComponent<UnityEngine.UI.Image>();
            panel.AddComponent<RectTransform>();
            panel.AddComponent<CanvasRenderer>();
            panel.transform.localScale = new Vector3(5, 2, 1);
            panel.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 0.180f);
            panel.transform.SetParent(canvas.transform, false);
            isplayList = false;
        }
    }
}
