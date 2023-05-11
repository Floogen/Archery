using Archery.Framework.Models.Weapons;
using Archery.Framework.Objects.Items;
using Archery.Framework.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System.Collections.Generic;
using System.Linq;

namespace Archery.Framework.Objects.Projectiles
{
    internal class ArrowProjectile : BasicProjectile
    {
        // TODO: Make these content pack values
        private static Rectangle _arrowCollisionBox = new Rectangle(0, 0, 4, 4);

        private const int VANILLA_STONE_SPRITE_ID = 390;

        private WeaponModel _weaponModel;
        private AmmoModel _ammoModel;
        private Farmer _owner;

        private int _tailTimer;
        private Queue<Vector2> _tail;

        private float _startingAlpha;
        private int _lightId;

        public ArrowProjectile(WeaponModel weaponModel, AmmoModel ammoModel, Farmer owner, int damageToFarmer, int bouncesTillDestruct, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, bool spriteFromObjectSheet = false, onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, VANILLA_STONE_SPRITE_ID, bouncesTillDestruct, ammoModel is not null && ammoModel.Tail is not null ? ammoModel.Tail.Amount : 0, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, owner, spriteFromObjectSheet, collisionBehavior)
        {
            _weaponModel = weaponModel;
            _ammoModel = ammoModel;
            _owner = owner;

            _tailTimer = 0;
            _tail = new Queue<Vector2>();

            _startingAlpha = 1f;

            base.maxTravelDistance.Value = _ammoModel.MaxTravelDistance;
        }

        public override bool update(GameTime time, GameLocation location)
        {
            if (Game1.IsMasterGame && base.hostTimeUntilAttackable > 0f)
            {
                base.hostTimeUntilAttackable -= (float)time.ElapsedGameTime.TotalSeconds;
                if (base.hostTimeUntilAttackable <= 0f)
                {
                    base.ignoreMeleeAttacks.Value = false;
                    base.hostTimeUntilAttackable = -1f;
                }
            }

            // TODO: Handle light as a property (lightId is private, may want local value)
            /*
            if ((bool)base.light)
            {
                if (!base.hasLit)
                {
                    base.hasLit = true;
                    base.lightID = Game1.random.Next(int.MinValue, int.MaxValue);
                    Game1.currentLightSources.Add(new LightSource(4, base.position + new Vector2(32f, 32f), 1f, new Color(0, 65, 128), base.lightID, LightSource.LightContext.None, 0L));
                }
                else
                {
                    _ = (Vector2)base.position;
                    Utility.repositionLightSource(base.lightID, base.position + new Vector2(32f, 32f));
                }
            }
            */

            base.rotation += base.rotationVelocity.Value;
            base.travelTime += time.ElapsedGameTime.Milliseconds;
            if (base.scaleGrow.Value != 0f)
            {
                base.localScale += base.scaleGrow.Value;
            }

            Vector2 old_position = base.position.Value;
            base.updatePosition(time);

            // Update the arrow tail
            updateTail(time);

            base.travelDistance += (old_position - base.position.Value).Length();
            if (base.maxTravelDistance.Value >= 0)
            {
                if (base.travelDistance > base.maxTravelDistance.Value - 128)
                {
                    // Fade arrows if starting to go past travel distance
                    _startingAlpha = (base.maxTravelDistance.Value - base.travelDistance) / 128f;
                }

                if (base.travelDistance >= base.maxTravelDistance.Value)
                {
                    if (base.hasLit)
                    {
                        // TODO: Handle removing light source
                        //Utility.removeLightSource(base.lightID);
                    }

                    return true;
                }
            }

            if (this.isColliding(location) && (base.travelTime > 100 || base.ignoreTravelGracePeriod.Value))
            {
                if (base.bouncesLeft.Value <= 0)
                {
                    return this.behaviorOnCollision(location);
                }

                base.bouncesLeft.Value--;
                bool[] array = Utility.horizontalOrVerticalCollisionDirections(this.getBoundingBox(), base.theOneWhoFiredMe.Get(location), projectile: true);
                if (array[0])
                {
                    base.xVelocity.Value = 0f - base.xVelocity.Value;
                }
                if (array[1])
                {
                    base.yVelocity.Value = 0f - base.yVelocity.Value;
                }
            }

            return false;
        }

        public bool behaviorOnCollision(GameLocation location)
        {
            if (base.hasLit)
            {
                // TODO: Handle removing light source
                //Utility.removeLightSource(base.lightID);
            }

            foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(this.getBoundingBox()))
            {
                if (location.terrainFeatures.ContainsKey(tile) && !location.terrainFeatures[tile].isPassable())
                {
                    base.behaviorOnCollisionWithTerrainFeature(location.terrainFeatures[tile], tile, location);
                    return true;
                }

                if (base.damagesMonsters.Value)
                {
                    NPC i = location.doesPositionCollideWithCharacter(this.getBoundingBox());
                    if (i is not null && i.IsMonster)
                    {
                        this.behaviorOnCollisionWithMonster(i, location);
                        return true;
                    }
                }
            }

            // Note: behaviorOnCollisionWithOther handles collisions with walls / barriers, will want to override
            base.behaviorOnCollisionWithOther(location);

            // Play ammo impact sound
            Toolkit.PlaySound(_ammoModel.ImpactSound, _ammoModel.Id, base.position.Value);

            return true;
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            if (n is not Monster)
            {
                return;
            }

            // See if the ammo should break
            var playerLuckChance = Utility.Clamp(Game1.player.LuckLevel / 10f, 0f, 1f) + Game1.player.DailyLuck;
            if (_ammoModel.CanBreak() && (_ammoModel.ShouldAlwaysBreak() || Game1.random.NextDouble() < _ammoModel.BreakChance - playerLuckChance))
            {
                // Draw debris based on ammo's sprite
                if (_ammoModel.Debris is not null)
                {
                    Game1.createRadialDebris(location, _ammoModel.TexturePath, _ammoModel.Debris.Source, (int)(base.position.X + 32f) / 64, (int)(base.position.Y + 32f) / 64, _ammoModel.Debris.Amount);
                }
            }
            else
            {
                // Drop the ammo
                Game1.createItemDebris(Arrow.CreateInstance(_ammoModel), n.getStandingPosition(), n.FacingDirection, location);
            }

            // Damage the monster
            location.damageMonster(n.GetBoundingBox(), this.damageToFarmer.Value, this.damageToFarmer.Value + 1, isBomb: false, _weaponModel.Knockback, 0, 0f, 1f, triggerMonsterInvincibleTimer: false, (base.theOneWhoFiredMe.Get(location) is Farmer) ? (base.theOneWhoFiredMe.Get(location) as Farmer) : Game1.player);
        }

        public override bool isColliding(GameLocation location)
        {
            var collisionBox = this.getBoundingBox();
            foreach (var monster in location.characters)
            {
                if (monster.IsMonster is false)
                {
                    continue;
                }

                if (monster.GetBoundingBox().Intersects(collisionBox))
                {
                    return true;
                }
            }

            return base.isColliding(location);
        }

        public override Rectangle getBoundingBox()
        {
            Vector2 pos = base.position.Value;

            float current_scale = base.localScale * 4f;
            int damageSizeWidth = (int)(_arrowCollisionBox.Width * current_scale);
            int damageSizeHeight = (int)(_arrowCollisionBox.Height * current_scale);

            return new Rectangle((int)pos.X - damageSizeWidth / 2, (int)pos.Y - damageSizeHeight / 2, damageSizeWidth, damageSizeHeight);
        }

        // Re-implementing this class, as it is private
        private void updateTail(GameTime time)
        {
            _tailTimer -= time.ElapsedGameTime.Milliseconds;
            if (_tailTimer <= 0)
            {
                _tailTimer = _ammoModel.Tail is not null ? _ammoModel.Tail.SpawnIntervalInMilliseconds : 50;
                _tail.Enqueue(this.position);
                if (_tail.Count > base.tailLength.Value)
                {
                    _tail.Dequeue();
                }
            }
        }


        public override void draw(SpriteBatch b)
        {
            var ammoSprite = _ammoModel.ProjectileSprite;
            if (ammoSprite is null)
            {
                return;
            }

            // Draw the arrow trail / tail
            float current_scale = 4f * base.localScale;
            float alpha = 1f;

            if (_ammoModel.Tail is not null)
            {
                for (int i = _tail.Count - 1; i >= 0; i--)
                {
                    b.Draw(_ammoModel.Texture, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == _tail.Count - 1) ? ((Vector2)base.position) : _tail.ElementAt(i + 1), _tail.ElementAt(i), _ammoModel.Tail.SpacingStep)), _ammoModel.Tail.Source, base.color.Value * alpha * _startingAlpha, base.rotation, _ammoModel.Tail.Offset, current_scale, SpriteEffects.None, (base.position.Y - (float)(_tail.Count - i) + 96f) / 10000f);

                    if (_ammoModel.Tail.AlphaStep is not null)
                    {
                        alpha -= _ammoModel.Tail.AlphaStep.Value;
                    }

                    if (_ammoModel.Tail.ScaleStep is not null)
                    {
                        current_scale -= (i * _ammoModel.Tail.ScaleStep.Value);
                    }
                }
            }

            // Draw the arrow
            b.Draw(_ammoModel.Texture, Game1.GlobalToLocal(Game1.viewport, base.position), ammoSprite.Source, base.color.Value * _startingAlpha, base.rotation, ammoSprite.Source.Size.ToVector2(), 4f * base.localScale, SpriteEffects.None, (base.position.Y + 96f) / 10000f);

            // TODO: Make this a config / button option
            //Framework.Utilities.Toolkit.DrawHitBox(b, getBoundingBox());
        }
    }
}
