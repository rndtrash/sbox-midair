﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Threading.Tasks;

namespace MidAir.UI.PostGameScreens.Elements
{
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
			AddClass( "vote-panel" );

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
				SetClass( "disabled", mapVote.PlayerId == Local.Client.PlayerId );
				SetClass( "voted-for", mapVote.PlayerId == Local.Client.PlayerId && mapVote.MapIndex == this.Index );

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
