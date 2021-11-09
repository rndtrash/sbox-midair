﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace Instagib.UI.PostGameScreens
{
	public class MapVoteScreen : BasePostGameScreen
	{
		private Label TimeLeftLabel { get; set; }
		public MapVoteScreen() : base()
		{
			StyleSheet.Load( "/Code/UI/PostGameScreens/MapVoteScreen.scss" );

			Add.Label( $"VOTE NEXT MAP", "title" );

			TimeLeftLabel = Add.Label( "", "time-left" );

			var mapList = Add.Panel( "map-list" );

			for ( int i = 0; i < InstagibGlobal.GetMaps().Length; i++ )
			{
				string mapName = InstagibGlobal.GetMaps()[i];
				var mapPanel = MapVotePanel.FromPackage( mapName, i );
				mapPanel.Parent = mapList;
			}
		}

		public override void Tick()
		{
			base.Tick();
			TimeLeftLabel.Text = "Time left: " + Game.Instance?.CurrentStateTime;
		}
	}

	public class MapVotePanel : Panel
	{
		private Label VoteCount { get; set; }
		private int Index { get; set; }

		public static MapVotePanel FromPackage( string packageName, int index )
		{
			var packageTask = Package.Fetch( packageName, true ).ContinueWith( t =>
			{
				var package = t.Result;
				return new MapVotePanel( package.Title, package.Thumb, index );
			} );

			return packageTask.Result;
		}

		public MapVotePanel( string mapName, string backgroundImage, int index )
		{
			VoteCount = Add.Label( "0", "vote-count" );

			Add.Label( "VOTES", "vote-subtext" );
			Add.Label( mapName, "map-name" );
			Style.BackgroundImage = Texture.Load( backgroundImage );

			AddEventListener( "onclick", () =>
			{
				if ( HasClass( "disabled" ) )
					return;

				Game.VoteMap( index );
				Sound.FromScreen( "vote_confirm" );
				_ = SetClickClass();
			} );

			Index = index;
		}

		public override void Tick()
		{
			base.Tick();

			int votes = 0;
			foreach ( var mapVote in Game.Instance?.MapVotes )
			{
				SetClass( "disabled", mapVote.PlayerId == Local.Client.SteamId );
				SetClass( "voted-for", mapVote.PlayerId == Local.Client.SteamId && mapVote.MapIndex == this.Index );
				if ( mapVote.MapIndex == this.Index )
					votes++;
			}

			VoteCount.Text = votes.ToString();
		}

		private async Task SetClickClass()
		{
			AddClass( "clicked" );
			await Task.Delay( 50 );
			RemoveClass( "clicked" );
		}
	}
}