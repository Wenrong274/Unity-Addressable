using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class AddressablesManager : MonoBehaviour
    {
        public static bool Isinitialized = false;

        IEnumerator Start()
        {
            Isinitialized = false;
            var InitAddressablesAsync = Addressables.InitializeAsync();
            yield return InitAddressablesAsync;
            yield return UpdateCatalogCoro();
            Isinitialized = true;
        }

        public void UpdateAllAddressablsGroups()
        {
            StartCoroutine(UpdateAllGroupsCoro());
        }

        public void ClearAllAsset()
        {
            StartCoroutine(ClearAllAssetCoro());
        }

        public void ClearLabelAsset(string label)
        {
            StartCoroutine(ClearAssetCoro(label));
        }

        #region Update Catalog
        IEnumerator UpdateCatalogCoro()
        {
            List<string> catalogsToUpdate = new List<string>();
            var checkCatalogHandle = Addressables.CheckForCatalogUpdates(false);
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
