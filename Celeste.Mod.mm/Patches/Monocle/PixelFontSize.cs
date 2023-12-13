#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monocle {
    class patch_PixelFontSize : PixelFontSize {

        public extern Vector2 orig_Measure(string text);
        public new Vector2 Measure(string text) {
            text = Emoji.Apply(text);
            return orig_Measure(text);
        }

        public extern void orig_Draw(char character, Vector2 position, Vector2 justify, Vector2 scale, Color color);
        public new void Draw(char character, Vector2 position, Vector2 justify, Vector2 scale, Color color) {
            if (Emoji.Start <= character &&
                character <= Emoji.Last &&
                !Emoji.IsMonochrome(character)) {
                color = new Color(color.A, color.A, color.A, color.A);
            }
            orig_Draw(character, position, justify, scale, color);
        }

        public extern void orig_Draw(string text, Vector2 position, Vector2 justify, Vector2 scale, Color color, float edgeDepth, Color edgeColor, float stroke, Color strokeColor);
        public new void Draw(string text, Vector2 position, Vector2 justify, Vector2 scale, Color color, float edgeDepth, Color edgeColor, float stroke, Color strokeColor) {
            text = Emoji.Apply(text);

            if (string.IsNullOrEmpty(text))
                return;

            Vector2 offset = Vector2.Zero;
            Vector2 justifyOffs = new Vector2(
                ((justify.X != 0f) ? WidthToNextLine(text, 0) : 0f) * justify.X,
                HeightOf(text) * justify.Y
            );

            var batch = Monocle.Draw.SpriteBatch;

            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    offset.X = 0f;
                    offset.Y += LineHeight;
                    if (justify.X != 0f)
                        justifyOffs.X = WidthToNextLine(text, i + 1) * justify.X;
                    continue;
                }

                PixelFontCharacter c = null;
                if (!Characters.TryGetValue(text[i], out c))
                    continue;


                var texture = c.Texture.Texture.Texture;
                var clipRect = new Rectangle?(c.Texture.ClipRect);
                var scaleFix = ((patch_MTexture) c.Texture).ScaleFix;
                var origin = (-c.Texture.DrawOffset) / scaleFix;

                Vector2 pos = position + (offset + new Vector2(c.XOffset, c.YOffset) - justifyOffs) * scale;
                scale *= scaleFix;

                if (stroke > 0f && !Outline) {
                    if (edgeDepth > 0f) {
                        //c.Texture.Draw(pos + new Vector2(0f, -stroke), Vector2.Zero, strokeColor, scale);
                        batch.Draw(texture, pos + new Vector2(0f, -stroke), c.Texture.ClipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        for (float num2 = -stroke; num2 < edgeDepth + stroke; num2 += stroke) {
                            batch.Draw(texture, pos + new Vector2(-stroke, num2), clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                            batch.Draw(texture, pos + new Vector2(stroke, num2), clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        }
                        batch.Draw(texture, pos + new Vector2(-stroke, edgeDepth + stroke), clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(0f, edgeDepth + stroke), clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(stroke, edgeDepth + stroke), clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                    } else {
                        batch.Draw(texture, pos + new Vector2(-1f, -1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(0f, -1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(1f, -1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(-1f, 0f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(1f, 0f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(-1f, 1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(0f, 1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                        batch.Draw(texture, pos + new Vector2(1f, 1f) * stroke, clipRect, strokeColor, 0f, origin, scale, SpriteEffects.None, 0f);
                    }
                }

                if (edgeDepth > 0f)
                    batch.Draw(texture, pos + Vector2.UnitY * edgeDepth, clipRect, edgeColor, 0f, origin, scale, SpriteEffects.None, 0f);

                Color cColor = color;
                if (Emoji.Start <= c.Character &&
                    c.Character <= Emoji.Last &&
                    !Emoji.IsMonochrome((char) c.Character)) {
                    cColor = new Color(color.A, color.A, color.A, color.A);
                }
                batch.Draw(texture, pos, clipRect, cColor, 0f, origin, scale, SpriteEffects.None, 0f);

                offset.X += c.XAdvance;

                if (i < text.Length - 1 && c.Kerning.TryGetValue(text[i + 1], out int kerning)) {
                    offset.X += kerning;
                }
            }
        }

    }
}
