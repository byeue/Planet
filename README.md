# 🪐 Planet
 A planning & team management Discord bot programmed in C# using the Discord.NET library [WIP]
----
This bot is primarily being made for my own personal use, as I want a customized "all-in-one" tool that allows me to plan and manage team projects on Discord easily.

Planned Features
----
🗄️ Resource Organizer: Compile or sort attachments and docs into channels for easy access.

👥 Meeting Call Planner: Set up a date, time, attendees, agenda/resources, ping reminders, and more.

👋 Team Member Profiles: View member's contact info, avalibility, project role, timezone, and more.

💬 Neat Notes: Record and organize ideas/notes by sending them as customized embeds in specified channels.

🧮 Trello API: View/Edit/Navigate Trello boards, lists, cards, and checklists.

💻 Github Webhook: Send relevant project commit notifications with commit information directly into channels.

Trello API Progress
----
- Link Trello account by manually providing account token (Need to fix OAuth bugs!)
- Easily set and change default Trello board to navigate/view
- Navigate/View Trello boards, lists, cards


**COMMANDS**
>*All Planet bot commands are case insensitive*

**`+trellotoken`** *`[token]`* ------------- Update user's trello token

**`+trelloboard`** *`[boardName]`* -------- Set user's default board for easy access

**`+trellolists`** ----------------------- View all lists (names) in the selected board

**`+trellolist`** *`[listName]`* ---------- View a list (names, label, label colour) in the selected board

**`+trellocard`** *`[cardName]`* ---------- View a card (name, label, label colour, description, checklists) in the selected board


**SCREENSHOTS**
>*Showcase current commands & progress*

<img width="293" alt="image" src="https://user-images.githubusercontent.com/35664551/125242474-bf86a980-e2ba-11eb-9b1d-bdcf40b63ef3.png">
<img width="575" alt="image" src="https://user-images.githubusercontent.com/35664551/125735441-73f91e37-f8fb-437e-bbce-485f4554eff0.png">
