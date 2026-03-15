using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
	internal class RewardBasedVideoAdClient : AndroidJavaProxy, IRewardBasedVideoAdClient
	{
		private AndroidJavaObject androidRewardBasedVideo;

		public event EventHandler<EventArgs> OnAdLoaded = delegate
		{
		};

		public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdOpening = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdStarted = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdClosed = delegate
		{
		};

		public event EventHandler<Reward> OnAdRewarded = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdLeavingApplication = delegate
		{
		};

		public RewardBasedVideoAdClient()
			: base("com.google.unity.ads.UnityRewardBasedVideoAdListener")
		{
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			androidRewardBasedVideo = new AndroidJavaObject("com.google.unity.ads.RewardBasedVideo", androidJavaObject, this);
		}

		public void CreateRewardBasedVideoAd()
		{
			androidRewardBasedVideo.Call("create");
		}

		public void LoadAd(AdRequest request, string adUnitId)
		{
			androidRewardBasedVideo.Call("loadAd", Utils.GetAdRequestJavaObject(request), adUnitId);
		}

		public bool IsLoaded()
		{
			return androidRewardBasedVideo.Call<bool>("isLoaded", new object[0]);
		}

		public void ShowRewardBasedVideoAd()
		{
			androidRewardBasedVideo.Call("show");
		}

		public void DestroyRewardBasedVideoAd()
		{
			androidRewardBasedVideo.Call("destroy");
		}

		private void onAdLoaded()
		{
			this.OnAdLoaded(this, EventArgs.Empty);
		}

		private void onAdFailedToLoad(string errorReason)
		{
			AdFailedToLoadEventArgs e = new AdFailedToLoadEventArgs();
			e.Message = errorReason;
			AdFailedToLoadEventArgs e2 = e;
			this.OnAdFailedToLoad(this, e2);
		}

		private void onAdOpened()
		{
			this.OnAdOpening(this, EventArgs.Empty);
		}

		private void onAdStarted()
		{
			this.OnAdStarted(this, EventArgs.Empty);
		}

		private void onAdClosed()
		{
			this.OnAdClosed(this, EventArgs.Empty);
		}

		private void onAdRewarded(string type, float amount)
		{
			Reward reward = new Reward();
			reward.Type = type;
			reward.Amount = amount;
			Reward e = reward;
			this.OnAdRewarded(this, e);
		}

		private void onAdLeftApplication()
		{
			this.OnAdLeavingApplication(this, EventArgs.Empty);
		}
	}
}
