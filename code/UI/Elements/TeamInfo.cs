using MidAir.GameTypes;
using MidAir.Teams;
using Sandbox;
using Sandbox.UI;

namespace MidAir.UI.Elements
{
	[UseTemplate]
	public class TeamInfo : Panel
	{
		private Label RedScoreLabel { get; set; }
		private Label BlueScoreLabel { get; set; }
		private Label PlayingAsLabel { get; set; }

		public TeamInfo()
		{
			var localTeam = Local.Client.GetTeam();

			PlayingAsLabel.Text = $"You are playing as {localTeam.TeamName}";
			PlayingAsLabel.AddClass( localTeam.TeamName );

			StyleSheet.Load( "/Code/UI/Elements/FlagInfo.scss" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Game.Instance.GameType is TdmGameType ctfGame )
			{
				RedScoreLabel.Text = $"{ctfGame.RedScore}";
				BlueScoreLabel.Text = $"{ctfGame.BlueScore}";
			}
		}
	}
}
