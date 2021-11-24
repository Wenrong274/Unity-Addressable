using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class DownloadManager : MonoBehaviour
    {
        [SerializeField] private AssetReference[] assetsObjects;

        IEnumerator Start()
        {
            var InitAddressablesAsync = Addressables.InitializeAsync();
            yield return InitAddressablesAsync;
            yield return UpdateCatalogCoro();
        }

        public void UpdateAllAddressablsGroups()
        {
            StartCoroutine(UpdateAllGroupsCoro());
        }

        public void LabelDownloadAsset(string label)
        {
            StartCoroutine(UpdateLabelAsset(label));
        }

        public void MultipleDonwloadAssets()
        {
            StartCoroutine(MultipleDonwloadAssets(assetsObjects));
        }

        public void ClearAllAsset()
        {
            StartCoroutine(ClearAllAssetCoro());
        }

        public void ClearLabelAsset(string label)
        {
            StartCoroutine(ClearAssetCoro(label));
        }

        public void ClearMultipleAssets()
        {
            StartCoroutine(ClearAssetCoro(assetsObjects));
        }

        #region Update Catalog
        IEnumerator UpdateCatalogCoro()
        {
            List<string> catalogsToUpdate = new List<string>();
            var checkCatalogHandle = Addressables.CheckForCatalogUpdates();
            yield return checkCatalogHandle;

            if (checkCatalogHandle.Status == AsyncOperationStatus.Succeeded)
                catalogsToUpdate = checkCatalogHandle.Result;

            if (catalogsToUpdate.Count > 0)
            {
                var updateCatalogHandle = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                yield return updateCatalogHandle;
            }
        }
        #endregion

        #region Update All Asset
        IEnumerator UpdateAllGroupsCoro()
        {
            foreach (var loc in Addressables.ResourceLocators)
            {
                foreach (var key in loc.Keys)
                {
                    var sizeAsync = Addressables.GetDownloadSizeAsync(key);
                    yield return sizeAsync;
                    long totalDownloadSize = sizeAsync.Result;

                    if (sizeAsync.Result > 0)
                    {
                        var downloadAsync = Addressables.DownloadDependenciesAsync(key);
                        while (!downloadAsync.IsDone)
                        {
                            float percent = downloadAsync.PercentComplete;
                            Debug.Log($"{key} = percent {(int)(totalDownloadSize * percent)}/{totalDownloadSize}");
                            yield return new WaitForEndOfFrame();
                        }
                        Addressables.Release(downloadAsync);
                    }
                    Addressables.Release(sizeAsync);
                }
            }
            Debug.Log("Download All Asset finish");
        }
        #endregion

        #region Update label Asset
        IEnumerator UpdateLabelAsset(string label)
        {
            long updateLabelSize = 0;
            var async = Addressables.GetDownloadSizeAsync(label);
            yield return async;
            if (async.Status == AsyncOperationStatus.Succeeded)
                updateLabelSize = async.Result;
            Addressables.Release(async);
            if (updateLabelSize == 0)
            {
                Debug.Log($"{label} last version");
                yield break;
            }
            yield return DownloadLabelAsset(label);
        }

        IEnumerator DownloadLabelAsset(string label)
        {
            var downloadAsync = Addressables.DownloadDependenciesAsync(label, false);

            while (!downloadAsync.IsDone)
            {
                float percent = downloadAsync.PercentComplete;
                Debug.Log($"{label}: {downloadAsync.PercentComplete * 100} %");
                yield return new WaitForEndOfFrame();
            }
            Addressables.Release(downloadAsync);

            Debug.Log($"{label} UpdateAssets finish");
        }
        #endregion

        #region Updata Multiple Assets

        IEnumerator MultipleDonwloadAssets(AssetReference[] assets)
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

        #region Clear Asset
        IEnumerator ClearAllAssetCoro()
        {
            foreach (var locats in Addressables.ResourceLocators)
            {
                var async = Addressables.ClearDependencyCacheAsync(locats.Keys, false);
                yield return async;
                Addressables.Release(async);
            }
            Caching.ClearCache();
        }

        IEnumerator ClearAssetCoro(string label)
        {
            var async = Addressables.LoadResourceLocationsAsync(label);
            yield return async;
            var locats = async.Result;
            foreach (var locat in locats)
                Addressables.ClearDependencyCacheAsync(locat.PrimaryKey);
        }

        IEnumerator ClearAssetCoro(AssetReference[] assets)
        {
            var async = Addressables.LoadResourceLocationsAsync(assets);
            yield return async;
            var locats = async.Result;
            foreach (var locat in locats)
                Addressables.ClearDependencyCacheAsync(locat.PrimaryKey);
        }
        #endregion
    }
}
