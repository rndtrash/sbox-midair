using Sandbox;

namespace MidAir.Teams
{
	public static class ClientExtensions
	{
		public static BaseTeam GetTeam( this Client client )
		{
			if ( client.Pawn is Player player )
			{
				return player.Team;
			}

			return null;
		}
	}
}
