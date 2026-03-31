using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class GPGSCalls : MonoBehaviour
{
	private GameController gc;

	private string message = string.Empty;

	private string[] ranks = new string[25];

	private string[] playerNames = new string[25];

	private string[] scores = new string[25];

	private int prevScore;

	private int currentPlayerPosition = -1;

	private bool loggedIntoGoogleGames;

	private string currentPlayerName = "Unknown";

	private string currentPlayerId = string.Empty;

	private string achievementFullOfStuff = "CgkIhPyWxdcYEAIQCw";

	private string achievementNewStar = "CgkIhPyWxdcYEAIQDg";

	private string achievementMichaelBay = "CgkIhPyWxdcYEAIQDA";

	private string achievementBlackHole = "CgkIhPyWxdcYEAIQDQ";

	private string achievementShareFiveTimes = "CgkIhPyWxdcYEAIQEw";

	private string achievementScreenBreaker = "CgkIhPyWxdcYEAIQIA";

	private string achievementThreesCrowd = "CgkIhPyWxdcYEAIQIQ";

	private string achievementOver9000 = "CgkIhPyWxdcYEAIQIg";

	private string achievementSolarSystemLover = "CgkIhPyWxdcYEAIQIw";

	private string achievementTheBest = "CgkIhPyWxdcYEAIQJA";

	private string achievementOrangeNewBlack = "CgkIhPyWxdcYEAIQJQ";

	private string achievementImBlue = "CgkIhPyWxdcYEAIQJg";

	private string leaderboardModeQuickTimeAttack = "CgkIhPyWxdcYEAIQAw";

	private string leaderboardModeBigTimeAttack = "CgkIhPyWxdcYEAIQCA";

	private string leaderboardModeTinyTimeAttack = "CgkIhPyWxdcYEAIQDw";

	private string leaderboardModeTwinTimeAttack = "CgkIhPyWxdcYEAIQEA";

	private string leaderboardModeTriadTimeAttack = "CgkIhPyWxdcYEAIQEQ";

	private string leaderboardModeQuadTimeAttack = "CgkIhPyWxdcYEAIQEg";

	private string leaderboardModeBasicDestroyer = "CgkIhPyWxdcYEAIQFA";

	private string leaderboardModeFastDestroyer = "CgkIhPyWxdcYEAIQFQ";

	private string leaderboardModeTheCrossDestroyer = "CgkIhPyWxdcYEAIQFg";

	private string leaderboardModeMetaOrbitDestroyer = "CgkIhPyWxdcYEAIQFw";

	private string leaderboardModeStarDestroyer = "CgkIhPyWxdcYEAIQGA";

	private string leaderboardModeSuperDestroyer = "CgkIhPyWxdcYEAIQGQ";

	private string leaderboardModeSingleSupernova = "CgkIhPyWxdcYEAIQGg";

	private string leaderboardModeTripleSupernova = "CgkIhPyWxdcYEAIQGw";

	private string leaderboardModeSymmetrySupernova = "CgkIhPyWxdcYEAIQHA";

	private string leaderboardModeOneImpactSupernova = "CgkIhPyWxdcYEAIQHQ";

	private string leaderboardModeCrashOfTheTitansSupernova = "CgkIhPyWxdcYEAIQHg";

	private string leaderboardModeChaosSupernova = "CgkIhPyWxdcYEAIQHw";

	private string eventPlanetsLost = "CgkIhPyWxdcYEAIQCQ";

	private void Start()
	{
		getGameController();
	}

	public void Reset()
	{
		currentPlayerPosition = -1;
		ranks = new string[25];
		playerNames = new string[25];
		scores = new string[25];
	}

	public void LogInGooglePlayGames()
	{
		//we do not need this shit
	}

	public bool GetLoggedIntoGoogleGames()
	{
		return loggedIntoGoogleGames;
	}

	public string getMessage()
	{
		return message;
	}

	public void UnlockAchievement(string name)
	{
		string empty = string.Empty;
		switch (name)
		{
		case "MoreThan20objects":
			empty = achievementFullOfStuff;
			break;
		case "PlanetToStar":
			empty = achievementNewStar;
			break;
		case "Supernova":
			empty = achievementMichaelBay;
			break;
		case "BlackHole":
			empty = achievementBlackHole;
			break;
		case "ScreenBreaker":
			empty = achievementScreenBreaker;
			break;
		case "ThreesCrowd":
			empty = achievementThreesCrowd;
			break;
		case "Over9000":
			empty = achievementOver9000;
			break;
		case "SolarSystemLover":
			empty = achievementSolarSystemLover;
			break;
		case "TheBest":
			empty = achievementTheBest;
			break;
		case "OrangeNewBlack":
			empty = achievementOrangeNewBlack;
			break;
		case "ImBlue":
			empty = achievementImBlue;
			break;
		}
		Social.ReportProgress(empty, 100.0, delegate
		{
		});
	}

	public void UnlockIncrementalAchievement(string name)
	{
		
	}

	public void ShowAchievements()
	{
		Social.ShowAchievementsUI();
	}

	public void PutScoreAndGetLeaderBoard(int score, string mapName)
	{
		
	}

	public void GetIngameLeaderboard(string mapName)
	{
		
	}

	internal void LoadUsersAndDisplay(ILeaderboard lb)
	{
		List<string> list = new List<string>();
		IScore[] array = lb.scores;
		foreach (IScore score in array)
		{
			list.Add(score.userID);
		}
		Social.LoadUsers(list.ToArray(), delegate(IUserProfile[] users)
		{
			for (int j = 0; j < lb.scores.Length; j++)
			{
				ranks[j] = "#" + lb.scores[j].rank;
				string text = FindUser(users, lb.scores[j].userID);
				playerNames[j] = text;
				scores[j] = lb.scores[j].formattedValue;
				if (Social.localUser.id == lb.scores[j].userID)
				{
					currentPlayerPosition = j;
				}
			}
			gc.SetLeaderboardLoaded();
		});
	}

	private string FindUser(IUserProfile[] users, string userID)
	{
		foreach (IUserProfile userProfile in users)
		{
			if (userProfile.id == userID)
			{
				return userProfile.userName;
			}
			if (currentPlayerId == userID)
			{
				return currentPlayerName;
			}
		}
		return "Unknown";
	}

	public string[] GetRanks()
	{
		return ranks;
	}

	public string[] GetPlayerNames()
	{
		return playerNames;
	}

	public string[] GetScores()
	{
		return scores;
	}

	public int GetCurrentPlayerPosition()
	{
		return currentPlayerPosition;
	}

	public int GetPrevScore()
	{
		return prevScore;
	}

	public void ShowLeaderboard()
	{
		if (loggedIntoGoogleGames)
		{
			Social.ShowLeaderboardUI();
		}
	}

	public void ShowSpecificLeaderboard(string mapName)
	{
	}

	public void LogOut()
	{
	}

	private string GetLeaderboardIdFromMapName(string mapName)
	{
		string empty = string.Empty;
		switch (mapName)
		{
		case "ModeQuickTimeAttack":
			empty = leaderboardModeQuickTimeAttack;
			break;
		case "ModeBigTimeAttack":
			empty = leaderboardModeBigTimeAttack;
			break;
		case "ModeTinyTimeAttack":
			empty = leaderboardModeTinyTimeAttack;
			break;
		case "ModeTwinTimeAttack":
			empty = leaderboardModeTwinTimeAttack;
			break;
		case "ModeTriadTimeAttack":
			empty = leaderboardModeTriadTimeAttack;
			break;
		case "ModeQuadTimeAttack":
			empty = leaderboardModeQuadTimeAttack;
			break;
		case "ModeBasicDestroyer":
			empty = leaderboardModeBasicDestroyer;
			break;
		case "ModeFastDestroyer":
			empty = leaderboardModeFastDestroyer;
			break;
		case "ModeTheCrossDestroyer":
			empty = leaderboardModeTheCrossDestroyer;
			break;
		case "ModeMetaOrbitDestroyer":
			empty = leaderboardModeMetaOrbitDestroyer;
			break;
		case "ModeStarDestroyer":
			empty = leaderboardModeStarDestroyer;
			break;
		case "ModeSuperDestroyer":
			empty = leaderboardModeSuperDestroyer;
			break;
		case "ModeSingleSupernova":
			empty = leaderboardModeSingleSupernova;
			break;
		case "ModeTripleSupernova":
			empty = leaderboardModeTripleSupernova;
			break;
		case "ModeSymmetrySupernova":
			empty = leaderboardModeSymmetrySupernova;
			break;
		case "ModeOneImpactSupernova":
			empty = leaderboardModeOneImpactSupernova;
			break;
		case "ModeCrashOfTheTitansSupernova":
			empty = leaderboardModeCrashOfTheTitansSupernova;
			break;
		case "ModeChaosSupernova":
			empty = leaderboardModeChaosSupernova;
			break;
		}
		return empty;
	}

	public void SetPlanetLostEvent()
	{
	}

	private void getGameController()
	{
		GameObject gameObject = GameObject.FindWithTag("GameController");
		if (gameObject != null)
		{
			gc = gameObject.GetComponent<GameController>();
		}
		else
		{
			Debug.Log("Cannot find 'GameController' script");
		}
	}
}
