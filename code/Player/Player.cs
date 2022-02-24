using MidAir.UI;
using MidAir.Weapons;
using Sandbox;
using Sandbox.Component;
using System.Linq;
using Event = Sandbox.Event;

namespace MidAir
{
	public partial class Player : Sandbox.Player
	{
		public float DashTimeout => 0.5f;
		public float DashVelocity => 350f;
		public float MinimalDistantionAboveGround => 70f;

		[Net, Local] private TimeSince TimeSinceSpawn { get; set; }
		[Net, Local] public TimeSince LastDash { get; set; }
		[Net, Local] public TimeSince LastHit { get; set; }
		public bool IsSpawnProtected => TimeSinceSpawn < 3;
		[Net] public bool IsInSafeZone { get; set; }
		[Net, Local] public bool AirControl { get; set; } = true;
		[Net, Local] public Entities.Checkpoint CurrentCheckpoint { get; set; }

		private Particles speedLines;

		//
		// Stats used for medals
		//
		public int CurrentStreak { get; set; }
		public float CurrentDamageDealt { get; set; }

		public Clothing.Container Clothing = new();

		public Player()
		{
			Inventory = new BaseInventory( this );
		}

		public Player( Client cl ) : this()
		{
			Clothing.LoadFromClient( cl );
		}

		public override void Spawn()
		{
			base.Spawn();
			EnableLagCompensation = true;
		}

		public override void Respawn()
		{
			Event.Run( "playerRespawn" );

			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new PlayerController();
			Animator = new StandardPlayerAnimator();
			CameraMode = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;
			Transmit = TransmitType.Always;

			Tags.Add( "player" );

			Clothing.DressEntity( this );

			Inventory.DeleteContents();
			Inventory.Add( new RocketLauncher(), true );

			CurrentStreak = 0;
			CurrentDamageDealt = 0;
			TimeSinceSpawn = 0;

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );
			SimulateActiveChild( cl, ActiveChild );

			if ( Position.z < -2500 )
			{
				TakeDamage( DamageInfo.Generic( 10000 ) );
			}

			if ( LastDash >= DashTimeout && Input.Down( InputButton.Attack2 ) ) // TODO: disable dash if the pawn gets hit by a rocket
			{
				var rot = EyeRotation.Angles().WithPitch( 0 ).ToRotation();
				Vector3[][] rayDirs = new Vector3[][]
				{
					new Vector3[]
					{
						rot.Left,
						rot.Right
					},
					new Vector3[]
					{
						rot.Forward,
						rot.Backward
					}
				};

				var controller = Controller as PlayerController;
				var startPos = Position + Vector3.Up * controller.BodyHeight / 2;
				var newVelocity = Vector3.Zero;
				foreach ( var oppositeDirs in rayDirs )
					foreach ( var dir in oppositeDirs )
					{
						var r = Trace.Ray( new( startPos, dir ), controller.BodyGirth * 0.75f ).WorldOnly().Run();
						if ( r.Hit )
						{
							newVelocity += r.Normal;
							newVelocity = newVelocity.WithZ( 0.25f );
							break;
						}
					}

				if ( newVelocity.IsNearZeroLength && GroundEntity is not null && Velocity.WithZ( 0 ).Length < DashVelocity )
				{
					newVelocity = Input.Left * EyeRotation.Left + (Input.Forward != 0 || Input.Left != 0 ? Input.Forward : 1) * EyeRotation.Forward + Vector3.Up * 0.5f;
				}

				if ( !newVelocity.IsNearZeroLength )
				{
					LastDash = 0;
					AirControl = true;

					controller.ClearGroundEntity();
					Velocity += newVelocity.Normal * DashVelocity;
					controller.JumpEffects();
				}
			}

			if ( !AirControl && GroundEntity is not null )
				AirControl = true;

			var glow = Components.GetOrCreate<Glow>();
			if ( cl == Local.Client )
			{
				glow.Active = false;

				//
				// Speed lines
				//
				if ( IsClient )
				{
					if ( Velocity.Length > 500 )
					{
						speedLines ??= Particles.Create( "particles/speed_lines.vpcf" );
						var perlinStrength = Velocity.Length.LerpInverse( 500, 700 ) * 0.5f;
						_ = new Sandbox.ScreenShake.Perlin( 1.0f, 1.0f, perlinStrength, 2.0f * perlinStrength );
					}
					else
					{
						speedLines?.Destroy();
						speedLines = null;
					}
				}

				return;
			}

			glow.Active = true;
			glow.RangeMin = -32;
			glow.RangeMax = 4096;
		}

		[Event.Tick.Client]
		public void OnClientTick()
		{
			// Outlines are per-client. They can be disabled and recolored at each clients' will
			// Note that this isn't synced between clients at all

			if ( IsServer )
				return;

			var hsvColor = Color.Red.ToHsv();

			if ( IsFriendly( Local.Client ) )
				hsvColor = Color.Cyan.ToHsv();

			hsvColor.Value = 1.0f;
			hsvColor.Saturation = 1.0f;
			Components.GetOrCreate<Glow>().Color = hsvColor.ToColor();
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Velocity = Vector3.Zero;

			var lookAtCamera = new LookAtCamera();
			CameraMode = lookAtCamera;

			lookAtCamera.TargetEntity = LastAttacker;
			lookAtCamera.Origin = EyePosition;
			lookAtCamera.Rotation = EyeRotation;
			lookAtCamera.TargetOffset = Vector3.Up * 64f;

			Inventory.DeleteContents();

			EnableDrawing = false;
			EnableAllCollisions = false;

			GibParticles( To.Everyone, Position + (Vector3.Up * (8)) );
			ShakeScreen( To.Everyone, Position );

			var childrenCopy = Children.ToList();
			foreach ( var child in childrenCopy )
			{
				if ( !child.Tags.Has( "clothes" ) ) continue;
				if ( child is not ModelEntity e ) continue;

				e.Delete();
			}
		}

		[ClientRpc]
		private void GibParticles( Vector3 position )
		{
			_ = Particles.Create( "particles/gib_blood.vpcf", position );
			Sound.FromWorld( "gibbing", position );
		}

		[ClientRpc]
		private void ShakeScreen( Vector3 position )
		{
			float strengthMul = 10f;
			float strengthDist = 512f;

			float strength = strengthDist - Local.Pawn.Position.Distance( position ).Clamp( 0, strengthDist );
			strength /= strengthDist;
			strength *= strengthMul;

			_ = new Sandbox.ScreenShake.Perlin( 1f, 0.75f, strength );
		}

		[ClientRpc]
		public void OnDamageOther( Vector3 pos, float amount )
		{
			using ( Prediction.Off() )
			{
				Log.Trace( "Playing kill sound" );
				PlaySound( "kill" );
				Hitmarker.CurrentHitmarker.OnHit();
			}
		}

		public bool IsMidAir()
		{
			return !IsInSafeZone
				&& !Trace.Ray( new Ray( Position, Vector3.Down ), MinimalDistantionAboveGround ).WorldOnly().Run().Hit;
		}

		public override void TakeDamage( DamageInfo info )
		{
			if ( IsSpawnProtected || info.Flags.HasFlag( DamageFlags.PhysicsImpact ) )
				return;

			if ( IsFriendly( info.Attacker ) )
				return;

			if ( LifeState == LifeState.Dead || Health <= 0 )
				return;

			LastAttacker = info.Attacker;
			LastAttackerWeapon = info.Weapon;

			this.ProceduralHitReaction( info );

			if ( IsServer )
			{
				if ( info.Attacker is not Player attacker ) // if the damage was caused by `kill` command
				{
					Health = 0f;
					OnKilled();
					return;
				}

				if ( info.Attacker != this ) // ...or we were hit by another player
				{
					attacker.OnDamageOther( To.Single( attacker ), info.Position, info.Damage );

					if ( IsMidAir() )
					{
						Health = 0f;
						OnKilled();
						return;
					}

					LastDash = -DashTimeout; // Timing out by twice the dash timeouts
					AirControl = false; // Disabling until the player touches some grass or does a dash
				}

				PlaySound( "rocket_jump" ); // else it was probably a rocket jump
			}
		}
	}
}
