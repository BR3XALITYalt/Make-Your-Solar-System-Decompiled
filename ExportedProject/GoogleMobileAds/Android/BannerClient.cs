using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
	internal class BannerClient : AndroidJavaProxy, IBannerClient
	{
		private AndroidJavaObject bannerView;

		public event EventHandler<EventArgs> OnAdLoaded;

		public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad;

		public event EventHandler<EventArgs> OnAdOpening;

		public event EventHandler<EventArgs> OnAdClosed;

		public event EventHandler<EventArgs> OnAdLeavingApplication;

		public BannerClient()
			: base("com.google.unity.ads.UnityAdListener")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			bannerView = new AndroidJavaObject("com.google.unity.ads.Banner", androidJavaObject, this);
		}

		public void CreateBannerView(string adUnitId, AdSize adSize, AdPosition position)
		{
			bannerView.Call("create", adUnitId, Utils.GetAdSizeJavaObject(adSize), (int)position);
		}

		public void LoadAd(AdRequest request)
		{
			bannerView.Call("loadAd", Utils.GetAdRequestJavaObject(request));
		}

		public void ShowBannerView()
		{
			bannerView.Call("show");
		}

		public void HideBannerView()
		{
			bannerView.Call("hide");
		}

		public void DestroyBannerView()
		{
			bannerView.Call("destroy");
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
