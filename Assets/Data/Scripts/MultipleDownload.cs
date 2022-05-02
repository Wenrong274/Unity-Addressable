using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class MultipleDownload : MonoBehaviour
    {
        [SerializeField] private AssetReference asset1;
        [SerializeField] private AssetReference[] assetsObjects;

        private IEnumerator Start()
        {
            enabled = false;
            yield return new WaitUntil(() => AddressablesManager.Isinitialized);
            enabled = true;
        }

        public void OnClick_DownloadAssetString()
        {
            StartCoroutine(UpdateAsset("Cube"));
        }

        public void OnClick_DownloadAsset()
        {
            StartCoroutine(UpdateAsset(asset1));
        }

        public void OnClick_DownloadLabelAsset()
        {
            StartCoroutine(UpdateAsset("default"));
        }

        public void OnClick_DownloadMultipleAssets()
        {
            StartCoroutine(DonwloadMultipleAssets(assetsObjects));
        }

        #region Update Asset String
        IEnumerator UpdateAsset(string asset)
        {
            long updateLabelSize = 0;
            var async = Addressables.GetDownloadSizeAsync(asset);
            yield return async;
            if (async.Status == AsyncOperationStatus.Succeeded)
                updateLabelSize = async.Result;
            Addressables.Release(async);
            if (updateLabelSize == 0)
            {
                Debug.Log($"{asset} last version");
                yield break;
            }

            var downloadAsync = Addressables.DownloadDependenciesAsync(asset, false);
            
            while (!downloadAsync.IsDone)
            {
                float percent = downloadAsync.PercentComplete;
                Debug.Log($"{asset}: {downloadAsync.PercentComplete * 100} %");
                yield return new WaitForEndOfFrame();
            }
            Addressables.Release(downloadAsync);

            Debug.Log($"{asset} UpdateAssets finish"); 
        }
        #endregion

        #region Update Aseet
        IEnumerator UpdateAsset(AssetReference asset)
        {
            long updateLabelSize = 0;
            var async = Addressables.GetDownloadSizeAsync(asset);
            yield return async;
            if (async.Status == AsyncOperationStatus.Succeeded)
                updateLabelSize = async.Result;
            Addressables.Release(async);
            if (updateLabelSize == 0)
            {
                Debug.Log($"{asset} last version");
                yield break;
            }
            var downloadAsync = Addressables.DownloadDependenciesAsync(asset, false);

            while (!downloadAsync.IsDone)
            {
                float percent = downloadAsync.PercentComplete;
                Debug.Log($"{asset}: {downloadAsync.PercentComplete * 100} %");
                yield return new WaitForEndOfFrame();
            }
            Addressables.Release(downloadAsync);

            Debug.Log($"{asset} UpdateAssets finish");
        }
        #endregion

        #region Updata Multiple Assets
        IEnumerator DonwloadMultipleAssets(AssetReference[] assets)
        {
            var assetKeys = assets.Cast<AssetReference>();
            var updateSizeHandle = Addressables.GetDownloadSizeAsync(assetKeys);
            long updateSize = 0;
            if (updateSizeHandle.Status == AsyncOperationStatus.Succeeded)
                updateSize = updateSizeHandle.Result;

            if (updateSize > 0)
            {
                var updateHandle = Addressables.DownloadDependenciesAsync(assetKeys, Addressables.MergeMode.Union);
                while (!updateHandle.IsDone)
                {
                    var st = updateHandle.GetDownloadStatus();
                    float percent = (float)st.DownloadedBytes / (float)st.TotalBytes;
                    Debug.Log(" Multiple Download percent " + percent);
                    yield return new WaitForEndOfFrame();
                }

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                    Debug.Log(" Multiple Download Succeeded");
                else
                    Debug.Log(" Multiple Download Failed");

                Addressables.Release(updateHandle);
            }
            Addressables.Release(updateSizeHandle);
        }
        #endregion
    }
}
