using Sandbox;
using System;
using System.Linq;

namespace MidAir.GameStates
{
	public class BaseGameState
	{
		protected RealTimeSince stateStart;

		public BaseGameState()
		{
			stateStart = 0;
		}

		public virtual string StateName() => GetType().ToString();

		public virtual string StateTime()
		{
			return GetTimeString( stateStart );
		}

		protected string GetTimeString( float s ) => GetTimeString( TimeSpan.FromSeconds( s ) );

		protected string GetTimeString( TimeSpan ts )
		{
			var minutes = ts.Minutes;
			var seconds = ts.Seconds;
			var milliseconds = ts.Milliseconds / 10; // we care only about the first two digits
			return $"{minutes:D2}:{seconds:D2}.{milliseconds:D2}";
		}

		public virtual void Tick() { }

		protected void SetState( BaseGameState newState )
		{
			(Game.Current as Game).CurrentState = newState;
		}

		protected int GetPlayerCount() => Client.All.Count();

		public virtual void OnKill( Client attackerClient, Client victimClient ) { }

		public virtual void OnDeath( Client cl ) { }

		public virtual void OnPlayerJoin( Client cl ) { }

		public virtual void OnPlayerLeave( Client cl ) { }
	}
}
