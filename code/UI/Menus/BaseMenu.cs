using Sandbox;
using Sandbox.UI;

namespace MidAir.UI.Menus
{
	public class BaseMenu : Menu
	{
		public BaseMenu() : base()
		{
			AddClass( "menu" );
			StyleSheet.Load( "/Code/UI/Menus/BaseMenu.scss" );
		}

		public override void Tick()
		{
			base.Tick();
			SetClass( "open", Input.Down( InputButton.Score ) );
		}

		public void Toggle()
		{
			MidAirHud.currentHud.SetCurrentMenu( new MainMenu() );
		}

		public void ShowSettings()
		{
			MidAirHud.currentHud.SetCurrentMenu( new SettingsMenu() );
		}
	}
}
