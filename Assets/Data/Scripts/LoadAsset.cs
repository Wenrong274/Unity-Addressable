using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class LoadAsset : MonoBehaviour
    {
        [SerializeField] private AssetReference asset1;
        AsyncOperationHandle<GameObject> asyns;
        [SerializeField] private GameObject[] gos;

        private IEnumerator Start()
        {
            enabled = false;
            yield return new WaitUntil(() => AddressablesManager.Isinitialized);
            enabled = true;
        }

        private void SetLoadAsset(AsyncOperationHandle<GameObject> async, GameObject[] gos)
        {
            this.asyns = async;
            this.gos = gos;
        }

        public void LoadingAssetAndInstantiateString(string str)
        {
            StartCoroutine(InstantiateAsset(str));
        }

        public void LoadingAssetAndInstantiateLabel(string str)
        {
            StartCoroutine(InstantiateLabel(str));
        }

        public void LoadingAssetAndInstantiateAssetReference()
        {
            StartCoroutine(InstantiateAsset(asset1));
        }

        public void Release_Asyns()
        {
            ReleaseAsset(asyns);
            for (int i = 0; i < gos.Length; i++)
                Destroy(gos[i]);
        }

        public void Release_GameObject()
        {
            for (int i = 0; i < gos.Length; i++)
                ReleaseAsset(gos[i]);

            for (int i = 0; i < gos.Length; i++)
                Destroy(gos[i]);
        }

        public void Release_AssetReference()
        {
            ReleaseAsset(asset1);
            for (int i = 0; i < gos.Length; i++)
                Destroy(gos[i]);
        }

        IEnumerator InstantiateAsset(string asset)
        {
            var async = Addressables.LoadAssetAsync<GameObject>(asset);
            yield return async;
            GameObject go = Instantiate(async.Result);
            SetLoadAsset(async, new GameObject[] { go });
        }

        IEnumerator InstantiateLabel(string label)
        {
            List<GameObject> golist = new List<GameObject>();
            AsyncOperationHandle<IList<GameObject>> async = Addressables.LoadAssetsAsync<GameObject>(label, null);
            yield return async;

            for (int i = 0; i < async.Result.Count; i++)
            {
                golist.Add(Instantiate(async.Result[i]));
            }
            //SetLoadAsset(null, golist.ToArray());
        }

        IEnumerator InstantiateAsset(AssetReference asset)
        {
            AsyncOperationHandle<GameObject> async = asset.LoadAssetAsync<GameObject>();
            yield return async;
            GameObject go = Instantiate(async.Result);
            SetLoadAsset(async, new GameObject[] { go });
        }

        private void ReleaseAsset(AsyncOperationHandle async)
        {
            Addressables.Release(async);
        }

        private void ReleaseAsset(GameObject asset)
        {
            Addressables.Release(asset);
        }

        private void ReleaseAsset(AssetReference asset)
        {
            asset.ReleaseAsset();
        }
    }
}