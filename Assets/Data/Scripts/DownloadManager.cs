using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class DownloadManager : MonoBehaviour
    {
        IEnumerator Start()
        {
            var InitAddressablesAsync = Addressables.InitializeAsync();
            yield return InitAddressablesAsync;
            yield return UpdateAllAddressablsGroupsCoro();
        }

        public void UpdateAllAddressablsGroups()
        {
            StartCoroutine(UpdateAllAddressablsGroupsCoro());
        }

        public void LabelDownloadAsset(string label)
        {
            StartCoroutine(LabelCheckAndDownloadAsset(label));
        }

        #region Update All Assets
        IEnumerator UpdateAllAddressablsGroupsCoro()
        {
            List<string> CatalogsToUpdate = new List<string>();
            List<IResourceLocator> UpdateAssets = new List<IResourceLocator>();

            yield return CheckCatalog(CatalogsToUpdate);

            if (CatalogsToUpdate.Count == 0)
            {
                Debug.Log("last version");
                yield break;
            }

            yield return UpdateCatalog(CatalogsToUpdate, UpdateAssets);
            yield return DownloadAssets(UpdateAssets);
        }

        IEnumerator CheckCatalog(List<string> UpdateAssets)
        {
            var CheckCatalogAsync = Addressables.CheckForCatalogUpdates(false);
            yield return CheckCatalogAsync;
            if (CheckCatalogAsync.Status == AsyncOperationStatus.Succeeded)
                UpdateAssets = CheckCatalogAsync.Result;
        }

        IEnumerator UpdateCatalog(List<string> updateAssets, List<IResourceLocator> DownladAssets)
        {
            var UpdateCatalogsAsync = Addressables.UpdateCatalogs(updateAssets, false);
            yield return UpdateCatalogsAsync;
            if (UpdateCatalogsAsync.Status == AsyncOperationStatus.Succeeded)
                DownladAssets = UpdateCatalogsAsync.Result;
        }

        IEnumerator DownloadAssets(List<IResourceLocator> DownladAssets)
        {
            foreach (var loc in DownladAssets)
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
            Debug.Log("UpdateAssets finish");
        }
        #endregion

        #region  Update Label Assets 
        IEnumerator LabelCheckAndDownloadAsset(string label)
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
    }
}
