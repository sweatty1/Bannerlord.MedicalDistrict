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

		// start of the dialog and game Menu code flows
		public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
		{
			this.AddGameMenus(campaignGameStarter);
		}

		public void AddGameMenus(CampaignGameStarter campaignGameStarter)
		{
			campaignGameStarter.AddGameMenuOption("town", "town_medical_district", "{=l9sFJawW}Go to the medical district", new GameMenuOption.OnConditionDelegate(game_menu_go_to_medical_district_on_condition), delegate (MenuCallbackArgs x)
			{
				GameMenu.SwitchToMenu("town_medical_district");
			}, false, 4, false);
			campaignGameStarter.AddGameMenu("town_medical_district", "You arrive at the towns medical district. As you approach you notice the local nurses and doctors running about treating the masses.", new OnInitDelegate(town_medical_district_on_init), GameOverlays.MenuOverlayType.SettlementWithBoth, GameMenu.MenuFlags.none, null);

			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_self_heal", "{=*}Get yourself treated. ({HEAL_SELF_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(player_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealPlayerCharacter();
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_companion_heal", "{=*}Get your companions treated. ({HEAL_COMPANION_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(companions_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealCompanionCharacters();
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_player_and_companion_heal", "{=*}Get your companions and yourself treated. ({HEAL_ALL_AMOUNT}{GOLD_ICON})", new GameMenuOption.OnConditionDelegate(player_and_companions_needs_heal_on_condition), delegate (MenuCallbackArgs x)
			{
				HealPlayerCharacterAndCompanions();
			}, false, -1, false);
			campaignGameStarter.AddGameMenuOption("town_medical_district", "town_medical_district_back", "{=qWAmxyYz}Back to town center", new GameMenuOption.OnConditionDelegate(back_on_condition), delegate (MenuCallbackArgs x)
			{
				GameMenu.SwitchToMenu("town");
			}, false, -1, false);
		}
		private bool game_menu_go_to_medical_district_on_condition(MenuCallbackArgs args)
		{
			bool shouldBeDisabled;
			TextObject disabledText;
			// figure this functionality out
			bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroAccessLocation(Settlement.CurrentSettlement, "tavern", out shouldBeDisabled, out disabledText);
			args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
			return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
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
			TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
			int numberInjured = 0;
			int price = 0;
			if (memberRoster.TotalHeroes > 0)
			{
				for (int i = 0; i < memberRoster.Count; i++)
				{
					Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
					if (heroObject != null)
					{
						if (heroObject.HitPoints < heroObject.MaxHitPoints && heroObject != Hero.MainHero)
						{
							price += PriceToHeal(heroObject);
							numberInjured++;
						}
					}
				}
			}
			if (numberInjured > 0)
			{
				MBTextManager.SetTextVariable("HEAL_COMPANION_AMOUNT", price);
				args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
				return true;
			}
			return false;
		}

		private void HealCompanionCharacters()
		{
			TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
			int numberTreated = 0;
			int price = 0;
			if (memberRoster.TotalHeroes > 0)
			{
				for (int i = 0; i < memberRoster.Count; i++)
				{
					Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
					if (heroObject != null)
					{
						if (heroObject.HitPoints < heroObject.MaxHitPoints && heroObject != Hero.MainHero)
						{
							numberTreated++;
							price += PriceToHeal(heroObject);
							heroObject.HitPoints = heroObject.MaxHitPoints;
						}
					}
				}
			}
			if (numberTreated > 0)
			{
				GiveGoldAction.ApplyBetweenCharacters(null, Hero.MainHero, -price, false);
			}
			GameMenu.SwitchToMenu("town_medical_district");
		}

		private bool player_and_companions_needs_heal_on_condition(MenuCallbackArgs args)
		{
			TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
			int numberInjured = 0;
			int price = 0;
			if (memberRoster.TotalHeroes > 0)
			{
				for (int i = 0; i < memberRoster.Count; i++)
				{
					Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
					if (heroObject != null)
					{
						if (heroObject.HitPoints < heroObject.MaxHitPoints && heroObject != Hero.MainHero)
						{
							numberInjured++;
							price += PriceToHeal(heroObject);
						}
					}
				}
			}
			if (numberInjured > 0 && Hero.MainHero.HitPoints < Hero.MainHero.MaxHitPoints)
			{
				price += PriceToHeal(Hero.MainHero);
				MBTextManager.SetTextVariable("HEAL_ALL_AMOUNT", price);
				args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
				return true;
			}
			return false;
		}

		private void HealPlayerCharacterAndCompanions()
		{
			TroopRoster memberRoster = MobileParty.MainParty.MemberRoster;
			int numberTreated = 0;
			int price = 0;
			if (memberRoster.TotalHeroes > 0)
			{
				for (int i = 0; i < memberRoster.Count; i++)
				{
					Hero heroObject = memberRoster.GetCharacterAtIndex(i).HeroObject;
					if (heroObject != null)
					{
						if (heroObject.HitPoints < heroObject.MaxHitPoints)
						{
							numberTreated++;
							price += PriceToHeal(heroObject);
							heroObject.HitPoints = heroObject.MaxHitPoints;
						}
					}
				}
			}
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
	}
}
