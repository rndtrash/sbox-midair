﻿using MidAir.UI.Menus;
using MidAir.UI.PostGameScreens;
using Sandbox;
using Sandbox.UI;

namespace MidAir.UI
{
	public partial class MidAirHud : HudEntity<RootPanel>
	{
		public static MidAirHud currentHud;

		public static Panel parallaxPanel;
		public static Panel staticPanel;

		private static Panel endGameScreen;
		private static Panel winnerScreen;
		private static Panel mapVoteScreen;

		private static BaseMenu currentMenu;

		private static Scoreboard<ScoreboardEntry> scoreboard;

		public MidAirHud()
		{
			if ( IsClient )
			{
				staticPanel = RootPanel.Add.Panel( "staticpanel" );
				staticPanel.StyleSheet.Load( "/Code/UI/ParallaxHud.scss" );
				scoreboard = staticPanel.AddChild<Scoreboard<ScoreboardEntry>>();
				staticPanel.AddChild<Crosshair>();
				staticPanel.AddChild<ClassicChatBox>();
				staticPanel.AddChild<Hitmarker>();
				staticPanel.AddChild<MessagesPanel>();
				staticPanel.AddChild<NameTags>();
				staticPanel.AddChild<KillFeed>();

				SetCurrentMenu( new MainMenu() );

				parallaxPanel = RootPanel.AddChild<ParallaxHud>();
				currentHud = this;
			}
		}

		bool isFirstTick = true;

		[Event.Tick.Client]
		public void OnTick()
		{
			if ( isFirstTick )
			{
				Game.Instance.GameType.CreateHUDElements( parallaxPanel, staticPanel );
				isFirstTick = false;
			}
		}

		public static void ToggleEndGameScreen( bool oldValue, bool newValue )
		{
			if ( newValue )
			{
				endGameScreen = staticPanel.AddChild<EndGameScreen>();
				scoreboard.ForceOpen = true;
			}
			else
			{
				endGameScreen?.Delete();
				scoreboard.ForceOpen = false;
			}
		}

		public void SetCurrentMenu( BaseMenu menu )
		{
			currentMenu?.Delete();
			currentMenu = menu;
			menu.Parent = staticPanel;
		}

		public void OnDeath( string killer )
		{
			Host.AssertClient();

			// We died
			parallaxPanel?.DeleteChildren();

			MessagesPanel.Instance.AddDeathMessage( "Rocket Launcher", killer );
		}

		public void OnRespawn()
		{
			Host.AssertClient();
			parallaxPanel = RootPanel.AddChild<ParallaxHud>();
			isFirstTick = true;
		}

		public void OnKilledMessage( Player attacker, Player victim, string[] medals )
		{
			if ( attacker.Client.PlayerId != (Local.Client?.PlayerId) )
				return;

			// We killed someone
			MessagesPanel.Instance.AddFragMessage( "Rocket Launcher", victim.Client.Name, medals );
		}
	}
}
