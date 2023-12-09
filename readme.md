# Permanent Slug Spawns

Adds Slug spawns to the map.

Has setting to ignore garage unlock (force spawn)

Config file format (CSV):

`yardID,trackID,tryOccupied,comment`

- yardID: one of the stations you want to trigger the spawn (HB,SM,etc)
- trackID: the track name ingame
- tryOccupied: try to spawn on an occupied track. careful on long tracks as it will overfill with time.
- comment: ignored, just for user overview

Lines with `#` prefix are ignored as comments

Example `slug_spawns.csv`:
```csv
SM,#Y-#S436#T,0,Tutorial shed
#HB,HB-A2P,0,Harbor parking
#CM,CM-A2D,0,Maintenance track
#GF,GF-A4P,1,Parking track
```
