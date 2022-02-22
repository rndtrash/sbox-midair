using Sandbox;

namespace MidAir.Entities
{
	[Library( "midair_goal" )]
	public partial class Goal : BaseTrigger
	{
		public static Goal Instance { get; set; }

		public override void Spawn()
		{
			if ( Game.Instance.GameType.LibraryName != "gametype_ascend" )
			{
				Delete();
				return;
			}

			base.Spawn();

			Transmit = TransmitType.Always;

			Instance = this;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();

			Instance = this;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player p && Game.Instance.GameType is GameTypes.AscendGameType a && a.Winner == null )
				a.Winner = p;
		}
	}
}
