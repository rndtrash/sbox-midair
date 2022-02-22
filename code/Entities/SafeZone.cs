using Sandbox;

namespace MidAir.Entities
{
	[Library("midair_safezone")]
	public class SafeZone : BaseTrigger
	{
		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player p )
				p.IsInSafeZone = true;
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			if ( other is Player p )
				p.IsInSafeZone = false;
		}
	}
}
