using Sandbox;

namespace MidAir.Entities
{
	[Library( "midair_goal" )]
	[Hammer.EntityTool( "Checkpoint (Ascend only)", "MidAir" )]
	public partial class Checkpoint : BaseTrigger
	{
		public static Checkpoint First { get; internal set; }

		[Property] public Checkpoint Next { get; set; }
		[Property] public bool IsFirst { get; internal set; }

		public override void Spawn()
		{
			if ( Game.Instance.GameType is not GameTypes.AscendGameType )
			{
				Delete();
				return;
			}

			base.Spawn();

			Transmit = TransmitType.Always;

			if ( IsFirst )
				First = this;

			Disable();
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			
			if ( IsFirst )
				First = this;
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other is Player p && Game.Instance.GameType is GameTypes.AscendGameType a && a.Winner == null )
				a.Winner = p;
		}

		[Event("midair.start")]
		public void OnGameStart()
		{
			Enable();
		}
	}
}
