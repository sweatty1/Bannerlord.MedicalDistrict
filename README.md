﻿# Bannerlord.MedicalDistrict
Adds a game menu to towns/cities called Medical District where you can pay to heal yourself or your companions for a small price. Aside from the cost another downside is no medicine exp for the health recovered in this way.

Link to Nexus: [here](https://www.nexusmods.com/mountandblade2bannerlord/mods/2423/)

# Features
Adds a game menu to town/cities called Medical District\
Heal your player character for a price proportional to the missing health (e.g. 100% health missing cost 1000 gold, 50% health 500 gold, 25% health 250 gold etc)\
Heal all your companions for a similar price structure, although they are done all at once. (e.g. two companions one at 75% health and the other at 50% the price would be 750 gold to heal them both)

# Manual Installing
 
 - Extract the zip file to 
 ```text 
 C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules.
 ```
- Navigate to "Modules > Bannerlord.MedicalDistrict > bin > Win64_Shipping_Client" in your game files.
- Right click the "Bannerlord.MedicalDistrict.dll" and click properties
- If you see an unblock at the bottom, click it. (Visual reference: https://www.limilabs.com/blog/unblock-dll-file)
- Start the Bannerlord launcher and then tick Medical District in the Singleplayer > Mods tab.

# Development
 
 The folder structure inside of the Bannerlord module folder would be as follows 
 ```text
- Bannerlord.MedicalDistrict
	- bin
		- Win64_Shipping_Client
			-- Bannerlord.MedicalDistrict.dll
    - SubModule.xml
```

the SubModule.xml being a copy of the one inside of this projects SubModuleXML folder and the Bannerlord.MedicalDistrict.dll being from the build output of the project

Easier way todo the above is to follow the install instructions.

The Refernces will be missing. Right click refernces select add then browse and navigate and select the taleworlds.*.dll from
```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client
```

Note: some dlls are found in base game Module folders like
```text
C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\StoryMode\bin\Win64_Shipping_Client\SotryMode.dll which is not present in the base bin\Win64_Shipping_Client
```

Build a new dll with your changes and move it into the Win64_Shipping_Client folder and run the Bannerlord launcher (just make sure on the mod tab the mod checkbox is ticked)

To remove the manual copy/paste or change the build output path to the Win64_Shipping_Client.
