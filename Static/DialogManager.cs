using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace HMFW
{
    /// <summary>
    /// 对话框UI管理器
    /// </summary>
    public class DialogManager : MonoSingleton<DialogManager>
    {
        private Text titleText;
        private Button closeBtn;
        private Button sureBtn;
        private Text sureBtnText;
        private Text contentText;
        private GameObject prefebObj;
        private Transform bgPanel;
        private System.Action<bool> sureCallBack;
        private Canvas myCanvas;
        private void Awake()
        {
            this.LoadPrefeb();
        }
        protected void LoadPrefeb()
        {
            Object tipsPrefeb = Resources.Load("DialogManagerPrefeb");
            prefebObj = GameObject.Instantiate(tipsPrefeb, transform) as GameObject;
            prefebObj.name = "DialogManagerPrefeb";
            myCanvas = prefebObj.GetComponent<Canvas>();
            myCanvas.sortingOrder = 999;
            bgPanel = prefebObj.transform.Find("BgPanel");

            titleText = bgPanel.Find("TitleText").GetComponent<Text>();
            closeBtn = bgPanel.Find("CloseBtn").GetComponent<Button>();
            sureBtn = bgPanel.Find("SureBtn").GetComponent<Button>();
            sureBtn.onClick.AddListener(OnSureBtn);
            sureBtnText = sureBtn.transform.Find("Text").GetComponent<Text>();
            contentText = bgPanel.Find("ContentText").GetComponent<Text>();
            bgPanel.gameObject.SetActive(false);
            closeBtn.onClick.AddListener(OnCloseBtn);

        }
        /// <summary>
        /// 显示会话面板
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="closeCallBack">关闭回调,参数传入为是否点击确定按钮</param>
        /// <param name="type">面板类型</param>
        /// <param name="title">标题</param>
        ///  <param name="sureBtnText">按钮文字</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public void ShowDialog(string content,System.Action<bool> closeCallBack,DialogType type, string title="通知",string sureBtnTextArg="确定",int width = 800,int height=500)
        {
            contentText.text = content;
            sureCallBack = closeCallBack;
            titleText.text = title;
            sureBtnText.text = sureBtnTextArg;
            bgPanel.transform.localScale = new Vector3(width / 800f, height / 500f, 1);
            switch (type)
            {
                case DialogType.AgreePanel:closeBtn.gameObject.SetActive(true); break;
                case DialogType.NoCloseBtnNotePanel:closeBtn.gameObject.SetActive(false);break;
            }
            bgPanel.gameObject.SetActive(true);
           
        }
        private void OnSureBtn()
        {
            this.closeDialog(true);
        }
        private void OnCloseBtn()
        {
            this.closeDialog(false);
        }
        private void closeDialog(bool bSureBtn)
        {
            bgPanel.gameObject.SetActive(false);
            if (sureCallBack != null)
            {
                sureCallBack.Invoke(bSureBtn);
            }
           
            
        }
    }

    /// <summary>
    /// 对话框类型
    /// </summary>
    public enum DialogType
    {
        /// <summary>
        /// 通知板子-无关闭按钮
        /// </summary>
        NoCloseBtnNotePanel,
        /// <summary>
        /// 同意/允许板子-有关闭按钮-即拒绝
        /// </summary>
        AgreePanel,
    }
}