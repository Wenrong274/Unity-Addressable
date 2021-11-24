# Unity-Addressable

主要是實作 Addressable hotfix 的寫法。

基本 Unity-Addressable 安裝及 Remote 設定可以參考這篇 [Unity筆記 Addressable Asset System][tutorial_1]。

詳細解說可以參考

Unity Addressables 深入浅出[(一)][tutorial_2_1]、[(二)][tutorial_2_2]、[(三)][tutorial_2_3]

[Unity Addressable 獨立資源包][tutorial_3]

## 實例

### Initialization

Initial Addressable System 是`必要`的假如不初始化會造成一些使用上的問題。

在 Start 上可直接初始化 Addressable System

```csharp
IEnumerator Start()
{
    var InitAddressablesAsync = Addressables.InitializeAsync();
    yield return InitAddressablesAsync;
}
```

### Update Catalog

Catalog 是所有檔案的紀錄檔(log)，不更新 Catalog 也是能下載 Asset，可是會造成無法 hotfix，所以需要再下載前更一次 Catalog。

#### Catalog Path

依 Window 為例，Catalog 更新後會放自動放置在

`C:\Users\[PC Name]\AppData\LocalLow\[Company Name]\[Product Name]\com.unity.addressables`

| 名稱           | 解釋                    |
| :------------- | :---------------------- |
| [PC Name]      | 系統使用者名稱          |
| [Company Name] | Unity 專案 Company Name |
| [Product Name] | Unity 專案 Product Name |

![asset_1]

```csharp
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
```

### Update Asset

下載好的 Asset，在測試時不清除是會造成無法測試下載流程，可是可以手動清除下載 Asset。

#### Asset Path

依 Window 為例，下載好的 Asset 更新後會放自動放置在

`C:\Users\[PC Name]\AppData\LocalLow\Unity\[Company Name]_[Product Name]`

| 名稱           | 解釋                    |
| :------------- | :---------------------- |
| [PC Name]      | 系統使用者名稱          |
| [Company Name] | Unity 專案 Company Name |
| [Product Name] | Unity 專案 Product Name |

![asset_2]

#### Update All Asset

這個方式是使用 AA 系統紀錄 Catalog 取得出來的位置（Locator），在使用 `GetDownloadSizeAsync` 來確認檔案有無更新，來達成更新所有檔案。

```csharp
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
}
```

#### Update label Asset

這個方式是使用 Label 下載特定資源。

```csharp
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
```

### Clear Asset

刪除 [Asset Path](#asset-path) 路徑下載的檔案。

#### Clear All Asset

`Caching.ClearCache()` 是能夠完整的清除下載的所有檔案，但是不能單獨使用，還要搭配 `Addressables.ClearDependencyCacheAsync` 才能清除 Catalog 紀錄的下載資訊。

```csharp
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
```

#### Clear Label Asset

```csharp
IEnumerator ClearAssetCoro(string label)
{
    var async = Addressables.LoadResourceLocationsAsync(label);
    yield return async;
    var locats = async.Result;
    foreach (var locat in locats)
        Addressables.ClearDependencyCacheAsync(locat.PrimaryKey);
}
```

## [Github][github]

______________________________________________________________________

[github]:https://github.com/hybrid274/Unity-Addressable

[tutorial_1]:https://medium.com/@nai.hsue/unity%E7%AD%86%E8%A8%98-addressable-asset-system-dbebf981143b

[tutorial_2_1]:https://blog.csdn.net/qq_14903317/article/details/108509938

[tutorial_2_2]:https://blog.csdn.net/qq_14903317/article/details/108529590

[tutorial_2_3]:https://blog.csdn.net/qq_14903317/article/details/108582372

[tutorial_3]:https://arclee0117.wordpress.com/2020/09/10/unity-addressable-%E7%8D%A8%E7%AB%8B%E8%B3%87%E6%BA%90%E5%8C%85/

[update_1]:https://i.imgur.com/p4KrH0y.jpg

[asset_1]:https://i.imgur.com/I0VTkCY.jpg

[asset_2]:https://i.imgur.com/bxwlzNS.jpg
