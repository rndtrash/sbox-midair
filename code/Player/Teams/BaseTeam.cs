﻿using Sandbox;
using System.Collections.Generic;

namespace Instagib.Teams
{
	public partial class BaseTeam : BaseNetworkable
	{
		[Net] public List<Player> Players { get; set; }

		public virtual int GetTeamColor()
		{
			return -1;
		}

		public virtual string GetTeamName()
		{
			return "Team";
		}

		public bool AreFriendly( Player a, Player b ) { return true; }
		public bool AreEnemies( Player a, Player b ) { return !AreFriendly( a, b ); }
	}
}
