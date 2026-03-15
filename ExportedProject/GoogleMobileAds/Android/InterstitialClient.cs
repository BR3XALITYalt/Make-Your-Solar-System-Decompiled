using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
	internal class InterstitialClient : AndroidJavaProxy, IInterstitialClient
	{
		private AndroidJavaObject interstitial;

		public event EventHandler<EventArgs> OnAdLoaded;

		public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

		public event EventHandler<EventArgs> OnAdOpening;

		public event EventHandler<EventArgs> OnAdClosed;

		public event EventHandler<EventArgs> OnAdLeavingApplication;

		public InterstitialClient()
			: base("com.google.unity.ads.UnityAdListener")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			interstitial = new AndroidJavaObject("com.google.unity.ads.Interstitial", androidJavaObject, this);
		}

		public void CreateInterstitialAd(string adUnitId)
		{
			interstitial.Call("create", adUnitId);
		}

		public void LoadAd(AdRequest request)
		{
			interstitial.Call("loadAd", Utils.GetAdRequestJavaObject(request));
		}

		public bool IsLoaded()
		{
			return interstitial.Call<bool>("isLoaded", new object[0]);
		}

		public void ShowInterstitial()
		{
			interstitial.Call("show");
		}

		public void DestroyInterstitial()
		{
			interstitial.Call("destroy");
		}

		public void SetDefaultInAppPurchaseProcessor(IDefaultInAppPurchaseProcessor processor)
		{
			DefaultInAppPurchaseListener defaultInAppPurchaseListener = new DefaultInAppPurchaseListener(processor);
			interstitial.Call("setPlayStorePurchaseParams", defaultInAppPurchaseListener, processor.AndroidPublicKey);
		}

		public void SetCustomInAppPurchaseProcessor(ICustomInAppPurchaseProcessor processor)
		{
			CustomInAppPurchaseListener customInAppPurchaseListener = new CustomInAppPurchaseListener(processor);
			interstitial.Call("setInAppPurchaseListener", customInAppPurchaseListener);
		}

		public void onAdLoaded()
		{
			this.OnAdLoaded(this, EventArgs.Empty);
		}

		public void onAdFailedToLoad(string errorReason)
		{
			AdFailedToLoadEventArgs e = new AdFailedToLoadEventArgs();
			e.Message = errorReason;
			AdFailedToLoadEventArgs e2 = e;
			this.OnAdFailedToLoad(this, e2);
		}

		public void onAdOpened()
		{
			this.OnAdOpening(this, EventArgs.Empty);
		}

		public void onAdClosed()
		{
			this.OnAdClosed(this, EventArgs.Empty);
		}

		public void onAdLeftApplication()
		{
			this.OnAdLeavingApplication(this, EventArgs.Empty);
		}
	}
}
