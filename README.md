# SRO.TextEditor

Simple but effective text code editor/viewer for SRO.

## Features :
- Search by Codename, Exact text, or Similar text
- Add new text or update it
- Text preview in different modes (support images)
- Beautify / Minify included
- Quick and free translator

## Preview :

<details>
      <summary>Click here to show images</summary>
      <br>
      <img width="1280" height="720" alt="preview_01" src="https://github.com/user-attachments/assets/c6714ca1-9513-4e42-a673-77d7a350efe3" />
      <img width="1280" height="720" alt="preview_02" src="https://github.com/user-attachments/assets/91a626d9-ec88-405b-9626-f7de7da9f4df" />
      <img width="1280" height="720" alt="preview_03" src="https://github.com/user-attachments/assets/410b09d6-238e-4749-a04e-39eec29bb509" />
</details>


## Database :

`SRO_TEXTDATA.bak` has been included in [releases](https://github.com/JellyBitz/SRO.TextEditor/releases), a database with texts taken from official clients (iSRO, trSRO, jSRO,..), and some translated with proper translators or filled to mimic consistency.  
Feel free to contribute any improvement, as you may find translations are not perfect if the context is too short, plus SRO UI is not that flexible.

### Languages :

- Korean (`kr`)
- Chinese (`zn_CN`)
- Deutch (`de`)
- Japanese (`ja`)
- English (`en`)
- Portuguese (`pt_BR`)
- French (`fr`)
- Russian (`ru`)
- Turkish (`tr`)
- Spanish (`es`)
- Arabic (`ar`)

### For the Client

The database texts can be extracted with [SRO.Db2Media](https://github.com/JellyBitz/SRO.Db2Media) using the query of your preference, **example**:

`SRO.Db2Media` > `Settings.json`
```json
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextData_Equip&Skill.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextData_EquipAndSkill"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextData_Object.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextData_Object"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextEvent.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextEvent"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextHelp.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextHelp"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextQuest_OtherString.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextQuest_OtherString"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextQuest_QuestString.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextQuest_QuestString"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextQuest_Speech&Name.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextQuest_SpeechAndName"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextUISystem.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextUISystem"
},
{
    "Enabled": true,
    "Database": "SRO_TEXTDATA",
    "Path": "TextZoneName.txt",
    "Query": "SELECT 1 AS [Service], CodeName AS [Codename], ko AS [Korean], '' AS [?], '' AS [?], zh_CN AS [Chinese], de AS [Deutch], ja AS [Japanese], en AS [English], pt_BR AS [Portuguese], fr AS [French], ru AS [Russian], tr AS [Turkish], es AS [Spanish], ar AS [Arabic] FROM TextZoneName"
}
```
