/**
 * LifeEngine is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * LifeEngine is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 *
 * Copyright © 2026 ArkansasIo
 */

using System;
using System.Collections.Generic;

using LifeEngine.Biology;

namespace LifeEngine.Combat
{
    /// <summary>
    /// The catalogue of effects: every buff, every debuff and every lasting
    /// condition, built on demand.
    ///
    /// Each one is returned fresh rather than shared, because an effect carries
    /// its own stack count and remaining time and two creatures must not share
    /// those.
    /// </summary>
    public static class EffectsCatalogue
    {
        // ------------------------------------------------------------------
        // Buffs
        // ------------------------------------------------------------------

        /// <summary>Rallied by the alpha's howl: steadier and bolder.</summary>
        public static Effect Rallied()
        {
            return new Effect("Rallied", EffectKind.Buff, EffectTarget.Morale, 0.3, 4.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("The alpha called, and it answered. Steadier and bolder.");
        }

        /// <summary>The first kill of a hunt: the pack is fed and confident.</summary>
        public static Effect Fed()
        {
            return new Effect("Fed", EffectKind.Buff, EffectTarget.MaxHealth, 15.0, 6.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("A full belly: more to give before it goes down.");
        }

        /// <summary>Hunting in the dark, as wolves do best.</summary>
        public static Effect NightEyes()
        {
            return new Effect("Night Eyes", EffectKind.Buff, EffectTarget.Accuracy, 0.15, 6.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("The dark is no handicap to a wolf.");
        }

        /// <summary>Fighting on home ground, in the pack's own territory.</summary>
        public static Effect HomeGround()
        {
            return new Effect("Home Ground", EffectKind.Buff, EffectTarget.Defence, 0.15, 8.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("Fighting where it knows every tree and stone.");
        }

        /// <summary>Braced for a charge, four feet planted.</summary>
        public static Effect Braced()
        {
            return new Effect("Braced", EffectKind.Buff, EffectTarget.DamageReduction, 0.2, 2.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("Set for the impact, weight on the back legs.");
        }

        /// <summary>Cornered, with nowhere to run: dangerous rather than afraid.</summary>
        public static Effect Cornered()
        {
            return new Effect("Cornered", EffectKind.Buff, EffectTarget.Damage, 0.4, 3.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("Nothing left to lose, so it fights twice as hard.");
        }

        /// <summary>The moon-mad state: stronger and faster, and past reasoning.</summary>
        public static Effect MoonMad()
        {
            return new Effect("Moon-Mad", EffectKind.Buff, EffectTarget.Strength, 0.15, 6.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("The moon has it. Stronger, faster, and not itself.");
        }

        /// <summary>A scar that has healed hard: the flesh there is numb.</summary>
        public static Effect Calloused()
        {
            return new Effect("Calloused", EffectKind.Buff, EffectTarget.DamageReduction, 0.1, 0.0)
                .Permanent()
                .Describe("Old scar tissue, which no longer feels much.");
        }

        /// <summary>Second wind: reserves found at the point of collapse.</summary>
        public static Effect SecondWind()
        {
            return new Effect("Second Wind", EffectKind.Buff, EffectTarget.StaminaRegeneration, 0.2, 3.0)
                .Stacking(StackMode.Refresh, 1)
                .Describe("Found something in reserve.");
        }

        // ------------------------------------------------------------------
        // Debuffs
        // ------------------------------------------------------------------

        /// <summary>A bitten leg: slower, and worse on its feet.</summary>
        public static Effect Lamed()
        {
            return new Effect("Lamed", EffectKind.Debuff, EffectTarget.Speed, -0.2, 5.0)
                .Stacking(StackMode.Strongest, 1)
                .Describe("A bitten leg. Slower, and easier to hit.");
        }

        /// <summary>Frightened: hesitant, and worse at everything.</summary>
        public static Effect Frightened()
        {
            return new Effect("Frightened", EffectKind.Debuff, EffectTarget.Fear, 0.4, 4.0)
                .Stacking(StackMode.Additive, 3)
                .Describe("Badly frightened, and thinking about running.");
        }

        /// <summary>Blinded by blood in the eyes.</summary>
        public static Effect Blinded()
        {
            return new Effect("Blinded", EffectKind.Debuff, EffectTarget.Accuracy, -0.3, 3.0)
                .Stacking(StackMode.Strongest, 1)
                .Breaks()
                .Describe("Blood in the eyes. It cannot see the next blow coming.");
        }

        /// <summary>Winded: the breath knocked out of it.</summary>
        public static Effect Winded()
        {
            return new Effect("Winded", EffectKind.Debuff, EffectTarget.StaminaRegeneration, -0.1, 4.0)
                .Stacking(StackMode.Strongest, 1)
                .Breaks()
                .Describe("Struck in the ribs and struggling for breath.");
        }

        /// <summary>Exhausted: the reserves are gone.</summary>
        public static Effect Exhausted()
        {
            return new Effect("Exhausted", EffectKind.Debuff, EffectTarget.Damage, -0.25, 6.0)
                .Stacking(StackMode.Strongest, 1)
                .Describe("Running on nothing. The blows are getting weak.");
        }

        /// <summary>Outnumbered: watching every direction at once.</summary>
        public static Effect Outnumbered()
        {
            return new Effect("Outnumbered", EffectKind.Debuff, EffectTarget.Defence, -0.2, 5.0)
                .Stacking(StackMode.Strongest, 1)
                .Describe("Too many jaws and not enough eyes.");
        }

        /// <summary>A curse of silver: nothing heals as it should.</summary>
        public static Effect SilverBrand()
        {
            return new Effect("Silver Brand", EffectKind.Debuff, EffectTarget.Healing, -0.4, 10.0)
                .Stacking(StackMode.Strongest, 1)
                .Describe("A silver wound. It will not close properly.");
        }

        // ------------------------------------------------------------------
        // Conditions: the lasting ones
        // ------------------------------------------------------------------

        /// <summary>Bleeding. Stacks, and keeps hurting after the fight.</summary>
        public static Effect Bleeding()
        {
            return new Effect("Bleeding", EffectKind.Condition, EffectTarget.Bleed, 0.0, 8.0)
                .Stacking(StackMode.Additive, 5)
                .Ticks(3.0)
                .Describe("An open wound. Losing blood every round.");
        }

        /// <summary>Poisoned: slow, relentless damage.</summary>
        public static Effect Poisoned()
        {
            return new Effect("Poisoned", EffectKind.Condition, EffectTarget.Poison, 0.0, 10.0)
                .Stacking(StackMode.Additive, 3)
                .Ticks(2.0)
                .Describe("Something venomous got a tooth in.");
        }

        /// <summary>Infected: a bite gone bad, which does not heal on its own.</summary>
        public static Effect Infected()
        {
            return new Effect("Infected", EffectKind.Condition, EffectTarget.Healing, -0.3, 0.0)
                .Permanent()
                .Ticks(1.0)
                .Describe("A bite gone bad. It festers and will not mend.");
        }

        /// <summary>Lame for good: a leg that never came right.</summary>
        public static Effect LameForever()
        {
            return new Effect("Lame", EffectKind.Condition, EffectTarget.Speed, -0.15, 0.0)
                .Permanent()
                .Describe("An old break that set badly. It will always limp.");
        }

        /// <summary>Moon-bound: the curse itself, carried permanently.</summary>
        public static Effect MoonBound()
        {
            return new Effect("Moon-Bound", EffectKind.Condition, EffectTarget.Morale, 0.05, 0.0)
                .Permanent()
                .Describe("Cursed. The moon has a claim on it that never lapses.");
        }

        /// <summary>Half-starved: nothing in reserve at all.</summary>
        public static Effect Starving()
        {
            return new Effect("Starving", EffectKind.Condition, EffectTarget.MaxHealth, -20.0, 0.0)
                .Permanent()
                .Ticks(1.0)
                .Describe("No food for days. Thin, weak and slow to heal.");
        }

        // ------------------------------------------------------------------
        // The catalogue, by name
        // ------------------------------------------------------------------

        /// <summary>Every effect the catalogue knows, built fresh.</summary>
        public static List<Effect> Catalogue()
        {
            List<Effect> all = new List<Effect>();

            all.Add(Rallied());
            all.Add(Fed());
            all.Add(NightEyes());
            all.Add(HomeGround());
            all.Add(Braced());
            all.Add(Cornered());
            all.Add(MoonMad());
            all.Add(Calloused());
            all.Add(SecondWind());

            all.Add(Lamed());
            all.Add(Frightened());
            all.Add(Blinded());
            all.Add(Winded());
            all.Add(Exhausted());
            all.Add(Outnumbered());
            all.Add(SilverBrand());

            all.Add(Bleeding());
            all.Add(Poisoned());
            all.Add(Infected());
            all.Add(LameForever());
            all.Add(MoonBound());
            all.Add(Starving());

            return all;
        }

        /// <summary>Builds an effect by name, or null if there is no such effect.</summary>
        public static Effect ByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            List<Effect> all = Catalogue();

            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i].Name, name, StringComparison.OrdinalIgnoreCase))
                    return all[i];
            }

            return null;
        }

        /// <summary>
        /// The effects a wolf carries into a fight simply because of what it is:
        /// its curse, its scars, its age, its belly.
        /// </summary>
        public static void ApplyInnate(Combatant fighter)
        {
            if (fighter == null || fighter.Wolf == null)
                return;

            Wolf wolf = fighter.Wolf;

            if (wolf.Traits.IsCursed)
                fighter.Effects.Apply(MoonBound());

            if (wolf.Traits.ScarCount >= 3)
                fighter.Effects.Apply(Calloused());

            if (wolf.Energy < 0.25)
                fighter.Effects.Apply(Starving());

            if (wolf.IsElder())
                fighter.Effects.Apply(LameForever());

            if (wolf.Traits.HumanRetention <= 0.1 && wolf.Traits.IsCursed)
                fighter.Effects.Apply(MoonMad());
        }

        /// <summary>
        /// What a night's hunt does to a fighter: the dark helps, and the moon
        /// changes the odds for anyone cursed.
        /// </summary>
        public static void ApplyNightConditions(Combatant fighter, bool fullMoon)
        {
            if (fighter == null)
                return;

            fighter.Effects.Apply(NightEyes());

            if (fullMoon && fighter.IsCursed)
                fighter.Effects.Apply(MoonMad());
        }

        public static string Describe()
        {
            return "Effects catalogue (" + Catalogue().Count + " entries)";
        }
    }
}
