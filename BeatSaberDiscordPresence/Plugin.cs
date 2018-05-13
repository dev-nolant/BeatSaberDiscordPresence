using System;
using System.Linq;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatSaberDiscordPresence
{
	public class Plugin : IPlugin
	{
		public static readonly DiscordRpc.RichPresence Presence = new DiscordRpc.RichPresence();
		private const string DiscordAppID = "445053620698742804";
		private MainGameSceneSetupData _mainSetupData;
		private GameStaticData _gameStaticData;
		private bool _init;
		
		public string Name
		{
			get { return "Discord Presence"; }
		}

		public string Version
		{
			get { return "v1.0"; }
		}
		
		public void OnApplicationStart()
		{
			if (_init) return;
			_init = true;
			SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
			
			var handlers = new DiscordRpc.EventHandlers();
			DiscordRpc.Initialize(DiscordAppID, ref handlers, false, string.Empty);
		}

		public void OnApplicationQuit()
		{
			SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
			DiscordRpc.Shutdown();
		}

		private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
		{
			if (newScene.buildIndex == 1)
			{
				//Menu scene loaded
				Presence.details = "In Menu";
				Presence.state = string.Empty;
				Presence.startTimestamp = default(long);
				DiscordRpc.UpdatePresence(Presence);
			}
			else if (newScene.buildIndex == 4)
			{
				_mainSetupData = Resources.FindObjectsOfTypeAll<MainGameSceneSetupData>().FirstOrDefault();
				_gameStaticData = Resources.FindObjectsOfTypeAll<GameStaticData>().FirstOrDefault();
				if (_mainSetupData == null || _gameStaticData == null)
				{
					Console.WriteLine("Discord Presence: Error finding the scriptable objects required to update presence.");
					return;
				}
				//Main game scene loaded;
				var song = _gameStaticData.GetLevelData(_mainSetupData.levelId);
				Presence.details = $"{song.songName} | {LevelStaticData.GetDifficultyName(_mainSetupData.difficulty)}";
				Presence.state = "";
				if (song.levelId.Contains('∎'))
				{
					Presence.state = "Custom | ";
				}
				Presence.state += GetGameplayModeName(_mainSetupData.gameplayMode);
				if (_mainSetupData.gameplayOptions.noEnergy)
				{
					Presence.state += " [No Fail]";
				}

				if (_mainSetupData.gameplayOptions.mirror)
				{
					Presence.state += " [Mirrored]";
				}
				
				Presence.startTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
				DiscordRpc.UpdatePresence(Presence);
			}
		}

		public void OnLevelWasLoaded(int level)
		{
			
		}

		public void OnLevelWasInitialized(int level)
		{
			
		}

		public void OnUpdate()
		{
			DiscordRpc.RunCallbacks();
		}

		public void OnFixedUpdate()
		{
			
		}

		public static string GetGameplayModeName(GameplayMode gameplayMode)
		{
			switch (gameplayMode)
			{
				case GameplayMode.SoloStandard:
					return "Solo Standard";
				case GameplayMode.SoloOneSaber:
					return "One Saber";
				case GameplayMode.SoloNoArrows:
					return "No Arrows";
				case GameplayMode.PartyStandard:
					return "Party";
				default:
					return "Solo Standard";
			}
		}
	}
}