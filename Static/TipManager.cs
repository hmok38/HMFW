using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// hmok的游戏框架
/// </summary>
namespace HMFW
{
    /// <summary>
    /// 自消失提示信息(安卓那样的) editdate: 20201123
    /// 本单例不需要手动挂载物体,只需要制作好预制体就可以动态加载
    /// </summary>
    public class TipManager : MonoSingleton<TipManager>
    {

        private GameObject prefebObj;
        private Canvas myCanvas;

        /// <summary>
        /// 消息队列
        /// </summary>
        private List<string> tipsCaches = new List<string>();
        private List<RectTransform> waiteShowPanels = new List<RectTransform>();
        private List<RectTransform> alreadyShowPanels = new List<RectTransform>();

        void Awake()
        {
            this.LoadPrefeb();
            myCanvas = prefebObj.GetComponent<Canvas>();
            myCanvas.sortingOrder = 1000;
            InitShowPanel();
        }


        protected void LoadPrefeb()
        {
            Object tipsPrefeb = Resources.Load("TipManagerPrefeb");
            prefebObj = GameObject.Instantiate(tipsPrefeb, transform) as GameObject;
            prefebObj.name = "TipManagerPrefeb";
        }

        private void InitShowPanel()
        {
            var Bg0 = prefebObj.transform.Find("ShowPanel") as RectTransform;
            waiteShowPanels.Add(Bg0);
            for (int i = 0; i < 10; i++)
            {
                RectTransform transform = GameObject.Instantiate(Bg0, prefebObj.transform);
                waiteShowPanels.Add(transform);
            }


        }
        public void ShowTips(string tipstr)
        {
            if (!tipsCaches.Contains(tipstr))
            {
                tipsCaches.Add(tipstr);
            }

        }

        // Update is called once per frame
        void Update()
        {
            ShowAMessage();
            AnimtionUpdate();
        }
        private void ShowAMessage()
        {
            if (waiteShowPanels.Count > 0 && this.tipsCaches.Count > 0)
            {
                string info = this.tipsCaches[0];
                this.tipsCaches.RemoveAt(0);
                RectTransform pannel = waiteShowPanels[0];
                waiteShowPanels.RemoveAt(0);
                SetShowPanels(info, pannel);
                alreadyShowPanels.Add(pannel);

            }

        }
        private void AnimtionUpdate()
        {

            for (int i = alreadyShowPanels.Count - 1; i >= 0; i--)
            {
                RectTransform panel = alreadyShowPanels[i];
                Animator animator = panel.GetChild(0).GetComponent<Animator>();
                var state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.normalizedTime > 1.0f)
                {
                    alreadyShowPanels.RemoveAt(i);
                    waiteShowPanels.Add(panel);
                    panel.gameObject.SetActive(false);
                }
            }

        }
        private void SetShowPanels(string infom, RectTransform panel)
        {
            panel.anchoredPosition = Vector3.zero;
            panel.gameObject.SetActive(true);
            Text InfoText = panel.GetComponentInChildren<Text>();
            InfoText.text = infom;
            RectTransform infoTextTran = (InfoText.transform as RectTransform);
          LayoutRebuilder.ForceRebuildLayoutImmediate(infoTextTran);

            RectTransform bgImg = panel.GetChild(0) as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(bgImg);
           
            Animator animator = panel.GetChild(0).GetComponent<Animator>();
            animator.Play("TipsAnim", 0);
            for (int i = 0; i < alreadyShowPanels.Count; i++)
            {
                RectTransform alreadyShowPanel = alreadyShowPanels[i];
                Vector3 pos = alreadyShowPanel.anchoredPosition;
                pos.y += (bgImg.sizeDelta.y + 20);
                alreadyShowPanel.anchoredPosition = pos;
            }

        }

    }
}

