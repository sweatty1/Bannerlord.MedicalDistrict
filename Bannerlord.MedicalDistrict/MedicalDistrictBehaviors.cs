using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;
using Helpers;

namespace Bannerlord.MedicalDistrict
{
	internal class MedicalDistrictBehaviors : CampaignBehaviorBase
    {
        public override void SyncData(IDataStore dataStore) { }
		public override void RegisterEvents()
		{
			CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
		}

		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
		{
			AddGameMenus(campaignGameStarter);
		}

		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddGameMenuOption("town", "town_medical_district", "{=l9sFJawW}Go to the medical district", new GameMenuOption.OnConditionDelegate(game_menu_go_to_medical_district_on_condition), delegate (MenuCallbackArgs x)
			{
				GameMenu.SwitchToMenu("town_medical_district");
			}, false, 4, false);
			campaignGameStarter.AddGameMenu("town_medical_district", "You arrive at the city's medical district. As you approach you notice the local nurses and doctors running about treating the masses.", new OnInitDelegate(town_medical_district_on_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none, null);

			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_self_heal", "{=*}Get yourself treated. ({HEAL_SELF_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(player_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealPlayerCharacter();
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_companion_heal", "{=*}Get your companions treated. ({HEAL_COMPANION_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(companions_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealPartyCharacters(false);
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_player_and_companion_heal", "{=*}Get your companions and yourself treated. ({HEAL_ALL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(party_characters_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealPartyCharacters(true);
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_back", "{=qWAmxyYz}Back to town center", new GameMenuOption.OnConditionDelegate(back_on_condition), delegate (MenuCallbackArgs x)
			{
				GameMenu.SwitchToMenu("town");
			}, false, -1, false);
		}
		private bool game_menu_go_to_medical_district_on_condition(MenuCallbackArgs args)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
			return MenuHelper.SetOptionProperties(args, true, false, TextObject.Empty);
		}
		private void town_medical_district_on_init(MenuCallbackArgs args)
		{
			args.MenuTitle = new TextObject("Medical District", null);
		}

		private bool player_needs_heal_on_condition(MenuCallbackArgs args)
		{
			if (Hero.MainHero.HitPoints < Hero.MainHero.MaxHitPoints)
			{
				int price = PriceToHeal(Hero.MainHero);
				MBTextManager.SetTextVariable("HEAL_SELF_AMOUNT", price);
				args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
				return true;
			}
			return false;
		}

		private void HealPlayerCharacter()
		{
			int price = PriceToHeal(Hero.MainHero);
			Hero.MainHero.HitPoints = Hero.MainHero.MaxHitPoints;
			GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -price, false);
			GameMenu.SwitchToMenu("town_medical_district");
		}

		private bool companions_needs_heal_on_condition(MenuCallbackArgs args)
		{
			int numberInjured = 0;
			int price = 0;
			CalculatePriceAndNumInjured(ref price, ref numberInjured, false, false);
			if (numberInjured > 0)
			{
				MBTextManager.SetTextVariable("HEAL_COMPANION_AMOUNT", price);
				args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
				return true;
			}
			return false;
		}

		private bool party_characters_needs_heal_on_condition(MenuCallbackArgs args)
		{
			int numberInjured = 0;
			int price = 0;
			CalculatePriceAndNumInjured(ref price, ref numberInjured, true, false);
			if (numberInjured > 1 && Hero.MainHero.HitPoints < Hero.MainHero.MaxHitPoints)
			{
				MBTextManager.SetTextVariable("HEAL_ALL_AMOUNT", price);
				args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
				return true;
			}
			return false;
		}

		private void HealPartyCharacters(bool healplayer)
		{
			int numberTreated = 0;
			int price = 0;
			CalculatePriceAndNumInjured(ref price, ref numberTreated, healplayer, true);
			if (numberTreated > 0)
			{
				GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -price, false);
			}
			GameMenu.SwitchToMenu("town_medical_district");
		}

		private bool back_on_condition(MenuCallbackArgs args)
		{
			args.optionLeaveType = GameMenuOption.LeaveType.Leave;
			return true;
		}

		private int PriceToHeal(Hero hero)
		{
			double basePrice = 1000;
			double percentHpMissing = (double)(hero.MaxHitPoints - hero.HitPoints)/hero.MaxHitPoints;
			return Convert.ToInt32(basePrice * percentHpMissing);
		}

		private void CalculatePriceAndNumInjured(ref int price, ref int numberTreated, bool includeMainHero, bool restoreHealth)
		{
			TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
			if (memberRoster.TotalHeroes > 0)
			{
				for (int i = 0; i < memberRoster.Count; i++)
				{
					Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
					if (heroObject != null)
					{
						if (heroObject.HitPoints < heroObject.MaxHitPoints && (includeMainHero || heroObject != Hero.MainHero))
						{
							numberTreated++;
							price += PriceToHeal(heroObject);
							if (restoreHealth)
							{
								heroObject.HitPoints = heroObject.MaxHitPoints;
							}
						}
					}
				}
			}
		}
	}
}
