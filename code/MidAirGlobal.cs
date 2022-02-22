using Sandbox;
using System;

namespace MidAir
{
	internal class MidAirGlobal
	{
		[ServerVar]
		public static bool DebugMode { get; set; } = false;

		[ServerVar]
		public static int FragLimit { get; set; } = 15;

		[ServerVar]
		public static float TimeLimit { get; set; } = 60f * 10f;

		// TODO: Move to game type
		public static int SortClients( Client a, Client b )
		{
			var aKills = a.GetInt( "kills", 0 );
			var bKills = b.GetInt( "kills", 0 );

			if ( bKills > aKills )
				return 1;
			if ( aKills > bKills )
				return -1;

			return 0;
		}

		public static string[] GetMaps()
		{
			var packageTask = Package.Fetch( Global.GameTitle, true ).ContinueWith( t =>
			{
				var package = t.Result;
				try
				{
					return package.GameConfiguration.MapList.ToArray();
				}
				catch ( Exception e )
				{
					return new string[] { };
				}
			} );

			return packageTask.Result;
		}
	}
}
