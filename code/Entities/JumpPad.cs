using Sandbox;

namespace MidAir.Entities
{
	[Library( "midair_jumppad" )]
	[Hammer.EntityTool( "Jump Pad", "MidAir" )]
	public partial class JumpPad : AnimEntity
	{
		[Property] public float Power { get; set; } = 400f;

		// TODO:
	}
}
