using Sandbox;
using System;
using System.Collections.Generic;
using Trace = Sandbox.Trace;

namespace MidAir.Weapons
{
	[Library( "rocketlauncher" )]
	partial class RocketLauncher : BaseWeapon
	{
		public class Rocket : ModelEntity
		{
			public float FlightTime => 5f;
			public float Speed => 1000f;
			public float Range => 128f;
			public TimeUntil Explode { get; internal set; }

			public override void Spawn()
			{
				base.Spawn();

				SetModel( "weapons/rocketlauncher/models/rocket.vmdl" );

				Explode = FlightTime;
			}

			[Event.Tick.Server]
			public void OnTick()
			{
				if ( LifeState != LifeState.Alive )
					return;

				if ( Explode < 0 )
				{
					Die();
					return;
				}

				var velocity = Rotation.Forward * Speed;

				var start = Position;
				var end = start + velocity * Time.Delta;

				var tr = Trace.Ray( start, end )
					.UseHitboxes()
					.Ignore( this )
					.Ignore( Owner )
					.Size( 10f )
					.Run();
				
				Position = tr.EndPosition;
				if ( tr.Hit )
				{
					Die();
				}
			}

			public void Die()
			{
				Host.AssertServer();

                LifeState = LifeState.Dead;

				using ( Prediction.Off() )
				{
					Particles.Create( "particles/explosion.vpcf", Position );
				}

				foreach ( var overlap in FindInSphere( Position, Range ) )
				{
					if ( overlap == this ) continue;
					if ( overlap is not ModelEntity ent || !ent.IsValid() ) continue;
					if ( ent.LifeState != LifeState.Alive || !ent.PhysicsBody.IsValid() || ent.IsWorld ) continue;

					var vec = overlap.Position - Position;
					var dist = vec.Length;
					{
						if ( ent is Player { Controller: PlayerController playerController } )
						{
							vec += Vector3.Up * playerController.BodyHeight / 2;
						}
					}
					var dir = vec.Normal;

					var distanceFactor = 1.0f - Math.Clamp( dist / Range, 0, 1 );
					distanceFactor *= 0.5f;
					var force = distanceFactor * ent.PhysicsBody.Mass;
					
					if ( ent.GroundEntity != null )
					{
						ent.GroundEntity = null;
						if ( ent is Player { Controller: PlayerController playerController } )
							playerController.ClearGroundEntity();
						else if ( ent is Rocket r )
							r.Die();
					}

					ent.ApplyAbsoluteImpulse( dir * force );
				}

				Delete();
			}
		}

		public ViewModel ViewModel => (ViewModelEntity as ViewModel) ?? default;

		public override string ViewModelPath => "weapons/rocketlauncher/models/rocketlauncher.vmdl";
		public override float PrimaryRate => 0.75f;

		private Particles beamParticles;
		public bool IsZooming { get; private set; }

		public override void Reload() { }

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "weapons/rocketlauncher/models/rocketlauncher.vmdl" );
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			base.SimulateAnimator( anim );
			anim.SetProperty( "holdtype", $"{3}" );
		}

		public override bool CanPrimaryAttack()
		{
			if ( !Input.Pressed( InputButton.Attack1 ) )
				return false;

			if ( Owner.Health <= 0 )
				return false;

			return base.CanPrimaryAttack();
		}

		public override bool CanSecondaryAttack() => false;

		public override void AttackSecondary() { }

		public override void AttackPrimary()
		{
			ViewModel?.OnFire();

			TimeSincePrimaryAttack = 0;

			using ( Prediction.Off() )
			{
				Owner.Client.AddInt( "totalShots" );
			}

			Shoot( Trace.Ray( new Ray( Owner.EyePosition, Owner.EyeRotation.Forward ), 32 ).WorldOnly().Run().EndPosition, Owner.EyeRotation );
		}

		private void Shoot( Vector3 pos, Rotation dir )
		{
			// Particles
			beamParticles?.Destroy( true );
			beamParticles = Particles.Create( "weapons/rocketlauncher/particles/rocketlauncher_beam.vpcf", EffectEntity,
				"muzzle", false );

			if ( !IsServer ) return;

			new Rocket()
			{
				Position = pos,
				Rotation = dir,
				Owner = this.Owner
			};
			ShootEffects();
		}

		public override void Simulate( Client owner )
		{
			base.Simulate( owner );

			IsZooming = Input.Down( InputButton.Run );
		}

		public override void BuildInput( InputBuilder owner )
		{
			if ( IsZooming )
			{
				// Set input sensitivity
				owner.ViewAngles = Angles.Lerp( owner.OriginalViewAngles, owner.ViewAngles,
					PlayerSettings.ZoomedFov / PlayerSettings.Fov );
			}
		}

		[ClientRpc]
		public virtual void ShootEffects()
		{
			Host.AssertClient();

			Sound.FromEntity( "rocketlauncher_fire", this );

			ViewModelEntity?.SetProperty( "fire", $"{true}" );
			CrosshairPanel?.CreateEvent( "onattack" );

			if ( IsLocalPawn )
			{
				_ = new Sandbox.ScreenShake.Perlin( 0.5f, 1.0f, 2.0f, 2.0f );
			}
		}

		public override void CreateViewModel()
		{
			Host.AssertClient();

			if ( string.IsNullOrEmpty( ViewModelPath ) )
				return;

			ViewModelEntity = new ViewModel();
			ViewModelEntity.Position = Position;
			ViewModelEntity.Owner = Owner;
			ViewModelEntity.EnableViewmodelRendering = true;
			ViewModelEntity.SetModel( ViewModelPath );
		}

		public override void DestroyViewModel()
		{
			ViewModelEntity?.Delete();
			ViewModelEntity = null;
		}
	}
}
