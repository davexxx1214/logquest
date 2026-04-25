# 16-bit Tile Sheet Slots

The game currently uses CSS pixel placeholders, but the data model is ready for bitmap tile sheets.

Recommended files:

- `heroes-16bit.png`: 6 columns x 4 rows, 64x64 tiles
  - Columns: warrior, mage, monk, hunter, knight, druid
  - Rows: idle, attack, hurt, cast
- `monsters-16bit.png`: 12 columns x 2 rows, 64x64 tiles
  - Columns follow the `monsters.*.spriteIndex` values in `app.js`
  - Rows: idle, attack
- `items-16bit.png`: 12 columns x 1 row, 32x32 tiles
  - Icons follow `itemCatalog.*.icon` in `app.js`

When real art is added, update the portrait and icon CSS to use `background-image`, `background-size`, and `background-position` from these indices.
