﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Instagib.UI.Menus
{
	public class RailgunRender : Panel
	{
		private ScenePanel scene;
		Angles CamAngles;

		private Vector2 renderSize = new( 800, 600 );

		public RailgunRender()
		{
			StyleSheet.Load( "/Code/UI/Menus/RailgunRender.scss" );

			CamAngles = new Angles( 0, 0.0f, 0 );

			using ( SceneWorld.SetCurrent( new SceneWorld() ) )
			{
				SceneObject.CreateModel( "weapons/railgun/models/wpn_qc_railgun.vmdl", new Transform( new( 0, 17f, 0f ) ) );
				// SceneObject.CreateModel( "models/citizen/citizen.vmdl", new Transform( new( 0, 0f, -64f ) ) );

				var lightStrength = 2000.0f;
				var lightRadius = 128.0f;
				var lightDist = 64.0f;

				Light.Point( Vector3.Up * lightDist, lightRadius, Color.White * lightStrength );
				Light.Point( Vector3.Left * lightDist, lightRadius, Color.White * lightStrength );
				Light.Point( Vector3.Right * lightDist, lightRadius, Color.White * lightStrength );
				Light.Point( Vector3.Down * lightDist, lightRadius, Color.White * lightStrength );

				scene = Add.ScenePanel( SceneWorld.Current, Vector3.Up * 10 + CamAngles.Direction * -50, CamAngles.ToRotation(), 45 );
				scene.Style.Width = Length.Percent( 100 );
				scene.Style.Height = Length.Percent( 100 );
			}
		}

		public override void OnDeleted()
		{
			base.OnDeleted();

			scene?.Delete();
			scene = null;
		}

		public override void OnButtonEvent( ButtonEvent e )
		{
			if ( e.Button == "mouseleft" )
			{
				SetMouseCapture( e.Pressed );
			}

			base.OnButtonEvent( e );
		}

		public override void Tick()
		{
			base.Tick(); 

			if ( HasMouseCapture )
			{
				CamAngles.pitch += Mouse.Delta.y;
				CamAngles.yaw -= Mouse.Delta.x;
			}
			else
			{
				CamAngles.yaw += Time.Delta * 45f;
				CamAngles.pitch = CamAngles.pitch.LerpTo( 0, 10f * Time.Delta );
			}

			CamAngles.yaw %= 360f;
			CamAngles.pitch = CamAngles.pitch.Clamp( -90f, 90f );

			scene.CameraPosition = CamAngles.Direction * -75;
			scene.CameraRotation = CamAngles.ToRotation();
		}
	}
}
