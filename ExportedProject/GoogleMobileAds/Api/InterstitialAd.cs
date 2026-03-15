using System;
using GoogleMobileAds.Common;

namespace GoogleMobileAds.Api
{
	public class InterstitialAd
	{
		private IInterstitialClient client;

		public event EventHandler<EventArgs> OnAdLoaded = delegate
		{
		};

		public event EventHandler<AdFailedToLoadEventArgs> OnAdFailedToLoad = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdOpening = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdClosed = delegate
		{
		};

		public event EventHandler<EventArgs> OnAdLeavingApplication = delegate
		{
		};

		public InterstitialAd(string adUnitId)
		{
			client = GoogleMobileAdsClientFactory.BuildInterstitialClient();
			client.CreateInterstitialAd(adUnitId);
			client.OnAdLoaded += delegate(object sender, EventArgs args)
			{
				this.OnAdLoaded(this, args);
			};
			client.OnAdFailedToLoad += delegate(object sender, AdFailedToLoadEventArgs args)
			{
				this.OnAdFailedToLoad(this, args);
			};
			client.OnAdOpening += delegate(object sender, EventArgs args)
			{
				this.OnAdOpening(this, args);
			};
			client.OnAdClosed += delegate(object sender, EventArgs args)
			{
				this.OnAdClosed(this, args);
			};
			client.OnAdLeavingApplication += delegate(object sender, EventArgs args)
			{
				this.OnAdLeavingApplication(this, args);
			};
		}

		public void LoadAd(AdRequest request)
		{
			client.LoadAd(request);
		}

		public bool IsLoaded()
		{
			return client.IsLoaded();
		}

		public void Show()
		{
			client.ShowInterstitial();
		}

		public void Destroy()
		{
			client.DestroyInterstitial();
		}

		public void SetInAppPurchaseProcessor(IDefaultInAppPurchaseProcessor processor)
		{
			client.SetDefaultInAppPurchaseProcessor(processor);
		}

		public void SetInAppPurchaseProcessor(ICustomInAppPurchaseProcessor processor)
		{
			client.SetCustomInAppPurchaseProcessor(processor);
		}
	}
}
