﻿using Sandbox;
using System;
using System.Linq;

namespace Instagib
{
	public partial class InstagibPlayer : Player
	{
		private DamageInfo lastDamageInfo;
		
		public Team Team { get; set; }
		
		private Vector3 lastCameraPos = Vector3.Zero;
		private Rotation lastCameraRot = Rotation.Identity;
		private float lastHudOffset;

		public InstagibPlayer()
		{
			Inventory = new BaseInventory( this );
		}
		
		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new InstagibController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera() { FieldOfView = 90 };

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Inventory.Add( new Railgun(), true );
			
			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );
		}

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;

			BecomeRagdollOnClient( Velocity, 0 );
			Camera = new SpectateRagdollCamera();
		}

		public override void TakeDamage( DamageInfo info )
		{
			base.TakeDamage( info );
			lastDamageInfo = info;
		}

		public override void PostCameraSetup( ref CameraSetup setup )
		{
			base.PostCameraSetup( ref setup );

			if ( lastCameraRot == Rotation.Identity )
				lastCameraRot = setup.Rotation;

			var angleDiff = Rotation.Difference( lastCameraRot, setup.Rotation );
			var angleDiffDegrees = angleDiff.Angle();
			var allowance = 10.0f;

			if ( angleDiffDegrees > allowance )
				lastCameraRot = Rotation.Lerp( lastCameraRot, setup.Rotation, 1.0f - (allowance / angleDiffDegrees) );

			if ( setup.Viewer != null )
				AddCameraEffects( ref setup );
		}

		float walkBob = 0;
		float lean = 0;
		float fov = 0;

		private void AddCameraEffects( ref CameraSetup setup )
		{
			var speed = Velocity.Length.LerpInverse( 0, 320 );
			var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

			var left = setup.Rotation.Left;
			var up = setup.Rotation.Up;

			if ( GroundEntity != null )
			{
				walkBob += Time.Delta * 25.0f * speed;
			}

			setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
			setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

			// Camera lean
			lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.015f, Time.Delta * 15.0f );

			var appliedLean = lean;
			appliedLean += MathF.Sin( walkBob ) * speed * 0.2f;
			setup.Rotation *= Rotation.From( 0, 0, appliedLean );

			speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

			fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 2.0f );

			setup.FieldOfView += fov;

			var tx = new Sandbox.UI.PanelTransform();
			tx.AddRotation( 0, 0, lean * -0.2f );

			var zOffset = (lastCameraPos - setup.Position).z * 2f;
			zOffset = lastHudOffset.LerpTo( zOffset, 25.0f * Time.Delta );
			tx.AddTranslateY( zOffset );

			lastHudOffset = zOffset;

			InstagibHud.CurrentHudPanel.Style.Transform = tx;
			InstagibHud.CurrentHudPanel.Style.Dirty();

			lastCameraPos = setup.Position;
		}
	}
}
