using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace MidAir.GameStates
{
	public class GameFinishedState : BaseGameState
	{
		public override string StateName() => "Post-game";

		private RealTimeUntil stateEnds;

		public GameFinishedState()
		{
			stateEnds = 15;

			if ( MidAirGlobal.DebugMode )
				stateEnds = 1000;
		}

		public override string StateTime() => GetTimeString( stateEnds );

		public override void Tick()
		{
			base.Tick();

			if ( stateEnds < 0 )
			{
				Dictionary<int, int> mapVotePairs = new();
				foreach ( var mapVote in Game.Instance.MapVotes )
				{
					if ( !mapVotePairs.ContainsKey( mapVote.MapIndex ) )
						mapVotePairs.Add( mapVote.MapIndex, 0 );

					mapVotePairs[mapVote.MapIndex]++;
				}

				var sortedMapVotePairs = from entry in mapVotePairs orderby entry.Value descending select entry;
				if ( sortedMapVotePairs.Count() == 0 )
				{

					Global.ChangeLevel( MidAirGlobal.GetMaps()[0] );
				}

				var votedMap = sortedMapVotePairs.First();
				Global.ChangeLevel( MidAirGlobal.GetMaps()[votedMap.Key] );
			}
		}
	}
}
