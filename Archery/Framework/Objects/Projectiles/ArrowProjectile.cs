using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;

namespace Archery.Framework.Objects.Projectiles
{
    internal class ArrowProjectile : BasicProjectile
    {
        // TODO: Make these content pack values
        private static Rectangle _arrowBounds = new Rectangle(4, 7, 8, 1);
        private static Rectangle _arrowCollisionBox = new Rectangle(0, 0, 4, 4);

        public ArrowProjectile(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, firer, spriteFromObjectSheet, collisionBehavior)
        {
            base.xVelocity.Value /= 4f;
            base.yVelocity.Value /= 4f;
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

            // TODO: Reimplement arrow trail / tail logic
            //base.updateTail(time);

            base.travelDistance += (old_position - base.position.Value).Length();
            if (base.maxTravelDistance.Value >= 0)
            {
                if (base.travelDistance > base.maxTravelDistance.Value - 128)
                {
                    // TODO: Implement fading arrows
                    //base.startingAlpha = ((float)(int)base.maxTravelDistance - base.travelDistance) / 128f;
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
                        base.behaviorOnCollisionWithMonster(i, location);
                        return true;
                    }
                }
            }

            // Note: behaviorOnCollisionWithOther handles collisions with walls / barriers, will want to override
            base.behaviorOnCollisionWithOther(location);
            return true;
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            base.behaviorOnCollisionWithMonster(n, location);
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

        public override void draw(SpriteBatch b)
        {
            // TODO: Clean Arrow.draw up
            float current_scale = 4f * base.localScale;
            float alpha = 1f; // base.startingAlpha;

            // Draw the arrow
            b.Draw(Archery.assetManager.baseArrowTexture, Game1.GlobalToLocal(Game1.viewport, base.position), _arrowBounds, base.color.Value * alpha, base.rotation, _arrowBounds.Size.ToVector2(), current_scale, SpriteEffects.None, (base.position.Y + 96f) / 10000f);

            // TODO: Make this a config / button option
            //Framework.Utilities.Toolkit.DrawHitBox(b, getBoundingBox());

            // TODO: Draw the arrow trail / tail
            /*
            for (int i = this.tail.Count - 1; i >= 0; i--)
            {
                b.Draw(this.spriteFromObjectSheet ? Game1.objectSpriteSheet : Projectile.projectileSheet, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == this.tail.Count - 1) ? ((Vector2)this.position) : this.tail.ElementAt(i + 1), this.tail.ElementAt(i), (float)this.tailCounter / 50f) + new Vector2(0f, 0f - (float)this.height) + new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(this.spriteFromObjectSheet ? Game1.objectSpriteSheet : Projectile.projectileSheet, this.currentTileSheetIndex, 16, 16), this.color.Value * alpha, this.rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (this.position.Y - (float)(this.tail.Count - i) + 96f) / 10000f);
                alpha -= 1f / (float)this.tail.Count;
                current_scale = 0.8f * (float)(4 - 4 / (i + 4));
            }
            */
        }
    }
}
