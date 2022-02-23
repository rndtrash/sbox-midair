using MidAir.UI.PostGameScreens;
using MidAir.UI.PostGameScreens.Elements;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;
using System.Linq;

namespace MidAir.GameTypes
{
	[Library( "gametype_ascend", Title = "Ascend", Description = "Reach the goal to win!" )]
	public partial class AscendGameType : BaseGameType
	{
		[Net] public Player Winner { get; set; } = null;

		public AscendGameType()
		{
			GameTypeName = "Ascend";
			GameTypeDescription = "Reach the goal to win!";
			IsExperimental = true;
		}

		public override void OnGameStart()
		{
			base.OnGameStart();

			if (Entities.Checkpoint.First is null)
			{
				Log.Error( "This map is not valid since it has no checkpoints." );
				return;
			}

			foreach (var cl in Client.All)
			{
				if ( cl.Pawn is not Player p )
					continue;

				p.CurrentCheckpoint = Entities.Checkpoint.First;
			}
		}

		private static string GetClassForPosition( int position )
		{
			switch ( position )
			{
				case 1:
					return "second";
				case 2:
					return "third";
			}
			return "";
		}

		public override bool GameShouldEnd()
		{
			if ( Winner is not null )
				return true;

			return base.GameShouldEnd();
		}

		protected static int SortClientsByGoal(Client c1, Client c2)
		{
			if ( c1.Pawn is not Player p1 || c2.Pawn is not Player p2 )
				return 0;

			return p1.Position.Distance(Entities.Goal.Instance.Position) < p2.Position.Distance(Entities.Goal.Instance.Position) ? -1 : 1;
		}

		public override void CreateHUDElements( Panel RootPanel, Panel StaticHudPanel )
		{
			base.CreateHUDElements( RootPanel, StaticHudPanel );

			// RootPanel.AddChild<CheckpointMark>();
			Log.Error( "TODO: add checkpoint mark" );
		}

		public override void CreateWinnerElements( EndGameScreen winnerScreen, Panel parent )
		{
			if (Winner is null)
			{
				foreach (var cl in Client.All)
				{
					if (cl == Local.Client)
					{
						parent.Add.Label( $"TIE", "title" );

						break;
					}
				}

				return;
			}

			var sortedClients = new List<Client>( Client.All.Except(new List<Client> { Winner.Client } ) );
			sortedClients.Sort( SortClientsByGoal );
			var playersPanel = parent.Add.Panel( "players" );
			int particleCount = 0;

			//
			// Local player position
			//

			{
				var client = Winner.Client;
				if (client == Local.Client)
				{
					parent.Add.Label( $"YOU PLACED #1", "title first" );

					particleCount = 3 * 16;
				}
			}

			for ( int i = 0; i < sortedClients.Count; i++ )
			{
				Client client = sortedClients[i];
				if ( client == Local.Client )
				{
					parent.Add.Label( $"YOU PLACED #{i + 1}", "title " + GetClassForPosition( i + 1 ) );

					particleCount = (2 - i) * 16;
				}
			}

			//
			// Other players
			//

			{
				var client = Winner.Client;
				var firstPlace = new WinnerText( "#1", client, "first" );
				firstPlace.Parent = playersPanel;
			}

			for ( int i = 0; i < sortedClients.Count; i++ )
			{
				Client client = sortedClients[i];
				switch ( i )
				{
					case 0:
						var secondPlace = new WinnerText( "#2", client, "second" );
						secondPlace.Parent = playersPanel;
						break;
					case 1:
						var thirdPlace = new WinnerText( "#3", client, "third" );
						thirdPlace.Parent = playersPanel;
						break;
				}
			}

			winnerScreen.CreateWinnerParticles( particleCount );
		}
	}
}
