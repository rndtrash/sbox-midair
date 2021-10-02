﻿using Instagib.UI;
using Sandbox;

namespace Instagib
{
	partial class Railgun
	{
		[Net, Predicted] public bool isGrappling { get; set; }
		[Net] public GrappleHookEntity grappleHookEntity { get; set; }
		[Net, Predicted] private TimeSince TimeSinceLastGrapple { get; set; }

		private Particles grappleParticles;

		private float GrappleCooldown => 0.5f;
		private float HookSpeed => 5f;
		private float PullStrength => 48f;
		private float BoostStrength => 4f;
		private float AntiVelocityScale => 1f;
		private float MaxDistance => 1250;
		private float GrappleTraceRadius => 64f;
		private float HeightJumpBoost => 128f;

		public void GrappleSimulate( Client owner )
		{
			if ( Input.Pressed( InputButton.Duck ) && !isGrappling && TimeSinceLastGrapple > GrappleCooldown )
				DeployGrapple();
			else if ( !Input.Down( InputButton.Duck ) && isGrappling )
				RemoveGrapple();

			if ( isGrappling && grappleHookEntity.IsValid() )
			{
				if ( Owner is Player { Controller: PlayerController controller } player )
				{
					player.GroundEntity = null;
					controller.ClearGroundEntity();
				}

				Vector3 playerVel = Owner.Velocity;
				Vector3 playerLookDir = Owner.EyeRot.Forward;
				Owner.Velocity += playerLookDir * BoostStrength;

				Vector3 playerLatchDir = (Owner.Position - grappleHookEntity.Position).Normal;
				Owner.Velocity -= playerLatchDir * PullStrength;

				grappleHookEntity.Rotation = Rotation.LookAt( playerLatchDir );

				float projLength = playerVel.Dot( playerLatchDir );
				if ( projLength > 0 )
				{
					Vector3 projVector = projLength * playerLatchDir;
					Owner.Velocity -= AntiVelocityScale * projVector;
				}
			}
		}

		[Event.Frame]
		public void OnFrame()
		{
			if ( Local.Client != Owner.GetClientOwner() )
				return;
			
			var tr = GrappleTrace( out var calcEndPos, out bool isExtendedRay );
			var state = GrappleIndicator.State.Default;
			if ( TimeSinceLastGrapple < GrappleCooldown || !tr.Hit )
			{
				state = GrappleIndicator.State.Cooldown;
			}
			if ( isGrappling )
			{
				state = GrappleIndicator.State.Active;
			}
			
			GrappleIndicator.SetCanGrapple( state );

			if ( isGrappling && grappleHookEntity.IsValid() )
			{
				GrappleIndicator.MoveTo( grappleHookEntity.Position, false );
			}
			else
			{
				if ( tr.Hit && isExtendedRay ) 
					GrappleIndicator.MoveTo( calcEndPos );
				else
					GrappleIndicator.MoveToCenter();
			}
		}

		private TraceResult GrappleTrace( out Vector3 calcEndPos, out bool isExtendedRay )
		{
			var tr = Trace.Ray( Owner.EyePos + Owner.EyeRot.Forward * GrappleTraceRadius, Owner.EyePos + Owner.EyeRot.Forward * MaxDistance )
				.Ignore( this )
				.Ignore( Owner )
				.WorldAndEntities()
				.HitLayer( CollisionLayer.Player, false ) // Why the fuck doesn't this work?
				.Run();
			isExtendedRay = false;
			calcEndPos = tr.EndPos;
			if ( tr.Hit && tr.Entity is not Player )
				return tr;
			
			var trExtended = Trace.Ray( Owner.EyePos + Owner.EyeRot.Forward * GrappleTraceRadius, Owner.EyePos + Owner.EyeRot.Forward * MaxDistance )
				.Ignore( this )
				.Ignore( Owner )
				.WorldAndEntities()
				.Radius( GrappleTraceRadius )
				.HitLayer( CollisionLayer.Player, false ) // Why the fuck doesn't this work???? x2
				.Run();
			isExtendedRay = true;
			calcEndPos = trExtended.EndPos - trExtended.Normal * GrappleTraceRadius;
			if ( trExtended.Hit && trExtended.Entity is not Player && ( trExtended.EndPos - Owner.EyePos ).Length > 96f )
				return trExtended;

			calcEndPos = Owner.EyePos + Owner.EyeRot.Forward * MaxDistance;
			return new TraceResult() { Hit = false, EndPos = calcEndPos };
		}

		protected virtual void DeployGrapple()
		{
			var tr = GrappleTrace( out var calcEndPos, out _ );
			if ( tr.Hit && tr.Entity != null )
			{
				isGrappling = true;

				using ( Prediction.Off() )
				{
					// PlaySound( "grapple" );

					if ( Owner is Player { Controller: PlayerController controller } player )
					{
						if ( controller.GroundEntity != null )
						{
							player.Velocity += Vector3.Up * HeightJumpBoost;
						}

						player.GroundEntity = null;
						controller.ClearGroundEntity();
					}

					if ( Host.IsServer )
					{
						grappleHookEntity = new()
						{
							Position = tr.StartPos,
							Target = calcEndPos,
							HookSpeed = HookSpeed,
							Rotation = Owner.EyeRot,
							Parent = tr.Entity,
							Owner = Owner
						};
						grappleHookEntity.Spawn();

						var rope = Particles.Create( "particles/grapple_beam.vpcf" );
						rope.SetEntity( 0, grappleHookEntity, Vector3.Backward * 32 );
						rope.SetEntity( 1, Owner, Vector3.Up * 32 );

						grappleParticles = rope;
					}
				}
			}
			else
			{
				using ( Prediction.Off() )
					PlaySound( "player_use_fail" );
			}
		}

		protected virtual void RemoveGrapple()
		{
			isGrappling = false;
			TimeSinceLastGrapple = 0;

			if ( IsServer )
			{
				DeleteHook();
			}

			if ( IsClient )
			{
				GrappleIndicator.MoveToCenter();
			}
		}

		protected virtual void DeleteHook()
		{
			if ( !IsServer )
				return;

			if ( grappleHookEntity != null && grappleHookEntity.IsValid() )
			{
				grappleHookEntity.Delete();
			}

			grappleHookEntity = null;

			grappleParticles?.Destroy( true );
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			DeleteHook();
		}
	}
}
