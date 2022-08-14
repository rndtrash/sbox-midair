using Sandbox;
using System;

namespace MidAir.GameStates
{
	public class MainGameState : BaseGameState
	{
		public override string StateName() => Game.Instance.GameType.GameTypeName;

		public MainGameState() : base()
		{
			// Respawn players
			foreach ( var entity in Client.All )
			{
				var player = entity.Pawn as Player;
				player?.Respawn();

				// Reset scores
				entity.SetInt( "kills", 0 );
				entity.SetInt( "deaths", 0 );
				entity.SetInt( "totalShots", 0 );
				entity.SetInt( "totalHits", 0 );
			}

			Event.Run( "midair.start" );
			Game.Instance.GameType.OnGameStart();
			GameServices.StartGame();
		}

		public override void OnKill( Client attackerClient, Client victimClient )
		{
			base.OnKill( attackerClient, victimClient );

			if ( !MidAirGlobal.DebugMode )
				GameServices.RecordEvent( attackerClient, "killed", victim: victimClient );
		}

		public override void OnDeath( Client cl )
		{
			base.OnDeath( cl );

			if ( !MidAirGlobal.DebugMode )
				GameServices.RecordEvent( cl, "died" );
		}

		public override string StateTime()
		{
			var ts = Time.Now - Game.Instance.GameType.GameBeginning;
			var timeEndSpan = TimeSpan.FromSeconds( Math.Max( MidAirGlobal.TimeLimit != 0 ? MidAirGlobal.TimeLimit - ts : ts, 0 ) );
			return GetTimeString( timeEndSpan );
		}

		public override void Tick()
		{
			base.Tick();

			if ( Game.Instance.GameType.GameShouldEnd() || GetPlayerCount() <= 1 )
			{
				SetState( new GameFinishedState() );

				if ( !MidAirGlobal.DebugMode )
					GameServices.EndGame();

				return;
			}
		}
	}
}
