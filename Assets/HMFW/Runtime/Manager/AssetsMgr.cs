using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HM;
using UnityEngine.SceneManagement;

namespace HMFW
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class AssetsMgr : AssetsMgrBase
    {
        public override T Load<T>(string resName)
        {
            return HMAddressableManager.Load<T>(resName);
        }

        public override Scene LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            bool activeOnLoad = true)
        {
            return HMAddressableManager.LoadScene(sceneName, loadSceneMode, activeOnLoad);
        }

        public override UniTask<T> LoadAsync<T>(string url)
        {
            return HM.HMAddressableManager.LoadAsync<T>(url);
        }


        public override UniTask<Scene> LoadSceneAsync(string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activeOnLoad = true)
        {
            return HM.HMAddressableManager.LoadSceneAsync(sceneName, loadSceneMode, activeOnLoad);
        }

        public override UniTask<List<T>> LoadAssetsAsync<T>(List<string> resNames)
        {
            return HM.HMAddressableManager.loadAssetsAsync<T>(resNames);
        }

        public override UniTask<List<T>> LoadAssetsAsyncByGroup<T>(string groupNameOrLabel)
        {
            return HM.HMAddressableManager.LoadAssetsAsyncByGroup<T>(groupNameOrLabel);
        }

        public override bool HasAssets(string resName)
        {
            return HM.HMAddressableManager.HasAssets(resName);
        }

        public override bool ReleaseRes(UnityEngine.Object res)
        {
            return HM.HMAddressableManager.ReleaseRes(res);
        }

        public override void ReleaseRes(string resName)
        {
            HM.HMAddressableManager.ReleaseRes(resName);
        }

        public override void ReleaseResGroup(string groupNameOrLabel)
        {
            HM.HMAddressableManager.ReleaseResGroup(groupNameOrLabel);
        }

        public override UniTask UnloadSceneAsync(string scenePath)
        {
            return HM.HMAddressableManager.UnloadSceneAsync(scenePath);
        }

        public override UniTask UnloadSceneAsync(Scene scene)
        {
            return HM.HMAddressableManager.UnloadSceneAsync(scene);
        }

        public override void ClearLastVerCacheRes()
        {
            HM.HMAddressableManager.ClearLastVerCacheRes();
        }
    }

    public abstract class AssetsMgrBase
    {
        /// <summary>
        /// 加载资源 同步加载,尽量使用异步加载
        /// </summary>
        /// <param name="resName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T Load<T>(string resName) where T : UnityEngine.Object;

        /// <summary>
        /// 同步加载场景
        /// 如果要手动释放,请保留好这个SceneInstance释放的时候需要它
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="loadSceneMode"></param>
        /// <param name="activeOnLoad"></param>
        /// <returns></returns>
        public abstract Scene LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single,
            bool activeOnLoad = true);

        public abstract UniTask<T> LoadAsync<T>(string url) where T : UnityEngine.Object;

        public abstract UniTask<Scene> LoadSceneAsync(string sceneName,
            LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool activeOnLoad = true);

        /// <summary>
        /// 加载多个资源
        /// </summary>
        /// <param name="resNames"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract UniTask<List<T>> LoadAssetsAsync<T>(List<string> resNames) where T : UnityEngine.Object;

        /// <summary>
        /// 通过组或者标签加载多个资源
        /// </summary>
        /// <param name="groupNameOrLabel"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract UniTask<List<T>> LoadAssetsAsyncByGroup<T>(string groupNameOrLabel)
            where T : UnityEngine.Object;

        /// <summary>
        /// 判断是否有资源
        /// </summary>
        /// <param name="resName"></param>
        /// <returns></returns>
        public abstract bool HasAssets(string resName);

        public abstract bool ReleaseRes(UnityEngine.Object res);

        public abstract void ReleaseRes(string resName);

        /// <summary>
        /// 通过组名或者标签释放多个资源
        /// </summary>
        /// <param name="groupNameOrLabel"></param>
        public abstract void ReleaseResGroup(string groupNameOrLabel);

        public abstract UniTask UnloadSceneAsync(string scenePath);

        public abstract UniTask UnloadSceneAsync(Scene scene);

        /// <summary>清理之前版本的缓存资源</summary>
        public abstract void ClearLastVerCacheRes();
    }
}