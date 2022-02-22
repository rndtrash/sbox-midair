using MidAir.Teams;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace MidAir.UI
{
	public partial class ScoreboardEntry : Panel
	{
		public Client Client;

		private Label playerName;
		private Label kills;
		private Label deaths;
		private Label ratio;
		private Label ping;
		private Label team;

		private Image avatar;

		public ScoreboardEntry()
		{
			AddClass( "entry" );

			avatar = Add.Image( null, "avatar" );
			playerName = Add.Label( "PlayerName", "name" );

			kills = Add.Label( "k", "kills" );
			deaths = Add.Label( "d", "deaths" );
			ratio = Add.Label( "r", "ratio" );
			ping = Add.Label( "ping", "ping" );
			team = Add.Label( "team", "team" );
		}

		RealTimeSince TimeSinceUpdate = 0;

		public override void Tick()
		{
			base.Tick();

			if ( !IsVisible )
				return;

			if ( !Client.IsValid() )
				return;

			if ( TimeSinceUpdate < 0.1f )
				return;

			TimeSinceUpdate = 0;
			UpdateData();
		}

		public virtual void UpdateData()
		{
			playerName.Text = Client.Name;

			//
			// Kills/Deaths
			//
			{
				var killVal = Client.GetInt( "kills", 0 );
				var deathVal = Client.GetInt( "deaths", 0 );

				kills.Text = killVal.ToString();
				deaths.Text = deathVal.ToString();

				var ratioVal = (killVal / (float)deathVal);
				if ( deathVal == 0 )
					ratioVal = killVal;

				ratio.Text = ratioVal.ToString( "N1" );
			}

			avatar.SetTexture( $"avatar:{Client.PlayerId}" );
			ping.Text = Client.Ping.ToString();
			var playerTeam = Client.GetTeam();
			team.Text = playerTeam.TeamName;

			SetClass( "me", Client == Local.Client );
		}

		public virtual void UpdateFrom( Client client )
		{
			Client = client;
			UpdateData();
		}
	}
}
