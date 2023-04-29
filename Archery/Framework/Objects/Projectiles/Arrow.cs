using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;

namespace Archery.Framework.Objects.Projectiles
{
    internal class Arrow : BasicProjectile
    {
        public Arrow(int damageToFarmer, int parentSheetIndex, int bouncesTillDestruct, int tailLength, float rotationVelocity, float xVelocity, float yVelocity, Vector2 startingPosition, string collisionSound, string firingSound, bool explode, bool damagesMonsters = false, GameLocation location = null, Character firer = null, bool spriteFromObjectSheet = false, onCollisionBehavior collisionBehavior = null) : base(damageToFarmer, parentSheetIndex, bouncesTillDestruct, tailLength, rotationVelocity, xVelocity, yVelocity, startingPosition, collisionSound, firingSound, explode, damagesMonsters, location, firer, spriteFromObjectSheet, collisionBehavior)
        {

        }

        public override void draw(SpriteBatch b)
        {
            float current_scale = 4f * this.localScale;
            float alpha = 1f; // base.startingAlpha;
            b.Draw(Archery.assetManager.baseArrowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, 0f - (float)this.height) + new Vector2(32f, 32f)), new Rectangle(4, 7, 8, 1), this.color.Value * alpha, this.startingRotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (this.position.Y + 96f) / 10000f);

            //base.draw(b);
        }
    }
}
