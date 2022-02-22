﻿using MidAir.Weapons;
using Sandbox;

namespace MidAir
{
	public class FirstPersonCamera : Camera
	{
		Vector3 lastPos;

		private float lastFov;

		public override void Activated()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			Position = pawn.EyePosition;
			Rotation = pawn.EyeRotation;

			lastPos = Position;
		}

		public override void Update()
		{
			var pawn = Local.Pawn;
			if ( pawn == null ) return;

			var eyePos = pawn.EyePosition;
			if ( eyePos.Distance( lastPos ) < 250 )
			{
				Position = Vector3.Lerp( eyePos.WithZ( lastPos.z ), eyePos, 20.0f * Time.Delta );
			}
			else
			{
				Position = eyePos;
			}

			Rotation = pawn.EyeRotation;

			FieldOfView = PlayerSettings.Fov;
			if ( pawn.ActiveChild is RocketLauncher { IsZooming: true } )
			{
				FieldOfView = PlayerSettings.ZoomedFov;
			}

			FieldOfView = lastFov.LerpTo( FieldOfView, 20 * Time.Delta );
			lastFov = FieldOfView;

			ZNear = 3;
			ZFar = 20000;

			Viewer = pawn;
			lastPos = Position;
		}
	}
}
