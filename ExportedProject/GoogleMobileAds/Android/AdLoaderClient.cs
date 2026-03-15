using System;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
	public class AdLoaderClient : AndroidJavaProxy, IAdLoaderClient
	{
		private AndroidJavaObject adLoader;

		private Dictionary<string, Action<CustomNativeTemplateAd, string>> CustomNativeTemplateCallbacks { get; set; }

		public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

		public event EventHandler<CustomNativeEventArgs> OnCustomNativeTemplateAdLoaded;

		public AdLoaderClient(AdLoader unityAdLoader)
			: base("com.google.unity.ads.UnityCustomNativeAdListener")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			adLoader = new AndroidJavaObject("com.google.unity.ads.NativeAdLoader", androidJavaObject, unityAdLoader.AdUnitId, this);
			CustomNativeTemplateCallbacks = unityAdLoader.CustomNativeTemplateClickHandlers;
			if (unityAdLoader.AdTypes.Contains(NativeAdType.CustomTemplate))
			{
				foreach (string templateId in unityAdLoader.TemplateIds)
				{
					adLoader.Call("configureCustomNativeTemplateAd", templateId, CustomNativeTemplateCallbacks.ContainsKey(templateId));
				}
			}
			adLoader.Call("create");
		}

		public void LoadAd(AdRequest request)
		{
			adLoader.Call("loadAd", Utils.GetAdRequestJavaObject(request));
		}

		public void onCustomTemplateAdLoaded(AndroidJavaObject ad)
		{
			CustomNativeEventArgs e = new CustomNativeEventArgs();
			e.nativeAd = new CustomNativeTemplateAd(new CustomNativeTemplateClient(ad));
			CustomNativeEventArgs e2 = e;
			this.OnCustomNativeTemplateAdLoaded(this, e2);
		}

		private void onAdFailedToLoad(string errorReason)
		{
			AdFailedToLoadEventArgs e = new AdFailedToLoadEventArgs();
			e.Message = errorReason;
			AdFailedToLoadEventArgs e2 = e;
			this.OnAdFailedToLoad(this, e2);
		}

		public void onCustomClick(AndroidJavaObject ad, string assetName)
		{
			CustomNativeTemplateAd customNativeTemplateAd = new CustomNativeTemplateAd(new CustomNativeTemplateClient(ad));
			CustomNativeTemplateCallbacks[customNativeTemplateAd.GetCustomTemplateId()](customNativeTemplateAd, assetName);
		}
	}
}
