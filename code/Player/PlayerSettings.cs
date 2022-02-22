using System.Security;
using Sandbox;

namespace MidAir
{
	public static class PlayerSettings
	{
		public static float ZoomedFov { get; set; } = 60;
		public static float Fov { get; set; } = 90;

		public static float ViewmodelOffset { get; set; } = 0;
		public static bool ViewmodelVisible { get; set; } = true;
		public static bool ViewmodelFlip { get; set; } = false;
		public static Color CrosshairColor { get; set; } = Color.Green;
		public static float ViewTiltMultiplier { get; set; } = 1.0f;

		public static Color EnemyOutlineColor { get; set; } = Color.Red;

		public static void Load()
		{
			Host.AssertClient();
			
			Fov = Cookie.Get<float>( "MidAir.Fov", 100 );
			ViewmodelOffset = Cookie.Get<float>( "MidAir.ViewmodelOffset", 0 );
			ViewmodelVisible = Cookie.Get( "MidAir.ViewmodelVisible", true );
			ViewmodelFlip = Cookie.Get( "MidAir.ViewmodelFlip", false );
			ViewTiltMultiplier = Cookie.Get( "MidAir.ViewTiltMultiplier", 1.0f );
			CrosshairColor = Color.Parse( Cookie.Get( "MidAir.CrosshairColor", Color.Red.Hex ) ) ?? Color.Green;
			EnemyOutlineColor = Color.Parse( Cookie.Get( "MidAir.EnemyOutlineColor", Color.Red.Hex ) ) ?? Color.Red;
		}

		public static void Save()
		{
			Host.AssertClient();
			
			Cookie.Set( "MidAir.Fov", Fov );
			Cookie.Set( "MidAir.ViewmodelOffset", ViewmodelOffset );
			Cookie.Set( "MidAir.ViewmodelVisible", ViewmodelVisible );
			Cookie.Set( "MidAir.ViewmodelFlip", ViewmodelFlip );
			Cookie.Set( "MidAir.ViewTiltMultiplier", ViewTiltMultiplier );
			Cookie.Set( "MidAir.CrosshairColor", CrosshairColor.Hex );
			Cookie.Set( "MidAir.EnemyOutlineColor", EnemyOutlineColor.Hex );
		}
	}
}
