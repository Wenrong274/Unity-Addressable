using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityAASample
{
    public class DownloadManager : MonoBehaviour
    {
        async void Start()
        {
            /// 手動初始化 Addressable Asset System
            var AAInit = Addressables.InitializeAsync();
            await AAInit.Task;
            await UpdateCatalog();
        }

        async Task UpdateCatalog()
        {
            var checkList = await checkCatalog();
            var catalog = await DownloadCatalog(checkList);
            await DownloadBundleList(catalog);

        }

        async Task<List<string>> checkCatalog()
        {
            var async = Addressables.CheckForCatalogUpdates(false);
            await async.Task;
            if (async.Status != AsyncOperationStatus.Succeeded)
                Debug.Log(async.OperationException.Message);
            return async.Result;
        }

        async Task<List<IResourceLocator>> DownloadCatalog(List<string> list)
        {
            var async = Addressables.UpdateCatalogs(list);
            await async.Task;
            if (async.Status != AsyncOperationStatus.Succeeded)
                Debug.Log(async.OperationException.Message);
            return async.Result;
        }

        async Task DownloadBundleList(List<IResourceLocator> locator)
        {
            foreach (var loc in locator)
                foreach (var key in loc.Keys)
                    await DownloadBundle(key);

        }

        async Task DownloadBundle(object key)
        {
            var sizeAsync = Addressables.GetDownloadSizeAsync(key);
            await sizeAsync.Task;

            /// size > 0 == 需要下載
            if (sizeAsync.Result > 0)
            {
                var downloadAsync = Addressables.DownloadDependenciesAsync(key);
                await downloadAsync.Task;
                Addressables.Release(downloadAsync);
            }
            Addressables.Release(sizeAsync);
        }
    }
}
