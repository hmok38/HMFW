using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUtil
{
    /// <summary>
    /// 设置Text内容,会判断text是否存在
    /// </summary>
    /// <param name="textComp"></param>
    /// <param name="str"></param>
   public static void TextSet(UnityEngine.UI.Text textComp,object str)
    {
        if (textComp)
        {
            textComp.text = str.ToString();
        }
    }
}
