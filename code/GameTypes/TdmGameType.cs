using Sandbox;
using MidAir.Teams;
using Sandbox.UI;
using MidAir.UI.Elements;
using MidAir.UI.PostGameScreens;
using System.Collections.Generic;
using Sandbox.UI.Construct;
using System.Linq;
using System;

namespace MidAir.GameTypes
{
	[Library( "gametype_tdm", Title = "Team Deathmatch", Description = "Frag more than the enemy team to win!" )]
	public partial class TdmGameType : BaseGameType
	{
		[Net] public BaseTeam BlueTeam { get; set; }
		[Net] public BaseTeam RedTeam { get; set; }

		[Net] public int BlueScore { get; set; }
		[Net] public int RedScore { get; set; }

		public TdmGameType()
		{
			GameTypeName = "Team Deathmatch";
			GameTypeDescription = "Frag more than the enemy team to win!";
			IsExperimental = true;

			BlueTeam = new()
			{
				TeamName = "Blue",
				TeamId = 0,
				TeamColor = "#71a5fe"
			};

			RedTeam = new()
			{
				TeamName = "Red",
				TeamId = 1,
				TeamColor = "#fe7171"
			};
		}

		public override void OnFrag( Client who, Client whom )
		{
			base.OnFrag( who, whom );

			if ( who.Pawn is not Player p )
				return;

			if ( p.Team == BlueTeam )
			{
				BlueScore++;
			}
			else
			{
				RedScore++;
			}
		}

		public override void CreateWinnerElements( EndGameScreen winnerScreen, Panel parent )
		{
			var sortedClients = new List<Client>( Client.All );
			sortedClients.Sort( MidAirGlobal.SortClients );
			var playersPanel = parent.Add.Panel( "players" );
			int particleCount = 0;

			BaseTeam winningTeam = null;
			if ( RedScore > BlueScore )
				winningTeam = RedTeam;
			else if ( BlueScore > RedScore )
				winningTeam = BlueTeam;

			if ( winningTeam != null )
				parent.Add.Label( $"{winningTeam.TeamName.ToUpper()} WINS", "title " + winningTeam.TeamName );
			else
				parent.Add.Label( $"TIE", "title" );

			{
				var redTeam = playersPanel.Add.Panel( "red-team" );
				redTeam.Add.Label( "RED", "team-name red" );
				redTeam.Add.Label( $"{RedScore}", "captures red" );
			}
			{
				var blueTeam = playersPanel.Add.Panel( "blue-team" );
				blueTeam.Add.Label( "BLUE", "team-name blue" );
				blueTeam.Add.Label( $"{BlueScore}", "captures blue" );
			}

			particleCount = Local.Client.GetTeam() == winningTeam ? 64 : 0;
			winnerScreen.CreateWinnerParticles( particleCount );
		}

		public override bool GameShouldEnd()
		{
			if ( Math.Max( BlueScore, RedScore ) >= MidAirGlobal.FragLimit )
				return true;

			return base.GameShouldEnd();
		}

		public override void AssignPlayerTeam( Player player )
		{
			var teamIndex = Client.All.Count % 2;
			var selectedTeam = BlueTeam;
			if ( teamIndex != 0 )
				selectedTeam = RedTeam;

			player.Team = selectedTeam.Clone();
			Log.Trace( $"Added player {player.Name} to team {selectedTeam.TeamName}" );
		}

		public override void CreateHUDElements( Panel RootPanel, Panel StaticHudPanel )
		{
			base.CreateHUDElements( RootPanel, StaticHudPanel );

			RootPanel.AddChild<TeamInfo>();
		}
	}
}
