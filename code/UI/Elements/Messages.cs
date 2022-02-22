﻿using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace MidAir.UI
{
	public class MessagesPanel : Panel
	{
		public static MessagesPanel Instance { get; private set; }
		public MessagesPanel()
		{
			Instance = this;

			SetClass( "frag", true );
		}

		public void AddFragMessage( string weapon, string target, string[] medals )
		{
			foreach ( var child in Children?.Where( c => c is FragMessage ) )
			{
				child?.Delete();
			}

			var fragMessage = new FragMessage( weapon, target, medals );
			fragMessage.Parent = this;
		}

		public void AddDeathMessage( string weapon, string target )
		{
			foreach ( var child in Children?.Where( c => c is DeathMessage ) )
			{
				child?.Delete();
			}

			var deathMessage = new DeathMessage( weapon, target );
			deathMessage.Parent = this;
		}
	}

	public class DeathMessage : Panel
	{

		public DeathMessage( string weapon, string target )
		{
			SetClass( "frag-message", true );
			StyleSheet.Load( "/Code/UI/Elements/MainPanel.scss" );

			Add.Label( "💀", "frag-skull" );
			var fragDetails = Add.Panel( "frag-details" );

			fragDetails.Add.Label( $"FRAGGED BY" );
			fragDetails.Add.Label( $"{target}", "player" );
			fragDetails.Add.Label( $"{weapon}", "weapon" );

			//
			// Timeout
			//
			_ = KillAfterTime();
		}

		async Task KillAfterTime()
		{
			await Task.Delay( 2500 );
			Delete();
		}
	}

	public class FragMessage : Panel
	{

		public FragMessage( string weapon, string target, string[] medals )
		{
			SetClass( "frag-message", true );
			StyleSheet.Load( "/Code/UI/MainPanel.scss" );

			Add.Label( "💀", "frag-skull" );
			var fragDetails = Add.Panel( "frag-details" );

			fragDetails.Add.Label( $"{target}", "player" );
			fragDetails.Add.Label( $"{weapon}", "weapon" );

			//
			// Medal display
			//
			var medalPanel = fragDetails.Add.Panel( "medals" );

			foreach ( string medal in medals )
				medalPanel.AddChild<Label>().SetText( medal );

			//
			// Timeout
			//
			_ = KillAfterTime();
		}

		async Task KillAfterTime()
		{
			await Task.Delay( 2500 );
			Delete();
		}
	}
}
