﻿using System.Collections.Generic;
using System.Linq;
using Instagib.UI;
using Sandbox;
using Sandbox.ScreenShake;

namespace Instagib
{
	[Library( "instagib", Title = "Instagib")]
	public partial class Game : Sandbox.Game
	{
		private static InstagibHud hud;
		public Game()
		{
			Precache.Add( "particles/gib_blood.vpcf" );
			Precache.Add( "sounds/jump.vsnd" );
			
			Precache.Add( "weapons/railgun/particles/railgun_beam.vpcf" );
			Precache.Add( "weapons/railgun/particles/railgun_pulse.vpcf" );
			Precache.Add( "weapons/railgun/sounds/railgun_fire.vsnd" );

			if ( IsClient )
			{
				PlayerSettings.Load();
			}
			
			if ( IsServer )
			{
				hud = new InstagibHud();
			}
		}

		public override void ClientJoined( Client client )
		{
			base.ClientJoined( client );

			var player = new Player();
			client.Pawn = player;
			
			client.SetScore( "steamid", client.SteamId );

			player.Respawn();
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was killed" );
			if ( pawn is not Player victim )
				return;

			// Apply a death
			var victimClient = victim.GetClientOwner();
			victimClient.SetScore( "deaths", victimClient.GetScore<int>( "deaths" ) + 1 );

			if ( pawn.LastAttacker is not Player attacker )
			{
				OnKilledMessage( 0, "", client.SteamId, client.Name, "died" );
				return;
			}

			// Apply a kill
			var attackerClient = attacker.GetClientOwner();
			attackerClient.SetScore( "kills", attackerClient.GetScore<int>( "kills" ) + 1 );

			// Killstreak tracking
			attacker.CurrentStreak++;

			//
			// Give out medals to the attacker
			//
			List<Medal> medals = Medals.KillMedals.Where( medal => medal.Condition.Invoke( attacker, victim ) ).ToList();

			string[] medalArr = new string[medals.Count];
			for ( int i = 0; i < medals.Count; ++i )
				medalArr[i] = medals[i].Name;

			// Display "YOU FRAGGED" message
			PlayerKilledRpc( To.Single( attacker ), attacker, victim, medalArr );

			OnKilledMessage( attackerClient.SteamId, attackerClient.Name, client.SteamId, client.Name, "Railgun" );
		}

		[ServerCmd( "recreatehud", Help = "Recreate hud object" )]
		public static void RecreateHud()
		{
			hud.Delete();
			hud = new();
			Log.Info( "Recreated HUD" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( Player attacker, Player victim, string[] medals )
		{
			InstagibHud.CurrentHud.OnKilledMessage( attacker, victim, medals );
		}
	}
}