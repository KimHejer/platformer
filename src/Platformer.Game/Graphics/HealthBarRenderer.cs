using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Game.Graphics;

public static class HealthBarRenderer
{
    private const int BarWidth = 40;
    private const int BarHeight = 5;
    private const int VerticalOffset = 12;

    public static void Draw(
        SpriteBatch spriteBatch,
        Texture2D pixel,
        Rectangle hitbox,
        int currentHealth,
        int maxHealth
    )
    {
        if (maxHealth <= 0 || currentHealth >= maxHealth)
            return;

        float healthRatio = MathHelper.Clamp(
            (float)currentHealth / maxHealth,
            0f,
            1f
        );

        int x = hitbox.Center.X - BarWidth / 2;
        int y = hitbox.Top - VerticalOffset;

        Rectangle background = new(
            x, y, BarWidth, BarHeight
        );

        Rectangle foreground = new(
            x, y,
            (int)(BarWidth * healthRatio),
            BarHeight
        );

        // Greyed out missing health
        spriteBatch.Draw(
            pixel,
            background,
            Color.DarkSlateGray
        );

        Color healthColor = healthRatio switch
        {
            > 0.5f => Color.LimeGreen,
            > 0.25f => Color.Gold,
            _ => Color.Red
        };

        spriteBatch.Draw(
            pixel,
            foreground,
            healthColor
        );
    }
}