using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace MidAir.UI.Elements
{
	public class Controls : Panel
	{
		class ButtonGlyph : Image
		{
			InputButton ib;

			public ButtonGlyph( InputButton ib )
			{
				this.ib = ib;
			}

			public override void Tick()
			{
				base.Tick();

				Texture = Input.GetGlyph( ib );
			}
		}

		public Controls()
		{
			StyleSheet.Load( "/Code/UI/Elements/Controls.scss" );
			Add.Label( "Controls:", "title" );
			AddControl( "Zoom", InputButton.Run );
			AddControl( "Shoot", InputButton.Attack1 );
			AddControl( "Dash", InputButton.Attack2 );
		}

		private void AddControl( string name, InputButton ib )
		{
			var panel = Add.Panel( "control" );
			panel.Add.Label( name );
			panel.AddChild( new ButtonGlyph( ib ) );
		}
	}
}
