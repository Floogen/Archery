using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Movies;
using StardewValley.Projectiles;

namespace Archery.Framework.Objects.Projectiles
{
    internal class Arrow : BasicProjectile
    {
        // TODO: Make these content pack values
        private static Rectangle _arrowBounds = new Rectangle(4, 7, 8, 1);
        private static Rectangle _arrowCollisionBox = new Rectangle(0, 0, 4, 4);

        public Arrow(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, firer, spriteFromObjectSheet, collisionBehavior)
        {
            base.xVelocity.Value /= 4f;
            base.yVelocity.Value /= 4f;
        }

        public override void behaviorOnCollisionWithMonster(NPC n, GameLocation location)
        {
            base.behaviorOnCollisionWithMonster(n, location);
        }

        public override void draw(SpriteBatch b)
        {
            // TODO: Clean Arrow.draw up
            float current_scale = 4f * this.localScale;
            float alpha = 1f; // base.startingAlpha;

            b.Draw(Archery.assetManager.baseArrowTexture, Game1.GlobalToLocal(Game1.viewport, this.position), _arrowBounds, this.color.Value * alpha, this.rotation, _arrowBounds.Size.ToVector2(), current_scale, SpriteEffects.None, (this.position.Y + 96f) / 10000f);

            // TODO: Make this a config / button option
            Framework.Utilities.Toolkit.DrawHitBox(b, getBoundingBox());
        }

        public override bool isColliding(GameLocation location)
        {
            return base.isColliding(location);
        }

        public override Rectangle getBoundingBox()
        {
            Vector2 pos = this.position.Value;

            float current_scale = this.localScale * 4f;
            int damageSizeWidth = (int)(_arrowCollisionBox.Width * current_scale);
            int damageSizeHeight = (int)(_arrowCollisionBox.Height * current_scale);

            return new Rectangle((int)pos.X - damageSizeWidth / 2, (int)pos.Y - damageSizeHeight / 2, damageSizeWidth, damageSizeHeight);
        }
    }
}
