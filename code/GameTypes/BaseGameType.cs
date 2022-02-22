using MidAir.UI.PostGameScreens;
using Sandbox;
using Sandbox.UI;

namespace MidAir.GameTypes
{
	public partial class BaseGameType : BaseNetworkable
	{
		[Net] public string GameTypeName { get; set; }
		[Net] public string GameTypeDescription { get; set; }
		[Net] public bool IsExperimental { get; set; }

		[Net] public float GameBeginning { get; protected set; }

		public string LibraryName { get; set; }

		public virtual void OnGameStart()
		{
			GameBeginning = Time.Now;
		}

		public virtual void OnFrag(Client who, Client whom) { }

		public virtual bool GameShouldEnd()
		{
			if ( MidAirGlobal.TimeLimit != 0 )
				return Time.Now - GameBeginning >= MidAirGlobal.TimeLimit;
			return false;
		}

		public virtual void AssignPlayerTeam( Player player )
		{
			player.Team = new Teams.BaseTeam()
			{
				TeamName = player.Client.Name,
				TeamColor = "#bada55",
				TeamId = player.NetworkIdent
			};
		}

		public virtual void Notify( string str )
		{
			// Shouldn't be using the chatbox for this
			// TODO: revisit
			ClassicChatBox.AddInformation( To.Everyone, str, null, true );
		}

		public virtual void CreateWinnerElements( EndGameScreen winnerScreen, Panel parent ) { }

		public virtual void CreateHUDElements( Panel panel, Panel StaticHudPanel ) { }
	}
}
