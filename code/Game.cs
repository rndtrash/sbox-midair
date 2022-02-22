using MidAir.GameTypes;
using MidAir.UI;
using Sandbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MidAir
{
	public partial class Game : Sandbox.Game
	{
		private static MidAirHud hud;
		public static Game Instance;

		[Net] public BaseGameType GameType { get; set; }

		public Game()
		{
			Precache.Add( "particles/gib_blood.vpcf" );
			Precache.Add( "particles/speed_lines.vpcf" );
			Precache.Add( "sounds/jump.vsnd" );

			Precache.Add( "weapons/rocketlauncher/particles/rocketlauncher_beam.vpcf" );
			Precache.Add( "weapons/rocketlauncher/particles/rocketlauncher_pulse.vpcf" );
			Precache.Add( "weapons/rocketlauncher/sounds/rocketlauncher_fire.vsnd" );

			if ( IsClient )
			{
				PlayerSettings.Load();
			}

			if ( IsServer )
			{
				hud = new MidAirHud();

				switch ( Global.MapName.Split( '_' )[^1] )
				{
					case "ascend":
						GameType = new AscendGameType();
						break;
					case "tdm":
						GameType = new TdmGameType();
						break;
					case "ffa":
					default:
						GameType = new FfaGameType();
						break;
				}
			}

			Instance = this;
		}

		public override void ClientJoined( Client cl )
		{
			base.ClientJoined( cl );
			CurrentState.OnPlayerJoin( cl );

			var player = new Player( cl );
			cl.Pawn = player;
			GameType.AssignPlayerTeam( player );
			player.Respawn();
		}

		public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
		{
			base.ClientDisconnect( cl, reason );
			CurrentState.OnPlayerLeave( cl );
		}

		public override void OnKilled( Client client, Entity pawn )
		{
			Host.AssertServer();

			Log.Info( $"{client.Name} was killed" );

			if ( pawn is not Player victim )
				return;

			CurrentState.OnDeath( victim.Client );

			// HACK: Assign a respawn timer for this player
			async Task RespawnTimer()
			{
				await Task.DelaySeconds( 3.0f );
				PlayerRespawnRpc( To.Single( victim ) );
			}
			_ = RespawnTimer();

			//
			// Get attacker info
			//
			if ( pawn.LastAttacker is not Player attacker )
			{
				PlayerDiedRpc( To.Single( victim ), null );
				OnKilledMessage( 0, "", client.PlayerId, client.Name, "died" );
				return;
			}

			// Killstreak tracking
			attacker.CurrentStreak++;
			CurrentState.OnKill( attacker.Client, victim.Client );

			PlayerDiedRpc( To.Single( victim ), attacker );

			//
			// Give out medals to the attacker
			//
			List<Medal> medals = Medals.GetMedalsForKill( attacker, victim );

			string[] medalArr = new string[medals.Count];
			for ( int i = 0; i < medals.Count; ++i )
				medalArr[i] = medals[i].Name;

			// Display "YOU FRAGGED" message
			PlayerKilledRpc( To.Single( attacker ), attacker, victim, medalArr );

			var attackerClient = attacker.Client;
			OnKilledMessage( attackerClient.PlayerId, attackerClient.Name, client.PlayerId, client.Name, "Rocket Launcher" );

			GameType.OnFrag( attackerClient, victim.Client );
		}

		[ClientRpc]
		public void PlayerRespawnRpc()
		{
			MidAirHud.currentHud?.OnRespawn();
		}

		[ServerCmd( "recreatehud", Help = "Recreate hud object" )]
		public static void RecreateHud()
		{
			hud.Delete();
			hud = new();
			Log.Info( "Recreated HUD" );
		}

		[ClientRpc]
		public void PlayerDiedRpc( Player attacker )
		{
			// Attacker, victim
			MidAirHud.currentHud.OnDeath( attacker?.Client?.Name ?? "Yourself" );
		}

		[ClientRpc]
		public void PlayerKilledRpc( Player attacker, Player victim, string[] medals )
		{
			// Attacker, victim
			MidAirHud.currentHud.OnKilledMessage( attacker, victim, medals );
		}
	}
}
