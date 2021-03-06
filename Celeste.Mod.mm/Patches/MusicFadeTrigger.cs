﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0169 // The field is never used

using Celeste.Mod;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework;

namespace Celeste {
    class patch_MusicFadeTrigger : MusicFadeTrigger {

        public patch_MusicFadeTrigger(EntityData data, Vector2 offset)
            : base(data, offset) {
            // no-op. MonoMod ignores this - we only need this to make the compiler shut up.
        }

        public extern void orig_OnStay(Player player);
        public override void OnStay(Player player) {
            Level level = Scene as Level;
            if (level == null || level.Session.Area.GetLevelSet() == "Celeste") {
                orig_OnStay(player);
                return;
            }

            Audio.SetMusicParam(
                !string.IsNullOrEmpty(Parameter) ? Parameter : "fade",
                LeftToRight ?
                Calc.ClampedMap(player.Center.X, Left, Right, FadeA, FadeB) :
                Calc.ClampedMap(player.Center.Y, Top, Bottom, FadeA, FadeB)
            );
        }

    }
}
