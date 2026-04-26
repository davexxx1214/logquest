# 16-bit Tile Sheets

Generated with GPT Image 2 for the current LogQuest prototype. Each final PNG has chroma-key removed and can be used directly by the browser UI.

## Files

- `heroes-16bit.png`
  - Layout: 6 columns x 4 rows
  - Columns: warrior, mage, monk, hunter, knight, druid
  - Rows: idle, attack, hurt, cast
  - Current UI uses the idle row for hero portraits.

- `heroes-16bit-v2.png`
  - Layout: 6 columns x 4 rows
  - Columns: warrior, mage, monk, hunter, knight, druid
  - Rows: walk 1, walk 2, battle idle, attack
  - All hero frames face right for left-to-right side scrolling.
  - Current UI uses this sheet for portraits and exploration animation.

- `monsters-16bit.png`
  - Layout: 6 columns x 4 rows
  - Row 1: slime, wolf, bandit, hornet, bat, goblin
  - Row 2: scarab, troll, skeleton, mire, cultist, gargoyle
  - Row 3: bramble, wisp, dryad, boar, apprentice, livingbook
  - Row 4: manawyrm, mirror, deserter, warhound, ballista, captain

- `monsters-16bit-v2.png`
  - Layout: 6 columns x 8 rows
  - Row pairs: idle row, attack row for each group of 6 monsters
  - Monster order matches `monsters.*.spriteIndex`
  - Current UI uses this sheet directly; enemies are expected to face left toward the right-facing heroes.

- `items-16bit.png`
  - Layout: 8 columns x 3 rows
  - Row 1: crude shortsword, hide bracers, traveler cloak, hornet-sting ring, miner axe, iron scale armor, hard shell shield, dim lantern wick
  - Row 2: cracked holy seal, tidal staff, bone charm, gargoyle pauldrons, thorn spear, moss mantle, moon-dew amulet, ancient wood ring
  - Row 3: stardust wand, scribe robe, prism ring, rune bookmark, border longsword, bastion breastplate, battle flag talisman, captain ring

## Source Files

The `*-source.png` files keep the original chroma-key background for future reprocessing.

## Code Mapping

- Hero columns come from `classData.*.spriteIndex`; animation rows are selected by `heroSpriteIndex`.
- Monster tiles come from `monsters.*.spriteIndex`; idle/attack row pairs are selected by `monsterSpriteIndex`.
- Item tiles come from `itemCatalog.*.spriteIndex`.
