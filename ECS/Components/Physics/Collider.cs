﻿using System;
using Nez.Physics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public abstract class Collider
	{
		public Entity entity;
		/// <summary>
		/// position is added to entity.position to get the final position for the collider
		/// </summary>
		public Vector2 position;
		Vector2 _origin;
		public Vector2 origin
		{
			get { return _origin; }
			set
			{
				if( _origin != value )
				{
					unregisterColliderWithPhysicsSystem();
					_origin = value;
					registerColliderWithPhysicsSystem();
				}
			}
		}

		/// <summary>
		/// helper property for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 originNormalized
		{
			get { return new Vector2( origin.X / width, origin.Y / height ); }
			set { origin = new Vector2( value.X * width, value.Y * height ); }
		}

		/// <summary>
		/// if this collider is a trigger it will not cause collisions but it will still trigger events
		/// </summary>
		public bool isTrigger;

		/// <summary>
		/// physicsLayer can be used as a filter when dealing with collisions.
		/// </summary>
		public int physicsLayer;
		public abstract float width { get; set; }
		public abstract float height { get; set; }

		public virtual Rectangle bounds
		{
			get
			{
				// TODO: cache this and block with a dirty flag so that we only update when necessary
				return RectangleExtension.fromFloats( entity.position.X + position.X - origin.X, entity.position.Y + position.Y - origin.Y, width, height );
			}
		}

		protected bool _isParentEntityAddedToScene;


		public Collider()
		{}


		public bool collidesWithAtPosition( Collider collider, Vector2 position )
		{
			var savedPosition = entity.position;
			entity.position = position;

			var result = collidesWith( collider );

			entity.position = savedPosition;
			return result;
		}


		public bool collidesWith( Collider collider )
		{
			if( collider is BoxCollider )
				return collidesWith( collider as BoxCollider );
			else if( collider is CircleCollider )
				return collidesWith( collider as CircleCollider );
			else if( collider is MultiCollider )
				return collidesWith( collider as MultiCollider );
			else
				throw new NotImplementedException( "Collisions against the collider type are not implemented!" );
		}


		public abstract bool collidesWith( Vector2 from, Vector2 to );
		public abstract bool collidesWith( BoxCollider boxCollider );
		public abstract bool collidesWith( CircleCollider circle );
		public abstract bool collidesWith( MultiCollider list );



		/// <summary>
		/// Called when the parent entity is added to a scene
		/// </summary>
		public virtual void onEntityAddedToScene()
		{
			_isParentEntityAddedToScene = true;
			registerColliderWithPhysicsSystem();
		}


		/// <summary>
		/// Called when the parent entity is removed from a scene
		/// </summary>
		public virtual void onEntityRemovedFromScene()
		{
			unregisterColliderWithPhysicsSystem();
			_isParentEntityAddedToScene = false;
		}


		/// <summary>
		/// the parent Entity will call this at various times (when added to a scene, enabled, etc)
		/// </summary>
		public virtual void registerColliderWithPhysicsSystem()
		{
			// entity could be null if proper such as origin are changed before we are added to an Entity
			if( _isParentEntityAddedToScene )
				entity.scene.physics.addCollider( this );
		}


		/// <summary>
		/// the parent Entity will call this at various times (when removed from a scene, disabled, etc)
		/// </summary>
		public virtual void unregisterColliderWithPhysicsSystem()
		{
			if( _isParentEntityAddedToScene )
				entity.scene.physics.removeCollider( this, true );
		}


		public virtual void debugRender( Graphics graphics )
		{
			graphics.drawHollowRect( bounds, Color.IndianRed );
		}

	}
}

